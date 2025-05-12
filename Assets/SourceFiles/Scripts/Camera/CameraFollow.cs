using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public PlayerController playerController;

    Vector3 cameraPosition;

    float cameraDeadZoneX = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(cameraPosition.x > playerController.transform.position.x + cameraDeadZoneX)
        {
            cameraPosition.x = playerController.transform.position.x + cameraDeadZoneX;
        }
        else if (cameraPosition.x < playerController.transform.position.x - cameraDeadZoneX)
        {
            cameraPosition.x = playerController.transform.position.x - cameraDeadZoneX;
        }
        cameraPosition.y = playerController.transform.position.y;

        transform.position = cameraPosition;
    }
}
