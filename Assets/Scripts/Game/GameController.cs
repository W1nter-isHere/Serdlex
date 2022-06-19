﻿using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameObject guesses;
        [SerializeField] private GameObject rowPrefab;
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private TMP_InputField textField;
        [SerializeField] private CanvasGroup characterErrorText;
        [SerializeField] private CanvasGroup invalidWordErrorText;

        [SerializeField] private GameOverScreen win;
        [SerializeField] private GameOverScreen lose;

        private int _characters;
        private int _chances;
        private string _chosenWord;
        private bool _checkWord;
        
        private int _chance;
        private string _lastText;
        private bool _entering;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) && !_entering)
            {
                StartCoroutine(EnterWord());
            }
        }

        private void Start()
        {
            var w = GlobalData.Data.ContainsKey("gameWordChosen") ? (string) GlobalData.Data["gameWordChosen"] : "genna";
            var c = GlobalData.Data.ContainsKey("gameChances") ? (int) GlobalData.Data["gameChances"] : 6;
            var v = GlobalData.Data.ContainsKey("gameValidateWord") && (bool) GlobalData.Data["gameValidateWord"];
            
            Init(w, c, v);
        }

        public void Init(string word, int chancesCount, bool validWord)
        {
            _chosenWord = word.ToLower().Replace(" ", "");
            _characters = _chosenWord.Length;
            _chances = chancesCount;
            _checkWord = validWord;
            
            _chance = 0;
            _lastText = "";
            
            CreateRows();
        }

        private void CreateRows()
        {
            for (var i = 0; i < _chances; i++)
            {
                var row = Instantiate(rowPrefab, guesses.transform);
                for (var j = 0; j < _characters; j++)
                {
                    Instantiate(characterPrefab, row.transform);
                }
            }
        }

        private IEnumerator EnterWord()
        {
            _entering = true;
            
            var word = _lastText.ToLower().Replace(" ", "");

            if (word.Length < _characters)
            {
                LeanTween.value(gameObject, f => characterErrorText.alpha = f, 0, 1, 0.5f);
                yield return new WaitForSeconds(2f);
                LeanTween.value(gameObject, f => characterErrorText.alpha = f, 1, 0, 0.5f);
                yield break;
            }
            
            textField.text = "";

            var row = guesses.transform.GetChild(_chance).gameObject;
            for (var i = 0; i < _characters; i++)
            {
                row.transform.GetChild(i).GetComponent<Character>().SetText(word[i].ToString());
                yield return new WaitForSeconds(0.2f);
            }
            
            if (_checkWord)
            {
                StartCoroutine(ValidateWord(word));
            }
            else
            {
                StartCoroutine(TestWord(word));
            }
        }

        private IEnumerator ValidateWord(string word)
        {
            var uwr = UnityWebRequest.Get("https://api.dictionaryapi.dev/api/v2/entries/en/" + word);
            yield return uwr.SendWebRequest();
            
            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                yield break;
            }

            var dataStr = uwr.downloadHandler.text;

            if (string.IsNullOrEmpty(dataStr)) yield break;
            
            var data = JsonConvert.DeserializeObject<dynamic>(dataStr);
                
            if (data is JArray)
            {
                StartCoroutine(TestWord(word));
            }
            else
            {
                LeanTween.value(gameObject, f => invalidWordErrorText.alpha = f, 0, 1, 0.5f);
                yield return new WaitForSeconds(2.5f);
                LeanTween.value(gameObject, f => invalidWordErrorText.alpha = f, 1, 0, 0.5f);
            }
        }

        private IEnumerator TestWord(string word)
        {
            if (_characters != _chosenWord.Length)
            {
                throw new Exception("This should not happen!");
            }

            var row = guesses.transform.GetChild(_chance).gameObject;
            var checkedIndices = new Dictionary<char, int>();
            var successes = 0;

            for (var i = 0; i < _characters; i++)
            {
                var character = row.transform.GetChild(i).GetComponent<Character>();
                var currentChar = word[i];
                var containsCurChar = checkedIndices.ContainsKey(currentChar);
                var nextCurChar = containsCurChar ? _chosenWord.IndexOf(currentChar, checkedIndices[currentChar] + 1) : -1;

                if (currentChar == _chosenWord[i])
                {
                    character.Green();
                    successes++;
                }
                else if (_chosenWord.Contains(currentChar) && (!containsCurChar || nextCurChar != -1))
                {
                    var currIndex = _chosenWord.IndexOf(currentChar);
                    
                    if (_chosenWord[currIndex] == word[currIndex] || (containsCurChar && _chosenWord[nextCurChar] == word[nextCurChar]))
                    {
                        character.Gray();
                        yield return new WaitForSeconds(0.2f);
                        continue;
                    }

                    character.Yellow();
                    checkedIndices.Remove(currentChar);
                    checkedIndices.Add(currentChar, containsCurChar ? nextCurChar : currIndex);
                }
                else
                {
                    character.Gray();
                }

                yield return new WaitForSeconds(0.2f);
            }

            _entering = false;
            
            if (successes == _characters)
            {
                win.GameOver();
                yield break;
            }
            
            _chance++;

            if (_chance >= _chances)
            {
                lose.GameOver();
            }
        }

        public void OnTextValueChanged(string value)
        {
            if (value.Length > _characters)
            {
                textField.text = _lastText;
                return;
            }
            
            _lastText = value;
        }
    }
}