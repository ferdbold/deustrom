using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

namespace Simoncouche.Bible {
    public class BibleQuoteWriter : MonoBehaviour {

        [Tooltip("Minimum number of a quote")]
        [SerializeField]
        private int _minRangeQuoteNumber = 0;

        [Tooltip("Maximum number of a quote")]
        [SerializeField]
        private int _maxRangeQuoteNumber = 99;

        /// <summary>Reference to our input field in the UI</summary>
        private InputField _inputField;

        /// <summary>This is the title of our input field</summary>
        [Tooltip("This is the title of our input field")]
        [SerializeField]
        private Text _titleInputField;

        /// <summary>Bool in order to know who won the game</summary>
        private bool _sobekWon = false;

        void Awake() {
            _inputField = gameObject.GetComponentInChildren<InputField>();
        }

        /// <summary>
        /// This function allows the player to write and is called via the GameManager
        /// </summary>
        /// <param name="winner"></param>
        public void BeginWriting(LevelManager.Player winner) {
            if (winner == LevelManager.Player.sobek) {
                _sobekWon = true;
                _titleInputField.text = "Write down a verse in Sobek's bible";
            } else {
                _sobekWon = false;
                _titleInputField.text = "Write down a verse in Cthulu's bible";
            }
            
        }

        void Update() {
            _inputField.Select();
            if (Input.GetButtonDown("Submit")) {
                this.SubmitQuote(_inputField.text);
            }
        }

        /// <summary>
        /// Open the save file and save a quote in it
        /// </summary>
        /// <param name="quote">the quote itself to save</param>
        /// <param name="isSobek">is sobek the winner ?</param>
        private void SaveQuote(string quote, bool isSobek) {
            Debug.Log(Application.persistentDataPath);
            BibleEntries tempEntries = BibleEntries.LoadBibleEntries();//Must load the data in order to do an insertion
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + BibleEntries.fileName);

            if (tempEntries == null) { //Must create new bible entries
                tempEntries = new BibleEntries();
            }
            int firstNumber = Random.Range(_minRangeQuoteNumber, _maxRangeQuoteNumber);
            int secondNumber = Random.Range(_minRangeQuoteNumber, _maxRangeQuoteNumber);

            BibleQuote quoteToInsert = new BibleQuote(firstNumber, secondNumber, quote, isSobek);
            if (isSobek) tempEntries.quoteListSobek.Add(quoteToInsert);
            else tempEntries.quoteListCthulu.Add(quoteToInsert);

            bf.Serialize(file, tempEntries);
            file.Close();
        }

        /// <summary>
        /// Event trigerred by our press enter input in order to save the current text to our file
        /// </summary>
        /// <param name="quote"></param>
        private void SubmitQuote(string quote) {
            SaveQuote(quote, _sobekWon);
            _inputField.enabled = false;
            //TODO : NOW TRIGGER ENDING SWITCH GAME STATE
            GameManager.Instance.SwitchScene(GameManager.Scene.Menu);
        }

        /// <summary>
        /// Function which print every entries in the two bibles
        /// </summary>
        private void PrintAllBibleQuotes() {
            BibleEntries entries = BibleEntries.LoadBibleEntries();
            foreach (BibleQuote bQuote in entries.quoteListCthulu) {
                Debug.Log(bQuote.quoteString + " - " + bQuote.godName + " " + bQuote.quoteFirstNo + ":" + bQuote.quoteSecondNo);
            }
            foreach (BibleQuote bQuote in entries.quoteListSobek) {
                Debug.Log(bQuote.quoteString + " - " + bQuote.godName + " " + bQuote.quoteFirstNo + ":" + bQuote.quoteSecondNo);
            }
        }

        /// <summary>
        /// Function which print every entries in the two bibles
        /// </summary>
        public void PrintQuote(BibleQuote quote) {
            if (quote != null) {
                Debug.Log(quote.quoteString + " - " + quote.godName + " " + quote.quoteFirstNo + ":" + quote.quoteSecondNo);
            } else {
                Debug.Log("Il n'existe aucun texte sacré");
            }
        }
    }


    /// <summary>
    /// This class contains all the quotes of the two bibles in two separate lists
    /// </summary>
    [System.Serializable]
    public class BibleEntries {
        /// <summary>File name of our save file</summary>
        public const string fileName = "/save.bible";

        /// <summary>
        /// This method is static in order to get every entries in the bibles of the two gods
        /// </summary>
        /// <returns>A BibleEntries object containing two lists of both Cthulu's quotes and Sobek's quotes</returns>
        public static BibleEntries LoadBibleEntries() {
            if (File.Exists(Application.persistentDataPath + fileName)) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + fileName, FileMode.Open);
                BibleEntries bibleEntries = (BibleEntries)bf.Deserialize(file);
                file.Close();
                return bibleEntries;
            } else {
                return null;
            }
        }

        /// <summary>
        /// This method gives a random quote from the two gods
        /// </summary>
        /// <returns>A random biblequote from the save system/returns>
        public static BibleQuote GetRandomQuote() {
            if (File.Exists(Application.persistentDataPath + fileName)) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + fileName, FileMode.Open);
                BibleEntries bibleEntries = (BibleEntries)bf.Deserialize(file);
                List<BibleQuote> quoteList = bibleEntries.quoteListCthulu;
                quoteList.AddRange(bibleEntries.quoteListSobek);
                int accessor = 0;
                if (quoteList.Count < 1) {
                    file.Close();
                    return null; //Si on a rien à retourner
                } else {
                    accessor = Random.Range(0, quoteList.Count - 1);
                }
                file.Close();
                return quoteList[accessor];
            } else {
                return null;
            }
        }

        public List<BibleQuote> quoteListCthulu;
        public List<BibleQuote> quoteListSobek;
        public BibleEntries() {
            quoteListCthulu = new List<BibleQuote>();
            quoteListSobek = new List<BibleQuote>();
        }
    }

    /// <summary>
    /// Simple class which has every element shown in a quote (god name, first no, second no and the quote itself)
    /// </summary>
    [System.Serializable]
    public class BibleQuote {
        public int quoteFirstNo { get; private set; }
        public int quoteSecondNo { get; private set; }
        public string quoteString { get; private set; }
        public string godName { get; private set; }
        public BibleQuote(int fNo, int lNo, string quote, bool isSobek) {
            quoteFirstNo = fNo;
            quoteSecondNo = lNo;
            quoteString = quote;
            godName = isSobek ? "Sobek" : "Cthulu";
        }
    }

}
