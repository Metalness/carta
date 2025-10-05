using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 2f;
    public float jumpForce = 5f;
    public bool planeMode = true;
    public float rotationSpeed = 5f;
    public float maxUpAngle = 35f;
    public float maxDownAngle = -80f;

    [Header("References")]
    public Rigidbody2D rb;
    public GameObject playerSprite;

    private PlayerInputActions inputActions;
    private float targetRotationZ;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += jumpPerformed;
    }

    void OnDisable()
    {
        inputActions.Player.Jump.performed -= jumpPerformed;
        inputActions.Player.Disable();
    }

    void FixedUpdate()
    {
        if (!planeMode)
        {
            playerSprite.SetActive(true);
            gameObject.SetActive(false);
            return;
        }

        // constant forward movement
        rb.linearVelocity = new Vector2(forwardSpeed, rb.linearVelocity.y);

        // calculate tilt based on vertical velocity
        float tilt = Mathf.Lerp(maxDownAngle, maxUpAngle, (-rb.linearVelocity.y + 5f) / 10f);
        tilt = Mathf.Clamp(tilt, maxDownAngle, maxUpAngle);

        // smoothly rotate towards tilt angle
        targetRotationZ = Mathf.LerpAngle(targetRotationZ, tilt, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(targetRotationZ);
    }

    private void jumpPerformed(InputAction.CallbackContext context)
    {
        // reset vertical velocity and jump
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
