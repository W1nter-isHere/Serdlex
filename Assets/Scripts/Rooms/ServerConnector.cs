using Photon.Pun;

namespace Rooms
{
    public class ServerConnector : MonoBehaviourPunCallbacks
    {
        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()
        {
            SceneTransitioner.Instance.TransitionToScene(4);
        }
    }
}
