using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController3))]
public class Player : MonoBehaviour
{
    // InputActions
    InputAction moveAction;
    InputAction jumpAction;

    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;
    float moveSpeed = 5;

    float gravity;
    float jumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    PlayerController3 controller;
    Rigidbody2D rigidBody;

    void Start()
    {
        controller = GetComponent<PlayerController3>();

        rigidBody = GetComponent<Rigidbody2D>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void FixedUpdate()
    {

        if (controller.collisions.above || controller.collisions.below)
        {
            rigidBody.linearVelocityY = 0;
        }

        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        Vector2 input = new Vector2(moveValue.x, 0.0f);

        if (jumpAction.WasPressedThisFrame() && controller.collisions.below)
        {
            rigidBody.linearVelocityY = jumpVelocity;
        }

        float targetVelocityX = input.x * moveSpeed;
        rigidBody.linearVelocityX = Mathf.SmoothDamp(rigidBody.linearVelocityX, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        controller.Move(rigidBody.linearVelocity);
    }
}
