using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : MultiplayerControls
{
    private float newDirectionMovementTimer = 0.0f;

    private RaycastHit hitInfo;
    private int wallLayer;
    private float[] distances;

    private List<Vector3> directions = new List<Vector3>
    {
        Vector3.forward,
        Vector3.right,
        -Vector3.forward,
        -Vector3.right
    };

    private int direction = 0;
    private List<int> preferredDirections;

    private Dictionary<int, List<int>> perferredDirectionDictionary = new Dictionary<int, List<int>>
    {
        { 0, new List<int>{2, 3} },
        { 1, new List<int>{0, 3} },
        { 2, new List<int>{0, 1} },
        { 3, new List<int>{2, 1} },
    };

    private float preferredDirectionMultiplier = 1.3f;
    private float backwardsMultiplier = 0.1f;
    private float repeatedStraightLineMultipier = 1.0f;
    private float turnBackMultiplier = 0.9f;
    private float keepGoingStraightMultiplier = 1.2f;

    public void Start()
    {
        photonView = gameObject.GetComponent<PhotonView>();

        if (!GameSettings.IncludeAIPlayers)
        {
            gameObject.SetActive(false);
            return;
        }

        AssignTeam();

        rb = GetComponent<Rigidbody>();
        startingPosition = rb.position;
        hitPoints = maxHitPoints;
        damageTime = 0f;

        if (team == TeamSettings.MyTeam)
        {
            CreatePlayerLabel();
        }

        mouseSpeed = PlayerSettings.MouseSensitivity;

        preferredDirections = perferredDirectionDictionary[playerNumber];
        wallLayer = LayerMask.GetMask("Walls", "ExcludeFromMinimap");
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

            if (movementEnabled)
            {
                Move(directions[direction]);
            }
        }
    }

    public void FixedUpdate()
    {
        if (team == TeamSettings.MyTeam)
        {
            TurnSign();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            CoolDownCheck();
        }
    }

    /// <summary>
    /// Allow an external AI controller to move the player.
    /// </summary>
    /// <param name="movement"> Movmement direction. </param>
    private void Move(Vector3 movement)
    {
        rb.MovePosition(rb.position + movement.normalized * speed * Time.deltaTime);
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
                if (preferredDirections.Contains(i))
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