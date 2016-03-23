using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BibleQuoteWidget : MonoBehaviour {

    private static GameObject _sobekWidgetPrefab;
    private static GameObject _cthulhuWidgetPrefab;

    private static GameObject[] _widgetPrefabs = new GameObject[2];

    // COMPONENTS

    public Text quoteText { get; private set; }
    public Text footnoteText { get; private set; }

    // METHODS

    private void Awake() {
        this.quoteText = transform.Find("Quote").GetComponent<Text>();
        this.footnoteText = transform.Find("Footnote").GetComponent<Text>();
    }

    public static BibleQuoteWidget Create(LevelManager.Player player, string quote, string author, int chapter, int verset) {
        if (_widgetPrefabs[(int)player] == null) {
            _widgetPrefabs[(int)player] = (player == LevelManager.Player.cthulu) ?
                Resources.Load("UI/CthulhuBibleQuoteWidget") as GameObject :
                Resources.Load("UI/SobekBibleQuoteWidget") as GameObject;
        };

        BibleQuoteWidget widget = ((GameObject)Instantiate(
            _widgetPrefabs[(int)player], 
            Vector3.zero, 
            Quaternion.identity
        )).GetComponent<BibleQuoteWidget>();

        widget.quoteText.text = quote;
        widget.footnoteText.text = author + " " + chapter.ToString() + ":" + verset.ToString();

        return widget;
    }
}
