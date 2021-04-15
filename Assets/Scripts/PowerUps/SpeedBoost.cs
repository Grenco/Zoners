using UnityEngine;

namespace Assets.Scripts.PowerUps
{
    class SpeedBoost : PowerUp
    {
        public float SpeedMultiplier = 1.5f;

        protected override void ApplyPowerUp()
        {
            player.speed *= SpeedMultiplier;
            Debug.Log($"Player {player.name} Speed: {player.speed}");
        }

        protected override void ReversePowerUp()
        {
            player.speed /= SpeedMultiplier;
            Debug.Log($"Player {player.name} Speed: {player.speed}");
        }
    }
}
