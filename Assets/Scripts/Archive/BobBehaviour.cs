using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BobBehaviour : MonoBehaviour
{
    string prompt;
    public float speakDistance = 5f;

    // Start is called before the first frame update
    void Start()
    {
        prompt = "Hello player, I'm Bob. This is CrapGame, it's not even really a game. Just go around and shoot with the clicky button to get rid of the red guys. Bye!";

    }

    public void SpokenTo()
    {
        //GameObject textPanel = GameObject.Find("TalkingTextPanel");

        GameObject uiObject = GameObject.Find("UI");

        //GameObject[] textPanel = uiObject.GetComponentsInChildren<GameObject>();
        GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject textPanel = gameObjects.Where(t => t.name == "TalkingTextPanel").ToArray()[0];
        //GameObject textPanel = Resources.FindObjectsOfTypeAll<GameObject>().Where(t => t.name == "TalkingTextPanel").ToArray()[1];


        TalkingText textbox = textPanel.GetComponent<TalkingText>();

        textbox.AddText(prompt);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
