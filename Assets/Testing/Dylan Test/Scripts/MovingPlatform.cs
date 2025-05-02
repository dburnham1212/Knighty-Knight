using NUnit.Framework;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform[] movementNodes;

    int currentNode;
    int nextNode;

    public bool loop;
    bool reversing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nextNode = 1;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.position = Vector2.MoveTowards(transform.position, movementNodes[nextNode].position, Time.deltaTime);

        if(Vector2.Distance(transform.position, movementNodes[nextNode].position) == 0)
        {
            if(loop)
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
            else
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
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collision.transform.SetParent(null);
    }

    private void OnDrawGizmos()
    {
        for(int i = 0; i < movementNodes.Length - 1; i++)
        {
            Gizmos.DrawLine(movementNodes[i].position, movementNodes[i + 1].position);
        }
        if (loop)
        {
            Gizmos.DrawLine(movementNodes[movementNodes.Length - 1].position, movementNodes[0].position);
        }
    }
}
