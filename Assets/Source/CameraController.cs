using UnityEngine;
using System.Collections;


public class CameraController : MonoBehaviour
{
    public float move_speed;

    public static CameraController cameraInstance;

    //private GameController gameControllerInstance;
    private Vector3 offset;

    Camera cameraAttached;

    void Start()
    {
        cameraInstance = this;
        cameraAttached = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        float move_x = Input.GetAxisRaw("Horizontal");
        float move_y = Input.GetAxisRaw("Vertical");
        float cameraZoom = Input.GetAxisRaw("Mouse ScrollWheel");

        cameraAttached.orthographicSize *= -cameraZoom + 1;

        var move = new Vector3(move_x, move_y, 0).normalized;
        transform.Translate(move * cameraAttached.orthographicSize / move_speed);
    }
}

