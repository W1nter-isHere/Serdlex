using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Rooms
{
    public class ServerConnector : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TextMeshProUGUI text;
        private Coroutine _coroutine;
        
        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
            _coroutine = StartCoroutine(ChangeText());
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

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            StopCoroutine(_coroutine);
            SceneTransitioner.Instance.TransitionToScene(4);
        }
    }
}
