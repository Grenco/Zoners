using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Photon.Pun;

namespace Assets.Scripts.PowerUps
{
    class PowerUpGenerator : MonoBehaviour
    {
        public int powerUpSpawnTime = 30; //seconds
        public List<string> powerUps;
        public MazeConstructor mazeConstructor;

        private float LastPowerUpTime { get; set; }
        private GameController GameController { get; set; }

        private void Start()
        {
            LastPowerUpTime = GameSettings.GameTime;
            GameController = gameObject.GetComponent<GameController>();
        }

        private void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (LastPowerUpTime - GameController.timeRemaining > powerUpSpawnTime)
                {
                    LastPowerUpTime = GameController.timeRemaining;
                    SpawnRandomPowerup();
                }
            }
        }

        private void SpawnRandomPowerup()
        {
            // Choose random spot on map (with no walls)
            Vector3 position = mazeConstructor.RandomEmptySpace();

            // Choose random powerup
            System.Random r = new System.Random();
            string powerUp = powerUps[r.Next(powerUps.Count - 1)];

            // Use PhotonNetwork.Instantiate to create the powerup for all players in the same place
            PhotonNetwork.Instantiate(powerUp, position, new Quaternion());
        }
    }
}
