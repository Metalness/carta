using UnityEngine;

public class triggerRespawn : MonoBehaviour
{
    public bool enable = true;
    public string deathMsg;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!enable || !collision.CompareTag("Player")){ return; }
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().killPlayer(deathMsg);
    }
}
