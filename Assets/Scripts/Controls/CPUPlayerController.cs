using System.Collections;
using System.Collections.Generic;
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
    Vector3 movement;

    public void Start()
    {
        playerController = gameObject.GetComponent<MultiplayerControls>();
    }

    public void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Check for dangers ahead

            // If dangerous, recompute path

            // Follow path

            RandomMovement();
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
}

