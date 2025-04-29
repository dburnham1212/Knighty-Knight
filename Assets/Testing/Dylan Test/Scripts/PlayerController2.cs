using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rigidbody2D;

public class PlayerController2 : MonoBehaviour
{
    InputAction moveAction;
    InputAction jumpAction;

    public Rigidbody2D.SlideMovement SlideMovement = new Rigidbody2D.SlideMovement();
    public Rigidbody2D.SlideResults SlideResults;

    public float HorizontalSpeed = 5f;

    private Rigidbody2D m_Rigidbody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");


        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Calculate the horizontal velocity from keyboard input.
        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        var velocity = new Vector2(moveValue.x * HorizontalSpeed, 0f);

        // Slide the rigidbody.
        SlideResults = m_Rigidbody.Slide(velocity, Time.deltaTime, SlideMovement);
    }
}
