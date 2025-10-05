using UnityEngine;

public class Plane_Mode_OFF : MonoBehaviour
{
    public Player player;

        public GameObject planeController;
    public GameObject planeSprite;

    void OnTriggerEnter2D(Collider2D collision)
    {
        player.planeMode = false; 
        planeController.SetActive(true);
        planeSprite.SetActive(true);
    }
}
