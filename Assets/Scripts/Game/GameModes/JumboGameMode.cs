using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rooms;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

namespace Game.GameModes
{
    public class JumboGameMode : BaseGameMode
    {
        public JumboGameMode(string displayName) : base(displayName)
        {
        }

        public override void OnGameStart(RoomManager roomManager, int chances, bool validateWord)
        {
            roomManager.StartCoroutine(StartGame(chances, validateWord));
        }

        private IEnumerator StartGame(int chances, bool validateWord)
        {
            var uwr = UnityWebRequest.Get("https://random-word-form.herokuapp.com/random/noun/");
            yield return uwr.SendWebRequest();
            
            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                yield break;
            }

            var dataStr = uwr.downloadHandler.text;
            if (string.IsNullOrEmpty(dataStr)) yield break;
            
            var data = JsonConvert.DeserializeObject<JArray>(dataStr);
            var word = data![0];
            
            Debug.Log(word.ToString());

            GlobalData.Set("wordleGame", new WordleGame(word.ToString(), chances, validateWord));
            SceneTransitioner.Instance.TransitionToScene(2);
        }

        public override IEnumerator OnWordFinished(GameController controller, string word)
        {
            GlobalData.Set("submittedGame", controller.GetCurrentGame());
            SceneTransitioner.Instance.TransitionToScene(10);
            yield break;
        }

        public override bool IsEnoughPlayers(int playerCount)
        {
            return playerCount > 0;
        }
    }
}