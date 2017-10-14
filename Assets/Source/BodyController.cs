using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    public Sprite[] sprites;
    Vector2 velocity;
    int ticksToFall;

    void Update()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[0];
    }

    public void Tick()
    {
        transform.position += new Vector3(velocity.x, velocity.y, 0);
        velocity.Scale(new Vector2(0.9f, 0.9f));
    }
}