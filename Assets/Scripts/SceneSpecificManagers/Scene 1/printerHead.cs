using System.Net;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class printerHead : MonoBehaviour
{

    public float health = 100f;

    public Transform end1;
    public Transform end2;
    private Transform currentTarget;
    public Animator faceAnimator;
    public Canvas sacrificeText;
    public GameObject inkBlobPrefab;

    public int maxAttackPhase = 600;
    public int attackTimer= 600;

    public float moveSpeed;
    public bool playerInRange = false;
    private PlayerInputActions inputActions;


    public bool attacking = false;
    private Transform player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").gameObject.transform;
        sacrificeText.enabled = false;

    }


    public void paperCut()
    {
        Debug.Log("test");
        if (health == 0)
        {
            faceAnimator.SetTrigger("puke");
            sacrificeText.enabled = true;
            

        }
        if (playerInRange)
        {
            health -= player.GetComponent<Player>().paperCutDamage / 5f;
        }

        GameObject.FindGameObjectWithTag("cinemachine").GetComponent<CinemachineBasicMultiChannelPerlin>().FrequencyGain = 100f;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            
            // GameObject.FindGameObjectWithTag("cinemachine").GetComponent<CinemachineBasicMultiChannelPerlin>().FrequencyGain = 1f;

        }
        if (collision.CompareTag("deadPaper"))
        {
            GameManager.Instance.sceneChange(5);
        }
    }

        void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            playerInRange = false;
            // GameObject.FindGameObjectWithTag("cinemachine").GetComponent<CinemachineBasicMultiChannelPerlin>().FrequencyGain = 1f;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        attackTimer = Mathf.Max(0, attackTimer - 1);


        if (attackTimer == 0)
        {
            GameObject.FindGameObjectWithTag("cinemachine").GetComponent<CinemachineBasicMultiChannelPerlin>().FrequencyGain = 1f;

            attackTimer = maxAttackPhase;
            attacking = !attacking;
        }
        if (!attacking){
            
        }
        else // attacking
        {
            if (attackTimer % 100 == 0)
            {
                Instantiate(inkBlobPrefab, new Vector3(this.transform.position.x, this.transform.position.y -4f, this.transform.position.z), Quaternion.identity);
                
            }
            var player = GameObject.FindGameObjectWithTag("Player").transform;

            // target position (player.x clamped between end1 & end2)
            Vector3 targetPos = new Vector3(
                Mathf.Clamp(player.position.x, end1.position.x, end2.position.x),
                transform.position.y,
                transform.position.z
            );

            // move towards player smoothly
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed
            );
        }


    }
}
