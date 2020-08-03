using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
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

    public string teamColor;
    public int playerNumber;
    private GameObject team;
    private TeamController teamController;
    private GameObject enemyTeam;
    private TeamController enemyTeamController;

    [Header("Movement Settings")]
    public float mouseSpeed = 10.0f;
    public static float speed = 10.0f;
    public static float jumpSpeed = 5.0f;

    [Header("Shooting Settings")]
    public int ammo = 10;
    private float shotCoolDown;
    public float fireRate = 1f; // shots/s
    GameObject ammoUI;

    [Header("Player Settings")]
    public static int maxHitPoints = 100;
    public int hitPoints;
    public static float damageSpeed = 50.0f; // HP/s
    private float damageTime;
    public float coolDowmTime = 0.0f;

    Ray ray;
    RaycastHit hit;
    GameObject hitObject;
    LineRenderer lr;

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

            GetPlayerNumber();
            playerName = photonView.Owner.NickName;
            gameObject.name = playerName;
        }

        AssignTeam();

        lr = gameObject.GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();
        startingPosition = rb.position;

        hitPoints = maxHitPoints;
        damageTime = 0f;

        if (teamColor == Launcher.team)
        {
            CreatePlayerLabel();
        }
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
        if (teamColor == Launcher.team)
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

    void RayShot()
    {
        ray = new Ray(gameObject.transform.position, gameObject.transform.forward);
        hit = new RaycastHit();
        Vector3 lineStart = gameObject.transform.position;
        Vector3 lineEnd = new Vector3();
        if (Physics.Raycast(ray, out hit))
        {
            hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("Enemy") || hitObject.CompareTag("NPC"))
            {
                lineEnd = hit.transform.position;
            }
            else
            {
                lineEnd = lineStart + gameObject.transform.forward * 1000f;
            }
        }
        else
        {
            hitObject = gameObject;
            lineEnd = lineStart + gameObject.transform.forward * 1000f;
        }
        DrawLine(lineStart, lineEnd, Color.red, Time.deltaTime);
    }

    void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    /// <summary>
    /// Add find the team and enemy team game objects and add the aplayer to the team.
    /// </summary>
    void AssignTeam()
    {
        if (teamColor == "red")
        {
            team = GameObject.Find("RedTeam");
            enemyTeam = GameObject.Find("BlueTeam");
        }
        else if (teamColor == "blue")
        {
            team = GameObject.Find("BlueTeam");
            enemyTeam = GameObject.Find("RedTeam");
        }

        teamController = team.GetComponent<TeamController>();
        enemyTeamController = enemyTeam.GetComponent<TeamController>();
        if (!isAIPlayer)
        {
            teamController.AddPlayer(gameObject);
        }
    }

    /// <summary>
    /// Find the player's reference number in the team.
    /// </summary>
    void GetPlayerNumber()
    {
        int[] teamList = (int[])PhotonNetwork.PlayerList[0].CustomProperties[teamColor + "Team"];
        playerNumber = System.Array.IndexOf(teamList, photonView.CreatorActorNr);
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

