using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using Simoncouche.UI;
using DG.Tweening;

/// <summary>The UIManager oversees the entire UI layer of the game.</summary>
public class UIManager : MonoBehaviour {

    [Header("Animation")]
    [SerializeField]
    [Tooltip("How long it takes for the orb to reach the points bar")]
    private float _orbAnimDuration = 1.0f;

    [SerializeField]
    [Tooltip("How much time variation there can be on the orb anim duration")]
    private float _orbAnimDurationVariation = 0.25f;

    [Header("Prefabs")]
    [SerializeField]
    private Image _sobekRunePrefab;

    [SerializeField]
    private Image _cthulhuRunePrefab;

    [SerializeField]
    private Sprite[] _roundSeals;
    [SerializeField]
    private Sprite _sealSobek;
    [SerializeField]
    private Sprite _sealCthulhu;

    private List<Image> sobekRunesPool = new List<Image>();
    private List<Image> cthulhuRunesPool = new List<Image>();

    // EVENTS

    public UnityEvent RoundEndAnimationCompleted { get; private set; }

    // COMPONENTS

    public Canvas root { get; private set; }
    public Camera particleCamera { get; private set; }

    private List<ScoreWidget> _scoreWidgets;
    private List<WinsWidget> _winsWidgets;
    private List<IslandCountWidget> _islandCountWidgets;
    public TutorialUI _tutoWidget { get; private set; }
    private PauseWidget _pauseWidget;

    private Image _seal;
    private Image _winSeal;
    private Text _promptText;
    private Text _miniPromptText;
    private CanvasGroup _round1Controls;
    private CanvasGroup _round2Controls;

    // METHODS

    public static TutorialUI SetupTutoWidget() {
        return GameObject.Find("UI/Tutorial").GetComponent<TutorialUI>();
    }

    public void Setup() {
        this.root = GameObject.Find("UI").GetComponent<Canvas>();
        this.particleCamera = GameObject.Find("UIParticles").GetComponent<Camera>();

        _scoreWidgets = new List<ScoreWidget>();
        _scoreWidgets.Add(GameObject.Find("UI/Scores/Sobek").GetComponent<ScoreWidget>());
        _scoreWidgets.Add(GameObject.Find("UI/Scores/Cthulhu").GetComponent<ScoreWidget>());
    
        _scoreWidgets[(int)LevelManager.Player.sobek].leadParticles = GameObject.Find("UIParticles/Sobek").transform;
        _scoreWidgets[(int)LevelManager.Player.cthulu].leadParticles = GameObject.Find("UIParticles/Cthulhu").transform;

        //Must play on awake these particle systems (cannot be set in the inspector)
        foreach (ParticleSystem ps in _scoreWidgets[(int)LevelManager.Player.sobek]
            .leadParticles.GetComponentsInChildren<ParticleSystem>()) {
            ps.playOnAwake = true;
        }
        foreach (ParticleSystem ps in _scoreWidgets[(int)LevelManager.Player.cthulu]
            .leadParticles.GetComponentsInChildren<ParticleSystem>()) {
            ps.playOnAwake = true;
        }

        _winsWidgets = new List<WinsWidget>();
        _winsWidgets.Add(GameObject.Find("UI/Wins/Sobek").GetComponent<WinsWidget>());
        _winsWidgets.Add(GameObject.Find("UI/Wins/Cthulhu").GetComponent<WinsWidget>());

        _islandCountWidgets = new List<IslandCountWidget>();
        _islandCountWidgets.Add(GameObject.Find("UI/Islands/Sobek").GetComponent<IslandCountWidget>());
        _islandCountWidgets.Add(GameObject.Find("UI/Islands/Cthulhu").GetComponent<IslandCountWidget>());

        _tutoWidget = UIManager.SetupTutoWidget();
        _pauseWidget = GameObject.Find("UI/Pause").GetComponent<PauseWidget>();

        _seal = GameObject.Find("UI/Seal").GetComponent<Image>();
        _winSeal = GameObject.Find("UI/WinSeal").GetComponent<Image>();
        _promptText = GameObject.Find("UI/PromptText").GetComponent<Text>();
        _miniPromptText = GameObject.Find("UI/MiniPromptText").GetComponent<Text>();

        _round1Controls = GameObject.Find("UI/ControlsHelp/Round 1").GetComponent<CanvasGroup>();
        _round2Controls = GameObject.Find("UI/ControlsHelp/Round 2").GetComponent<CanvasGroup>();

        this.RoundEndAnimationCompleted = new UnityEvent();

        RefreshWins();

        // Pooling
        sobekRunesPool.Clear();
        cthulhuRunesPool.Clear();
        for (int i = 0; i < 15; i++) {
            InstantiateRune(LevelManager.Player.sobek);
            InstantiateRune(LevelManager.Player.cthulu);
        }

        // Set seal
        _seal.sprite = _roundSeals[GameManager.levelManager.currentRound];

        // Event listeners

        GameManager.levelManager.MatchEnd.AddListener(OnMatchEnd);

        GameManager.Instance.Paused.AddListener(OnPause);
        GameManager.Instance.Unpaused.AddListener(OnUnpause);
    }

    private void Start() {
        if (GameManager.Instance.currentScene == GameManager.Scene.PlayLevel) {
            _promptText.GetComponent<RectTransform>().localScale = Vector3.zero;

            _winSeal.color = new Color(_winSeal.color.r, _winSeal.color.g, _winSeal.color.b, 0f);
            _winSeal.GetComponent<RectTransform>().localScale = Vector3.one * 5;
        }
    }

    private void Update() {
        if (GameManager.Instance.currentScene == GameManager.Scene.PlayLevel && GameManager.Instance.gameStarted) {
            UpdateLeadParticles();

            foreach (LevelManager.Player player in System.Enum.GetValues(typeof(LevelManager.Player))) {
                _islandCountWidgets[(int)player].value = GameManager.islandManager.GetPlayerIslandCount(player);
            }
        }
    }

    /// <summary>
    /// Displays visual feedback about a player gaining a point.
    /// </summary>
    /// <param name="player">The player gaining a point (either 0 or 1).</param>
    /// <param name="sourcePos">The position of the object that generated a point for the player.</param>
    public void AddPoint(LevelManager.Player player, Vector3 sourcePos) {
        if (root != null) {
            Image newScoreOrb = GetRune(player);
            
            newScoreOrb.transform.SetParent(this.root.transform);
            newScoreOrb.rectTransform.position = Camera.main.WorldToScreenPoint(sourcePos);

            // Tween orb to destination
            float animDuration = _orbAnimDuration + Random.Range(0, _orbAnimDurationVariation) - _orbAnimDurationVariation / 2;
            newScoreOrb.rectTransform.DOMove(_scoreWidgets[(int)player].GetFillEndPosition(), animDuration)
                .SetEase(Ease.InOutCubic)
                .OnComplete(() => this.OnOrbAnimComplete(player, newScoreOrb));
        }
    }

    /// <summary>
    /// Update the visibility of both players' lead particles
    /// </summary>
    private void UpdateLeadParticles() {
		if (_scoreWidgets[0] != null) {
			_scoreWidgets[(int)LevelManager.Player.cthulu].leadParticles.gameObject.SetActive (
				GameManager.islandManager.GetPlayerIslandCount(LevelManager.Player.cthulu) >
				GameManager.islandManager.GetPlayerIslandCount(LevelManager.Player.sobek)
			);

			_scoreWidgets[(int)LevelManager.Player.sobek].leadParticles.gameObject.SetActive (
				GameManager.islandManager.GetPlayerIslandCount(LevelManager.Player.sobek) >
				GameManager.islandManager.GetPlayerIslandCount(LevelManager.Player.cthulu)
			);
		}
    }

    private void RefreshWins() {
        _winsWidgets[(int)LevelManager.Player.cthulu].score = GameManager.levelManager.cthuluMatchWon;
        _winsWidgets[(int)LevelManager.Player.sobek].score = GameManager.levelManager.sobekMatchWon;
    }

    private void OnPause() {
        if (_pauseWidget != null && _tutoWidget != null) {
			if (_tutoWidget.isCompleted) {
				_pauseWidget.enabled = true;
			}
		}
    }

	private void OnUnpause() {
		if (_pauseWidget != null) {
			_pauseWidget.enabled = false;
		}
    }

    public void OnTutoComplete() {
        if (GameManager.levelManager.currentRound == 0) {
            _round1Controls.alpha = 1;
            _round1Controls.DOFade(0f, 5f).SetDelay(7f);
        } else if (GameManager.levelManager.currentRound == 1) {
            _round2Controls.alpha = 1;
            _round2Controls.DOFade(0f, 5f).SetDelay(7f);
        }
    }

    private void OnOrbAnimComplete(LevelManager.Player player, Image orb) {
        ReturnRune(orb, player);
        _scoreWidgets[(int)player].AddPoints(1);
    }

    private void OnMatchEnd(LevelManager.Player winner) {
        string godName = (winner == LevelManager.Player.cthulu) ? "CTHULHU" : "SOBEK";

        _promptText.text = "VICTOIRE " + godName;
        _promptText.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 3);
        _winSeal.sprite = (winner == LevelManager.Player.cthulu) ? _sealCthulhu : _sealSobek;

        // Anim sequence
        _promptText.GetComponent<RectTransform>().DOShakeRotation(0.5f, 45);
        _promptText.GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce).OnComplete(() => {
            _winSeal.DOColor(Color.white, 0.2f);
            _winSeal.GetComponent<RectTransform>().DOScale(Vector3.one, 0.25f).SetEase(Ease.OutCirc);

            Camera.main.DOShakePosition(0.3f, 5, 30).SetDelay(0.25f);
            _winSeal.GetComponent<RectTransform>().DOShakePosition(0.3f, 15, 30).SetDelay(0.25f).OnComplete(() => {
                _miniPromptText.GetComponent<RectTransform>().DOShakeRotation(0.3f, 30).SetDelay(0.75f);
                _miniPromptText.GetComponent<RectTransform>().DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBounce).SetDelay(0.75f).OnComplete(() => {
                    this.RoundEndAnimationCompleted.Invoke();
                });
            });
        });
    }

    #region Pooling

    private Image GetRune(LevelManager.Player player) {
        Image returnImage = null;

        if (player == LevelManager.Player.sobek) {         
            if (sobekRunesPool.Count <= 0) InstantiateRune(player);
            returnImage = sobekRunesPool[0];
            sobekRunesPool.RemoveAt(0);
        } else {
            if (cthulhuRunesPool.Count <= 0) InstantiateRune(player);
            returnImage = cthulhuRunesPool[0];
            cthulhuRunesPool.RemoveAt(0);
        }

        returnImage.gameObject.SetActive(true);
        return returnImage;
    }

    private void ReturnRune(Image rune, LevelManager.Player player) {
        rune.gameObject.SetActive(false);
        if (player == LevelManager.Player.sobek) sobekRunesPool.Add(rune);
        else cthulhuRunesPool.Add(rune);
    }

    private void InstantiateRune(LevelManager.Player player) {
        Image instantiatedObj;

        if (player == LevelManager.Player.sobek) {
            instantiatedObj = (Image)GameObject.Instantiate(_sobekRunePrefab);
            instantiatedObj.transform.SetParent(this.root.transform);
            instantiatedObj.gameObject.SetActive(false);
            sobekRunesPool.Add(instantiatedObj);
        } else {
            instantiatedObj = (Image)GameObject.Instantiate(_cthulhuRunePrefab);
            instantiatedObj.transform.SetParent(this.root.transform);
            instantiatedObj.gameObject.SetActive(false);
            cthulhuRunesPool.Add(instantiatedObj);
        }
    }

    #endregion
}
