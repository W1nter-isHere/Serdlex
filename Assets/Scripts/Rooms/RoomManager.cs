using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Buttons;
using ExitGames.Client.Photon;
using Game;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rooms
{
    public class RoomManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        [SerializeField] private Transform playerList;
        [SerializeField] private GameObject playerRepPrefab;
        [SerializeField] private TextMeshProUGUI roomCode;
        [SerializeField] private TextMeshProUGUI readyButton;
        [SerializeField] private CanvasGroup starting;

        [SerializeField] private Slider chancesSlider;
        [SerializeField] private Toggle toggle;
        
        private Dictionary<string, KeyValuePair<int, GameObject>> _players;
        private List<string> _readiedPlayers;
        private int _gameMode;
        [CanBeNull] private IEnumerator _startingSequence;

        private void Start()
        {
            _players = new Dictionary<string, KeyValuePair<int, GameObject>>();
            _readiedPlayers = new List<string>();
            _gameMode = GlobalData.GetOrDefault("currGameMode", () => GameModes.Invalid);
            
            starting.gameObject.SetActive(false);
            roomCode.text = "Room Code: " + GlobalData.GetOrDefault("currRoomCode", () => "!ERROR!");

            // default settings
            GlobalData.Set("gameValidateWord", true);
            GlobalData.Set("roomChances", 6f);

            // add this
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(PhotonEvents.NewPlayerJoinedRoom, new [] {GlobalData.GetOrDefault("currPlayerName", () => "Unknown Player")}, raiseEventOptions, SendOptions.SendReliable);
        }

        private void PlayerJoined(string p, int sender)
        {
            if (_players.ContainsKey(p)) return;
            var i = Instantiate(playerRepPrefab, playerList);
            i.GetComponentInChildren<TextMeshProUGUI>().text = p;
            _players.Add(p, new KeyValuePair<int, GameObject>(sender, i));
        }

        private void PlayerLeft(string player)
        {
            if (!_players.ContainsKey(player)) return;
            Destroy(_players[player].Value);
            _players.Remove(player);
        }

        public void OnEvent(EventData photonEvent)
        {
            var c = photonEvent.Code;
            var actorNum = photonEvent.Sender;

            switch (c)
            {
                case PhotonEvents.NewPlayerJoinedRoom:
                {
                    var p = (string[]) photonEvent.CustomData;

                    foreach (var p1 in p)
                    {
                        PlayerJoined(p1, actorNum);
                    }

                    var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
                    PhotonNetwork.RaiseEvent(PhotonEvents.PlayersInRoom, _players.Keys.Where(k => !p.Contains(k)).ToArray(), raiseEventOptions, SendOptions.SendReliable);
                    PhotonNetwork.RaiseEvent(PhotonEvents.SyncReadyWithNewPlayer, _readiedPlayers.ToArray(), raiseEventOptions, SendOptions.SendReliable);

                    if (_gameMode == GameModes.Invalid) break;
                    PhotonNetwork.RaiseEvent(PhotonEvents.InitializeGame, _gameMode, raiseEventOptions, SendOptions.SendReliable);

                    break;
                }
                case PhotonEvents.PlayersInRoom:
                    var p3 = (string[]) photonEvent.CustomData;
                    
                    foreach (var p1 in p3)
                    {
                        PlayerJoined(p1, actorNum);
                    }

                    break;
                case PhotonEvents.ToggleReady:
                    var p2 = (string) photonEvent.CustomData;
                    _players[p2].Value.GetComponent<PlayerRep>().Ready();
                    if (_readiedPlayers.Contains(p2))
                    {
                        _readiedPlayers.Remove(p2);
                    }
                    else
                    {
                        _readiedPlayers.Add(p2);
                    }
                    CheckGame();
                    break;
                case PhotonEvents.InitializeGame:
                    _gameMode = (int) photonEvent.CustomData;
                    GlobalData.Set("currGameMode", _gameMode);
                    break;
                case PhotonEvents.SyncReadyWithNewPlayer:
                    var readiedPlayers = (string[]) photonEvent.CustomData;
                    _readiedPlayers = new List<string>(readiedPlayers);
                    break;
                case PhotonEvents.RoomChancesSliderChanged:
                    var data = (float) photonEvent.CustomData;
                    chancesSlider.SetValueWithoutNotify(data);
                    chancesSlider.GetComponent<SliderTextUpdater>().ValueChanged();
                    GlobalData.Set("roomChances", data);
                    break;
                case PhotonEvents.ValidateWordToggleChanged:
                    var validateWord = (bool) photonEvent.CustomData;
                    toggle.SetIsOnWithoutNotify(validateWord);
                    GlobalData.Set("gameValidateWord", validateWord);
                    break;
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            var player = _players.FirstOrDefault(kv => kv.Value.Key == otherPlayer.ActorNumber);

            if (_players.ContainsKey(player.Key))
            {
                PlayerLeft(player.Key);
            }
        }

        public void ChancesSliderChanged(float value)
        {
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            PhotonNetwork.RaiseEvent(PhotonEvents.RoomChancesSliderChanged, value, raiseEventOptions, SendOptions.SendReliable);
            GlobalData.Set("roomChances", value);
        }

        public void ValidateWordToggleChanged(bool value)
        {
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            PhotonNetwork.RaiseEvent(PhotonEvents.ValidateWordToggleChanged, value, raiseEventOptions, SendOptions.SendReliable);
            GlobalData.Set("gameValidateWord", value);
        }
        
        private void CheckGame()
        {
            var count = _readiedPlayers.Count;
            if (count >= _players.Count /*&& count > 1*/)
            {
                starting.gameObject.SetActive(true);
                var text = starting.GetComponentInChildren<TextMeshProUGUI>();
                GetComponent<AudioSource>().Play();
                text.text = "3";
                _startingSequence = StartGame(text);
                StartCoroutine(_startingSequence);
            }
            else if (_startingSequence != null)
            {
                StopCoroutine(_startingSequence);
                _startingSequence = null;
                GetComponent<AudioSource>().Stop();
                starting.gameObject.SetActive(false);
            }
        }

        private IEnumerator StartGame(TMP_Text text)
        {
            LeanTween.value(gameObject, f => starting.alpha = f, 0, 1, 0.2f);
            yield return new WaitForSeconds(2f);
            text.text = "2";
            yield return new WaitForSeconds(2f);
            text.text = "1";
            yield return new WaitForSeconds(3f);

            if (_gameMode == GameModes.Individuals)
            {
                SceneTransitioner.Instance.TransitionToScene(8);
            }
        }

        public void Ready()
        {
            readyButton.text = readyButton.text == "Ready" ? "Unready" : "Ready";
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(PhotonEvents.ToggleReady, GlobalData.GetOrDefault("currPlayerName", () => "Unknown Player"), raiseEventOptions, SendOptions.SendReliable);
        }
    }
}