using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class playerAnimationLink : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private ParticleSystem smokeTrail;
    public Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void walkingPushEventForward()
    {
        player.pushPlayerPaperForward();
        smokeTrail.Play();

    }

    void walkingPushEventBackward()
    {
        player.pushPlayerPaperBackward();
        smokeTrail.Play();


    }

    void jumpLoaded()
    {
        player.jumpLoaded = true;
        player.allowMovement = false;
        player.loadingJump = false;
    }


    void pushLeft()
    {
        player.applyImpulseForce(Vector2.left * 10f);
    }

    void pushRight()
    {
        player.applyImpulseForce(Vector2.right * 10f);
    }
}
