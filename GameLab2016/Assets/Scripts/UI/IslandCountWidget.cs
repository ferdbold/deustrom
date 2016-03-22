using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class IslandCountWidget : MonoBehaviour {

    [Tooltip("Minimum amount of islands for the big value anim to kick in")]
    [SerializeField]
    private int _bigValueAnimThreshold = 6;

    [SerializeField]
    [Range(0f, 1f)]
    private float _bigValueAnimAmplitude = 0.2f;

    [SerializeField]
    private float _bigValueAnimSpeed = 3f;

    [SerializeField]
    private float _gainAnimDuration = 0.5f;

    [Tooltip("Color of the animation fired when an island is gained")]
    [SerializeField]
    private Color _gainAnimColor;

    private int _value = 0;
    private bool _bigValueAnimRunning = false;

    // COMPONENTS

    private Text _text;

    private void Awake() {
        _text = transform.Find("IslandCount").GetComponent<Text>();
    }

    private void Update() {
        if (_bigValueAnimRunning) {
            float scaleValue = 1.1f + _bigValueAnimAmplitude * Mathf.Sin(Time.time * _bigValueAnimSpeed);
            _text.rectTransform.localScale = new Vector3(scaleValue, scaleValue, 1);
            _text.color = _gainAnimColor;
        }
    }

    private void FireGainAnim() {
        _text.color = _gainAnimColor;
        _text.DOColor(Color.white, _gainAnimDuration);

        if (!_bigValueAnimRunning) {
            _text.rectTransform.localScale = new Vector3(2f, 2f, 1);
            _text.transform.DOScale(Vector3.one, _gainAnimDuration).SetEase(Ease.OutCubic);
        }
    }

    public int value {
        set {
            int oldValue = _value;
            bool oldBigValueAnimRunning = _bigValueAnimRunning;

            _value = value;
            _text.text = value.ToString();

            if (oldValue < value) {
                FireGainAnim();
            }

            _bigValueAnimRunning = (value >= _bigValueAnimThreshold);

            // Reset scale in case big value anim was just turned off
            if (!_bigValueAnimRunning && oldBigValueAnimRunning) {
                _text.rectTransform.localScale = Vector3.one;
                _text.color = Color.white;
            }
        }
    }
}
