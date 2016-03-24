using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Simoncouche.Bible;

namespace Simoncouche.UI {
    [RequireComponent(typeof(Simoncouche.Bible.BibleQuoteWriter))]
    public class BibleWriterWidget : MonoBehaviour {

        private string _verseValue;
        private string _authorValue;

        private bool _waitingForBeginningInput = true;
        private InputField _focusedInput;

        // COMPONENTS

        private BibleQuoteWriter _writer;

        private Text _titleLabel;

        private InputField _verseField;
        private InputField _authorField;

        private Text _versePlaceholder;
        private Text _authorPlaceholder;

        private Transform[] _scrolls = new Transform[2];

        // METHODS

        private void Awake() {
            string godName = (GameManager.Instance.lastWinner == LevelManager.Player.cthulu) ? "CTHULHU" : "SOBEK";

            _writer = GetComponent<BibleQuoteWriter>();

            _titleLabel = transform.Find("TitleLabel").GetComponent<Text>();
            _verseField = transform.Find("VerseField").GetComponent<InputField>();
            _authorField = transform.Find("AuthorField").GetComponent<InputField>();
            _versePlaceholder = transform.Find("VerseField/Placeholder").GetComponent<Text>();
            _authorPlaceholder = transform.Find("AuthorField/Placeholder").GetComponent<Text>();

            _scrolls[(int)LevelManager.Player.cthulu] = transform.Find("Scrolls/Cthulhu").transform;
            _scrolls[(int)LevelManager.Player.sobek] = transform.Find("Scrolls/Sobek").transform;

            // Initialization
            _scrolls[(int)LevelManager.Player.cthulu].gameObject.SetActive(GameManager.Instance.lastWinner == LevelManager.Player.cthulu);
            _scrolls[(int)LevelManager.Player.sobek].gameObject.SetActive(GameManager.Instance.lastWinner == LevelManager.Player.sobek);

            _titleLabel.text = string.Format("CULTISTE DE {0}!", godName);

            switch (GameManager.Instance.lastWinner) {
            case LevelManager.Player.cthulu:
                _versePlaceholder.text = "Et depuis ce jour, il fut interdit de manger du poulpe le dimanche. #praisethetentacles";
                _authorPlaceholder.text = "Archevêque Ventouse";
                break;
            case LevelManager.Player.sobek:
                _versePlaceholder.text = "Et depuis ce jour, plus aucune sacoche en peau de crocodile ne fut jamais produite. #fashion";
                _authorPlaceholder.text = "Croque-mister";
                break;
            }

            _focusedInput = null;
        }

        private void Update() {

            // #SHAME

            // Submit handling
            if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && _verseField.text != "" && _authorField.text != "") {
                _writer.SaveQuote(_verseField.text, _authorField.text, (GameManager.Instance.lastWinner == LevelManager.Player.sobek));
                GameManager.Instance.SwitchScene(GameManager.Scene.Menu);
            } 

            // Tab order handling
            else if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
                if (_focusedInput == _verseField) {
                    _authorField.Select();
                } else {
                    _verseField.Select();
                }
            }
                
            // First input handling
            else if (Input.anyKeyDown && _focusedInput == null) {
                _verseField.Select();
                _verseField.text = Input.inputString;

                StartCoroutine(UnselectField());
            }

            // Updating focus state after logic pass
            if (_verseField.isFocused) {
                _focusedInput = _verseField;
            } else if (_authorField.isFocused) {
                _focusedInput = _authorField;
            } else {
                _focusedInput = null;
            }
        }

        private IEnumerator UnselectField() {
            yield return 0;
            _verseField.MoveTextEnd(false);
        }
    }
}