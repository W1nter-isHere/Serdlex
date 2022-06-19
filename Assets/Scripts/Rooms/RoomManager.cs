using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Rooms
{
    public class RoomManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        [SerializeField] private Transform playerList;
        [SerializeField] private GameObject playerRepPrefab;
        [SerializeField] private TextMeshProUGUI roomCode;
        [SerializeField] private TextMeshProUGUI readyButton;
        [SerializeField] private CanvasGroup starting;

        private Dictionary<string, KeyValuePair<int, GameObject>> _players;
        private List<string> _readiedPlayers;
        private int _gameMode;
        [CanBeNull] private IEnumerator _startingSequence;

        private void Start()
        {
            _players = new Dictionary<string, KeyValuePair<int, GameObject>>();
            _readiedPlayers = new List<string>();
            _gameMode = GlobalData.Data.ContainsKey("currGameMode") ? (int) GlobalData.Data["currGameMode"] : -1;
            
            starting.gameObject.SetActive(false);
            roomCode.text = "Room Code: " + GlobalData.Data["currRoomCode"];

            // add this
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(0, new [] {(string) GlobalData.Data["currPlayerName"]}, raiseEventOptions, SendOptions.SendReliable);
        }

        private void PlayerJoined(string p, int sender)
        {
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
                case 0:
                {
                    var p = (string[]) photonEvent.CustomData;

                    foreach (var p1 in p)
                    {
                        if (_players.ContainsKey(p1)) continue;
                        PlayerJoined(p1, actorNum);
                    }

                    var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
                    PhotonNetwork.RaiseEvent(1, _players.Keys.Where(k => !p.Contains(k)).ToArray(), raiseEventOptions, SendOptions.SendReliable);
                    break;
                }
                case 1:
                    var p3 = (string[]) photonEvent.CustomData;
                    foreach (var p1 in p3)
                    {
                        if (_players.ContainsKey(p1)) continue;
                        PlayerJoined(p1, actorNum);
                    }

                    break;
                case 2:
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
                case 3:
                    _gameMode = (int) photonEvent.CustomData;
                    GlobalData.Set("currGameMode", _gameMode);
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

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (_gameMode == -1) return;
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
            PhotonNetwork.RaiseEvent(3, _gameMode, raiseEventOptions, SendOptions.SendReliable);
        }

        private void CheckGame()
        {
            var count = _readiedPlayers.Count;
            if (count >= _players.Count && count > 1)
            {
                starting.gameObject.SetActive(true);
                var text = starting.GetComponentInChildren<TextMeshProUGUI>();
                text.text = "3";
                _startingSequence = StartGame(text);
                StartCoroutine(_startingSequence);
            }
            else if (_startingSequence != null)
            {
                StopCoroutine(_startingSequence);
                _startingSequence = null;
                starting.gameObject.SetActive(false);
            }
        }

        private IEnumerator StartGame(TMP_Text text)
        {
            LeanTween.value(gameObject, f => starting.alpha = f, 0, 1, 0.2f);
            
            yield return new WaitForSeconds(1f);
            text.text = "2";
            yield return new WaitForSeconds(1f);
            text.text = "1";
            yield return new WaitForSeconds(1f);

            // if (_gameMode == 0)
            // {
                SceneTransitioner.Instance.TransitionToScene(8);
            // }
        }

        public void Ready()
        {
            readyButton.text = readyButton.text == "Ready" ? "Unready" : "Ready";
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(2, GlobalData.Data["currPlayerName"], raiseEventOptions, SendOptions.SendReliable);
        }
    }
}