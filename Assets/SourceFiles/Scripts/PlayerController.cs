using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rigidbody2D;

public class PlayerController : MonoBehaviour
{
    // public members
    // movement
    public float moveSpeed;
    public float jumpSpeed;
    // static RigidBody struct for sliding
    public SlideMovement slideMovement;

    // private members
    // Components
    SpriteRenderer spriteRenderer;
    Rigidbody2D m_Rigidbody;
    BoxCollider2D boxCollider;
    Animator animator;
    // InputActions
    InputAction moveAction;
    InputAction jumpAction;
    InputAction attackAction;
    // Layers
    LayerMask groundMask;
    LayerMask platformMask;
    LayerMask enemyMask; // just use water until enemy mask exists
    // state checks
    // ground / platform
    Vector2 boxSize;
    float castDistance;
    bool isGrounded;
    bool isOnPlatform;
    int layerWait;
    // jumping
    bool isJumping;
    bool jumpedThisFrame;
    // static RigidBody struct for sliding
    SlideResults slideResults;
    // slide stick fix
    Vector2 previousPosition;
    int stuckCount;
    // attack stuff
    Collider2D[] enemies;
    List<Collider2D> hitEnemies;
    Vector2 attackPosition;
    float attackRadius;
    int attackIterator;

    // properties
    public bool DropAction { get; set; }
    public Inventory Inventory { get; private set; }

    // Unity Messages
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // initialize required private members
        spriteRenderer = GetComponent<SpriteRenderer>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        attackAction = InputSystem.actions.FindAction("Attack");

        groundMask = LayerMask.GetMask("Ground");
        platformMask = LayerMask.GetMask("Platform");
        enemyMask = LayerMask.GetMask("Water");

        castDistance = 0.4f;
        boxSize = new Vector2(boxCollider.size.x * transform.localScale.x, castDistance / 2);
        layerWait = 0;

        previousPosition = transform.position;
        stuckCount = 0;

        // initialize properties
        DropAction = false;
        Inventory = new Inventory();

        m_Rigidbody.excludeLayers += platformMask;
        m_Rigidbody.excludeLayers += enemyMask;

        print(m_Rigidbody.excludeLayers.value);
    }

    // FixedUpdate is called once per fixed-frame
    private void FixedUpdate()
    {
        jumpedThisFrame = false;
        // gets the input from moveAction
        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        CheckGround();
        CheckPlatform();
        if (m_Rigidbody.linearVelocity.y < 0 && !isOnPlatform && layerWait == 0)
        {
            if (Physics2D.BoxCast(new Vector2(
            transform.position.x, transform.position.y - boxCollider.size.y * transform.localScale.y / 2 + castDistance / 2),
            boxSize, 0, -transform.up, castDistance * 1.5f, platformMask))
            {
                m_Rigidbody.excludeLayers -= platformMask;
                layerWait = 1;
            }
        }
        if (layerWait > 0 && layerWait < 20)
            layerWait++;
        else
            layerWait = 0;

        if (!animator.GetBool("isAttacking"))
        {
            FlipSprite(moveValue.x);

            if (jumpAction.WasPressedThisFrame() && !isJumping)
            {
                isJumping = true;

                if (isOnPlatform)
                {
                    m_Rigidbody.excludeLayers += platformMask;
                    layerWait = 1;
                }

                if (!isOnPlatform || moveValue.y >= 0)
                    Jump();
            }

            if (attackAction.WasPressedThisFrame())
                animator.SetBool("isAttacking", true);
        }

        HorizontalMovement(moveValue.x);

        previousPosition = transform.position;
    }

    private void Update()
    {
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isOnPlatform)
            if (isJumping)
                isJumping = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
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

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Platform") && !jumpAction.WasPressedThisFrame() && !isOnPlatform)
            m_Rigidbody.excludeLayers += platformMask;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            Item item = collision.gameObject.GetComponent<Item>();
            int index = (int)item.itemName;
            print(index);

            switch (item.type)
            {
                case Inventory.Type.Consumable:
                    index -= Inventory.KEY_ITEMS;
                    print(index);

                    if (index >= 0 && index < Inventory.CONSUMABLES)
                        Inventory.Consumables[index].Add(item.count);
                    else
                        print("type and name mismatch");
                    break;
                case Inventory.Type.Upgrade:
                    index -= Inventory.KEY_ITEMS;
                    print(index);

                    if (index >= 0 && index < Inventory.CONSUMABLES)
                        Inventory.Consumables[index].Upgrade();
                    else
                        print("type and name mismatch");
                    break;
                default:
                    if (index >= 0 && index < Inventory.KEY_ITEMS)
                        Inventory.KeyItems[index].Upgrade();
                    else
                        print("type and name mismatch");
                    break;
            }

            Destroy(collision.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (boxCollider != null)
            Gizmos.DrawWireCube(new Vector2 (transform.position.x, transform.position.y - boxCollider.size.y * transform.localScale.y / 2), boxSize);
        
        Gizmos.DrawWireSphere(attackPosition, attackRadius);
    }

    // methods
    void CheckGround()
    {
        isGrounded = Physics2D.BoxCast(new Vector2(transform.position.x,
            boxCollider.bounds.min.y + castDistance / 2),
            boxSize, 0, -transform.up, castDistance, groundMask);

        if (!isGrounded)
            m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    void CheckPlatform()
    {
        isOnPlatform = Physics2D.BoxCast(new Vector2(
            transform.position.x, transform.position.y - boxCollider.size.y * transform.localScale.y / 2 + castDistance / 2),
            boxSize, 0, -transform.up, castDistance, platformMask);
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
        m_Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        m_Rigidbody.linearVelocityY = jumpSpeed;
        jumpedThisFrame = true;
    }

    void HorizontalMovement(float moveValueX)
    {
        if (!isJumping && !isOnPlatform)
        {
            Vector2 velocity = new Vector2(moveValueX * moveSpeed, 0.0f);

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
                        velocity = new Vector2(moveValueX * moveSpeed, 0.0f);
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
            m_Rigidbody.linearVelocityX = moveValueX * moveSpeed;
    }

    // animation events
    public void AttackStart()
    {
        attackIterator = 0;
        hitEnemies = new List<Collider2D>();
    }

    public void Attack()
    {
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

        enemies = Physics2D.OverlapCircleAll(attackPosition, attackRadius, enemyMask);
        foreach (Collider2D enemy in enemies)
        {
            EnemyScript enemyScript = enemy.GetComponent<EnemyScript>();

            if (enemyScript.CanBeHit)
            {
                enemyScript.Health -= 3;

                if (enemyScript.Health > 0)
                {
                    hitEnemies.Add(enemy);
                    enemyScript.CanBeHit = false;
                }
                else
                    Destroy(enemy.gameObject);
            }
        }

        attackIterator++;
    }

    public void AttackEnd()
    {
        for (int index = 0; index < hitEnemies.Count; index++)
        {
            EnemyScript enemyScript = hitEnemies[index].GetComponent<EnemyScript>();
            enemyScript.CanBeHit = true;
            hitEnemies.Remove(hitEnemies[index]);
        }

        attackRadius = 0;
        animator.SetBool("isAttacking", false);
    }
}
