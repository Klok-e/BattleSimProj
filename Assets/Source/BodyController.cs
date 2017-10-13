using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    public Sprite[] sprites;

    void Update()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[0];
    }
}