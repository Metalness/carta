using UnityEngine;

public class triggerRespawn : MonoBehaviour
{
    public string deathMsg;
    void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().killPlayer(deathMsg);
    }
}
