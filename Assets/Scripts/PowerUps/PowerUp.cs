using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts.PowerUps
{
    public class PowerUp : MonoBehaviour
    {
        public float effectTime = 5f;
        protected float activeTime = 0f;
        protected bool isActive = false;
        protected MultiplayerControls player;

        protected void Update()
        {
            if (isActive)
            {
                activeTime += Time.deltaTime;
                if (activeTime > effectTime)
                {
                    isActive = false;
                    ReversePowerUp(player);
                    gameObject.SetActive(false);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            player = collision.gameObject.GetComponent<MultiplayerControls>();
            ApplyPowerUp(player);
            isActive = true;
            gameObject.transform.localScale = new Vector3(0, 0, 0);
        }

        protected virtual void ApplyPowerUp(MultiplayerControls player)
        {
            player.DisableControls();
        }

        protected virtual void ReversePowerUp(MultiplayerControls player)
        {
            player.EnableControls();
        }
    }
}
