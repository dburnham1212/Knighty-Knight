using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rigidbody2D;

public class PlayerController : MonoBehaviour
{
    // private members
    // Components
    SpriteRenderer spriteRenderer;
    Rigidbody2D m_Rigidbody;
    // InputActions
    InputAction moveAction;
    InputAction jumpAction;
    // Layers
    LayerMask groundMask;
    // ground and jump checks
    bool isGrounded;
    bool isJumping;
    bool jumpedThisFrame;
    // static RigidBody struct for sliding
    SlideResults SlideResults;
    // slide stick fix
    Vector2 previousPosition;
    int stuckCount;

    // public members
    // movement
    public float moveSpeed;
    public float jumpSpeed;
    // ground check
    public Transform groundCheck;
    public float groundCheckRadius;
    public Vector2 boxSize;
    public float castDistance;
    // static RigidBody struct for sliding
    public SlideMovement SlideMovement;

    // Unity Messages
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize required private members
        m_Rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        groundMask = LayerMask.GetMask("Ground");

        previousPosition = transform.position;
        stuckCount = 0;
    }

    // FixedUpdate is called once per fixed-frame
    void FixedUpdate()
    {
        // gets the input from moveAction
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        /*
         * Direction Grid
         * (-0.71,  0.71) (0.00,  1.00) (0.71,  0.71)
         * (-1.00,  0.00) (0.00,  0.00) (1.00,  0.00)
         * (-0.71, -0.71) (0.00, -1.00) (0.71, -0.71)
         */
        // wanna move this down, so it's only called if it's true
        jumpedThisFrame = false;

        CheckGround();
        FlipSprite(moveValue.x);

        if (jumpAction.WasPressedThisFrame() && !isJumping)
            Jump();

        HorizontalMovement(moveValue.x);

        previousPosition = transform.position;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isGrounded && !jumpedThisFrame)
        {
            if (isJumping)
                isJumping = false;

            m_Rigidbody.linearVelocity = Vector2.zero;
            m_Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            SlideMovement.useSimulationMove = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        print("Hit");

        if (collision.gameObject.tag == "Item")
            Destroy(collision.gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }

    // methods
    void CheckGround()
    {
        isGrounded = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundMask);

        if(!isGrounded)
            m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    void FlipSprite(float moveValueX)
    {
        if (moveValueX > 0.0f && spriteRenderer.flipX)
            spriteRenderer.flipX = false;
        else if (moveValueX < 0.0f && !spriteRenderer.flipX)
            spriteRenderer.flipX = true;
    }

    void Jump()
    {
        isJumping = true;
        m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        m_Rigidbody.linearVelocityY = jumpSpeed;
        jumpedThisFrame = true;
    }

    void HorizontalMovement(float moveValueX)
    {
        if (!isJumping)
        {
            Vector2 velocity = new Vector2(moveValueX * moveSpeed, 0.0f);


            float currentSlopeAngle = Math.Abs(Vector2.Angle(SlideResults.surfaceHit.normal, Vector2.up));

            if (currentSlopeAngle > SlideMovement.gravitySlipAngle)
            {
                // We are sliding
                velocity = Vector2.zero;

                if (previousPosition == (Vector2)transform.position)
                {
                    stuckCount++;

                    if (stuckCount > 1)
                    {
                        velocity = new Vector2(moveValueX * moveSpeed, 0.0f);
                        stuckCount = 0;
                    }
                }
                else if (stuckCount > 0)
                {
                    stuckCount = 0;
                }
            }

            SlideResults = m_Rigidbody.Slide(velocity, Time.deltaTime, SlideMovement);

            if (!SlideMovement.useSimulationMove)
            {
                SlideMovement.useSimulationMove = true;
            }
        }
        else
        {
            m_Rigidbody.linearVelocityX = moveValueX * moveSpeed;
        }
    }
}
