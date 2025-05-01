using System;
using Unity.VisualScripting;
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

    LayerMask groundMask;

    public float moveSpeed = 5f;
    float jumpSpeed = 7.5f;

    bool isJumping;

    private Rigidbody2D m_Rigidbody;

    public bool prevDynamicRigidBody;

    bool isGrounded;

    public bool performedThisFrame;

    public Transform groundCheck;
    public float groundCheckRadius = .5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        groundMask = LayerMask.GetMask("Ground");

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Calculate the horizontal velocity from keyboard input.
        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        performedThisFrame = false;

        CheckGround();

        if(jumpAction.WasPressedThisFrame() && !isJumping)
        {
            isJumping = true;
            m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
            m_Rigidbody.linearVelocityY = jumpSpeed;
            performedThisFrame = true;
        }

        if (!isJumping)
        {
            Vector2 velocity = new Vector2(moveValue.x * moveSpeed, 0.0f);

            float currentSlopeAngle = Math.Abs(Vector2.Angle(SlideResults.surfaceHit.normal, Vector2.up));

            if(currentSlopeAngle > SlideMovement.gravitySlipAngle)
            {
                // We are sliding
                velocity = Vector2.zero;
            }

            SlideResults = m_Rigidbody.Slide(velocity, Time.deltaTime, SlideMovement);

            if(!SlideMovement.useSimulationMove)
            {
                SlideMovement.useSimulationMove = true;
            }
        }
        else
        {
            m_Rigidbody.linearVelocityX = moveValue.x * moveSpeed;
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        if(!isGrounded)
        {
            m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isGrounded && !performedThisFrame)
        {
            if (isJumping)
            {
                isJumping = false;
            }
            m_Rigidbody.linearVelocity = Vector2.zero;
            m_Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            SlideMovement.useSimulationMove = false;
        }
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
