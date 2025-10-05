using UnityEngine;

public class staplerPin : MonoBehaviour
{
    public float lifetime = 30f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Pin hit: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("Pin hit the player!");
            Destroy(gameObject, 0.5f);

            other.GetComponent<Player>().damagePlayer(1);

        }

        else if (!other.CompareTag("Stapler"))
        {
            Destroy(gameObject);
            
        }
    }
}
