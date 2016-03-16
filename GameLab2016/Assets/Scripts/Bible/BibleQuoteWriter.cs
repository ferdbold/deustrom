using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

namespace Simoncouche.Bible {
    [RequireComponent(typeof(InputField))]
    public class BibleQuoteWriter : MonoBehaviour {

        /// <summary>File name of our save file</summary>
        public const string fileName = "/save.bible";

        [Tooltip("Minimum number of a quote")]
        [SerializeField]
        private int _minRangeQuoteNumber = 0;

        [Tooltip("Maximum number of a quote")]
        [SerializeField]
        private int _maxRangeQuoteNumber = 99;

        /// <summary>Reference to our input field in the UI</summary>
        private InputField _inputField;

        /// <summary>Bool in order to know who won the game</summary>
        private bool sobekWon = false;

        void Awake() {
            _inputField = gameObject.GetComponentInChildren<InputField>();
        }

        // Use this for initialization
        void Start() {
            //TO DO : SET WHO WON THE GAME HERE
            //sobekWon = 
            _inputField.Select();
        }

        void Update() {
            _inputField.Select();
            if (Input.GetButtonDown("Submit")) {
                this.SubmitQuote(_inputField.text);
            }
        }

        private void SaveQuote(string quote, bool isSobek) {
            Debug.Log(Application.persistentDataPath);
            BibleEntries tempEntries = LoadBibleEntries();//Must load the data in order to do an insertion
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + fileName);

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

        private void SubmitQuote(string quote) {
            SaveQuote(quote, sobekWon);
            _inputField.enabled = false;
            //TODO : NOW TRIGGER ENDING SWITCH GAME STATE
        }

        /// <summary>
        /// This method is static in order to get every entries in the bibles of the two gods
        /// </summary>
        /// <returns>A BibleEntries object containing two lists of both Cthulu's quotes and Sobek's quotes</returns>
        public static BibleEntries LoadBibleEntries() {
            if (File.Exists(Application.persistentDataPath + fileName)){
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
        /// Function which print every entries in the two bibles
        /// </summary>
        private void PrintBibleQuotes() {
            BibleEntries entries = LoadBibleEntries();
            foreach (BibleQuote bQuote in entries.quoteListCthulu) {
                Debug.Log(bQuote.quoteString + " - " + bQuote.godName + " " + bQuote.quoteFirstNo + ":" + bQuote.quoteSecondNo);
            }
            foreach (BibleQuote bQuote in entries.quoteListSobek) {
                Debug.Log(bQuote.quoteString + " - " + bQuote.godName + " " + bQuote.quoteFirstNo + ":" + bQuote.quoteSecondNo);
            }
        }
    }


    /// <summary>
    /// This class contains all the quotes of the two bibles in two separate lists
    /// </summary>
    [System.Serializable]
    public class BibleEntries {
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
