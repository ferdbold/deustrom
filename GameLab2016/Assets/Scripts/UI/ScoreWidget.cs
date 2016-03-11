using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Simoncouche.UI {
    /// <summary>A ScoreWidget tracks the points accumulated by a single player under the form of a parchment slider.</summary>
    public class ScoreWidget : MonoBehaviour {

        [SerializeField]
        private float _gainAnimDuration = 0.5f;

        // COMPONENTS

        private Slider _directSlider;
        private Slider _lerpingSlider;

        // METHODS

        private void Awake () {
            _directSlider = transform.Find("DirectSlider").GetComponent<Slider>();
            _lerpingSlider = transform.Find("LerpingSlider").GetComponent<Slider>();
        }

        private void Start() {
            _directSlider.maxValue = GameManager.Instance.pointsGoal;
            _lerpingSlider.maxValue = GameManager.Instance.pointsGoal;
        }

        public void AddPoints(int amount) {
            _directSlider.value += amount;
            _lerpingSlider.DOValue(_directSlider.value, _gainAnimDuration).SetEase(Ease.OutCubic);
        }

        public Vector3 GetFillEndPosition() {
            return new Vector3(
                _directSlider.fillRect.position.x, 
                _directSlider.fillRect.position.y + (_directSlider.fillRect.rect.height / 2 * GameManager.uiManager.root.scaleFactor),
                0
            );
        }
    }
}
