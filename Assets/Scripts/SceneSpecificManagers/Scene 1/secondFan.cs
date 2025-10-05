using UnityEngine;

public class secondFan : MonoBehaviour
{
    public Canvas canvas;
    void Start()
    {
        canvas.enabled = false;
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        canvas.enabled = true;
        if (collision.CompareTag("deadPaper"))
        {
            GetComponentInChildren<Animator>().enabled = false;
            GetComponentInChildren<triggerRespawn>().enable = false;
        }
    }
}
