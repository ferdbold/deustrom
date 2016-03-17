using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace Simoncouche.Bible {
    public class BibleQuoteReader : MonoBehaviour {

        private bool _currentGodToCheckIsSobek=false;
        private Text _textBible;

        void Awake() {
            _textBible = GetComponentInChildren<Text>();
        }

        // Use this for initialization
        void Start() {
            _textBible.text = GetQuotesOfGod(_currentGodToCheckIsSobek);
        }

        private string GetQuotesOfGod(bool isSobek) {
            string quotes="";
            BibleEntries bibleEntries = BibleEntries.LoadBibleEntries();
            if (bibleEntries == null) {
                return quotes = "Aucun texte sacré n'a été écrit pour " + (isSobek ? "Sobek" : "Cthulu");
            } else {
                if (isSobek) {
                    if (bibleEntries.quoteListSobek.Count < 1) return quotes = "Aucun texte sacré n'a été écrit pour " + (isSobek ? "Sobek" : "Cthulu");
                    foreach (BibleQuote bQuote in bibleEntries.quoteListSobek) {
                        quotes += bQuote.quoteString + "\n\n\t\t" + " - " + bQuote.godName + " " + bQuote.quoteFirstNo + ":" + bQuote.quoteSecondNo + "\n\n";
                    }
                    return quotes;
                } else {
                    if (bibleEntries.quoteListCthulu.Count < 1) return quotes = "Aucun texte sacré n'a été écrit pour " + (isSobek ? "Sobek" : "Cthulu");
                    foreach (BibleQuote bQuote in bibleEntries.quoteListCthulu) {
                        quotes += bQuote.quoteString + "\n\n\t\t" + " - " + bQuote.godName + " " + bQuote.quoteFirstNo + ":" + bQuote.quoteSecondNo + "\n\n";
                    }
                    return quotes;
                }
            }

        }
    }
}
