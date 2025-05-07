using System;
using UnityEngine;
using static UnityEngine.Rigidbody2D;

public class EnemyScript : CharacterScript
{
    // private members
    // InputActions
    bool jumpAction;
    bool attackAction;
    // stuck on wall/ slope
    int walledCount;
    // attack stuff
    BoxCollider2D playerCollider;
    Vector2 attackPosition;
    float attackRadius;
    int attackIterator;

    // public members
    public GameObject player;
    public int maxHealth;

    // properties
    public int Health {  get; set; }
    public bool CanBeHit { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
        walledCount = 0;

        playerCollider = player.GetComponent<BoxCollider2D>();

        // initialize properties
        Health = maxHealth;
        CanBeHit = true;
    }

    // FixedUpdate is called once per fixed-frame
    void FixedUpdate()
    {
        UpdateStart();

        if (BoxCollider.bounds.max.x < playerCollider.bounds.min.x)
            MoveValue = Vector2.right;
        else if (BoxCollider.bounds.min.x > playerCollider.bounds.max.x)
            MoveValue = Vector2.left;

        CheckGround();
        CheckPlatform();

        if (!Animator.GetBool("isAttacking"))
        {
            FlipSprite();

            if (jumpAction && !IsJumping)
                Jump();

            attackAction = true;
            if (attackAction)
            {
                Animator.SetBool("isAttacking", true);
                attackAction = false;
            }
        }

        HorizontalMovement();

        if (MoveValue.x != 0 && PreviousPosition == (Vector2)transform.position)
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

        PreviousPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionEnter();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CollisionStay();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        CollisionExit(collision, jumpAction);
    }

    private void OnDrawGizmos()
    {
        DrawGizmos();

        Gizmos.DrawWireSphere(attackPosition, attackRadius);
    }

    // animation events
    public void AttackStart()
    {
        attackIterator = 0;
    }

    public void Attack()
    {
        float right, left, top, bottom;

        attackPosition.x = !SpriteRenderer.flipX ?
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

        if (right > playerCollider.bounds.min.x && top > playerCollider.bounds.min.y
            && left < playerCollider.bounds.max.x && bottom < playerCollider.bounds.max.y)
        {
            print("Hit!");
        }

        attackIterator++;
    }

    public void AttackEnd()
    {
        attackRadius = 0;
        Animator.SetBool("isAttacking", false);
    }
}
