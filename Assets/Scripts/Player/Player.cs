using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using UnityEngine.Events;

using UnityEngine.SceneManagement;


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
    public float maxHealth = 100f;
    public TextMeshProUGUI HealthText;

    public bool planeMode = false;
    //i dont think we used player form state
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
    public float paperCutVerticalForce;
    [SerializeField] private Transform pivot;

    PlayerInputActions inputActions;

    public float tapeStickDuration = 5f;

    public GameObject planeController;
    public GameObject planeSprite;

    public UnityEvent shake;

    public AudioReverbZone audioReverbZone;



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
        SoundManager.PlaySound(SoundManager.SoundType.paperCut);
        playerRb.AddForceY(paperCutVerticalForce, ForceMode2D.Impulse);
        shake.Invoke();
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
                GameObject deadPaper = Instantiate(deadPaperPrefab, position: this.transform.position, Quaternion.identity);
                deadPaper.GetComponent<SpriteRenderer>().sprite = playerAnimationLink.GetComponent<SpriteRenderer>().sprite;
                setSacColor();

            }
            if (totalLeftPlayerSacrifices == 0)
            {
                killPlayer("too many sacrifices.");
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

        if (totalLeftPlayerSacrifices < 1 || totalLeftPlayerSacrifices > 6)
            return;

        Color newColor = sacColors[6 - totalLeftPlayerSacrifices];

        playerAnimationLink.GetComponent<SpriteRenderer>().color = newColor;
    }
    private void OnEnable()
    {
        inputActions.Player.Jump.performed += jumpPerformed;
        inputActions.Player.Jump.canceled += jumpReleased;
        inputActions.Player.RightDash.performed += rightDash;
        inputActions.Player.LeftDash.performed += leftDash;
        inputActions.Player.Interact.performed += paperCut;
        inputActions.Player.Sacrifice.performed += sacMenu;
        inputActions.Player.Enable();
    }
    private int prevRoom;
    private float prevDiffusion;
    public void killPlayer(string text)
    {
        prevRoom = audioReverbZone.room;
        prevDiffusion = audioReverbZone.diffusion;

        audioReverbZone.room = -300;
        audioReverbZone.diffusion = 15;

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
            SoundManager.PlaySound(SoundManager.SoundType.paperDash);

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
            SoundManager.PlaySound(SoundManager.SoundType.paperDash);


        }
    }

    public void applyImpulseForce(Vector2 force)
    {
        playerRb.AddForce(force, ForceMode2D.Impulse);
    }


    private IEnumerator ApplyDashAfterDelay(float delay, Vector2 force)
    {
        yield return new WaitForSeconds(delay);
        playerRb.AddForce(force, ForceMode2D.Force);
    }
    private void jumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("what");
        if (onRespawnScreen)
        {
            audioReverbZone.room = prevRoom;
            audioReverbZone.diffusion = prevDiffusion;
            if (totalLeftPlayerSacrifices == 0)
            {
                GameManager.Instance.sceneChange(0);
                GameManager.Instance.EraseSave();
            }
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

    void UpdateHealthText()
    {
        string conditionText = GetPaperCondition(playerHealth, maxHealth);
        HealthText.text = conditionText + " condition";
    }

    string GetPaperCondition(float currentHealth, float maxHealth)
    {
        float healthPercentage = (currentHealth / maxHealth) * 100f;

        if (healthPercentage >= 100f)
            return "Pristine";
        else if (healthPercentage >= 90f)
            return "Mint";
        else if (healthPercentage >= 75f)
            return "Near Mint";
        else if (healthPercentage >= 60f)
            return "Fine";
        else if (healthPercentage >= 45f)
            return "Good";
        else if (healthPercentage >= 30f)
            return "Fair";
        else if (healthPercentage >= 15f)
            return "Poor";
        else if (healthPercentage >= 5f)
            return "Damaged";
        else
            return "Destroyed";
    }
    void Update()
    {
        if (sacMenuOpen) { mousePosSacText(); }
        UpdateHealthText();

    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        if (playerHealth <= 0)
        {
            killPlayer("destroyed.");
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
            SoundManager.PlaySound(SoundManager.SoundType.paperSliding, 0.5f);
            playerAnimationLink.animator.SetTrigger("walkForward");
        }


        if (moveValue.x < -0.5 && allowMovement && moveTime == 0)
        {
            moveTime = 15;
            SoundManager.PlaySound(SoundManager.SoundType.paperSliding, 0.5f);
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
    public GameObject GSMText;

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
        float halfW = Screen.width / 2f;
        float halfH = Screen.height / 2f;

        float dx = mousePos.x - halfW;
        float dy = mousePos.y - halfH;


        DashText.GetComponent<TextMeshProUGUI>().text = "Reduce dash speed by 10%";
        AccordianText.GetComponent<TextMeshProUGUI>().text = "Reduce jump by 10%";
        PaperCutText.GetComponent<TextMeshProUGUI>().text = "Reduce cut damage by 10%";
        GSMText.GetComponent<TextMeshProUGUI>().text = (100 - (10 * (6 - totalLeftPlayerSacrifices))).ToString() + " GSM";
        TapeText.GetComponent<TextMeshProUGUI>().text = tapeUnlocked
    ? "Reduce duration by 10%"
    : "Locked";

        DashText.SetActive(false);
        AccordianText.SetActive(false);
        PaperCutText.SetActive(false);
        TapeText.SetActive(false);

        // show one depending on quadrant
        if (dx >= 0 && dy >= 0)         // top right
            DashText.SetActive(true);
        else if (dx < 0 && dy >= 0)     // top left
            AccordianText.SetActive(true);
        else if (dx < 0 && dy < 0)      // bottom left
            PaperCutText.SetActive(true);
        else if (dx >= 0 && dy < 0)     // bottom right
            TapeText.SetActive(true);
    }
}
