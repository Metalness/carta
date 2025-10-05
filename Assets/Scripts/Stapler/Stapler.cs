using UnityEngine;
using System.Collections;

public class Stapler : MonoBehaviour
{
    [Header("Movement + Attack Settings")]
    public float health;
    public float hopForce = 1f;
    public float attackRange = 3f;
    public float detectionRange = 7f;
    private float aimHeightOffset = 1f;

    public float shootingSpeed = 30f;
    public float burstDelay = 0.2f;

    [Header("Prefabs")]
    public GameObject pinPrefab;
    public Transform pinSpawnPointLeft;
    public Transform pinSpawnPointRight;


    [Header("Cooldowns (frames)")]
    public float maxHopCooldown = 60f;   // ~1s at 60fps
    public float maxShootCooldown = 90f; // ~1.5s at 60fps

    public float hopCooldown = 0f;
    public float shootCooldown = 0f;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

    public bool facingRight = false; // tracks last hop direction
    public bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
            Debug.LogWarning("Stapler: No player found in scene!");
    }

    public void damagePlayer(int damage)
    {
        health -= damage;

    }

    void FixedUpdate()
    {
        if (health < 0)
        {
            DestroyImmediate(this);
        }
        if (player == null) return;

        // tick down cooldowns
        hopCooldown = Mathf.Max(0, hopCooldown - 1f);
        shootCooldown = Mathf.Max(0, shootCooldown - 1f);

        float distance = player.position.x - transform.position.x;
        float absDistance = Mathf.Abs(distance);

        // try hopping towards player
        if (!isAttacking && absDistance < detectionRange && absDistance > attackRange && hopCooldown == 0f)
        {
            hopCooldown = maxHopCooldown;

            if (distance < 0) // player is left
            {
                animator.SetTrigger("HopLeft");
                Debug.Log("hopping left");
            }
            else // player is right
            {
                animator.SetTrigger("HopRight");
                Debug.Log("hopping right");
            }
        }
        // try shooting at player
        else if (!isAttacking && absDistance <= attackRange && shootCooldown == 0f)
        {
            isAttacking = true;

            if (distance < 0) // player is left
            {
                animator.SetTrigger("ShootLeft");
                Debug.Log("shooting left");
            }
            else // player is right
            {
                animator.SetTrigger("ShootRight");
                Debug.Log("shooting right");
            }
        }
    }

    // Called by animation event to hop right
    public void ApplyHopForceRight()
    {
        rb.AddForce(Vector2.right * hopForce, ForceMode2D.Impulse);
        facingRight = true;
        Debug.Log("Stapler: Applied hop force to the right.");
    }

    // Called by animation event to hop left
    public void ApplyHopForceLeft()
    {
        rb.AddForce(Vector2.left * hopForce, ForceMode2D.Impulse);
        facingRight = false;
        Debug.Log("Stapler: Applied hop force to the left.");
    }

    // Called by animation event to shoot a pin


public void ShootLeft()
{
    StartCoroutine(ShootBurst(pinSpawnPointLeft));
}

public void ShootRight()
{
    StartCoroutine(ShootBurst(pinSpawnPointRight));
}

private IEnumerator ShootBurst(Transform spawnPoint)
{
        shootCooldown = maxShootCooldown;
        isAttacking = false;
    if (pinPrefab == null || player == null || spawnPoint == null)
        {
            Debug.LogWarning("Stapler: Missing prefab, player, or spawn point!");
            yield break;
        }

    for (int i = 0; i < 3; i++)
    {
        Vector2 targetPos = new Vector2(player.position.x, player.position.y + aimHeightOffset);
        // Direction from spawn point to player
            Vector2 shootDir = (targetPos - (Vector2)spawnPoint.position).normalized;

        // Spawn pin
        GameObject pin = Instantiate(pinPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody2D pinRb = pin.GetComponent<Rigidbody2D>();

        if (pinRb != null)
        {
            pinRb.linearVelocity = shootDir * shootingSpeed;
        }

        Debug.Log($"Stapler: Burst shot {i+1}/3 from {spawnPoint.name}");

        // Delay before next shot
        yield return new WaitForSeconds(burstDelay);
    }
}


}
