using UnityEngine;

public class planeModeON : MonoBehaviour
{
    public Player player;

    void OnTriggerEnter2D(Collider2D collision)
    {
        player.planeMode = true;
    }
}
