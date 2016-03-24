using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Simoncouche.Controller;
using Simoncouche.Islands;

public class PlayerEvent : UnityEvent<LevelManager.Player> {}

public class LevelManager {

    #region Players
    /// <summary>  Sobek (player one) </summary>
    public static GameObject sobekPlayer { get; private set; }

    /// <summary> Cthulu (player two)</summary>
    public static GameObject cthulhuPlayer { get; private set; }

    #endregion

    #region Score
    /// <summary>Sobek (player one)</summary>
    public int sobekScore { get; private set; }

    /// <summary>Cthulu (player two)</summary>
    public int cthuluScore { get; private set; }

    /// <summary>Number of match to win a game </summary>
    public int matchToWin { get; private set; }

    /// <summary>Sobek number of match won </summary>
    public int sobekMatchWon { get; private set; }

    /// <summary>Cthulu number of match won </summary>
    public int cthuluMatchWon { get; private set; }

	public bool matchEnded { get; private set; }
    #endregion

    #region Events

    /// <summary>
    /// Fired as soon as a winner is declared at match end
    /// </summary>
    public PlayerEvent MatchEnd { get; private set; }

    #endregion

    /// <summary>
    /// Is the level waiting for a player to input A after a round has concluded
    /// </summary>
    public bool _waitingForMatchEndInput;
    private LevelManager.Player _winner;

    #region Setup

    public LevelManager(int numberOfMatchToWin) {
        matchToWin = numberOfMatchToWin;
        Setup();
    }

    public void Setup() {
        this.MatchEnd = new PlayerEvent();

        SetupPlayers();
        sobekScore = 0;
        cthuluScore = 0;
        OnMatchStart();
        GameManager.Instance.disableScoring = false;
    }

    /// <summary> Setup ref to players </summary>
    private void SetupPlayers() {
        bool mustEnableChains = false;
        if (sobekMatchWon + cthuluMatchWon >= GameManager.Instance.amountOfRoundsUntilChainsEnabled) {
            mustEnableChains = true;
        } else {
            mustEnableChains = false;
        }
        GameManager.Instance.mustEnableChainThrower = mustEnableChains;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 2) Debug.LogWarning("Their is more than 2 player in the scene");

        foreach (GameObject player in players) {
            if (player.name=="Sobek") {
                sobekPlayer = player;
            } else {
                cthulhuPlayer = player;
            }
            player.GetComponent<Simoncouche.Chain.HookThrower>().SetupInput(player.GetComponent<PlayerController>().IsPlayerOne);
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
                if (sobekScore >= GameManager.Instance.pointsGoal) {
                    sobekScore = GameManager.Instance.pointsGoal;
                    OnMatchEnd(Player.sobek);
                }
            } else {
                cthuluScore += scoreAdded;
                if (cthuluScore >= GameManager.Instance.pointsGoal) {
                    cthuluScore = GameManager.Instance.pointsGoal;
                    OnMatchEnd(Player.cthulu);
                }
            }
            for (int i = 0; i < scoreAdded; i++) {
                if (GameManager.uiManager != null) GameManager.uiManager.AddPoint(player, originPos);
            }
        }
    }
    #endregion

    #region Events

    /// <summary>
    /// Event called when the match starts
    /// </summary>
    private void OnMatchStart() {
        switch (sobekMatchWon + cthuluMatchWon) {
            case 0: GameManager.Instance.StartVideoTutorial(TutorialUI.TutoChoice.one); break;
            case 1: GameManager.Instance.StartVideoTutorial(TutorialUI.TutoChoice.two); break;
        }

		this.matchEnded = false;
        //GameManager.Instance.StartCoroutine(WaitTimeUntilPlayerCanPlay(GameManager.Instance.timeUntilControllersAreEnabled));
    }

    IEnumerator WaitTimeUntilPlayerCanPlay(float time) {
        //GameManager.inputManager.enabled = false;
        //GameManager.Instance.Pause();
        yield return new WaitForRealSeconds(time);
        //GameManager.Instance.UnPause();
        //GameManager.inputManager.enabled = true;
    }

    /// <summary>
    /// Event called when the match ends
    /// </summary>
    /// <param name="winner"></param>
    private void OnMatchEnd(Player winner) {
        _winner = winner;
		this.matchEnded = true;

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

        _waitingForMatchEndInput = true;
        GameManager.Instance.disableScoring = true;
        // FIXME: I don't need to explain why this is bad.
        foreach (IslandFeeder feeder in GameObject.FindObjectsOfType<IslandFeeder>()) {
            Object.Destroy(feeder);
        }

        this.MatchEnd.Invoke(winner);
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

    /// <summary>Get the current round number, 0-indexed</summary>
    public int currentRound {
        get {
            return this.sobekMatchWon + this.cthuluMatchWon;
        }
    }
}
