using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Game;
using Game.GameModes;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UIElements;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using AudioType = Utils.AudioType;

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
        private BaseGameMode _gameModeObject;
        [CanBeNull] private IEnumerator _startingSequence;

        private void Start()
        {
            PhotonPeer.RegisterType(typeof(WordleGame), 0, WordleGame.Serialize, WordleGame.Deserialize);

            _players = new Dictionary<string, KeyValuePair<int, GameObject>>();
            _readiedPlayers = new List<string>();
            _gameMode = GlobalData.GetOrDefault("currGameMode", () => GameModeTypes.Invalid);

            _gameModeObject = _gameMode == -1 ? null : GameModesRegistry.GameModes[_gameMode];
            
            starting.gameObject.SetActive(false);
            roomCode.text = "Room Code: " + GlobalData.GetOrDefault("currRoomCode", () => "!ERROR!");

            // default settings
            GlobalData.Set("gameValidateWord", true);
            GlobalData.Set("roomChances", 6f);

            // add this
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            var playerName = GlobalData.GetOrDefault("currPlayerName", () => "Unknown Player");
            PhotonNetwork.RaiseEvent(PhotonEvents.NewPlayerJoinedRoom, new [] {playerName}, raiseEventOptions, SendOptions.SendReliable);
            PlayerJoined(playerName, PhotonNetwork.LocalPlayer.ActorNumber);
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
        
        // TODO: 

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

                    // tells new player the players already in the room
                    PhotonNetwork.RaiseEvent(PhotonEvents.PlayersInRoom, _players.Keys.Where(k => !p.Contains(k)).ToArray(), raiseEventOptions, SendOptions.SendReliable);
                    // tells new player the readied players in the room
                    PhotonNetwork.RaiseEvent(PhotonEvents.SyncReadyWithNewPlayer, _readiedPlayers.ToArray(), raiseEventOptions, SendOptions.SendReliable);
                    // tells new player the room chance slider setting
                    PhotonNetwork.RaiseEvent(PhotonEvents.RoomChancesSliderChanged, chancesSlider.value, raiseEventOptions, SendOptions.SendReliable);
                    // tells new player the room validate word toggle setting
                    PhotonNetwork.RaiseEvent(PhotonEvents.ValidateWordToggleChanged, toggle.isOn, raiseEventOptions, SendOptions.SendReliable);
                    
                    // sync game mode if this has valid game mode
                    if (_gameMode == GameModeTypes.Invalid) break;
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
                    _gameModeObject = _gameMode == -1 ? null : GameModesRegistry.GameModes[_gameMode];
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
            if (count >= _players.Count && count > 1)
            {
                starting.gameObject.SetActive(true);
                var text = starting.GetComponentInChildren<TextMeshProUGUI>();
                AudioManager.Instance.Play(AudioType.Countdown);
                text.text = "3";
                _startingSequence = StartGame(text);
                StartCoroutine(_startingSequence);
            }
            else if (_startingSequence != null)
            {
                StopCoroutine(_startingSequence);
                _startingSequence = null;
                AudioManager.Instance.Interrupt();
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

            GlobalData.Set("gameModeObject", _gameModeObject);
            _gameModeObject.OnGameStart(this, (int) chancesSlider.value, toggle.isOn);
        }

        public void Ready()
        {
            readyButton.text = readyButton.text == "Ready" ? "Unready" : "Ready";
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(PhotonEvents.ToggleReady, GlobalData.GetOrDefault("currPlayerName", () => "Unknown Player"), raiseEventOptions, SendOptions.SendReliable);
        }
    }
}