using System;
using System.Collections;
using System.Collections.Generic;
using Game.GameModes;
using TMPro;
using UnityEngine;
using Utils;

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
        [SerializeField] private LoseScreen lose;

        private int _characters;
        private int _chances;
        private string _chosenWord;
        private bool _checkWord;

        private string _lastText;
        private bool _entering;
        private WordleGame _game;

        private BaseGameMode _gameMode;
        private WordValidationState _validationState;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) && !_entering)
            {
                StartCoroutine(EnterWord());
            }
        }

        private void Start()
        {
            win.gameObject.SetActive(false);
            lose.gameObject.SetActive(false);
            Init(GlobalData.GetOrDefault("wordleGame", () => WordleGame.Default));
        }

        public void Init(WordleGame game)
        {
            _chosenWord = game.Word;
            _characters = game.CharactersCount;
            _chances = game.TotalChances;
            _checkWord = game.ValidateWord;

            _game = game;
            _lastText = "";

            _gameMode = GlobalData.GetOrDefault("gameModeObject", () => GameModesRegistry.ClassicIndividuals);
            characterErrorText.GetComponent<TextMeshProUGUI>().text = "Enter at least " + _characters + " characters";
            _gameMode.OnInit(this, _game);

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

                if (_game.Tries == 0 || i >= _game.Tries) continue;
                InitializeRow(row, _game.Guesses[i]);
            }
        }

        public IEnumerator EnterWord()
        {
            _entering = true;

            var word = _lastText.ToLower().Replace(" ", "");

            textField.text = "";

            yield return StartCoroutine(_gameMode.OnWordEnter(this, word));
            yield return StartCoroutine(_gameMode.OnWordCheck(this, word));
            yield return StartCoroutine(_gameMode.OnWordFinished(this, word));
        }

        public IEnumerator ShowGuessedWord(string word)
        {
            var row = guesses.transform.GetChild(_game.Tries).gameObject;
            for (var i = 0; i < _characters; i++)
            {
                row.transform.GetChild(i).GetComponent<Character>().SetText(word[i].ToString());
                yield return new WaitForSeconds(0.2f);
            }
        }

        public IEnumerator CheckWord(string word)
        {
            if (_checkWord)
            {
                yield return StartCoroutine(RawValidateWord(word));
            }
            else
            {
                yield return StartCoroutine(RawTestWord(word));
            }
        }

        private IEnumerator RawValidateWord(string word)
        {
            _validationState = WordValidationState.Validating;
            yield return StartCoroutine(_gameMode.IsWordValid(this, word));

            switch (_validationState)
            {
                case WordValidationState.Valid:
                    StartCoroutine(RawTestWord(word));
                    break;
                case WordValidationState.NotValid:
                    StartCoroutine(ShowNotValidWordError());
                    break;
                default:
                    throw new Exception("Validation state must not be Validating after IsWordValid is called!");
            }
        }

        private IEnumerator RawTestWord(string word)
        {
            if (_characters != _chosenWord.Length)
            {
                throw new Exception("This should not happen!");
            }

            var row = guesses.transform.GetChild(_game.Tries).gameObject;
            var checkedIndices = new Dictionary<char, int>();
            var successes = 0;

            for (var i = 0; i < _characters; i++)
            {
                var character = row.transform.GetChild(i).GetComponent<Character>();
                var currentChar = word[i];
                var containsCurChar = checkedIndices.ContainsKey(currentChar);
                var nextCurChar = containsCurChar
                    ? _chosenWord.IndexOf(currentChar, checkedIndices[currentChar] + 1)
                    : -1;

                if (currentChar == _chosenWord[i])
                {
                    character.Green();
                    successes++;
                }
                else if (_chosenWord.Contains(currentChar) && (!containsCurChar || nextCurChar != -1))
                {
                    var currIndex = _chosenWord.IndexOf(currentChar);

                    if (_chosenWord[currIndex] == word[currIndex] ||
                        (containsCurChar && _chosenWord[nextCurChar] == word[nextCurChar]))
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

            _game.Guesses.Add(word);
            _entering = false;

            if (successes == _characters)
            {
                win.gameObject.SetActive(true);
                win.GameOver();
                yield break;
            }

            _game.Tries++;

            if (_game.Tries >= _chances)
            {
                lose.gameObject.SetActive(true);
                lose.WordWas(_chosenWord);
                lose.GameOver();
            }
        }

        public IEnumerator ShowObject(CanvasGroup canvasGroup, float time)
        {
            canvasGroup.gameObject.SetActive(true);
            LeanTween.value(gameObject, f => canvasGroup.alpha = f, 0, 1, time);
            yield return new WaitForSeconds(time);
        }

        public IEnumerator DisableObject(CanvasGroup canvasGroup, float time)
        {
            LeanTween.value(gameObject, f => canvasGroup.alpha = f, 1, 0, time);
            yield return new WaitForSeconds(time);
            canvasGroup.gameObject.SetActive(false);
        }

        public IEnumerator ShowBriefly(CanvasGroup canvasGroup, float transitionTime, float holdTime = 2f)
        {
            yield return ShowObject(canvasGroup, transitionTime);
            yield return new WaitForSeconds(holdTime);
            yield return DisableObject(canvasGroup, transitionTime);
        }

        public IEnumerator ShowNotEnoughCharactersError()
        {
            yield return ShowBriefly(characterErrorText, 0.5f);
            yield return new WaitForSeconds(0.5f);
            _entering = false;
        }

        public IEnumerator ShowNotValidWordError()
        {
            yield return ShowBriefly(invalidWordErrorText, 0.5f);
            yield return new WaitForSeconds(0.5f);
            _entering = false;
        }

        public void InitializeRow(GameObject row, string word)
        {
            var checkedIndices = new Dictionary<char, int>();

            for (var i = 0; i < _characters; i++)
            {
                var character = row.transform.GetChild(i).GetComponent<Character>();
                var currentChar = word[i];
                var containsCurChar = checkedIndices.ContainsKey(currentChar);
                var nextCurChar = containsCurChar
                    ? _chosenWord.IndexOf(currentChar, checkedIndices[currentChar] + 1)
                    : -1;
                
                character.SetText(currentChar.ToString());

                if (currentChar == _chosenWord[i])
                {
                    character.Green();
                }
                else if (_chosenWord.Contains(currentChar) && (!containsCurChar || nextCurChar != -1))
                {
                    var currIndex = _chosenWord.IndexOf(currentChar);

                    if (_chosenWord[currIndex] == word[currIndex] ||
                        (containsCurChar && _chosenWord[nextCurChar] == word[nextCurChar]))
                    {
                        character.Gray();
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
            }
        }
        
        public WordleGame GetCurrentGame()
        {
            return _game;
        }

        public void SetValidationState(WordValidationState state)
        {
            _validationState = state;
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