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
    public bool movementEnabled = true;
    public bool isCPUPlayer = false;
    Rigidbody rb;
    public GameObject sign;
    private Vector3 startingPosition;

    public string teamColor;
    GameObject team;
    TeamController teamController;
    GameObject enemyTeam;
    TeamController enemyTeamController;

    [Header("Movement Settings")]
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float mouseSpeed = 10.0f;

    [Header("Shooting Settings")]
    public int ammo = 10;
    private float shotCoolDown;
    public float fireRate = 1f; // shots/s
    GameObject ammoUI;

    [Header("Player Settings")]
    public int maxHitPoints = 100;
    public int hitPoints;
    public float damageSpeed = 10.0f; // HP/s
    private float damageTime;
    public float coolDowmTime = 0.0f;

    Ray ray;
    RaycastHit hit;
    GameObject hitObject;
    LineRenderer lr;

    PhotonView photonView;
    private string playerName = "";


    void Start()
    {
        photonView = gameObject.GetComponent<PhotonView>();
        if (!isCPUPlayer)
        {
            if (photonView.IsMine)
            {
                playerCam.gameObject.SetActive(true);
                playerCam.enabled = true;
            }
            AssignTeam();
            playerName = photonView.Owner.NickName;
            gameObject.name = playerName;
        }

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
        if (photonView.IsMine)
        {
            if (movementEnabled)
            {
                //if (characterController.isGrounded)
                //{
                // We are grounded, so recalculate
                // move direction directly from axes
                Vector3 moveDirectionVertical = gameObject.transform.forward * Input.GetAxis("Vertical");
                Vector3 moveDirectionHorizontal = gameObject.transform.right * Input.GetAxis("Horizontal");
                moveDirection = (moveDirectionVertical + moveDirectionHorizontal).normalized;
                moveDirection.y = 0.0f;
                moveDirection *= speed;

                if (Input.GetButton("Jump"))
                {
                    moveDirection.y = jumpSpeed;
                }
                //}

                // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
                // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
                // as an acceleration (ms^-2)
                //moveDirection.y -= gravity * Time.deltaTime;
            }
            else
            {
                moveDirection = Vector3.zero;
            }

            turnX = Input.GetAxis("Mouse X") * mouseSpeed;
            turnY = Input.GetAxis("Mouse Y") * mouseSpeed;
        }
        //RayShot();
        //shotCoolDown -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!isCPUPlayer)
        {
            Turn();
            if (photonView.IsMine)
            {
                Move();
                CoolDownCheck();
            }
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
        teamController.AddPlayer(gameObject);
    }

    void CreatePlayerLabel()
    {
        TextMesh tm = sign.GetComponent<TextMesh>();
        tm.text = gameObject.name;
    }

    public void TakeDamage()
    {
        damageTime += Time.deltaTime;
        if (damageTime > 1 / damageSpeed)
        {
            hitPoints -= 1;
            damageTime = 0;
        }
    }

    public void KillPlayer()
    {
        DisableControls();
        coolDowmTime = 5.0f;
        hitPoints = maxHitPoints;

        rb.MovePosition(startingPosition);

        photonView.RPC("KillPlayerRPC", RpcTarget.All);
    }

    [PunRPC]
    private void KillPlayerRPC()
    {
        teamController.RemovePlayer(gameObject);
        enemyTeamController.score++;
    }

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

    public void CPUMove(Vector3 movement)
    {
        rb.MovePosition(rb.position + movement * Time.deltaTime);
    }

    private void Turn()
    {
        gameObject.transform.Rotate(0, turnX, 0); // Player rotates on Y axis, your Cam is child, then rotates too

        // Security check to not rotate 360º 
        if (playerCam.transform.eulerAngles.x + (-turnY) <= 80 || playerCam.transform.eulerAngles.x + (-turnY) >= 280)
        {
            playerCam.transform.RotateAround(gameObject.transform.position, playerCam.transform.right, -turnY);
        }
        sign.transform.rotation = Camera.main.transform.rotation; // Causes the text faces camera
    }


    private void CoolDownCheck()
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

