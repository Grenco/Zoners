using UnityEngine;
using Photon.Pun;

namespace Assets.Scripts.PowerUps
{
    class ZonePowerBoost : PowerUp
    {
        public float PowerMultiplier = 2f;

        protected override void ApplyPowerUp()
        {
            gameObject.GetComponent<PhotonView>().RPC("ZoneStrengthBoost", RpcTarget.All, new object[] { player.team, PowerMultiplier });
        }

        protected override void ReversePowerUp()
        {
            gameObject.GetComponent<PhotonView>().RPC("ZoneStrengthBoost", RpcTarget.All, new object[] { player.team, 1f });
        }

        [PunRPC]
        private void ZoneStrengthBoost(string team, float boost)
        {
            ZoneController[] zones = FindObjectsOfType<ZoneController>();

            foreach (ZoneController zone in zones)
            {
                if (zone.team == team)
                {
                    zone.BoostStrength(boost);
                    Debug.Log($"{team} team zone power boosted");
                    return;
                }
            }
        }
    }
}
