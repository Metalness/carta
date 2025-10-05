using UnityEngine;

public class SpawnOnTrigger2D : MonoBehaviour
{
    [Header("Prefabs (up to 5)")]
    public GameObject[] prefabs = new GameObject[5];
    
    [Header("Spawn Settings")]
    public int[] spawnCounts = new int[5]; // how many of each prefab to spawn
    public Transform spawnAreaCenter;
    public Vector2 spawnAreaSize = new Vector2(5f, 5f);

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return; // prevent multiple triggers
        if (!other.CompareTag("Player")) return;

        triggered = true;

        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i] == null) continue;

            for (int j = 0; j < spawnCounts[i]; j++)
            {
                Vector2 randomPos = (Vector2)spawnAreaCenter.position + 
                    new Vector2(Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2));

                Instantiate(prefabs[i], randomPos, Quaternion.identity);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (spawnAreaCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnAreaCenter.position, spawnAreaSize);
        }
    }
}
