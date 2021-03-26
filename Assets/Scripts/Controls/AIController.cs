using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private MultiplayerControls playerController;

    private float newDirectionMovementTimer = 0.0f;
    private Vector3 movement;

    private Ray ray;
    private RaycastHit hitInfo;
    private int wallLayer;
    private float[] distances;
    private List<Vector3> directions;
    private int direction = 0;
    public List<int> prefferedDirections;

    private float preferredDirectionMultiplier = 1.3f;
    private float backwardsMultiplier = 0.1f;
    private float repeatedStraightLineMultipier = 1.0f;
    private float turnBackMultiplier = 0.9f;
    private float keepGoingStraightMultiplier = 1.2f;

    public void Start()
    {
        playerController = gameObject.GetComponent<MultiplayerControls>();
        wallLayer = LayerMask.GetMask("Walls", "ExcludeFromMinimap");

        directions = new List<Vector3>();
        directions.Add(Vector3.forward);
        directions.Add(Vector3.right);
        directions.Add(-Vector3.forward);
        directions.Add(-Vector3.right);

        distances = new float[4];
    }

    public void Update()
    {
        if (PhotonNetwork.IsMasterClient && GameController.gameActive)
        {
            if (newDirectionMovementTimer > 0)
            {
                newDirectionMovementTimer -= Time.deltaTime;
            }
            else
            {
                ChooseDirection();
            }

            if (playerController.movementEnabled)
            {
                playerController.AIMove(directions[direction]);
            }
        }
    }

    /// <summary>
    /// Decide a direction to travel based on the surrounding walls and players.
    /// </summary>
    public void ChooseDirection()
    {
        // Loop through forward, right, backward and left
        for (int i = 0; i < directions.Count; i++)
        {
            // Perform a raycast to find how far the nearest obstacle is.
            if (Physics.Raycast(gameObject.transform.position, directions[i], out hitInfo, 1000, wallLayer))
            {
                distances[i] = hitInfo.distance;

                // Use multipliers to bias the player towards a certain direction.
                if (prefferedDirections.Contains(i))
                {
                    distances[i] *= preferredDirectionMultiplier;
                }

                // Influence the player against turning back on themselves.
                if ((direction + 2) % directions.Count == i)
                {
                    distances[i] *= backwardsMultiplier;
                    distances[i] *= repeatedStraightLineMultipier;
                }

                // Influence the player to keep going straight, unless they have been moving in that direction for too long.
                if (direction == i)
                {
                    distances[i] *= repeatedStraightLineMultipier;
                    distances[i] *= keepGoingStraightMultiplier;
                }
            }
            else
            {
                distances[i] = 0.0f;
            }
        }

        // Find the best (highest) distance.
        int newDirection = System.Array.IndexOf(distances, distances.Max());

        // If the player turns back on itself, encourage it to turn 90deg.
        if ((direction + 2) % directions.Count == newDirection)
        {
            repeatedStraightLineMultipier *= turnBackMultiplier;
        }
        else if (newDirection != direction)
        {
            // If the player turns 90deg, can't change direction for given time period.
            repeatedStraightLineMultipier = 1.0f;
            newDirectionMovementTimer = 0.7f;
        }

        direction = newDirection;
    }
}