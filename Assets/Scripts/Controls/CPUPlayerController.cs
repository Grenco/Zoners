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

    public void Start()
    {
        
    }

    public void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Check for dangers ahead

            // If dangerous, recompute path

            // Follow path

        }
    }

    public void ComputePath()
    {
        // Employ Djikstra's algorithm to compute path
        if (FindCurrentSpace(out Vector2Int currentSpace))
        {
            HashSet<Vector2Int> usedSet = new HashSet<Vector2Int>(blockedSpaces);
            usedSet.Add(currentSpace);
        }
    }

    public void FindBlockedSpaces(int r, int c)
    {
        blockedSpaces = new HashSet<Vector2Int>();
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; i++)
            {
                if (mapData[i,j] == 0)
                {
                    blockedSpaces.Add(new Vector2Int(i, j));
                }
            }
        }
    }

    public bool FindCurrentSpace(out Vector2Int currentSpace)
    {
        currentSpace = new Vector2Int();
        return false;
    }

    public void FollowPath()
    {

    }
}

