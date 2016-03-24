using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

using Simoncouche.Bible;

namespace Simoncouche.UI {

    /// <summary>
    /// A BibleReaderWidget can display the contents of both the bibles of the gods to the user.
    /// </summary>
    public class BibleReaderWidget : MonoBehaviour {

        [SerializeField]
        private float _slideAnimDuration = 0.5f;

        [SerializeField]
        private float _swapAnimDuration = 0.1f;

        [SerializeField]
        private float _scrollSpeed = 15f;

        [SerializeField]
        private LevelManager.Player _currentPlayer = LevelManager.Player.cthulu;

        private bool _swapped = false;

        // COMPONENTS

        private RectTransform _rectTransform;
        private RectTransform _swapIcon;

        private List<Image> _pictos;
        private RectTransform _scrollsContainer;
        private List<RectTransform> _scrolls;
        private List<RectTransform> _scrollContents;

        // METHODS

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();
            _swapIcon = transform.Find("SwapIcon").GetComponent<RectTransform>();

            _pictos = new List<Image>();
            _pictos.Add(transform.Find("Pictos/Sobek").GetComponent<Image>());
            _pictos.Add(transform.Find("Pictos/Cthulhu").GetComponent<Image>());
                
            _scrollsContainer = transform.Find("Scrolls").GetComponent<RectTransform>();

            _scrolls = new List<RectTransform>();
            _scrolls.Add(transform.Find("Scrolls/Sobek").GetComponent<RectTransform>());
            _scrolls.Add(transform.Find("Scrolls/Cthulhu").GetComponent<RectTransform>());

            _scrollContents = new List<RectTransform>();
            _scrollContents.Add(transform.Find("Scrolls/Sobek/Scroll View/Viewport/Content").GetComponent<RectTransform>());
            _scrollContents.Add(transform.Find("Scrolls/Cthulhu/Scroll View/Viewport/Content").GetComponent<RectTransform>());

            _rectTransform.anchoredPosition = new Vector2(0, -600);
        }

        private void Start() {
            LoadBible();

            // Input
            GameManager.inputManager.AddEvent(InputManager.Axis.p1_rightAnalog, ScrollReader);
            GameManager.inputManager.AddEvent(InputManager.Axis.p1_leftTrigger, OnLeftTrigger);
        }

        private void OnEnable() {
            _rectTransform.DOMoveY(0, _slideAnimDuration).SetEase(Ease.OutCubic);
        }

        private void OnDisable() {
            _rectTransform.DOMoveY(-Screen.height, _slideAnimDuration).SetEase(Ease.InCubic);
        }

        private void OnLeftTrigger(float[] axis) {
            if (axis[0] > 0.5f && !_swapped) {
                SwapPlayer();
                _swapped = true;
            } else if (axis[0] == 0) {
                _swapped = false;
            }
        }

        /// <summary>
        /// Load the bible from the bible manager and build the scroll views
        /// </summary>
        private void LoadBible() {
            BibleEntries entries = BibleEntries.LoadBibleEntries();

            // Reverse entry order for twitter-like behaviour
            entries.quoteListCthulu.Reverse();
            entries.quoteListSobek.Reverse();

            if (entries != null) {
                foreach (BibleQuote entry in entries.quoteListCthulu) {
                    BibleQuoteWidget widget = BibleQuoteWidget.Create(
                        LevelManager.Player.cthulu, 
                        entry.quoteString, 
                        entry.godName, 
                        entry.quoteFirstNo,
                        entry.quoteSecondNo
                    );

                    widget.transform.SetParent(_scrollContents[(int)LevelManager.Player.cthulu]);
                    widget.transform.localScale = Vector3.one;
                }

                foreach (BibleQuote entry in entries.quoteListSobek) {
                    BibleQuoteWidget widget = BibleQuoteWidget.Create(
                        LevelManager.Player.sobek, 
                        entry.quoteString, 
                        entry.godName, 
                        entry.quoteFirstNo,
                        entry.quoteSecondNo
                    );

                    Debug.Log(_scrollContents.Count);
                    widget.transform.SetParent(_scrollContents[(int)LevelManager.Player.sobek]);
                    widget.transform.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Swaps the active god in the widget
        /// </summary>
        private void SwapPlayer() {
            LevelManager.Player otherPlayer = _currentPlayer;
            _currentPlayer = (_currentPlayer == LevelManager.Player.cthulu) ? 
                LevelManager.Player.sobek : 
                LevelManager.Player.cthulu;

            // Animation sequence
            RectTransform currentPlayerPictoRT = _pictos[(int)_currentPlayer].GetComponent<RectTransform>();
            RectTransform otherPlayerPictoRT = _pictos[(int)otherPlayer].GetComponent<RectTransform>();

            _swapIcon.DOShakeScale(0.3f, 0.5f);

            currentPlayerPictoRT.DOScale(Vector3.one, _swapAnimDuration);
            currentPlayerPictoRT.DOLocalMoveX(0, _swapAnimDuration);
            currentPlayerPictoRT.SetAsLastSibling();

            otherPlayerPictoRT.DOScale(Vector3.one/2, _swapAnimDuration);
            otherPlayerPictoRT.DOLocalMoveX(-70, _swapAnimDuration);

            _scrollsContainer.DOMoveY(-(50 + _rectTransform.rect.height/720 * Screen.height/2), _swapAnimDuration).SetEase(Ease.OutCubic).OnComplete(() => {
                _scrolls[(int)_currentPlayer].gameObject.SetActive(true);
                _scrolls[(int)otherPlayer].gameObject.SetActive(false);

                _scrollsContainer.DOLocalMoveY(230, _swapAnimDuration).SetEase(Ease.OutCubic);
            });
        }

        private void ScrollReader(float[] axii) {
            if (axii[1] != 0f) {
                RectTransform content = _scrollContents[(int)_currentPlayer];
                RectTransform parent = content.transform.parent.GetComponent<RectTransform>();

                content.localPosition = new Vector2(
                    content.localPosition.x,
                    Mathf.Clamp(content.localPosition.y - axii[1] * _scrollSpeed,
                        0, Mathf.Max(0, content.rect.height - parent.rect.height))
                );
            }
        }
    }
}
