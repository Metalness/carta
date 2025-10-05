using UnityEngine;
using System.Collections;
using Unity.Mathematics;
public class InkBlob : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float chargeRange = 2f;
    public float explodeDelay = 1f; // time to explode after charge
        public float explodeRadius = 1.5f;

    public int damage = 1;

    [SerializeField] private ParticleSystem explosionParticleSystem;

    private Animator animator;
    private Transform player;
    private bool isCharging = false;
    private bool isDead = false;
    void Awake()
    {
        // explosionParticleSystem.gameObject.SetActive(false);
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
            Debug.LogWarning("InkBlob: Player not found!");
    }

    void Update()
    {
        if (player == null || isCharging || isDead) return;

        transform.rotation = Quaternion.Euler(0, 0, 0);

        float deltaX = player.position.x - transform.position.x;
        float distance = Vector2.Distance(player.position, transform.position);

        // Move toward player if not in charge range
        if (distance > chargeRange )
        {

            if (deltaX > 0)
                animator.SetTrigger("RunRight");
            else
                animator.SetTrigger("RunLeft");

            Vector2 moveDir = deltaX > 0 ? Vector2.right : Vector2.left;
            transform.Translate(moveDir * moveSpeed * Time.deltaTime);
        }
        else
        {
            // Start charging
            isCharging = true;
            if (deltaX > 0)
                animator.SetTrigger("ChargeRight");
            else
                animator.SetTrigger("ChargeLeft");

            Debug.Log($"InkBlob: Charging {(deltaX > 0 ? "Right" : "Left")}");
            explosionParticleSystem.Play();
            Invoke(nameof(Explode), explodeDelay);
        }
    }

    private void Explode()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Die");
        Debug.Log("InkBlob: Exploding!");

        // Damage player if in range (example)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // hit.GetComponent<PlayerHealth>()?.TakeDamage(damage);
                Debug.Log("InkBlob: Hit player with ink!");
            }
        }
        explosionParticleSystem.gameObject.SetActive(true);
        StartCoroutine(DisableAfterTime(0.5f));// destroy after explosion animation
        Destroy(gameObject, 3f);
        
    }
    
    IEnumerator DisableAfterTime(float t)
{
    yield return new WaitForSeconds(t);
    gameObject.GetComponent<Renderer>().enabled = false;
}

    private void OnDrawGizmosSelected()
    {
        // visualize explosion range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }
}
