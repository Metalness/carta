using UnityEngine;

public class Scissor : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float minImpulseForce = 8f;
    [SerializeField] private float maxImpulseForce = 15f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float minChangeInterval = 0.8f;
    [SerializeField] private float maxChangeInterval = 2.5f;
    [SerializeField] private float verticalBias = 0.7f;
    
    [Header("Attack Phase Settings")]
    [SerializeField] private float attackCooldown = 5f;
    [SerializeField] private float attackSpeed = 20f;
    [SerializeField] private float attackDuration = 1.5f;
    [SerializeField] private float attackRotationSpeed = 360f;
    
    [Header("Collision Settings")]
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float bounceForce = 5f;
    
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;
    
    private Rigidbody2D rb;
    private float changeDirectionTimer = 0f;
    private float currentChangeInterval;
    private float attackTimer = 0f;
    private bool isAttacking = false;
    private float attackPhaseTimer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (cam == null)
            cam = Camera.main;
        
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        ApplyRandomDiagonalImpulse();
        SetRandomChangeInterval();
        attackTimer = attackCooldown;
    }
    
    void Update()
    {
        if (isAttacking)
        {
            // Attack phase behavior
            transform.Rotate(0f, 0f, attackRotationSpeed * Time.deltaTime);
            
            attackPhaseTimer -= Time.deltaTime;
            if (attackPhaseTimer <= 0f)
            {
                // End attack phase
                isAttacking = false;
                attackTimer = attackCooldown;
                ApplyRandomDiagonalImpulse();
                SetRandomChangeInterval();
            }
        }
        else
        {
            // Normal chaotic flying
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            
            changeDirectionTimer -= Time.deltaTime;
            if (changeDirectionTimer <= 0f)
            {
                ApplyRandomDiagonalImpulse();
                SetRandomChangeInterval();
            }
            
            // Attack cooldown
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f && player != null)
            {
                StartAttackPhase();
            }
        }
        
        WrapHorizontally();
    }
    
    void StartAttackPhase()
    {
        isAttacking = true;
        attackPhaseTimer = attackDuration;
        
        // Clear current velocity and rush toward player
        if (rb != null && player != null)
        {
            rb.linearVelocity = Vector2.zero;
            
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            rb.AddForce(directionToPlayer * attackSpeed, ForceMode2D.Impulse);
        }
    }
    
    void SetRandomChangeInterval()
    {
        currentChangeInterval = Random.Range(minChangeInterval, maxChangeInterval);
        changeDirectionTimer = currentChangeInterval;
    }
    
    void ApplyRandomDiagonalImpulse()
    {
        if (rb == null) return;
        
        float horizontalDir = Random.Range(-1f, 1f);
        float verticalDir = Random.Range(-1f, 1f);
        
        verticalDir *= (1f + verticalBias);
        
        Vector2 direction = new Vector2(horizontalDir, verticalDir).normalized;
        
        // AVOID player in non-attacking state
        if (player != null)
        {
            Vector2 toPlayer = (player.position - transform.position).normalized;
            // Push direction away from player
            direction -= toPlayer * 0.6f;
            direction.Normalize();
        }
        
        float randomForce = Random.Range(minImpulseForce, maxImpulseForce);
        
        rb.AddForce(direction * randomForce, ForceMode2D.Impulse);
    }
    
    void WrapHorizontally()
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        bool wrapped = false;
        
        if (viewportPos.x < -0.1f)
        {
            viewportPos.x = 1.1f;
            wrapped = true;
        }
        else if (viewportPos.x > 1.1f)
        {
            viewportPos.x = -0.1f;
            wrapped = true;
        }
        
        if (wrapped)
        {
            float screenZ = Mathf.Abs(cam.transform.position.z - transform.position.z);
            Vector3 newWorldPos = cam.ViewportToWorldPoint(new Vector3(viewportPos.x, viewportPos.y, screenZ));
            newWorldPos.z = transform.position.z;
            transform.position = newWorldPos;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(damageAmount);
            // }
            
            // Bounce off player
            Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;
            bounceDirection.y += Random.Range(0.3f, 0.7f);
            bounceDirection.Normalize();
            
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
            }
            
            // If attacking, end attack phase
            if (isAttacking)
            {
                isAttacking = false;
                attackTimer = attackCooldown;
            }
        }
        else
        {
            // Bounce off walls instead of getting stuck
            Vector2 bounceDirection = Vector2.Reflect(rb.linearVelocity.normalized, collision.contacts[0].normal);
            bounceDirection.y += Random.Range(0.2f, 0.5f);
            bounceDirection.Normalize();
            
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
            }
        }
    }
}