using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformClipper : MonoBehaviour
{
    // this is temporary until can be moved to PlayerController
    InputAction moveAction;
    InputAction jumpAction;
    /////////////////////////////
    Vector2 center;
    float left;
    float right;
    float top;
    float bottom;

    Vector2 playerCenter;
    float playerLeft;
    float playerRight;
    float playerTop;
    float playerBottom;

    public GameObject player;
    BoxCollider2D playerCollider;
    PlayerController playerController;
    new BoxCollider2D collider;
    new Rigidbody2D rigidbody;

    int timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCollider = player.GetComponent<BoxCollider2D>();
        playerController = player.GetComponent<PlayerController>();
        collider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();

        ////////////////////////////////////////////////////////
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        ////////////////////////////////////////////////////////
        timer = 0;

        SetClipping();
    }

    // FixedUpdate is called once per fixed-framerate frame
    void FixedUpdate()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        if (timer == 0)
            SetClipping();
        else if (timer == 6)
            timer = 0;
        else
            timer++;

        // this will be a either a member or properties taken from PlayerController.cs
        if (jumpAction.WasPressedThisFrame() && moveAction.ReadValue<Vector2>().y < 0f)
        {
            rigidbody.simulated = false;
            timer++;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // most of this is for testing and can be removed, only need top and playerBottom
        center = collider.transform.position;
        left = center.x - transform.localScale.x / 2 * collider.size.x;
        right = center.x + transform.localScale.x / 2 * collider.size.x;
        top = collider.transform.position.y + transform.localScale.y / 2 * collider.size.y;
        bottom = center.y - transform.localScale.y / 2 * collider.size.y;

        playerCenter = playerCollider.transform.position;
        playerLeft = playerCenter.x - player.transform.localScale.x / 2 * playerCollider.size.x;
        playerRight = playerCenter.x + player.transform.localScale.x / 2 * playerCollider.size.x;
        playerTop = playerCenter.y + player.transform.localScale.y / 2 * playerCollider.size.y;
        playerBottom = playerCollider.transform.position.y - player.transform.localScale.y / 2 * playerCollider.size.y;
    }

    private void OnDrawGizmos()
    {
        float radius = .1f;
        // platform points
        Gizmos.DrawWireSphere(new Vector2(left, center.y), radius);
        Gizmos.DrawWireSphere(new Vector2(right, center.y), radius);
        Gizmos.DrawWireSphere(new Vector2(center.x, top), radius);
        Gizmos.DrawWireSphere(new Vector2(center.x, bottom), radius);

        // player points
        Gizmos.DrawWireSphere(new Vector2(playerLeft, playerCenter.y), radius);
        Gizmos.DrawWireSphere(new Vector2(playerRight, playerCenter.y), radius);
        Gizmos.DrawWireSphere(new Vector2(playerCenter.x, playerTop), radius);
        Gizmos.DrawWireSphere(new Vector2(playerCenter.x, playerBottom), radius);
    }

    void SetClipping()
    {
        if (playerBottom < top)
            rigidbody.simulated = false;
        else
            rigidbody.simulated = true;
    }
}
