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

        //protected void Start()
        //{
        //    // NOTE:
        //    // Unity or the PhotonTransformView do not save the editor tick boxes for these sync settings
        //    // so I'll have to manually set them here...

        //    if (PhotonNetwork.IsMasterClient)
        //    {
        //        PhotonTransformView ptv = GetComponent<PhotonTransformView>();
        //        //ptv.m_SynchronizePosition = true;
        //        ptv.m_SynchronizeRotation = false;
        //        //ptv.m_SynchronizeScale = false;
        //    }
        //}

        protected void Update()
        {
            if (isActive)
            {
                activeTime += Time.deltaTime;
                if (activeTime > effectTime)
                {
                    isActive = false;
                    ReversePowerUp();
                    gameObject.SetActive(false);
                    gameObject.GetComponent<PhotonView>().RPC("RemovePowerUp", RpcTarget.All);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isActive && collision.gameObject.TryGetComponent(out player))
            {
                if ((player.isAIPlayer && PhotonNetwork.IsMasterClient) ||
                    collision.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    ApplyPowerUp();
                    isActive = true;
                    gameObject.GetComponent<PhotonView>().RPC("MovePowerUp", RpcTarget.All, new object[] { 0f, -500f, 0f });
                }
            }
        }

        protected virtual void ApplyPowerUp()
        {
            player.DisableControls();
        }

        protected virtual void ReversePowerUp()
        {
            player.EnableControls();
        }

        [PunRPC]
        protected void RemovePowerUp()
        {
            gameObject.SetActive(false);
        }

        [PunRPC]
        protected void MovePowerUp(float x, float y, float z)
        {
            gameObject.transform.localPosition = new Vector3(x, y, z);
        }
    }
}
