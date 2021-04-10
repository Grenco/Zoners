using UnityEngine;
using UnityEngine.UI;

public class UpdateHP : MonoBehaviour
{
    private Text text;
    private PlayerControls player;

    // Start is called before the first frame update
    private void Start()
    {
        text = gameObject.GetComponent<Text>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerControls>();
    }

    // Update is called once per frame
    private void Update()
    {
        string hp = player.hitPoints.ToString();

        text.text = $"HP: {hp}";
    }
}