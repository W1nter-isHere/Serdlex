using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Buttons
{
    public class SubmitButton : MonoBehaviour, IOnEventCallback
    {
        [SerializeField] private TMP_InputField word;
        [SerializeField] private TMP_InputField chances;
        [SerializeField] private Toggle validate;
        [SerializeField] private Slider slider;

        private float _timer;

        private void Start()
        {
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
            var isWord = string.IsNullOrEmpty(word.text);

            var w = isWord ? "genna" : word.text.Split(" ")[0];
            if (!int.TryParse(chances.text, out var c))
            {
                c = 6;
            }

            var v = !isWord && validate.isOn;

            // TODO: FIX THIS??????????????????????????
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[]
                {
                    GlobalRandom.Instance.RandUniqueInt(0, PhotonNetwork.CurrentRoom.PlayerCount - 1)
                }
            };
            PhotonNetwork.RaiseEvent(3, new[] {w, c.ToString(), v.ToString()}, raiseEventOptions, SendOptions.SendReliable);
            StartCoroutine(Transition());
        }

        private IEnumerator Transition()
        {
            yield return new WaitUntil(() => _timer <= 0);
            SceneTransitioner.Instance.TransitionToScene(2);
        }

        public void OnEvent(EventData photonEvent)
        {
            var code = photonEvent.Code;
            if (code != 4) return;

            var data = (string[]) photonEvent.CustomData;
            var w = data[0];
            var c = int.Parse(data[1]);
            var v = bool.Parse(data[2]);

            GlobalData.Set("gameWordChosen", w);
            GlobalData.Set("gameChances", c);
            GlobalData.Set("gameValidateWord", v);
        }
    }
}