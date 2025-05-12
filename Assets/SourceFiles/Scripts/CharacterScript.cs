using System;
using UnityEngine;
using static UnityEngine.Rigidbody2D;

public class CharacterScript : MonoBehaviour
{
    // public members
    // movement
    public float moveSpeed;
    public float jumpSpeed;
    // static RigidBody struct for sliding
    public SlideMovement slideMovement;

    Vector2 OldPosition;

    // properties
    // private set accessor
    // Components
    public SpriteRenderer SpriteRenderer {  get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public BoxCollider2D BoxCollider { get; private set; }
    public Animator Animator { get; private set; }
    // Layers
    public LayerMask GroundMask { get; private set; }
    public LayerMask PlatformMask { get; private set; }
    public LayerMask EnemyMask { get; private set; }
    // ground / platform
    public Vector2 BoxSize { get; private set; }
    public float CastDistance { get; private set; }
    // static RigidBody struct for sliding
    public SlideResults SlideResults { get; private set; }
    // public set accessor
    // state checks
    // movement
    public Vector2 MoveValue { get; set; }
    // ground / platform
    public bool WasGrounded { get; set; }
    public bool IsGrounded { get; set; }
    public int StuckCount { get; set; }
    // platform
    public bool IsOnPlatform { get; set; }
    public int LayerWait { get; set; }
    // jumping
    public bool IsJumping { get; set; }
    public bool JumpedThisFrame { get; set; }
    // position compare
    public Vector2 PreviousPosition { get; set; }

    float CurrentSlopeAngle { get; set; }

    // methods
    public void Initialize()
    {
        // initialize required private members
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Rigidbody = GetComponent<Rigidbody2D>();
        BoxCollider = GetComponent<BoxCollider2D>();
        Animator = GetComponent<Animator>();

        GroundMask = LayerMask.GetMask("Ground");
        PlatformMask = LayerMask.GetMask("Platform");
        EnemyMask = LayerMask.GetMask("Water"); // water til an enemy mask is added

        Rigidbody.excludeLayers += PlatformMask;
        Rigidbody.excludeLayers += EnemyMask;

        CastDistance = 0.4f;
        BoxSize = new Vector2(BoxCollider.size.x * transform.localScale.x, CastDistance / 2);
        LayerWait = 0;

        PreviousPosition = transform.position;
        StuckCount = 0;
    }

    public void UpdateStart()
    {
        JumpedThisFrame = false;

        if (LayerWait > 0 && LayerWait < 20)
            LayerWait++;
        else
            LayerWait = 0;
    }

    public bool CanJump()
    {
        bool jumpAllowed = false;

        if (IsGrounded)
        {
            if (CurrentSlopeAngle <= slideMovement.gravitySlipAngle)
                jumpAllowed = true;
            else if (OldPosition.y == Rigidbody.position.y)
                jumpAllowed = true;
        }
        else if (IsOnPlatform)
            jumpAllowed = true;

        return jumpAllowed;
    }

    public void CheckGround()
    {
        WasGrounded = IsGrounded;

        IsGrounded = Physics2D.BoxCast(new Vector2(transform.position.x,
            BoxCollider.bounds.min.y + CastDistance / 2),
            BoxSize, 0, -transform.up, CastDistance, GroundMask);

        if (!IsGrounded && WasGrounded)
            Rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    public void CheckPlatform()
    {
        IsOnPlatform = Physics2D.BoxCast(new Vector2(
            transform.position.x, transform.position.y - BoxCollider.size.y * transform.localScale.y / 2 + CastDistance / 2),
            BoxSize, 0, -transform.up, CastDistance, PlatformMask);

        if (Rigidbody.linearVelocity.y < 0 && !IsOnPlatform && LayerWait == 0)
        {
            if (Physics2D.BoxCast(new Vector2(
            transform.position.x, transform.position.y - BoxCollider.size.y * transform.localScale.y / 2 + CastDistance / 2),
            BoxSize, 0, -transform.up, CastDistance * 1.5f, PlatformMask))
            {
                Rigidbody.excludeLayers -= PlatformMask;
                LayerWait = 1;
            }
        }
    }

    public void FlipSprite()
    {
        if (MoveValue.x > 0.0f && SpriteRenderer.flipX)
            SpriteRenderer.flipX = false;
        else if (MoveValue.x < 0.0f && !SpriteRenderer.flipX)
            SpriteRenderer.flipX = true;
    }

    public void LeavePlatform()
    {
        Rigidbody.excludeLayers += PlatformMask;
        LayerWait = 1;
    }

    public void Jump()
    {
        IsJumping = true;
        Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        Rigidbody.linearVelocityY = jumpSpeed;
        JumpedThisFrame = true;
    }

    public void HorizontalMovement()
    {
        if (!IsJumping && !IsOnPlatform && IsGrounded)
        {
            Vector2 velocity = new Vector2(MoveValue.x * moveSpeed, 0.0f);

            CurrentSlopeAngle = Math.Abs(Vector2.Angle(SlideResults.surfaceHit.normal, Vector2.up));

            if (CurrentSlopeAngle > slideMovement.gravitySlipAngle)
            {
                // We are sliding
                velocity = Vector2.zero;

                if (PreviousPosition == (Vector2)transform.position)
                {
                    StuckCount++;

                    if (StuckCount > 1)
                    {
                        velocity = new Vector2(MoveValue.x * moveSpeed, 0.0f);
                        StuckCount = 0;
                    }
                }
                else if (StuckCount > 0)
                    StuckCount = 0;
            }

            OldPosition = Rigidbody.position;

            SlideResults = Rigidbody.Slide(velocity, Time.deltaTime, slideMovement);

            if (!slideMovement.useSimulationMove)
                slideMovement.useSimulationMove = true;
        }
        else
            Rigidbody.linearVelocityX = MoveValue.x * moveSpeed;
    }

    public void CollisionEnter()
    {
        if (IsOnPlatform)
            if (IsJumping)
                IsJumping = false;
    }

    public void CollisionStay()
    {
        if (IsGrounded && !JumpedThisFrame)
        {
            if (IsJumping)
                IsJumping = false;

            Rigidbody.linearVelocity = Vector2.zero;
            Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            slideMovement.useSimulationMove = false;
        }
    }

    public void CollisionExit(Collision2D collision, bool jumpAction)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Platform") && !jumpAction && !IsOnPlatform)
            Rigidbody.excludeLayers += PlatformMask;
    }

    public void DrawGizmos()
    {
        if (BoxCollider != null)
            Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y - BoxCollider.size.y * transform.localScale.y / 2), BoxSize);
    }
}
