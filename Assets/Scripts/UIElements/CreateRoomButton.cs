using System;
using System.Collections;
using System.Linq;
using Game.GameModes;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Utils;

namespace UIElements
{
    public class CreateRoomButton : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_Dropdown gameMode;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField roomName;
        [SerializeField] private CanvasGroup errorMessage;

        private void Start()
        {
            gameMode.ClearOptions();
            gameMode.AddOptions(GameModesRegistry.GameModes.Select(mode => mode.DisplayName).ToList());
        }

        public void CreateRoom()
        {
            if (string.IsNullOrEmpty(nameInput.text) || string.IsNullOrEmpty(roomName.text))
            {
                StartCoroutine(Error());
                return;
            }
            
            PhotonNetwork.CreateRoom(roomName.text);
        }

        private IEnumerator Error()
        {
            LeanTween.value(gameObject, f => errorMessage.alpha = f, 0, 1, 0.5f);
            yield return new WaitForSeconds(2.5f);
            LeanTween.value(gameObject, f => errorMessage.alpha = f, 1, 0, 0.5f); 
        }

        public override void OnJoinedRoom()
        {
            GlobalData.ClearData();
            
            GlobalData.Set("currPlayerName", nameInput.text);
            GlobalData.Set("currRoomCode", roomName.text);
            GlobalData.Set("currGameMode", gameMode.value);
            
            SceneTransitioner.Instance.TransitionToScene(7);
        }
    }
}