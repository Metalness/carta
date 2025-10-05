using UnityEngine;
using UnityEngine.InputSystem;


public class fanThingScene1_1 : MonoBehaviour
{
    public Canvas canvas;

    public GameObject uncutWire;
    public GameObject cutWire;
    public Animator fan;
    private PlayerInputActions inputActions;
    private int pressed = 0;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canvas.enabled = true;
        }
    }


    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canvas.enabled = false;
        }
    }

    private void OnEnable()
    {
        // Assuming you already have your InputAction called "jump"
        inputActions.Player.Interact.performed += CutWire;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= CutWire;
        inputActions.Player.Disable();
    }
    void Awake()
    {
        inputActions = new PlayerInputActions();

    }
    void OnTriggerStay2D(Collider2D collision)
    {
        if (pressed == 1)
        {
            uncutWire.SetActive(false);
            cutWire.SetActive(true);
            fan.enabled = false;
        }
    }
    public void CutWire(InputAction.CallbackContext context)
    {
        pressed = 5;

    }


    void FixedUpdate()
    {
        pressed = Mathf.Max(0, pressed - 1);
    }

}
