using UnityEngine;

public class ProjectileHandler : MonoBehaviour
{
    public Projectile baseProjectile;

    public GameObject originPoint;

    float spawnTime = 1;
    float currentSpawnTime = 0;

    float playerDetectDistance = 10;

    PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(originPoint.transform.position, (playerController.transform.position - transform.position).normalized, playerDetectDistance); 

        if(hit && hit.collider.gameObject.CompareTag("Player") && Vector2.Distance(playerController.transform.position, transform.position) < playerDetectDistance)
        {
            if (currentSpawnTime < spawnTime)
                currentSpawnTime += Time.deltaTime;
            else
            {
                SpawnProjectile();
                currentSpawnTime = 0;
            }
        }
    }

    void SpawnProjectile()
    {
        GameObject newProjectile = Instantiate(baseProjectile.gameObject, originPoint.transform.position, Quaternion.identity);

        Projectile createdProjectile = newProjectile.GetComponent<Projectile>();

        createdProjectile.direction = (playerController.transform.position - transform.position).normalized;
    }
}
