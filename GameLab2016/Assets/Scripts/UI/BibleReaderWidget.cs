using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

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
        private LevelManager.Player _currentPlayer = LevelManager.Player.cthulu;

        // COMPONENTS

        private RectTransform _rectTransform;

        private List<Image> _pictos;
        private RectTransform _scrollsContainer;
        private List<RectTransform> _scrolls;

        // METHODS

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();

            _pictos = new List<Image>();
            _pictos.Add(transform.Find("Pictos/Sobek").GetComponent<Image>());
            _pictos.Add(transform.Find("Pictos/Cthulhu").GetComponent<Image>());
                
            _scrollsContainer = transform.Find("Scrolls").GetComponent<RectTransform>();

            _scrolls = new List<RectTransform>();
            _scrolls.Add(transform.Find("Scrolls/Sobek").GetComponent<RectTransform>());
            _scrolls.Add(transform.Find("Scrolls/Cthulhu").GetComponent<RectTransform>());

            _rectTransform.anchoredPosition = new Vector2(0, -600);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.I)) {
                Debug.Log("swap");
                SwapPlayer();
            }
        }

        private void OnEnable() {
            /*if (_currentPlayer == LevelManager.Player.cthulu) {
                _animator.SetTrigger("Cthulhu");
            } else {
                _animator.SetTrigger("Sobek");
            }*/
            _rectTransform.DOMoveY(0, _slideAnimDuration).SetEase(Ease.OutCubic);
        }

        private void OnDisable() {
            // _animator.SetTrigger("Close");
            _rectTransform.DOMoveY(-_rectTransform.rect.height, _slideAnimDuration).SetEase(Ease.InCubic);
        }

        public void SwapPlayer() {
            LevelManager.Player otherPlayer = _currentPlayer;
            _currentPlayer = (_currentPlayer == LevelManager.Player.cthulu) ? 
                LevelManager.Player.sobek : 
                LevelManager.Player.cthulu;

            /*_animator.SetTrigger("Swap");
            if (_currentPlayer == LevelManager.Player.sobek) {
                _animator.SetTrigger("Sobek");
            } else {
                _animator.SetTrigger("Cthulhu");
            }*/

            // Animation sequence
            RectTransform currentPlayerPictoRT = _pictos[(int)_currentPlayer].GetComponent<RectTransform>();
            RectTransform otherPlayerPictoRT = _pictos[(int)otherPlayer].GetComponent<RectTransform>();

            currentPlayerPictoRT.DOScale(Vector3.one, _swapAnimDuration);
            currentPlayerPictoRT.DOLocalMoveX(0, _swapAnimDuration);
            currentPlayerPictoRT.SetAsLastSibling();

            otherPlayerPictoRT.DOScale(Vector3.one/2, _swapAnimDuration);
            otherPlayerPictoRT.DOLocalMoveX(-70, _swapAnimDuration);

            _scrollsContainer.DOMoveY(-_rectTransform.rect.height/2, _swapAnimDuration).SetEase(Ease.OutCubic).OnComplete(() => {
                _scrolls[(int)_currentPlayer].gameObject.SetActive(true);
                _scrolls[(int)otherPlayer].gameObject.SetActive(false);

                _scrollsContainer.DOLocalMoveY(-40, _swapAnimDuration).SetEase(Ease.OutCubic);
            });
        }
    }
}
