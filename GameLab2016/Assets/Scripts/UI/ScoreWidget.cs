using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Simoncouche.UI {
    /// <summary>A ScoreWidget tracks the points accumulated by a single player under the form of a parchment slider.</summary>
    public class ScoreWidget : MonoBehaviour {

        [SerializeField]
        private float _gainAnimDuration = 0.5f;

        // COMPONENTS

        public Transform leadParticles { get; set; }

        private Slider _directSlider;
        private Slider _lerpingSlider;
        private Image _directRoll;
        private Image _lerpingRoll;

        // METHODS

        private void Awake() {
            _directSlider = transform.Find("DirectSlider").GetComponent<Slider>();
            _lerpingSlider = transform.Find("LerpingSlider").GetComponent<Slider>();
            _directRoll = transform.Find("DirectRoll").GetComponent<Image>();
            _lerpingRoll = transform.Find("LerpingRoll").GetComponent<Image>();
        }

        private void Start() {
            _directSlider.maxValue = GameManager.Instance.pointsGoal;
            _lerpingSlider.maxValue = GameManager.Instance.pointsGoal;

            _directSlider.value = 0;
            _lerpingSlider.value = 0;
        }

        private void Update() {
            if (this.leadParticles != null) {
                Vector3 uiWorldPos =  GameManager.uiManager.particleCamera.ScreenToWorldPoint(this.GetLerpingFillEndPosition());
                this.leadParticles.position = new Vector3(
                    uiWorldPos.x,
                    uiWorldPos.y + 0.6f,
                    1
                );
            }
        }

        /// <summary>Add points to the widget.</summary>
        /// <param name="amount">The amount of points to add</param>
        public void AddPoints(int amount) {
            Vector3 rollPosition = _directRoll.rectTransform.position;

            // Activate rolls the first time points are added
            _directRoll.enabled = true;
            _lerpingRoll.enabled = true;

            // Update direct roll position
            _directSlider.value += amount;
            rollPosition.x = this.GetFillEndPosition().x;
            _directRoll.rectTransform.position = rollPosition;

            // Queue up lerping animation
            _lerpingSlider.DOValue(_directSlider.value, _gainAnimDuration)
                .SetEase(Ease.OutCubic)
                .OnUpdate(this.UpdateRollTopPosition);
        }

        /// <summary>Updates the lerping roll position on every update step of the lerping slider animation</summary>
        private void UpdateRollTopPosition() {
            Vector3 rollPosition = _lerpingRoll.rectTransform.position;
            rollPosition.x = _lerpingSlider.fillRect.position.x + (_lerpingSlider.fillRect.rect.width / 2 * GameManager.uiManager.root.scaleFactor * ((transform.localRotation.eulerAngles.z == 0) ? 1 : -1));
            _lerpingRoll.rectTransform.position = rollPosition;
        }

        /// <summary>
        /// Returns the screen position of the direct roll fill rect end
        /// </summary>
        /// <returns>The fill end position.</returns>
        public Vector3 GetFillEndPosition() {
            return new Vector3(
                _directSlider.fillRect.position.x + (_directSlider.fillRect.rect.width / 2 * GameManager.uiManager.root.scaleFactor * ((transform.localRotation.eulerAngles.z == 0) ? 1 : -1)), 
                _directSlider.fillRect.position.y - (_directSlider.fillRect.rect.height / 2 * GameManager.uiManager.root.scaleFactor),
                0
            );
        }

        /// <summary>
        /// Returns the screen position of the lerping roll fill rect end
        /// </summary>
        public Vector3 GetLerpingFillEndPosition() {
            return new Vector3(
                _lerpingSlider.fillRect.position.x + (_lerpingSlider.fillRect.rect.width / 2 * GameManager.uiManager.root.scaleFactor * ((transform.localRotation.eulerAngles.z == 0) ? 1 : -1)), 
                _lerpingSlider.fillRect.position.y - (_lerpingSlider.fillRect.rect.height / 2 * GameManager.uiManager.root.scaleFactor),
                0
            );
        }
    }
}
