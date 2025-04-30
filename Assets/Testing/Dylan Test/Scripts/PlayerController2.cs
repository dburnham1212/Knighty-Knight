using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rigidbody2D;

public class PlayerController2 : MonoBehaviour
{
    InputAction moveAction;
    InputAction jumpAction;

    public Rigidbody2D.SlideMovement SlideMovement;
    public Rigidbody2D.SlideResults SlideResults;

    public float moveSpeed = 5f;
    float jumpSpeed = 5f;

    private Rigidbody2D m_Rigidbody;

    public LayerMask groundLayer;

    public bool wasGrounded;
    public bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius = .5f;

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

        Vector2 velocity = new Vector2(moveValue.x * moveSpeed, 0f);

        CheckGround();

        SlideResults = m_Rigidbody.Slide(velocity, Time.deltaTime, SlideMovement);

    }

    public void CheckGround()
    {
        wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hit");
        if(collision.gameObject.tag == "Item")
        {
            Destroy(collision.gameObject);
        }
    }
}
