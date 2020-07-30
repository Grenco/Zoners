using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using Photon.Pun;

public class CPUPlayerController : MonoBehaviour
{
    int[,] mapData;
    int[] path;
    HashSet<Vector2Int> blockedSpaces;
    Vector2Int endSpace;
    MultiplayerControls playerController;

    float randomMovementTimer = 0.0f;
    float newDirectionMovementTimer = 0.0f;
    Vector3 movement;

    Ray ray;
    RaycastHit hitInfo;
    int wallLayer;
    float[] distances;
    List<Vector3> directions;
    int direction = 0;
    public List<int> prefferedDirections;

    float preferredDirectionMultiplier = 1.2f;
    float backwardsMultiplier = 0.1f;
    float repeatedStraightLineMultipier = 1.0f;
    float turnBackMultiplier = 0.9f;

    Vector3 collisionOffset = new Vector3(0.01f, 0.0f, 0.01f);

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
        if (PhotonNetwork.IsMasterClient)
        {
            // Check for dangers ahead

            // If dangerous, recompute path

            // Follow path

            //RandomMovement();
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
                playerController.CPUMove(directions[direction] + collisionOffset);
            }
        }
    }

    private void ComputePath()
    {
        // Employ Djikstra's algorithm to compute path
        if (FindCurrentSpace(out Vector2Int currentSpace))
        {
            HashSet<Vector2Int> usedSet = new HashSet<Vector2Int>(blockedSpaces);
            usedSet.Add(currentSpace);
        }
    }

    private void FindBlockedSpaces(int r, int c)
    {
        blockedSpaces = new HashSet<Vector2Int>();
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; i++)
            {
                if (mapData[i, j] == 0)
                {
                    blockedSpaces.Add(new Vector2Int(i, j));
                }
            }
        }
    }

    private bool FindCurrentSpace(out Vector2Int currentSpace)
    {
        currentSpace = new Vector2Int();
        return false;
    }

    private void FollowPath()
    {

    }

    public void RandomMovement()
    {
        if (randomMovementTimer <= 0)
        {
            float x = Random.value - 0.5f;
            float z = Random.value - 0.5f;
            movement = new Vector3(x, 0.0f, z).normalized;
            randomMovementTimer = 2.0f;
        }
        playerController.CPUMove(movement);
        randomMovementTimer -= Time.deltaTime;
    }

    public void ChooseDirection()
    {
        for (int i = 0; i < directions.Count; i++)
        {
            if (Physics.Raycast(gameObject.transform.position, directions[i], out hitInfo, 1000, wallLayer))
            {
                distances[i] = hitInfo.distance;

                if (prefferedDirections.Contains(i))
                {
                    distances[i] *= preferredDirectionMultiplier;
                }

                if ((direction + 2) % directions.Count == i)
                {
                    distances[i] *= backwardsMultiplier;
                    distances[i] *= repeatedStraightLineMultipier;
                }

                if (direction == i)
                {
                    distances[i] *= repeatedStraightLineMultipier;
                }
            }
            else
            {
                distances[i] = 0.0f;
            }
        }

        int newDirection = System.Array.IndexOf(distances, distances.Max());

        if ((direction + 2) % directions.Count == newDirection)
        {
            repeatedStraightLineMultipier *= turnBackMultiplier;
        }
        else if (newDirection != direction)
        {
            repeatedStraightLineMultipier = 1.0f;
            newDirectionMovementTimer = 0.7f;
        }

        direction = newDirection;
    }
}

