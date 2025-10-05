using UnityEngine;
using System.Collections;

public class InkBlob : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float chargeRange = 2f;
    public float explodeDelay = 1f;
    public float explodeRadius = 1.5f;
    public int maxDamage = 3; // max damage if player is at center
    public float minDamageMultiplier = 0.3f; // how much damage at edge

    [SerializeField] private ParticleSystem explosionParticleSystem;

    private Animator animator;
    private Transform player;
    private bool isCharging = false;
    private bool isDead = false;

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

        transform.rotation = Quaternion.identity;

        float deltaX = player.position.x - transform.position.x;
        float distance = Vector2.Distance(player.position, transform.position);

        if (distance > chargeRange)
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
            isCharging = true;
            animator.SetTrigger(deltaX > 0 ? "ChargeRight" : "ChargeLeft");
            explosionParticleSystem.Play();
            Invoke(nameof(Explode), explodeDelay);
        }
    }

    private void Explode()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Die");
        explosionParticleSystem.gameObject.SetActive(true);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                float dist = Vector2.Distance(hit.transform.position, transform.position);
                float t = Mathf.Clamp01(dist / explodeRadius); // 0 = center, 1 = edge
                int damage = Mathf.RoundToInt(Mathf.Lerp(maxDamage, maxDamage * minDamageMultiplier, t));

                Debug.Log($"InkBlob: Player hit! Distance={dist:F2}, Damage={damage}");
                hit.GetComponent<Player>()?.damagePlayer(damage);
            }
        }

        StartCoroutine(DisableAfterTime(0.5f));
        Destroy(gameObject, 3f);
    }

    IEnumerator DisableAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        GetComponent<Renderer>().enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }
}
