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

    SpriteRenderer spriteRenderer;

    public float moveSpeed = 5f;
    float jumpSpeed = 7.5f;

    bool isJumping;

    private Rigidbody2D m_Rigidbody;

    public bool prevDynamicRigidBody;

    bool isGrounded;

    public bool performedThisFrame;

    public Transform groundCheck;
    public float groundCheckRadius = .5f;

    public Vector2 boxSize;
    public float castDistance;

    Vector2 previousPosition;
    int stuckCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        groundMask = LayerMask.GetMask("Ground");

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        m_Rigidbody = GetComponent<Rigidbody2D>();

        previousPosition = transform.position;
        stuckCount = 0;

        spriteRenderer = GetComponent<SpriteRenderer>();
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
        
        if(moveValue.x > 0.0f)
        {
            spriteRenderer.flipX = false;
        }
        else if(moveValue.x < 0.0f)
        {
            spriteRenderer.flipX = true;
        }


        if (!isJumping)
        {
            Vector2 velocity = new Vector2(moveValue.x * moveSpeed, 0.0f);

            
            float currentSlopeAngle = Math.Abs(Vector2.Angle(SlideResults.surfaceHit.normal, Vector2.up));

            if(currentSlopeAngle > SlideMovement.gravitySlipAngle)
            {
                // We are sliding
                velocity = Vector2.zero;

                if (previousPosition == (Vector2)transform.position)
                {
                    stuckCount++;

                    if (stuckCount > 1)
                    {
                        velocity = new Vector2(moveValue.x * moveSpeed, 0.0f);
                        stuckCount = 0;
                    }
                }
                else if (stuckCount > 0)
                {
                    stuckCount = 0;
                }
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

        previousPosition = transform.position;
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundMask);

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
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
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
