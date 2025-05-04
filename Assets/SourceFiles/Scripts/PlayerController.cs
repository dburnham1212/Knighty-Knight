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

    // public members
    // movement
    public float moveSpeed;
    public float jumpSpeed;
    // ground check
    public Vector2 boxSize;
    public float castDistance;
    // static RigidBody struct for sliding
    public SlideMovement slideMovement;

    // properties
    public bool DropAction {  get; set; }
    public Inventory Inventory { get; private set; }

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
        platformMask = LayerMask.GetMask("Platform");

        previousPosition = transform.position;
        stuckCount = 0;

        // initialize properties
        DropAction = false;
        Inventory = new Inventory();
    }

    // FixedUpdate is called once per fixed-frame
    void FixedUpdate()
    {
        jumpedThisFrame = false;
        // gets the input from moveAction
        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        CheckGround();
        CheckPlatform();
        FlipSprite(moveValue.x);

        if (jumpAction.WasPressedThisFrame() && !isJumping)
            if (moveValue.y >= 0)
                Jump();
            else
                DropAction = true;

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
            slideMovement.useSimulationMove = false;
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isOnPlatform)
            if(isJumping)
                isJumping = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
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

    void CheckPlatform()
    {
        isOnPlatform = Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, platformMask);   
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
}
