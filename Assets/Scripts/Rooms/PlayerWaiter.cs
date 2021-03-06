using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Game;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Utils;
namespace Rooms
{
    public class PlayerWaiter : MonoBehaviourPunCallbacks, IOnEventCallback
    {

        [SerializeField] private TextMeshProUGUI text;
        
        private List<WordleGame> _wordles;
        private WordleGame _myGame;
        private Coroutine _coroutine;
        private bool _transitioning;

        private void Start()
        {
            _coroutine = StartCoroutine(ChangeText());
            _wordles = new List<WordleGame>();
            _myGame = new WordleGame(GlobalData.GetOrDefault("submittedGame", () => WordleGame.Default));
            var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(PhotonEvents.GameSubmitted, _myGame, raiseEventOptions, SendOptions.SendReliable);
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
            // when everyone has finished
            if (_wordles.Count >= PhotonNetwork.CurrentRoom.PlayerCount && !_transitioning)
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            _transitioning = true;

            _wordles.Sort((game1, game2) => StringComparer.InvariantCulture.Compare(game1.Word, game2.Word));
            var myGameIndex = _wordles.IndexOf(_myGame);
            
            if (myGameIndex == -1)
            {
                throw new Exception("Something went wrong in encryption sorting at word passing phase");
            }

            GlobalData.Set("wordleGame", myGameIndex == 0 ? _wordles.Last() : _wordles[myGameIndex-1]);
            SceneTransitioner.Instance.TransitionToScene(2);
        }
        
        private void OnDestroy()
        {
            StopCoroutine(_coroutine);
        }

        public virtual void OnEvent(EventData photonEvent)
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
