using System.Runtime.CompilerServices;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
// old script, not in use
public class PlayerControllerOld : MonoBehaviour
{
    InputAction moveAction;
    InputAction jumpAction;

    // Speed Variables
    float moveSpeedX = 5f;
    float jumpSpeed = 7.5f;

    // Movement Variables
    Vector2 moveValue;

    // Ground Checks
    public float groundCheckRadius;
    public LayerMask groundLayer;
    public Transform groundCheck;
    bool wasGrounded;
    bool isGrounded;

    // Jump Variables
    bool canJump;
    bool isJumping;
    bool hasDoubleJumped = false;

    // Slope Checks
    public float slopeCheckDistanceY;
    public float slopeCheckDistanceX;
    float slopeDownAngle;
    float slopeDownAngleOld = 0.0f;
    float slopeSideAngle;
    Vector2 slopeNormalPerp;
    bool isOnSlope;
    public float maxSlopeAngle;
    bool canWalkOnSlope;

    public PhysicsMaterial2D noFriction;
    public PhysicsMaterial2D fullFriction;

    // Rigid Body
    Rigidbody2D rigidBody;
    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        rigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Update the movement value because it is needed in slope check and move character
        moveValue = moveAction.ReadValue<Vector2>();

        CheckGround();
        SlopeCheck();
        MoveCharacter();

        bool jumpedThisFrame = false;

        if(jumpAction.WasPerformedThisFrame() && canJump )
        {
            rigidBody.linearVelocityY = jumpSpeed;
            isJumping = true;
            canJump = false;
            jumpedThisFrame = true;
            Debug.Log("jumping");
        }
        if(jumpAction.WasPerformedThisFrame() && !hasDoubleJumped && !isGrounded && !jumpedThisFrame)
        {
            rigidBody.linearVelocityY = jumpSpeed;
            hasDoubleJumped = true;
            Debug.Log("double jumping");
        }
    }

    void MoveCharacter()
    {
        if (isGrounded && !isOnSlope && !isJumping)
        {
            rigidBody.linearVelocity = new Vector2(moveValue.x * moveSpeedX, rigidBody.linearVelocityY);
        }
        else if (isGrounded && isOnSlope && !isJumping && canWalkOnSlope)
        {
            rigidBody.linearVelocity = new Vector2(moveSpeedX * slopeNormalPerp.x * -moveValue.x, moveSpeedX * slopeNormalPerp.y * -moveValue.x);
        }
        else if (!isGrounded)
        {
            rigidBody.linearVelocity = new Vector2(moveValue.x * moveSpeedX, rigidBody.linearVelocityY);
        }
    }

    private void SlopeCheck()
    {
        Vector2 checkPos = transform.position - (Vector3)(new Vector2(0.0f, 1f));

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, slopeCheckDistanceX, groundLayer);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistanceX, groundLayer);

        if (slopeHitFront)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        } 
        else if (slopeHitBack)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
        }
    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistanceY, groundLayer);

        if(hit)
        {
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;

            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if(slopeDownAngle != slopeDownAngleOld)
            {
                isOnSlope = true;
            }

            slopeDownAngleOld = slopeDownAngle;

            Debug.DrawRay(hit.point, slopeNormalPerp, Color.red);
            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }

        if(slopeDownAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }
    
        if(isOnSlope && moveValue.x == 0 && canWalkOnSlope)
        {
            rigidBody.sharedMaterial = fullFriction;
        }
        else
        {
            rigidBody.sharedMaterial = noFriction;
        }
    }

    public void CheckGround()
    {
        wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && isGrounded)
        {
            isJumping = false;
        }

        if(isGrounded && !isJumping && slopeDownAngle <= maxSlopeAngle)
        {
            canJump = true;
            hasDoubleJumped = false;
        }
        else
        {
            canJump = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hit");
        if (collision.gameObject.tag == "Item")
        {
            Destroy(collision.gameObject);
        }
    }
}