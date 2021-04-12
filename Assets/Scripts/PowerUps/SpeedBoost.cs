using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.PowerUps
{
    class SpeedBoost : PowerUp
    {
        protected override void ApplyPowerUp(MultiplayerControls player)
        {
            player.speed *= 2;
        }

        protected override void ReversePowerUp(MultiplayerControls player)
        {
            player.speed /= 2;
        }
    }
}
