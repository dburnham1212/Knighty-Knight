using System;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rigidbody2D;

public class EnemyScript : MonoBehaviour
{
    // private members
    // Components
    SpriteRenderer spriteRenderer;
    Rigidbody2D m_Rigidbody;
    BoxCollider2D m_BoxCollider;
    Animator animator;
    // InputActions
    Vector2 moveAction;
    bool jumpAction;
    bool attackAction;
    // Layers
    LayerMask groundMask;
    LayerMask platformMask;
    // ground and jump checks
    bool isGrounded;
    bool isJumping;
    bool jumpedThisFrame;
    // platform checks
    bool isOnPlatform;
    // static RigidBody struct for sliding
    SlideResults slideResults;
    // slide stick fix
    Vector2 previousPosition;
    int stuckCount;
    // stuck on wall/ slope
    int walledCount;
    // attack stuff
    BoxCollider2D playerCollider;
    Vector2 attackPosition;
    float attackRadius;
    int attackIterator;
    float playerRight;
    float playerLeft;
    float playerTop;
    float playerBottom;

    // public members
    public GameObject player;
    // movement
    public int maxHealth;
    public float moveSpeed;
    public float jumpSpeed;
    // ground check
    public Vector2 boxSize;
    public float castDistance;
    // static RigidBody struct for sliding
    public SlideMovement slideMovement;

    // properties
    public int Health {  get; set; }
    public bool CanBeHit { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize required private members
        spriteRenderer = GetComponent<SpriteRenderer>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_BoxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        groundMask = LayerMask.GetMask("Ground");
        platformMask = LayerMask.GetMask("Platform");

        previousPosition = Vector2.zero;
        stuckCount = 0;
        walledCount = 0;

        playerCollider = player.GetComponent<BoxCollider2D>();

        // initialize properties
        Health = maxHealth;
        CanBeHit = true;
    }

    // FixedUpdate is called once per fixed-frame
    void FixedUpdate()
    {
        moveAction = Vector2.zero;

        playerRight = playerCollider.transform.position.x + player.transform.localScale.x / 2 * playerCollider.size.x;
        playerLeft = playerCollider.transform.position.x - player.transform.localScale.x / 2 * playerCollider.size.x;
        playerTop = playerCollider.transform.position.y + player.transform.localScale.y / 2 * playerCollider.size.y;
        playerBottom = playerCollider.transform.position.y - player.transform.localScale.y / 2 * playerCollider.size.y;
        
        jumpedThisFrame = false;

        if (m_BoxCollider.transform.position.x + transform.localScale.x / 2 * m_BoxCollider.size.x < playerLeft)
            moveAction.x = 1;
        else if (m_BoxCollider.transform.position.x - transform.localScale.x / 2 * m_BoxCollider.size.x > playerRight)
            moveAction.x = -1;

        CheckGround();
        CheckPlatform();

        if (!animator.GetBool("isAttacking"))
        {
            FlipSprite();

            if (jumpAction && !isJumping)
            {
                Jump();
            }
            attackAction = true;
            if (attackAction)
            {
                animator.SetBool("isAttacking", true);
                attackAction = false;
            }
        }

        HorizontalMovement();

        if (moveAction.x != 0 && previousPosition == (Vector2)transform.position)
        {
            walledCount++;

            if (walledCount > 5)
            {
                jumpAction = true;
                walledCount = 0;
            }
        }
        else if (walledCount > 0)
            walledCount = 0;

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
            slideMovement.useSimulationMove = false;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isOnPlatform)
            if (isJumping)
                isJumping = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
        Gizmos.DrawWireSphere(attackPosition, attackRadius);
    }

    // methods
    void CheckGround()
    {
        isGrounded = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundMask);

        if (!isGrounded)
            m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    void CheckPlatform()
    {
        isOnPlatform = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, platformMask);
    }

    void FlipSprite()
    {
        if (moveAction.x > 0.0f && spriteRenderer.flipX)
            spriteRenderer.flipX = false;
        else if (moveAction.x < 0.0f && !spriteRenderer.flipX)
            spriteRenderer.flipX = true;
    }

    void Jump()
    {
        isJumping = true;
        m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        m_Rigidbody.linearVelocityY = jumpSpeed;
        jumpedThisFrame = true;
        jumpAction = false;
    }

    void HorizontalMovement()
    {
        if (!isJumping && !isOnPlatform)
        {
            Vector2 velocity = new Vector2(moveAction.x * moveSpeed, 0.0f);

            float currentSlopeAngle = Math.Abs(Vector2.Angle(slideResults.surfaceHit.normal, Vector2.up));

            if (currentSlopeAngle > slideMovement.gravitySlipAngle)
            {
                // We are sliding
                velocity = Vector2.zero;

                if (previousPosition == (Vector2)transform.position)
                {
                    stuckCount++;

                    if (stuckCount > 1)
                    {
                        velocity = new Vector2(moveAction.x * moveSpeed, 0.0f);
                        stuckCount = 0;
                    }
                }
                else if (stuckCount > 0)
                    stuckCount = 0;
            }

            slideResults = m_Rigidbody.Slide(velocity, Time.deltaTime, slideMovement);

            if (!slideMovement.useSimulationMove)
                slideMovement.useSimulationMove = true;
        }
        else
            m_Rigidbody.linearVelocityX = moveAction.x * moveSpeed;
    }

    // animation events
    public void AttackStart()
    {
        attackIterator = 0;
    }

    public void Attack()
    {
        float right, left, top, bottom;

        attackPosition.x = !spriteRenderer.flipX ?
            transform.position.x + transform.localScale.x / 2 :
            transform.position.x - transform.localScale.x / 2;

        switch (attackIterator)
        {
            case 1:
                attackPosition.y = transform.position.y + transform.localScale.y / 2 * 4 / 6;
                break;
            case 2:
                attackPosition.y = transform.position.y - transform.localScale.y / 2 * 2 / 6;
                break;
            case 3:
                attackPosition.y = transform.position.y - transform.localScale.y / 2 * 4 / 6;
                break;
            default:
                attackRadius = 0.25f;
                attackPosition.y = transform.position.y + transform.localScale.y / 2 * 4 / 6;
                break;
        }

        right = attackPosition.x + attackRadius;
        left = attackPosition.x - attackRadius;
        top = attackPosition.y + attackRadius;
        bottom = attackPosition.y - attackRadius;

        if (right > playerLeft && left < playerRight
            && top > playerBottom && bottom < playerTop)
        {
            print("Hit!");
        }

        attackIterator++;
    }

    public void AttackEnd()
    {
        attackRadius = 0;
        animator.SetBool("isAttacking", false);
    }
}
