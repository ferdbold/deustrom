using UnityEngine;
using UnityEngine.UI;
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

    private List<Image> sobekRunesPool = new List<Image>();
    private List<Image> cthulhuRunesPool = new List<Image>();

    // COMPONENTS

    public Canvas root { get; private set; }
    private List<ScoreWidget> _scoreWidgets;
    private List<WinsWidget> _winsWidgets;

    // METHODS

    public void Setup() {
        this.root = GameObject.Find("UI").GetComponent<Canvas>();

        _scoreWidgets = new List<ScoreWidget>();
        _scoreWidgets.Add(GameObject.Find("UI/Scores/Sobek").GetComponent<ScoreWidget>());
        _scoreWidgets.Add(GameObject.Find("UI/Scores/Cthulhu").GetComponent<ScoreWidget>());
    
        _winsWidgets = new List<WinsWidget>();
        _winsWidgets.Add(GameObject.Find("UI/Wins/Sobek").GetComponent<WinsWidget>());
        _winsWidgets.Add(GameObject.Find("UI/Wins/Cthulhu").GetComponent<WinsWidget>());
    
        RefreshWins();

        //Pooling
        for (int i = 0; i < 15; i++) {
            InstantiateRune(LevelManager.Player.sobek);
            InstantiateRune(LevelManager.Player.cthulu);
        }
    }

    // FIXME: This is for test purposes and should be removed in the final build
    private void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
            this.AddPoint(LevelManager.Player.cthulu, new Vector3(10, 8));
            this.AddPoint(LevelManager.Player.cthulu, new Vector3(12, 6));
            this.AddPoint(LevelManager.Player.cthulu, new Vector3(14, 1));
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            this.AddPoint(LevelManager.Player.sobek, new Vector3(7, 5));
            this.AddPoint(LevelManager.Player.sobek, new Vector3(5, 10));
            this.AddPoint(LevelManager.Player.sobek, new Vector3(2, 7));
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

    private void RefreshWins() {
        //Debug.Log(GameManager.levelManager);

        _winsWidgets[(int)LevelManager.Player.cthulu].score = GameManager.levelManager.cthuluMatchWon;
        _winsWidgets[(int)LevelManager.Player.sobek].score = GameManager.levelManager.sobekMatchWon;
    }

    private void OnOrbAnimComplete(LevelManager.Player player, Image orb) {
        ReturnRune(orb, player);
        _scoreWidgets[(int)player].AddPoints(1);
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
            instantiatedObj.gameObject.SetActive(false);
            sobekRunesPool.Add(instantiatedObj);
        } else {
            instantiatedObj = (Image)GameObject.Instantiate(_cthulhuRunePrefab);
            instantiatedObj.gameObject.SetActive(false);
            cthulhuRunesPool.Add(instantiatedObj);
        }
    }

    #endregion
}
