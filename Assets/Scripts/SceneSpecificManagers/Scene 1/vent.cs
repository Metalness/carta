using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class vent : MonoBehaviour
{

    [SerializeField] private BoxCollider2D dropCollider;

    private PlayerInputActions inputActions;
    private int pressed = 0;


    private Canvas canvas;
    void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        canvas.enabled = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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


    void Awake()
    {
        inputActions = new PlayerInputActions();
        dropCollider.enabled = true;

    }

    private void OnEnable()
    {
        inputActions.Player.Interact.performed += enterVent;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= enterVent;
        inputActions.Player.Disable();
    }


    public void enterVent(InputAction.CallbackContext context)
    {
        pressed = 5;


    }


    void OnTriggerStay2D(Collider2D collision)
    {
        if (pressed == 1)
        {
            GameManager.Instance.sceneChange(2);
        }
    }

    void FixedUpdate()
    {
        pressed = Mathf.Max(0, pressed - 1);
    }


}
