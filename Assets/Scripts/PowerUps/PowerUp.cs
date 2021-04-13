using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace Assets.Scripts.PowerUps
{
    public class PowerUp : MonoBehaviour
    {
        public float effectTime = 5f;
        protected float activeTime = 0f;
        protected bool isActive = false;
        protected MultiplayerControls player;

        protected void Start()
        {
            // NOTE:
            // Unity or the PhotonTransformView do not save the editor tick boxes for these sync settings
            // so I'll have to manually set them here...
            PhotonTransformView ptv = GetComponent<PhotonTransformView>();
            ptv.m_SynchronizePosition = true;
            ptv.m_SynchronizeRotation = false;
            ptv.m_SynchronizeScale = false;
            //ptv.m_SynchronizeGlobal = true;
        }

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
            if (collision.gameObject.TryGetComponent(out player))
            {
                if ((player.isAIPlayer && PhotonNetwork.IsMasterClient) ||
                    collision.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    ApplyPowerUp(player);
                    isActive = true;
                    gameObject.transform.localScale = new Vector3(0, 0, 0);
                    gameObject.GetComponent<PhotonView>().RPC("RemovePowerUp", RpcTarget.Others);
                }
            }
        }

        protected virtual void ApplyPowerUp(MultiplayerControls player)
        {
            player.DisableControls();
        }

        protected virtual void ReversePowerUp(MultiplayerControls player)
        {
            player.EnableControls();
        }

        [PunRPC]
        protected void RemovePowerUp()
        {
            gameObject.SetActive(false);
        }
    }
}
