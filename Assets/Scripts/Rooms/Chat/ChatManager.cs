using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Utils;

namespace Rooms.Chat
{
    public class ChatManager : MonoBehaviour, IChatClientListener
    {
        [SerializeField] private TMP_InputField messageField;
        [SerializeField] private TextMeshProUGUI chatBox;
        
        private ChatClient _client;
        private string _channelName;

        private void Start()
        {
            _client = new ChatClient(this);
            _client.Connect(
                PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
                PhotonNetwork.AppVersion,
                new AuthenticationValues(GlobalData.GetOrDefault("currPlayerName", () => "Unknown Player"))
            );
        }

        private void Update()
        {
            _client.Service();
            
            if (Input.GetKeyDown(KeyCode.Return) && _client.CanChatInChannel(_channelName))
            {
                SendChatMessage();
            }
        }

        public void SendChatMessage()
        {
            if (string.IsNullOrEmpty(messageField.text)) return;
            if (_client.PublishMessage(_channelName, messageField.text))
            {
                messageField.text = "";
            }
        }

        public void DebugReturn(DebugLevel level, string message)
        {
        }

        public void OnDisconnected()
        {
        }

        public void OnConnected()
        {
            _channelName = PhotonNetwork.CurrentRoom.Name + "_chat";
            _client.Subscribe(_channelName);
        }

        public void OnChatStateChange(ChatState state)
        {
        }

        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            if (channelName != _channelName) return;
            for (var i = 0; i < senders.Length; i++)
            {
                chatBox.text += $"{senders[i]}: {messages[i]}\n";
            }
        }

        public void OnPrivateMessage(string sender, object message, string channelName)
        {
        }

        public void OnSubscribed(string[] channels, bool[] results)
        {
            
        }

        public void OnUnsubscribed(string[] channels)
        {
        }

        public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
        {
        }

        public void OnUserSubscribed(string channel, string user)
        {
        }

        public void OnUserUnsubscribed(string channel, string user)
        {
        }
    }
}