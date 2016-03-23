﻿using UnityEngine;
using UnityEngine.UI;

namespace Simoncouche.UI {

    /// <summary>
    /// a MenuUI is the main controller class for the main menu UI.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class MenuUI : MonoBehaviour {

        [SerializeField]
        private Button firstActiveButton;

        // COMPONENTS

        public CanvasScaler scaler { get; private set; }

        private BibleReaderWidget _bibleReaderWidget;

        private void Awake() {
            this.scaler = GetComponent<CanvasScaler>();

            _bibleReaderWidget = transform.Find("BibleReader").GetComponent<BibleReaderWidget>();
            _bibleReaderWidget.enabled = false;

            firstActiveButton.Select();
        }

    	public void PlayButton() {
            GameManager.Instance.SwitchScene(GameManager.Scene.PlayLevel, CutsceneManager.Cutscene.Intro);
        }

        public void BiblesButton() {
            _bibleReaderWidget.enabled = !_bibleReaderWidget.enabled;
        }

        public void QuitGame() {
            GameManager.Instance.QuitGame();
        }
    }
}