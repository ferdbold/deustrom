using UnityEngine;
using System.Collections;

public class InGameTutorial : MonoBehaviour {

    private static GameObject tutoObject = null;

    public enum ScreenPos { topLeft, topMiddle, topRight, botLeft, botMiddle, botRight }


    public GameObject[] tutorials;

    public static void Create(ScreenPos position, int tutorialIndex, float timeShowned) {
        if (tutoObject == null) {
            tutoObject = Resources.Load("InGameTutorial") as GameObject;
        }

        InGameTutorial tuto = Instantiate(tutoObject).GetComponent<InGameTutorial>();
        if (tutorialIndex >= tuto.tutorials.Length) {
            Debug.LogError("Trying to access a tutorial out of range");
            Destroy(tuto.gameObject);
            return;
        }

        //Place pos
        Transform pos = tuto.transform;
        switch (position) {
            case ScreenPos.botLeft: pos = tuto.transform.FindChild("BotLeft"); break;
            case ScreenPos.botMiddle: pos = tuto.transform.FindChild("BotMiddle"); break;
            case ScreenPos.botRight: pos = tuto.transform.FindChild("BotRight"); break;
            case ScreenPos.topLeft: pos = tuto.transform.FindChild("TopLeft"); break;
            case ScreenPos.topMiddle: pos = tuto.transform.FindChild("TopMiddle"); break;
            case ScreenPos.topRight: pos = tuto.transform.FindChild("TopRight"); break;
        }

        GameObject choice = Instantiate(tuto.tutorials[tutorialIndex]) as GameObject;
        choice.transform.SetParent(choice.transform, false);
        tuto.StartCoroutine(tuto.WaitTime(timeShowned));
    }

    public IEnumerator WaitTime(float time) {
        yield return new WaitForRealSeconds(time);
        Destroy(gameObject);
    }
}
