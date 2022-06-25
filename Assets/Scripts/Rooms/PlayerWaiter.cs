using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Game;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Rooms
{
    public class PlayerWaiter : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        [SerializeField] private TextMeshProUGUI text;
        
        private List<WordleGame> _wordles;
        private Coroutine _coroutine;
        
        private void Start()
        {
            _coroutine = StartCoroutine(ChangeText());
            _wordles = new List<WordleGame>();
            
            var game = GlobalData.GetOrDefault("submittedGame", () => WordleGame.Default);
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(PhotonEvents.GameSubmitted, game, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private IEnumerator ChangeText()
        {
            while (true)
            {
                text.text += ".";
                yield return new WaitForSeconds(2f);
                text.text += ".";
                yield return new WaitForSeconds(2f);
                text.text += ".";
                yield return new WaitForSeconds(2f);
                text.text = text.text[..^3];
                yield return new WaitForSeconds(4f);
            }
        }

        private void Update()
        {
            if (_wordles.Count == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                GlobalData.Set("wordleGame", _wordles[Random.Range(1, _wordles.Count)]);
                SceneTransitioner.Instance.TransitionToScene(2);
            }
        }

        private void OnDestroy()
        {
            StopCoroutine(_coroutine);
        }

        public void OnEvent(EventData photonEvent)
        {
            var c = photonEvent.Code;

            switch (c)
            {
                case PhotonEvents.GameSubmitted:
                    _wordles.Add((WordleGame) photonEvent.CustomData);
                    var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
                    PhotonNetwork.RaiseEvent(PhotonEvents.ExistingGamesSubmitted, _wordles.ToArray(), raiseEventOptions, SendOptions.SendReliable);
                    break;
                case PhotonEvents.ExistingGamesSubmitted:
                    var games = (WordleGame[]) photonEvent.CustomData;
                    foreach (var game in games)
                    {
                        if (_wordles.Contains(game)) continue;
                        _wordles.Add(game);
                    }
                    break;
            }
        }
    }
}
