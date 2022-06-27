using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Buttons
{
    public class SliderTextUpdater : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        private int _addedCharLength;
        private Slider _slider;
        
        private void Start()
        {
            _slider = GetComponent<Slider>();
            var strToAdd = ((int)_slider.value).ToString();
            text.text += strToAdd;
            _addedCharLength = strToAdd.Length;
        }

        public void ValueChanged()
        {
            text.text = text.text[..^_addedCharLength];
            var strToAdd = ((int)_slider.value).ToString();
            text.text += strToAdd;
            _addedCharLength = strToAdd.Length;
        }
    }
}
