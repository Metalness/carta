using UnityEngine;

public class InkPot : MonoBehaviour
{
    [Header("References")]
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    public GameObject inkBlobPrefab;

    [Header("Charge Settings")]
    public float chargeSpeed = 12f;
    public float chargeCooldown = 2f;
    public float overshootTime = 0.5f; // time to keep charging after crossing player
    private float overshootTimer = 0f;

    private bool crossedPlayer = false;
    public float walkSpeed = 2f;         // walking speed between charges
    private float cooldownTimer = 0f;

    [Header("Death Settings")]
    public string deathTrigger = "death";
    public Vector2 spawnAreaOffset = new Vector2(0.5f, 0.5f);

    private bool isCharging = false;
    private int direction = 1; // 1 = right, -1 = left
    private bool isDead = false;

    private float playerSideAtStart; 
    private Vector3 crossPoint; // where we crossed player

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDead) return;

        cooldownTimer -= Time.deltaTime;

        if (!isCharging && cooldownTimer <= 0f)
        {
            StartCharge();
        }

        if (isCharging)
        {
            rb.linearVelocity = new Vector2(direction * chargeSpeed, rb.linearVelocity.y);

            float relativeNow = transform.position.x - player.position.x;

            if (!crossedPlayer && Mathf.Sign(relativeNow) != Mathf.Sign(playerSideAtStart))
            {
                // just crossed the player
                crossedPlayer = true;
                overshootTimer = overshootTime;
            }

            if (crossedPlayer)
            {
                overshootTimer -= Time.deltaTime;
                if (overshootTimer <= 0f)
                {
                    EndCharge();
                }
            }
        }
        else if (!isCharging && !isDead)
        {
            // keep walking slowly between charges
            rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
        }
    }

    void StartCharge()
    {
        direction = (transform.position.x < player.position.x) ? 1 : -1;
        playerSideAtStart = transform.position.x - player.position.x;
        crossedPlayer = false;
        overshootTimer = 0f;
        animator.SetTrigger(direction == 1 ? "chargeRight" : "chargeLeft");
        isCharging = true;
    }

    void EndCharge()
    {
        isCharging = false;
        cooldownTimer = chargeCooldown;

        // transition into walking
        animator.SetTrigger(direction == 1 ? "idleWalkRight" : "idleWalkLeft");
        rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            Die();
        }
    }

void Die()
{
    isDead = true;
    isCharging = false;
    rb.linearVelocity = Vector2.zero;

    animator.SetTrigger(deathTrigger);

    int count = Random.Range(7, 16);
    for (int i = 0; i < count; i++)
    {
        Vector3 spawnPos = transform.position +
                           new Vector3(Random.Range(-spawnAreaOffset.x, spawnAreaOffset.x),
                                       Random.Range(-spawnAreaOffset.y, spawnAreaOffset.y),
                                       0f);

        GameObject blob = Instantiate(inkBlobPrefab, spawnPos, Quaternion.identity);

        // Apply a random velocity to "explode" outward
        Rigidbody2D blobRb = blob.GetComponent<Rigidbody2D>();
        if (blobRb != null)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(4f, 8f); // tweak for desired explosion speed
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            blobRb.linearVelocity = dir * speed;
        }

        // If blob has particle system, trigger it
        ParticleSystem ps = blob.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }
    }

    Destroy(gameObject, 2f);
}
}
