using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class FollowPathTest : MonoBehaviour
{
    public PathCreator pathCreator;
    public float speed;
    float distanceTravelled;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();        
    }

    void Update()
    {
        distanceTravelled += speed * Time.deltaTime;

        Vector3 currentPosition = rb.position;
        Vector3 pathPosition = pathCreator.path.GetPointAtDistance(distanceTravelled);
        rb.MovePosition(new Vector3(pathPosition.x, currentPosition.y, pathPosition.z));
    }
}
