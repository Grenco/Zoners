using UnityEngine;
using UnityEngine.UI;

public class UpdateAmmo : MonoBehaviour
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
        string ammo = player.ammo.ToString();

        text.text = $"Ammo: {ammo}";
    }
}