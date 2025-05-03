using UnityEngine;

public class PlatformClipper : MonoBehaviour
{
    // private members
    // Components
    Rigidbody2D m_Rigidbody;
    BoxCollider2D m_Collider;
    // player Components
    BoxCollider2D playerCollider;
    PlayerController playerController;
    // counts frames to give player time to fall through the platform
    int timer;

    // public members
    // player object
    public GameObject player;

    // Unity Messages
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize required private members
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Collider = GetComponent<BoxCollider2D>();

        playerCollider = player.GetComponent<BoxCollider2D>();
        playerController = player.GetComponent<PlayerController>();

        timer = 0;
    }

    // FixedUpdate is called once per fixed-framerate frame
    void FixedUpdate()
    {
        if (timer == 0)
        {
            SetClipping();

            if (playerController.DropAction)
                Drop();
        }
        else if (timer < 2)
            timer++;
        else
        {
            playerController.DropAction = false;
            timer = 0;
        }
    }

    // methods
    void SetClipping()
    {
        float top = m_Collider.transform.position.y + transform.localScale.y / 2 * m_Collider.size.y;
        float playerBottom = playerCollider.transform.position.y - player.transform.localScale.y / 2 * playerCollider.size.y;

        if (playerBottom < top)
            m_Rigidbody.simulated = false;
        else
            m_Rigidbody.simulated = true;
    }

    void Drop()
    {
        m_Rigidbody.simulated = false;
        timer++;
    }
}
