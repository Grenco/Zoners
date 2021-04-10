using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.EmissionModule em;
    private CharacterController controller;
    private float movementSpeed = 5f;
    private System.Random random;
    private Vector3 moveDir;
    private float gravity = 20.0f;

    private float moveTime;

    private EnemyArea enemyArea;

    // Start is called before the first frame update
    private void Start()
    {
        ps = gameObject.GetComponent<ParticleSystem>();
        controller = gameObject.GetComponent<CharacterController>();
        enemyArea = GetComponentInParent<EnemyArea>();
        random = new System.Random();
        moveDir = new Vector3();
        moveTime = 0;
    }

    public void Kill()
    {
        ps.Play();
        Destroy(gameObject, ps.main.duration);
        enemyArea.RemoveEnemy(gameObject);
    }

    // Update is called once per frame
    private void Update()
    {
        if (moveTime < 0)
        {
            moveDir.x = Random.Range(-50, 50);
            moveDir.z = Random.Range(-50, 50);

            moveTime = 1;
        }

        if (controller.isGrounded)
        {
            moveDir.y = 0.0f;
        }

        moveDir = moveDir.normalized;

        moveDir.y -= gravity * Time.deltaTime;
        controller.Move(moveDir * movementSpeed * Time.deltaTime);
        moveTime -= Time.deltaTime;
    }
}