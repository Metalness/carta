using UnityEngine;

public class papeCutScript : MonoBehaviour
{
    private Animator animator;


    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("endCut");

    }


    public void doCut()
    {
        animator.SetTrigger("paperCut");
    }

    public void endCut()
    {
        animator.SetTrigger("endCut");
    }
}
