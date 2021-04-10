using UnityEngine;
using UnityEngine.UI;

// This script moves the character controller forward
// and sideways based on the arrow keys.
// It also jumps when pressing space.
// Make sure to attach a character controller to the same game object.
// It is recommended that you make only one call to Move or SimpleMove per frame.

public class PlayerControls : MonoBehaviour
{
    private CharacterController characterController;
    private GameObject player;
    private GameObject playerCam;
    private Vector3 moveDirection = Vector3.zero;
    private bool movementEnabled = true;

    [Header("Movement Settings")]
    public float speed = 6.0f;

    public float jumpSpeed = 5.0f;
    public float gravity = 20.0f;
    public float mouseSpeed = 5.0f;

    [Header("Shooting Settings")]
    public int ammo = 10;

    private float shotCoolDown;
    public float fireRate = 1f; // shots/s
    private GameObject ammoUI;

    [Header("Player Settings")]
    public int maxHitPoints = 100;

    public int hitPoints;
    public float damageSpeed = 10; // HP/s
    private float damageTime;

    private Ray ray;
    private RaycastHit hit;
    private GameObject hitObject;
    private LineRenderer lr;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        //player = GameObject.FindWithTag("Player");
        player = gameObject;
        playerCam = GameObject.FindWithTag("MainCamera");
        Cursor.lockState = CursorLockMode.Locked;

        lr = gameObject.GetComponent<LineRenderer>();

        hitPoints = maxHitPoints;
        damageTime = 0;
    }

    private void Shoot()
    {
        if (ammo > 0 && shotCoolDown <= 0)
        {
            if (hitObject.CompareTag("Enemy"))
            {
                //hitObject.SetActive(false);
                EnemyBehaviour enemy = hitObject.GetComponent<EnemyBehaviour>();
                enemy.Kill();
            }
            ammo--;

            shotCoolDown = 1 / fireRate;
        }
    }

    private void Action()
    {
        if (hitObject.CompareTag("NPC") && hit.distance < 5)
        {
            BobBehaviour npc = hitObject.GetComponent<BobBehaviour>();
            if (hit.distance < npc.speakDistance)
            {
                npc.SpokenTo();
                DisableControls();
                ammo = 10;
            }
        }
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private void RayShot()
    {
        ray = new Ray(player.transform.position, player.transform.forward);
        hit = new RaycastHit();
        Vector3 lineStart = player.transform.position;
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
                lineEnd = lineStart + player.transform.forward * 1000f;
            }
        }
        else
        {
            hitObject = player;
            lineEnd = lineStart + player.transform.forward * 1000f;
        }
        DrawLine(lineStart, lineEnd, Color.red, Time.deltaTime);

        GameObject textBox = GameObject.Find("InteractionText");
        Text text = textBox.GetComponent<Text>();
        text.text = "";

        if (hitObject.CompareTag("NPC"))
        {
            BobBehaviour npc = hitObject.GetComponent<BobBehaviour>();
            if (hit.distance < npc.speakDistance)
            {
                text.text = "e: Talk";
            }
        }
    }

    public void TakeDamage()
    {
        damageTime += Time.deltaTime;
        if (damageTime > 1 / damageSpeed)
        {
            hitPoints -= 1;
            damageTime = 0;
        }

        if (hitPoints < 0)
        {
            hitPoints = 0;
        }

        if (hitPoints == 0)
        {
            GameObject endingTextBox = GameObject.Find("GameEndText");
            Text endingText = endingTextBox.GetComponent<Text>();
            endingText.text = "GAME OVER";
            DisableControls();
        }
    }

    public void EnableControls()
    {
        movementEnabled = true;
    }

    public void DisableControls()
    {
        movementEnabled = false;
    }

    private void Update()
    {
        // Exit Sample
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        if (movementEnabled)
        {
            if (characterController.isGrounded)
            {
                // We are grounded, so recalculate
                // move direction directly from axes
                Vector3 moveDirectionVertical = player.transform.forward * Input.GetAxis("Vertical");
                Vector3 moveDirectionHorizontal = player.transform.right * Input.GetAxis("Horizontal");
                Vector3 moveDirectionCombined = moveDirectionVertical + moveDirectionHorizontal;

                moveDirection = moveDirectionCombined.normalized;
                moveDirection.y = 0.0f;
                moveDirection *= speed;

                if (Input.GetButton("Jump"))
                {
                    moveDirection.y = jumpSpeed;
                }
            }

            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            //moveDirection.y -= gravity * Time.deltaTime;

            // Move the controller
            characterController.Move(moveDirection * Time.deltaTime);

            if (Input.GetAxis("Fire1") > 0)
            {
                Shoot();
            }

            if (Input.GetAxis("Action") > 0)
            {
                Action();
            }
        }
        //#####################################################
        float X = Input.GetAxis("Mouse X") * mouseSpeed;
        float Y = Input.GetAxis("Mouse Y") * mouseSpeed;

        player.transform.Rotate(0, X, 0); // Player rotates on Y axis, your Cam is child, then rotates too

        // Security check to not rotate 360º
        if (playerCam.transform.eulerAngles.x + (-Y) > 80 && playerCam.transform.eulerAngles.x + (-Y) < 280)
        { }
        else
        {
            playerCam.transform.RotateAround(player.transform.position, playerCam.transform.right, -Y);
        }

        RayShot();

        shotCoolDown -= Time.deltaTime;
    }
}