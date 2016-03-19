﻿using UnityEngine;
using System.Collections;
using Simoncouche.Controller;

public class LevelManager {

    #region Players
    /// <summary>  Sobek (player one) </summary>
    public static GameObject sobekPlayer { get; private set; }

    /// <summary> Cthulu (player two)</summary>
    public static GameObject cthulhuPlayer { get; private set; }
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
        Setup();
    }

    public void Setup() {
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
            if (player.name=="Sobek") {
                sobekPlayer = player;
            } else {
                cthulhuPlayer = player;
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
        if (!GameManager.Instance.disableScoring) {
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
                if (ui != null) ui.AddPoint(player, originPos);
            }
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
                new Simoncouche.Utils.WaitWithCallback(1f, OnMatchEndSobek);
                return;
            }
        } else {
            ++cthuluMatchWon;
            if (cthuluMatchWon >= matchToWin) {
                new Simoncouche.Utils.WaitWithCallback(1f, OnMatchEndCthulu);
                return;
            }
        }

        sobekScore = -10000;
        cthuluScore = -10000;
        GameManager.Instance.SwitchScene(GameManager.Scene.PlayLevel, winner == Player.sobek ? CutsceneManager.Cutscene.Sobek_WinMatch : CutsceneManager.Cutscene.Cthulu_WinMatch, dontClose: true);
    }

    #region Used for callback
    private void OnMatchEndCthulu() {
        OnGameEnd(Player.cthulu);
    }

    private void OnMatchEndSobek() {
        OnGameEnd(Player.sobek);
    }
    #endregion

    /// <summary>
    /// Event called when the Game ends
    /// </summary>
    /// <param name="winner"></param>
    private void OnGameEnd(Player winner) {
        GameManager.Instance.lastWinner = winner;
        GameManager.Instance.SwitchScene(
            GameManager.Scene.BibleWriter,
            winner == Player.sobek ? CutsceneManager.Cutscene.Sobek_WinGame : CutsceneManager.Cutscene.Cthulu_WinGame
        );
    }

    #endregion
}
