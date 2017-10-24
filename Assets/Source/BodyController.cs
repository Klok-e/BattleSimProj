using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{

    Vector2 velocity;
    int ticksToFall;
    
    void Update()
    {

    }

    public void Tick()
    {
        transform.position += new Vector3(velocity.x, velocity.y, 0);
        velocity.Scale(new Vector2(0.9f, 0.9f));
    }
}