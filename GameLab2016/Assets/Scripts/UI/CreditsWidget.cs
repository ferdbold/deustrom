using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace Simoncouche.UI {
    public class CreditsWidget : MonoBehaviour {

        [SerializeField]
        private float _slideAnimDuration = 0.5f;

        [SerializeField]
        private float _scrollSpeed = 15f;

        // COMPONENTS

        private RectTransform _rectTransform;
        private RectTransform _content;

        // METHODS

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();
            _content = transform.Find("Scroll/Scroll View/Viewport/Content").GetComponent<RectTransform>();
        
            _rectTransform.anchoredPosition = new Vector2(0, -Screen.height);
        }

        private void Start() {
            // Input
            GameManager.inputManager.AddEvent(InputManager.Axis.p1_rightAnalog, ScrollCredits);
        }

        private void OnEnable() {
            _rectTransform.DOMoveY(0, _slideAnimDuration).SetEase(Ease.OutCubic);
        }

        private void OnDisable() {
            _rectTransform.DOMoveY(-Screen.height, _slideAnimDuration).SetEase(Ease.InCubic);
        }

        private void ScrollCredits(float[] axii) {
            if (axii[1] != 0f) {
                RectTransform parent = _content.transform.parent.GetComponent<RectTransform>();

                _content.localPosition = new Vector2(
                    _content.localPosition.x,
                    Mathf.Clamp(_content.localPosition.y - axii[1] * _scrollSpeed,
                        0, Mathf.Max(0, _content.rect.height - parent.rect.height))
                );
            }
        }
    }
}
