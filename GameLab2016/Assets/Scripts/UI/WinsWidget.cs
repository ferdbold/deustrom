using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Simoncouche.UI {
    /// <summary>
    /// A WinsWidget displays to the user the number of wins a player has, using icons.
    /// </summary>
    public class WinsWidget : MonoBehaviour {
        
        public int score {
            set {
                int clampedValue = Mathf.Clamp(value, 0, 2);

                for (int i = 0; i < clampedValue; i++) {
                    transform.GetChild(i).GetComponent<Image>().color = Color.white;
                }
            }
        }
    }
}