using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Simoncouche.Bible;

namespace Simoncouche.UI {
    
    [RequireComponent(typeof(RectTransform))]
    public class PauseWidget : MonoBehaviour {

        [SerializeField]
        private float _slideAnimDuration = 0.5f;

        // COMPONENTS

        private RectTransform _rectTransform;

        private Button _resumeButton;
        private Button _menuButton;
        private Button _quitButton;

        private RectTransform[] _scrolls = new RectTransform[2];

        private Text _quoteText;
        private Text _authorText;

        // METHODS

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();

            _resumeButton = transform.Find("Curtain/Button Resume").GetComponent<Button>();
            _menuButton = transform.Find("Curtain/Button Menu").GetComponent<Button>();
            _quitButton = transform.Find("Curtain/Button Quit").GetComponent<Button>();

            _scrolls[(int)LevelManager.Player.cthulu] = transform.Find("BibleQuote/CthulhuScroll").GetComponent<RectTransform>();
            _scrolls[(int)LevelManager.Player.sobek] = transform.Find("BibleQuote/SobekScroll").GetComponent<RectTransform>();
        
            _quoteText = transform.Find("BibleQuote/Quote").GetComponent<Text>();
            _authorText = transform.Find("BibleQuote/Author").GetComponent<Text>();

            // Event listeners
            _resumeButton.onClick.AddListener(OnResume);
            _menuButton.onClick.AddListener(OnReturnMenu);
            _quitButton.onClick.AddListener(OnQuit);

            // Initialization
            _rectTransform.anchoredPosition = new Vector2(0, 999999999);
        }

        private void OnEnable() {
            GenerateRandomQuote();

            _rectTransform.anchoredPosition = Vector2.zero;
            _resumeButton.Select();
        }

        private void OnDisable() {
            _rectTransform.anchoredPosition = new Vector2(0, 999999999);
        }

        private void OnResume() {
            if (!GameManager.levelManager.matchEnded && GameManager.Instance.isPaused) {
				GameManager.Instance.UnPause();
			}
        }

        private void OnReturnMenu() {
			if (!GameManager.levelManager.matchEnded && GameManager.Instance.isPaused) {
				GameManager.Instance.UnPause();
				GameManager.Instance.SwitchScene (GameManager.Scene.Menu);
			}
        }

        private void OnQuit() {
			if (!GameManager.levelManager.matchEnded && GameManager.Instance.isPaused) {
				GameManager.Instance.QuitGame();
			}
        }

        private void GenerateRandomQuote() {
            LevelManager.Player bible;
            BibleQuote bibleQuote = BibleEntries.GetRandomQuote(out bible);

            if (bibleQuote != null) {
                _quoteText.text = bibleQuote.quoteString;
                _authorText.text = string.Format("{0} {1}:{2}", bibleQuote.godName, bibleQuote.quoteFirstNo, bibleQuote.quoteSecondNo);
            } else {
                _quoteText.text = "Encore aucun texte sacré n'a été entré dans la bible!";
                _authorText.text = string.Empty;
            }
            
            _scrolls[(int)LevelManager.Player.cthulu].gameObject.SetActive(bible == LevelManager.Player.cthulu);
            _scrolls[(int)LevelManager.Player.sobek].gameObject.SetActive(bible == LevelManager.Player.sobek);
        }
        
    }
}