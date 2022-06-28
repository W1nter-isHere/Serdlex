using Photon.Pun;
using Photon.Realtime;

namespace Buttons
{
    public class BackButton : MonoBehaviourPunCallbacks
    {
        public void Back()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.InRoom)
                {
                    PhotonNetwork.LeaveRoom();
                }

                if (PhotonNetwork.InLobby)
                {
                    PhotonNetwork.LeaveLobby();
                }
                PhotonNetwork.Disconnect();
            }
            else
            {
                SceneTransitioner.Instance.TransitionToScene(0);
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            if (cause == DisconnectCause.DisconnectByClientLogic)
            {
                SceneTransitioner.Instance.TransitionToScene(0);
            }
        }
    }
}