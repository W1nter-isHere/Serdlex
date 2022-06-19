using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Buttons
{
    public class JoinRoomButton : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private CanvasGroup errorMessage;

        public void Connect()
        {
            if (string.IsNullOrEmpty(nameInput.text) || string.IsNullOrEmpty(codeInput.text))
            {
                StartCoroutine(Error());
                return;
            }

            PhotonNetwork.JoinRoom(codeInput.text);
        }
        
        private IEnumerator Error()
        {
            LeanTween.value(gameObject, f => errorMessage.alpha = f, 0, 1, 0.5f);
            yield return new WaitForSeconds(2.5f);
            LeanTween.value(gameObject, f => errorMessage.alpha = f, 1, 0, 0.5f); 
        }

        public override void OnJoinedRoom()
        {
            GlobalData.Data.Clear();
            
            GlobalData.Set("currPlayerName", nameInput.text);
            GlobalData.Set("currRoomCode", codeInput.text);
            SceneTransitioner.Instance.TransitionToScene(7);
        }
    }
}