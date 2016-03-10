using UnityEngine;
using System.Collections.Generic;
using Simoncouche.UI;

/// <summary>The UIManager oversees the entire UI layer of the game.</summary>
public class UIManager : MonoBehaviour {
    
    private List<ScoreWidget> _scoreWidgets;

    private void Awake() {
        _scoreWidgets = new List<ScoreWidget>();
        _scoreWidgets.Add(GameObject.Find("UI/Scores/Sobek").GetComponent<ScoreWidget>());
        _scoreWidgets.Add(GameObject.Find("UI/Scores/Cthulhu").GetComponent<ScoreWidget>());
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.K))
            _scoreWidgets[0].AddPoints(10);

        if (Input.GetKeyDown(KeyCode.L))
            _scoreWidgets[1].AddPoints(10);
    }
}
