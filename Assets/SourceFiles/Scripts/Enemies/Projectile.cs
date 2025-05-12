using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector2 direction;

    float projectileSpeed = 7.5f;

    enum ProjectileTag { Player, Enemy };

    ProjectileTag projectileTag;

    Rigidbody2D rigidBody;

    float currentProjectileLife = 0;
    float projectileLife = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentProjectileLife < projectileLife)
        {
            rigidBody.linearVelocity = direction * projectileSpeed;
            currentProjectileLife += Time.deltaTime;
        }
        else
            Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
