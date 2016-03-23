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

        /// <summary>
        /// Open the save file and save a quote in it
        /// </summary>
        /// <param name="quote">the quote itself to save</param>
        /// <param name="isSobek">is sobek the winner ?</param>
        public void SaveQuote(string quote, string author, bool isSobek) {
            Debug.Log(Application.persistentDataPath);
            BibleEntries tempEntries = BibleEntries.LoadBibleEntries();//Must load the data in order to do an insertion
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + BibleEntries.fileName);

            if (tempEntries == null) { //Must create new bible entries
                tempEntries = new BibleEntries();
            }
            int firstNumber = Random.Range(_minRangeQuoteNumber, _maxRangeQuoteNumber);
            int secondNumber = Random.Range(_minRangeQuoteNumber, _maxRangeQuoteNumber);

            BibleQuote quoteToInsert = new BibleQuote(firstNumber, secondNumber, quote, author);
            if (isSobek) tempEntries.quoteListSobek.Add(quoteToInsert);
            else tempEntries.quoteListCthulu.Add(quoteToInsert);

            bf.Serialize(file, tempEntries);
            file.Close();
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
        public BibleQuote(int fNo, int lNo, string quote, string author) {
            quoteFirstNo = fNo;
            quoteSecondNo = lNo;
            quoteString = quote;
            godName = author;
        }
    }

}
