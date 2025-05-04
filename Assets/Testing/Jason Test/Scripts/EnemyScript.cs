using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int maxHealth;
    public int Health {  get; set; }
    public bool CanBeHit { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = maxHealth;
        CanBeHit = true;
    }

    // FixedUpdate is called once per fixed-frame
    void FixedUpdate()
    {
        
    }
}
