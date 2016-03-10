using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Simoncouche.UI {
    /// <summary>A ScoreWidget tracks the points accumulated by a single player under the form of a parchment slider.</summary>
    [RequireComponent(typeof(Slider))]
    public class ScoreWidget : MonoBehaviour {

        [SerializeField]
        private float GAIN_ANIM_DURATION = 0.5f;

        // COMPONENTS

        private Slider _slider;

        // METHODS

        private void Awake () {
            _slider = GetComponent<Slider>();
        }

        public void AddPoints(int amount) {
            _slider.DOValue(_slider.value + amount, this.GAIN_ANIM_DURATION).SetEase(Ease.OutCubic); 
        }
    }
}
