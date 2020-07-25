using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateAmmo : MonoBehaviour
{
    Text text;
    PlayerControls player;
    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<Text>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerControls>();
    }

    // Update is called once per frame
    void Update()
    {
        string ammo = player.ammo.ToString();
        
        text.text = $"Ammo: {ammo}";
    }
}
