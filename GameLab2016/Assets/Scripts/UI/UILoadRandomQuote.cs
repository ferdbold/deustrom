using UnityEngine;
using System.Collections;
using Simoncouche.Bible;
using UnityEngine.UI;

public class UILoadRandomQuote : MonoBehaviour {

    private Text textElementVerseQuote;
    private Text textElementAuthorAndNo;

    void Awake() {
        Text[] array = GetComponentsInChildren<Text>();
        textElementVerseQuote = array[0];
        textElementAuthorAndNo = array[1];
    }

	// Use this for initialization
	void Start () {
        BibleQuote bibleQuote = BibleEntries.GetRandomQuote();
        textElementVerseQuote.text = bibleQuote.quoteString;
        textElementAuthorAndNo.text = "- " + bibleQuote.godName + " " + bibleQuote.quoteFirstNo + ":" + bibleQuote.quoteSecondNo;
    }
}
