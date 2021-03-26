using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MultiplayerControls : MonoBehaviour
{
    //CharacterController characterController;
    public Camera playerCam;
    private Vector3 moveDirection = Vector3.zero;
    private float turnX;
    private float turnY;
    private bool jumpCheck;
    public bool movementEnabled = true;
    public bool isAIPlayer = false;
    private Rigidbody rb;
    public GameObject sign;
    private Vector3 startingPosition;
    private TeamController teamController;
    private TeamController enemyTeamController;

    public string team;
    public int playerNumber;

    [Header("Movement Settings")]
    public float mouseSpeed = 10.0f;
    public static float speed = 10.0f;
    public static float jumpSpeed = 5.0f;

    [Header("Player Settings")]
    public static int maxHitPoints = 100;
    public int hitPoints;
    public static float damageSpeed = 50.0f; // HP/s
    private float damageTime;
    public float coolDowmTime = 0.0f;

    private PhotonView photonView;
    private string playerName = "";


    void Start()
    {
        photonView = gameObject.GetComponent<PhotonView>();
        if (!isAIPlayer)
        {
            if (photonView.IsMine)
            {
                playerCam.gameObject.SetActive(true);
                playerCam.enabled = true;
            }

            playerName = photonView.Owner.NickName;
            gameObject.name = playerName;
            playerNumber = TeamSettings.PositionInTeam(photonView.Owner);
        }
        else if (!GameSettings.IncludeAIPlayers)
        {
            gameObject.SetActive(false);
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
    }

    // Update is called once per frame
    void Update()
    {
        // Convert the user input into a momvemnt and turn direction.
        if (photonView.IsMine && !isAIPlayer)
        {
            if (movementEnabled)
            {
                Vector3 moveDirectionVertical = gameObject.transform.forward * Input.GetAxis("Vertical");
                Vector3 moveDirectionHorizontal = gameObject.transform.right * Input.GetAxis("Horizontal");
                moveDirection = (moveDirectionVertical + moveDirectionHorizontal).normalized;
                moveDirection.y = 0.0f;
                moveDirection *= speed;

                jumpCheck = Input.GetButton("Jump");
            }
            else
            {
                moveDirection = Vector3.zero;
            }

            turnX = Input.GetAxis("Mouse X") * mouseSpeed;
            turnY = Input.GetAxis("Mouse Y") * mouseSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (team == TeamSettings.MyTeam)
        {
            TurnSign();
        }
        if (photonView.IsMine && !isAIPlayer)
        {
            Turn();
            Move();
            if (jumpCheck && IsGrounded())
            {
                Jump();
            }
            CoolDownCheck();
        }
        if (isAIPlayer && PhotonNetwork.IsMasterClient)
        {
            CoolDownCheck();
        }
    }

    /// <summary>
    /// Add find the team and enemy team game objects and add the aplayer to the team.
    /// </summary>
    void AssignTeam()
    {
        teamController = GameObject.Find(team + "Team").GetComponent<TeamController>();
        enemyTeamController = GameObject.Find(TeamSettings.OtherTeam(team) + "Team").GetComponent<TeamController>();
        if (!isAIPlayer)
        {
            teamController.AddPlayer(gameObject);
        }
    }

    /// <summary>
    /// Add the player name to the sign above the player's head.
    /// </summary>
    void CreatePlayerLabel()
    {
        TextMesh tm = sign.GetComponent<TextMesh>();
        tm.text = gameObject.name;
    }

    /// <summary>
    /// Take damage for a single frame when inside the enemy's danger zone.
    /// </summary>
    public void TakeDamage()
    {
        damageTime += Time.deltaTime * enemyTeamController.damageMultiplier;
        if (damageTime > 1 / damageSpeed)
        {
            hitPoints -= 1;
            damageTime = 0;
        }
    }

    /// <summary>
    /// Remove the player from the team and send them back to the respawn point for a cooldown time.
    /// </summary>
    public void KillPlayer()
    {
        DisableControls();
        coolDowmTime = 5.0f;
        hitPoints = maxHitPoints;
        rb.MovePosition(startingPosition);
        photonView.RPC("KillPlayerRPC", RpcTarget.All);
    }

    /// <summary>
    /// Notify the other players on the network that this player has died
    /// </summary>
    [PunRPC]
    private void KillPlayerRPC()
    {
        teamController.RemovePlayer(playerNumber);
        enemyTeamController.score++;
    }

    /// <summary>
    /// Notify other player on the network that this plaeyr has respawned.
    /// </summary>
    [PunRPC]
    private void RevivePlayerRPC()
    {
        teamController.AddPlayer(gameObject);
    }

    public void EnableControls()
    {
        movementEnabled = true;
    }

    public void DisableControls()
    {
        movementEnabled = false;
    }

    private void Move()
    {
        // Move the controller
        rb.MovePosition(rb.position + moveDirection * Time.deltaTime);
    }

    private void Jump()
    {
        rb.velocity = new Vector3(0, jumpSpeed, 0);
    }

    /// <summary>
    /// Allow an external AI controller to move the player.
    /// </summary>
    /// <param name="movement"> Movmement direction. </param>
    public void AIMove(Vector3 movement)
    {
        rb.MovePosition(rb.position + movement.normalized * speed * Time.deltaTime);
    }

    private void Turn()
    {
        gameObject.transform.Rotate(0, turnX, 0); // Player rotates on Y axis, your Cam is child, then rotates too

        // Security check to not rotate 360º 
        if (playerCam.transform.eulerAngles.x + (-turnY) <= 80 || playerCam.transform.eulerAngles.x + (-turnY) >= 280)
        {
            playerCam.transform.RotateAround(gameObject.transform.position, playerCam.transform.right, -turnY);
        }
    }

    /// <summary>
    /// Turn the player's sign to face the main camera.
    /// </summary>
    private void TurnSign()
    {
        // Ensures the player names are facing the camera
        if (Camera.main != null)
        {
            sign.transform.rotation = Camera.main.transform.rotation;
        }
    }

    /// <summary>
    /// Check if the character is touching the ground.
    /// </summary>
    /// <returns> True if the character is touching the ground. </returns>
    private bool IsGrounded()
    {
        float distToGround = gameObject.GetComponent<Collider>().bounds.extents.y;
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }

    /// <summary>
    /// If the player has died, reduce the cooldown time and check if the player can respawn.
    /// </summary>
    public void CoolDownCheck()
    {
        if (coolDowmTime > 0)
        {
            coolDowmTime -= Time.deltaTime;
            if (coolDowmTime <= 0)
            {
                EnableControls();
                photonView.RPC("RevivePlayerRPC", RpcTarget.All);
            }
        }
    }
}

