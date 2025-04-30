using UnityEngine;

public class DoSomething : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    Rigidbody2D m_rigidBody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        m_rigidBody.linearVelocityY = moveSpeed;
    }
}
