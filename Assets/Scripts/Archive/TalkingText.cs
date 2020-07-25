using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkingText : MonoBehaviour
{
    GameObject panel;
    GameObject textBox;
    GameObject player;
    public float coolDownTime = 0.5f;
    float timeRemaining = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        panel = gameObject;//GameObject.Find("TalkingTextPanel");
        textBox = GameObject.Find("TalkingText");
        player = GameObject.FindWithTag("Player");
        panel.SetActive(false);
    }

    public void AddText(string prompt)
    {
        Text text = textBox.GetComponent<Text>();
        text.text = prompt;
        panel.SetActive(true);
        timeRemaining = coolDownTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (panel.activeSelf && timeRemaining <= 0)
        {
            float check = Input.GetAxis("Action");
            if (check > 0)
            {
                panel.SetActive(false);

                PlayerControls controls = player.GetComponent<PlayerControls>();

                controls.EnableControls();
            }
        }
        timeRemaining -= Time.deltaTime;
    }
}
