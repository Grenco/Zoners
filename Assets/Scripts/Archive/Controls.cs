using UnityEngine;

public class Controls : MonoBehaviour
{
    private GameObject player;
    private Vector3 translation;
    private Vector3 rotation;
    public float moveSpeed = 10; // m/s
    public float rotationSpeed = 0.01f;// deg/s
    private GameObject playerCamera;
    private Ray ray;
    public int ammo = 10;
    private float shotCoolDown;
    public float fireRate = 1f; // shot/s

    private CharacterController characterController;

    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerCamera = GameObject.FindWithTag("MainCamera");
        characterController = GetComponent<CharacterController>();
    }

    private void Shoot()
    {
        if (ammo > 0 && shotCoolDown < 0)
        {
            ray = new Ray(player.transform.position, player.transform.forward);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.collider.gameObject;
                hitObject.SetActive(false);
            }
            ammo--;

            shotCoolDown = 1 / fireRate;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        //float mouseX = Input.GetAxis("Mouse X");
        //float mouseY = Input.GetAxis("Mouse Y");
        float horizontalTurn = Input.GetAxis("Turn Horizontal");
        float fire = Input.GetAxis("Fire1");

        translation = new Vector3(horizontalInput, 0, verticalInput) * moveSpeed * Time.deltaTime;
        rotation = new Vector3(0, horizontalTurn, 0) * rotationSpeed * Time.deltaTime;
        rotation.Normalize();

        player.transform.Translate(translation);
        player.transform.Rotate(rotation);

        //playerCamera.transform.Translate(translation);
        playerCamera.transform.position = player.transform.position - player.transform.forward * 5f + Vector3.up;
        playerCamera.transform.forward = player.transform.forward;

        if (fire > 0)
        {
            Shoot();
        }
        shotCoolDown -= Time.deltaTime;
    }
}