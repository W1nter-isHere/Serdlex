using ExitGames.Client.Photon;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Buttons
{
    public class SubmitButton : MonoBehaviour
    {
        [SerializeField] private TMP_InputField word;
        [SerializeField] private Toggle validate;
        [SerializeField] private Slider slider;

        private float _timer;

        private void Start()
        {
            PhotonPeer.RegisterType(typeof(WordleGame), 0, WordleGame.Serialize, WordleGame.Deserialize);

            _timer = 90;
            slider.maxValue = 90;
            slider.minValue = 0;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            slider.value = _timer;

            if (_timer <= 0)
            {
                Submit();
            }
        }

        public void Submit()
        {
            var game = new WordleGame(word.text, validate.isOn, 6);
            GlobalData.Set("submittedGame", game);
            SceneTransitioner.Instance.TransitionToScene(10);
        }
    }
}