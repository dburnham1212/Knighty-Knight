using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform[] movementNodes;

    int currentNode;
    int nextNode;
    public enum PlatformType { Looping, Alternating, Reactive, ReactiveTwoWay };

    public PlatformType platformType;

    public float platformSpeed;

    bool reversing = false;

    // Reactive platform variables
    bool reactiveMoving = false;
    bool isOnPlatform = false;
    bool isAtSpawn = true;
    public float reactiveLagTimeToRespawn = 5f;
    float reactiveLagTimeCounter = 0;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        nextNode = 1;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (platformType == PlatformType.Looping 
            || platformType == PlatformType.Alternating 
            || platformType == PlatformType.Reactive && reactiveMoving
            || platformType == PlatformType.ReactiveTwoWay && reactiveMoving)
            transform.position = Vector2.MoveTowards(transform.position, movementNodes[nextNode].position, Time.deltaTime * platformSpeed);

        if(platformType == PlatformType.Reactive && !isOnPlatform && !isAtSpawn)
        {
            reactiveLagTimeCounter += Time.deltaTime;

            if (reactiveLagTimeCounter >= reactiveLagTimeToRespawn)
            {
                transform.position = movementNodes[0].position;
                reactiveMoving = false;
                isAtSpawn = true;
                nextNode = 1;
            }
        }

        if(Vector2.Distance(transform.position, movementNodes[nextNode].position) == 0)
        {
            if(platformType == PlatformType.Looping)
            {
                if(nextNode < movementNodes.Length - 1)
                {
                    nextNode++;
                }
                else
                {
                    nextNode = 0;
                }
            }
            else if(platformType == PlatformType.Alternating)
            {
                if(nextNode == movementNodes.Length - 1)
                {
                    reversing = true;
                }
                if(nextNode == 0)
                {
                    reversing = false;
                }
                if(reversing && nextNode > 0)
                {
                    nextNode--;
                }
                else if(!reversing && nextNode < movementNodes.Length)
                {
                    nextNode++;
                }
            }
            else if(platformType == PlatformType.Reactive && reactiveMoving)
            {
                if(nextNode < movementNodes.Length - 1)
                {
                    nextNode++;
                }
                else
                {
                    reactiveMoving = false;
                }
            }
            else if(platformType == PlatformType.ReactiveTwoWay && reactiveMoving)
            {
                if (nextNode == movementNodes.Length - 1)
                {
                    reversing = true;
                    reactiveMoving = false;
                }
                if (nextNode == 0)
                {
                    reversing = false;
                    reactiveMoving = false;
                }
                if (reversing && nextNode > 0)
                {
                    nextNode--;
                }
                else if (!reversing && nextNode < movementNodes.Length)
                {
                    nextNode++;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.transform.SetParent(transform);
        if(platformType == PlatformType.Reactive)
        {
            reactiveMoving = true;
            reactiveLagTimeCounter = 0.0f;
            isOnPlatform = true;
            isAtSpawn = false;
        }
        if (platformType == PlatformType.ReactiveTwoWay)
        {
            reactiveMoving = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collision.transform.SetParent(null);
        if(platformType == PlatformType.Reactive)
        {
            isOnPlatform = false;
        }
    }

    private void OnDrawGizmos()
    {
        for(int i = 0; i < movementNodes.Length - 1; i++)
        {
            Gizmos.DrawLine(movementNodes[i].position, movementNodes[i + 1].position);
        }
        if (platformType == PlatformType.Looping)
        {
            Gizmos.DrawLine(movementNodes[movementNodes.Length - 1].position, movementNodes[0].position);
        }
    }

}