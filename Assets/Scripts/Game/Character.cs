using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Character : MonoBehaviour
    {
        private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI text;
        
        private void Start()
        {
            _backgroundImage = GetComponent<Image>();
        }

        public void Green()
        {
            _backgroundImage.color = Color.green;
        }

        public void Yellow()
        {
            _backgroundImage.color = Color.yellow;
        }

        public void Gray()
        {
            _backgroundImage.color = Color.gray;
        }

        public void SetText(string c)
        {
            text.text = c;
        }
    }
}