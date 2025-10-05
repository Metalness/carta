using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Player : MonoBehaviour
{

    public GameObject deathCanvas;
    public int totalLeftPlayerSacrifices = 6;
    public bool onRespawnScreen = false;
    public bool allowMovement = true;
    public bool allowJump = true;
    public bool loadingJump = false;
    public bool jumpLoaded = false;
    public bool isGrounded = false;
    public bool isFalling = false;
    public bool isDashing = false;
    public bool tapeUnlocked = false;
    public float playerHealth = 100f;

    public bool planeMode = false;

    public enum PlayerFormState
    {
        FlatPaper,
        Dashing,
        Plane,
        Accordian
    }
    public float paperCutDamage = 20f;

    [SerializeField] private float flatMoveForceMag = 1f;
    [SerializeField] public float jumpForceMag = 1f;
    [SerializeField] public float dashForceMag = 3f;
    [SerializeField] private int dashDuration = 3;
    public float groundCheckRadius = 1f;
    private int moveTime;
    private int jumpCooldown;
    [SerializeField] private int maxJumpCooldown = 600;

    private int dashCooldown = 100;
    [SerializeField] private int maxDashCooldown = 200;

    private Rigidbody2D playerRb;
    private Camera mainCam;
    private InputAction moveAction;


    [SerializeField] private playerAnimationLink playerAnimationLink;
    public papeCutScript paperCutScript;
    [SerializeField] private Transform pivot;

    PlayerInputActions inputActions;

    public float tapeStickDuration = 5f;

    public GameObject planeController;
    public GameObject planeSprite;


    // Animations


    void Start()
    {
        planeController.SetActive(false);
        playerRb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");
        mainCam = Camera.main;
        sacMenuCanvas.SetActive(false);
        deathCanvas.SetActive(false);

        setSacColor();
    }

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

        public float radius = 5f;

    void DamageNearbyStaplers(int damageAmount)
    {
        GameObject[] staplers = GameObject.FindGameObjectsWithTag("Stapler");

        foreach (GameObject stapler in staplers)
        {
            float distance = Vector2.Distance(transform.position, stapler.transform.position);
            if (distance <= radius)
            {
                // assumes stapler has a script with DealDamage(int amount)
                stapler.GetComponent<Stapler>().damagePlayer(5);
            }
        }
    }

    // optional: visualize the radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckRadius);
    
    }

    public void paperCut(InputAction.CallbackContext context)
    {
        DamageNearbyStaplers(15);

        if (SceneManager.GetActiveScene().buildIndex == 4)
        {
            GameObject.FindGameObjectWithTag("printerHead").GetComponent<printerHead>().paperCut();
        }

        //sac menu check
        if (sacMenuOpen)
        {
            sacMenuOpen = false;
            GameManager.Instance.resetTimeScale();
            sacMenuCanvas.SetActive(false);

            Vector2 mousePos = Mouse.current.position.ReadValue();
            //top left:- accordian, topright:-dash, bottom left:- paper cut, bottom right:- tape
            // get screen center
            float halfW = Screen.width / 2f;
            float halfH = Screen.height / 2f;

            // shift coordinates relative to center
            float dx = mousePos.x - halfW;
            float dy = mousePos.y - halfH;

            int quadrant = (dx >= 0 ? 1 : 0) + (dy >= 0 ? 2 : 0);

            switch (quadrant)
            {
                case 3:
                    Debug.Log("dash");
                    dashForceMag = dashForceMag - (dashForceMag / 10);
                    GameManager.Instance.changePlayerValue(0, dashForceMag);
                    break;
                case 2:
                    jumpForceMag = jumpForceMag - (jumpForceMag / 10);
                    GameManager.Instance.changePlayerValue(1, jumpForceMag);
                    Debug.Log("accordian");
                    break;
                case 0:
                    paperCutDamage = paperCutDamage - (paperCutDamage / 10);
                    GameManager.Instance.changePlayerValue(2, paperCutDamage);
                    Debug.Log("paper cut");
                    break;
                case 1:
                    if (tapeUnlocked)
                    {
                        tapeStickDuration = tapeStickDuration - (tapeStickDuration / 10);
                        GameManager.Instance.changePlayerValue(3, tapeStickDuration);

                        Debug.Log("tape");
                    }
                    break;
            }
            if (totalLeftPlayerSacrifices > 0)
            {
                if (!tapeUnlocked && quadrant == 1) { return; }
                totalLeftPlayerSacrifices -= 1;
                Debug.Log("what thee acutal fuck");
                Instantiate(deadPaperPrefab, position: this.transform.position, Quaternion.identity);
                setSacColor();

            }
            if (totalLeftPlayerSacrifices == 0)
            {
                //todo die
            }
            else { GameManager.Instance.updateSacrificePlayer(totalLeftPlayerSacrifices); }

            GameManager.Instance.SaveGame();
            return;

        }
        if (isDashing || loadingJump) { return; }
        paperCutScript.doCut();
    }


    private void setSacColor()
    {
        Color[] sacColors = new Color[] { sac1, sac2, sac3, sac4, sac5, sac6 };

        // Make sure value is in range 1-6
        if (totalLeftPlayerSacrifices < 1 || totalLeftPlayerSacrifices > 6)
            return;

        // Array is 0-indexed, so subtract 1
        Color newColor = sacColors[6 - totalLeftPlayerSacrifices];

        playerAnimationLink.GetComponent<SpriteRenderer>().color = newColor;
    }
    private void OnEnable()
    {
        // Assuming you already have your InputAction called "jump"
        inputActions.Player.Jump.performed += jumpPerformed;
        inputActions.Player.Jump.canceled += jumpReleased;
        inputActions.Player.RightDash.performed += rightDash;
        inputActions.Player.LeftDash.performed += leftDash;
        inputActions.Player.Interact.performed += paperCut;
        inputActions.Player.Sacrifice.performed += sacMenu;
        inputActions.Player.Enable();
    }
    public void killPlayer(string text)
    {
        onRespawnScreen = true;
        allowMovement = false;
        deathCanvas.SetActive(true);
        deathCanvas.GetComponentInChildren<TextMeshProUGUI>().text = text;
        GameManager.Instance.setTimeScale(0.1f);
    }

    private void OnDisable()
    {
        inputActions.Player.Jump.performed -= jumpPerformed;
        inputActions.Player.Jump.canceled -= jumpReleased;
        inputActions.Player.RightDash.performed -= rightDash;
        inputActions.Player.LeftDash.performed -= leftDash;
        inputActions.Player.Interact.performed -= paperCut;
        inputActions.Player.Sacrifice.performed -= sacMenu;

        inputActions.Player.Disable();
    }

    private void rightDash(InputAction.CallbackContext context)
    {
        if (allowMovement && dashCooldown == 0)
        {
            dashCooldown = maxDashCooldown;
            playerAnimationLink.animator.SetTrigger("dashRight");
            isDashing = true;
            StartCoroutine(ApplyDashAfterDelay(0.2f, Vector2.right * dashForceMag));

        }
    }
    private void leftDash(InputAction.CallbackContext context)
    {
        if (allowMovement && dashCooldown == 0)
        {
            dashCooldown = maxDashCooldown;
            playerAnimationLink.animator.SetTrigger("dashLeft");

            isDashing = true;
            StartCoroutine(ApplyDashAfterDelay(0.2f, Vector2.left * dashForceMag));

        }
    }

    public void applyImpulseForce(Vector2 force)
    {
        playerRb.AddForce(force, ForceMode2D.Impulse);
    }


    private IEnumerator ApplyDashAfterDelay(float delay, Vector2 force)
    {
        yield return new WaitForSeconds(delay);

        // apply force in the "up" direction
        playerRb.AddForce(force, ForceMode2D.Force);
    }
    private void jumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("what");
        if (onRespawnScreen)
        {
            transform.position = GameObject.FindGameObjectWithTag("Respawn").transform.position;
            onRespawnScreen = false;
            deathCanvas.SetActive(false);
            GameManager.Instance.resetTimeScale();
            allowMovement = true;
            return;
        }
        if (allowJump && isGrounded && context.duration > 0.4f && jumpCooldown == 0)
        {
            jumpCooldown = maxJumpCooldown;
            loadingJump = true;
            playerAnimationLink.animator.SetTrigger("loadJump");
            allowMovement = false;
        }
    }

    private void jumpReleased(InputAction.CallbackContext context)
    {
        if (!jumpLoaded) { return; }
        loadingJump = false;
        jumpLoaded = false;
        playerAnimationLink.animator.SetTrigger("JUMP");
        allowMovement = true;
        playerRb.AddForce(pivot.right * jumpForceMag, ForceMode2D.Impulse);
    }
    void Update()
    {
            if(sacMenuOpen){ mousePosSacText(); }
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        if (playerHealth <= 0)
        {
            killPlayer("oof");
        }
                if (planeMode)
        {
            planeController.SetActive(true);
            planeSprite.SetActive(false);
        }
    
        // if (!isGrounded && !isDashing && !jumpLoaded)
        // {
        //     playerAnimationLink.animator.SetTrigger("fallingStart");
        //     isFalling = true;
        // }

        // if (isGrounded && isFalling)
        // {
        //     isFalling = false;
        //     playerAnimationLink.animator.SetTrigger("fallingEnd");
        // }
        if (isDashing)
        {
            if (playerRb.linearVelocity.x > 0)
            {
                transform.Rotate(0f, 0f, -2f);
            }
            else
            {
                transform.Rotate(0f, 0f, 2f);
            }


            isDashing = false;
            playerAnimationLink.animator.SetTrigger("endDash");
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        jumpCooldown = Mathf.Max(0, jumpCooldown - 1);
        dashCooldown = Mathf.Max(0, dashCooldown - 1);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckRadius);

        isGrounded = hit.collider != null;

        if (isGrounded && (!loadingJump || !jumpLoaded))
        {
            pivot.rotation = Quaternion.Euler(0f, 0f, 0f);

        }

        if (jumpLoaded || loadingJump)
        {
            // get mouse position from Input System
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Debug.Log(mousePos);

            float angle = ((1 - (mousePos.x / Screen.width)) * 90) + 45;

            // apply rotation
            pivot.rotation = Quaternion.Euler(0f, 0f, angle);
            return;
        }

        moveTime = Mathf.Max(0, moveTime - 1);
        Vector2 moveValue = moveAction.ReadValue<Vector2>();


        if (moveValue.x > 0.5 && allowMovement && moveTime == 0)
        {
            moveTime = 15;
            SoundManager.PlaySound(SoundManager.SoundType.paperSliding);
            playerAnimationLink.animator.SetTrigger("walkForward");
        }


        if (moveValue.x < -0.5 && allowMovement && moveTime == 0)
        {
            moveTime = 15;
            SoundManager.PlaySound(SoundManager.SoundType.paperSliding);
            playerAnimationLink.animator.SetTrigger("walkBackward");
        }

    }

    public void pushPlayerPaperForward()
    {
        playerRb.AddForce(Vector2.right * flatMoveForceMag);
    }

    public void pushPlayerPaperBackward()
    {

        playerRb.AddForce(Vector2.left * flatMoveForceMag);
    }


    public void damagePlayer(float health = 1f)
    {
        playerHealth -= health;
    }



    //sacrifices code


    public GameObject deadPaperPrefab;
    public GameObject sacMenuCanvas;
    public bool sacMenuOpen = false;

    public GameObject DashText;
    public GameObject AccordianText;
    public GameObject PaperCutText;
    public GameObject TapeText;

    public Color sac1;
    public Color sac2;
    public Color sac3;
    public Color sac4;
    public Color sac5;
    public Color sac6;
    //sacrifice menu    
    void sacMenu(InputAction.CallbackContext context)
    {
        sacMenuOpen = true;
        GameManager.Instance.setTimeScale(0.05f);
        sacMenuCanvas.SetActive(true);




    }



    void mousePosSacText()
    {
                Vector2 mousePos = Mouse.current.position.ReadValue();
        //top left:- accordian, topright:-dash, bottom left:- paper cut, bottom right:- tape
        // get screen center
        float halfW = Screen.width / 2f;
        float halfH = Screen.height / 2f;

        // shift coordinates relative to center
        float dx = mousePos.x - halfW;
        float dy = mousePos.y - halfH;


        DashText.GetComponent<TextMeshProUGUI>().text = "Reduce dash speed by 10%";
        AccordianText.GetComponent<TextMeshProUGUI>().text = "Reduce jump by 10%";
        PaperCutText.GetComponent<TextMeshProUGUI>().text = "Reduce cut damage by 10%";
        TapeText.GetComponent<TextMeshProUGUI>().text = tapeUnlocked 
    ? "Reduce duration by 10%" 
    : "Locked";

        // Hide all first
        DashText.SetActive(false);
        AccordianText.SetActive(false);
        PaperCutText.SetActive(false);
        TapeText.SetActive(false);

        // Show one depending on quadrant
        if (dx >= 0 && dy >= 0)         // Top Right
            DashText.SetActive(true);
        else if (dx < 0 && dy >= 0)     // Top Left
            AccordianText.SetActive(true);
        else if (dx < 0 && dy < 0)      // Bottom Left
            PaperCutText.SetActive(true);
        else if (dx >= 0 && dy < 0)     // Bottom Right
            TapeText.SetActive(true);
    }
}
