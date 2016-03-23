using UnityEngine;
using UnityEngine.UI;

using Simoncouche.Bible;

namespace Simoncouche.UI {
    [RequireComponent(typeof(Simoncouche.Bible.BibleQuoteWriter))]
    public class BibleWriterWidget : MonoBehaviour {

        private string _verseValue;
        private string _authorValue;

        // COMPONENTS

        private BibleQuoteWriter _writer;

        private Text _titleLabel;
        private Text _descriptionLabel;

        private InputField _verseField;
        private InputField _authorField;

        private Text _versePlaceholder;
        private Text _authorPlaceholder;

        private Transform[] _scrolls = new Transform[2];

        // METHODS

        private void Awake() {
            _writer = GetComponent<BibleQuoteWriter>();

            _titleLabel = transform.Find("TitleLabel").GetComponent<Text>();
            _descriptionLabel = transform.Find("DescriptionLabel").GetComponent<Text>();

            _verseField = transform.Find("VerseField").GetComponent<InputField>();
            _authorField = transform.Find("AuthorField").GetComponent<InputField>();

            _versePlaceholder = transform.Find("VerseField/Placeholder").GetComponent<Text>();
            _authorPlaceholder = transform.Find("AuthorField/Placeholder").GetComponent<Text>();

            _scrolls[LevelManager.Player.cthulu] = transform.Find("Scrolls/Cthulhu").transform;
            _scrolls[LevelManager.Player.sobek] = transform.Find("Scrolls/Sobek").transform;


            // Event binding
            _verseField.onEndEdit.AddListener(OnVerseEndEdit);
            _authorField.onEndEdit.AddListener(OnAuthorEndEdit);

            // Initialization
            _scrolls[LevelManager.Player.cthulu].gameObject.SetActive(GameManager.Instance.lastWinner == LevelManager.Player.cthulu);
            _scrolls[LevelManager.Player.sobek].gameObject.SetActive(GameManager.Instance.lastWinner == LevelManager.Player.sobek);
            
            switch (GameManager.Instance.lastWinner) {
            default:
            break;
            }
        }

        private void Start() {
            _verseField.Select();
        }

        private void OnVerseEndEdit(string value) {
            _verseValue = value;
            _authorField.Select();
        }

        private void OnAuthorEndEdit(string value) {
            _authorValue = value;
            _writer.SaveQuote(_verseField, _authorValue, (GameManager.Instance.lastWinner == LevelManager.Player.sobek));
        }
    }
}