using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public Sprite[] sprites;

    public bool isEmpty;

    // Update is called once per frame
    void Update()
    {
        if (isEmpty)
        {
            GetComponent<SpriteRenderer>().sprite = sprites[0];
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = sprites[1];
        }
    }
}
