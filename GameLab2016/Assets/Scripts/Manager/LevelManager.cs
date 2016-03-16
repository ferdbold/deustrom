using UnityEngine;
using System.Collections;
using Simoncouche.Controller;

public class LevelManager : MonoBehaviour {

    #region Players
    /// <summary>  Sobek (player one) </summary>
    public GameObject sobekPlayer { get; private set; }

    /// <summary> Cthulu (player two)</summary>
    public GameObject cthuluPlayer { get; private set; }
    #endregion

    #region Score
    /// <summary> Sobek (player one)</summary>
    public int sobekScore { get; private set; }

    /// <summary> Cthulu (player two)</summary>
    public int cthuluScore { get; private set; }

    /// <summary> Score need to win </summary>
    public int scoreNeededToWin { get; private set; }

    /// <summary> Number of match to win a game </summary>
    public int matchToWin { get; private set; }

    /// <summary> Sobek number of match won </summary>
    public int sobekMatchWon { get; private set; }

    /// <summary> Cthulu number of match won </summary>
    public int cthuluMatchWon { get; private set; }

    private UIManager ui;
    #endregion

    #region Setup

    public LevelManager(int scoreToWin, int numberOfMatchToWin) {
        scoreNeededToWin = scoreToWin;
        matchToWin = numberOfMatchToWin;
        OnLevelWasLoaded(0);
    }

    private void OnLevelWasLoaded(int index) {
        SetupPlayers();
        ui = GameObject.FindObjectOfType<UIManager>();
        sobekScore = 0;
        cthuluScore = 0;
    }

    /// <summary> Setup ref to players </summary>
    private void SetupPlayers() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 2) Debug.LogWarning("Their is more than 2 player in the scene");

        foreach (GameObject player in players) {
            if (player.GetComponent<PlayerController>().IsPlayerOne) {
                sobekPlayer = player;
            } else {
                cthuluPlayer = player;
            }
        }
    }

    #endregion

    #region Score Related

    public enum Player { sobek, cthulu }

    /// <summary>
    /// Add score to current game
    /// </summary>
    /// <param name="player">The player to add point to</param>
    /// <param name="scoreAdded">The score added to the player score</param>
    /// <param name="originPos">The position of the object that produced the points </param>
    public void AddScore(Player player, int scoreAdded, Vector3 originPos) {
        if (player == Player.sobek) {
            sobekScore += scoreAdded;
            if (sobekScore >= scoreNeededToWin) {
                sobekScore = scoreNeededToWin;
                OnMatchEnd(Player.sobek);
            }
        } else {
            cthuluScore += scoreAdded;
            if (cthuluScore >= scoreNeededToWin) {
                cthuluScore = scoreNeededToWin;
                OnMatchEnd(Player.cthulu);
            }
        }
        for (int i = 0; i < scoreAdded; i++) {
            ui.AddPoint(player == Player.sobek ? 0 : 1, originPos);
        }
    }

    /// <summary>
    /// Event called when the match ends
    /// </summary>
    /// <param name="winner"></param>
    private void OnMatchEnd(Player winner) {
        if (winner == Player.sobek) {
            ++sobekMatchWon;
            if (sobekMatchWon >= matchToWin) {
                OnGameEnd(Player.sobek);
                return;
            }
        } else {
            ++cthuluMatchWon;
            if (cthuluMatchWon >= matchToWin) {
                OnGameEnd(Player.cthulu);
                return;
            }
        }
        GameManager.Instance.SwitchScene(GameManager.Scene.PlayLevel, dontClose: true);
    }

    /// <summary>
    /// Event called when the Game ends
    /// </summary>
    /// <param name="winner"></param>
    private void OnGameEnd(Player winner) {
        GameManager.Instance.SwitchScene(
            GameManager.Scene.BibleWriting,
            winner == Player.sobek ? CutsceneManager.Cutscene.Sobek_Win : CutsceneManager.Cutscene.Cthulu_Win
        );
    }

    #endregion
}
