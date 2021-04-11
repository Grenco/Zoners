using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        MultiplayerControls player = collision.gameObject.GetComponent<MultiplayerControls>();
        ApplyPowerUp(player);
    }

    protected void ApplyPowerUp(MultiplayerControls player)
    {
        player.DisableControls();
        gameObject.SetActive(false);
    }
}
