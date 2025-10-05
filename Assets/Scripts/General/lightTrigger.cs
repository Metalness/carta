using UnityEngine;

public class lightTrigger : MonoBehaviour
{
    public Animator animator;


    void Start()
    {
        animator.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        animator.enabled = true;
    }
}
