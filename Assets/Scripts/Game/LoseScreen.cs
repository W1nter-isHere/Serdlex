using System.Collections;
using TMPro;
using UnityEngine;

namespace Game
{
    public class LoseScreen : GameOverScreen
    {
        [SerializeField] private TextMeshProUGUI wordWas;

        public void WordWas(string word)
        {
            StartCoroutine(TypeWord(word));
        }

        private IEnumerator TypeWord(string word)
        {
            foreach (var character in word)
            {
                wordWas.text += character;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}