using UnityEngine;
using System.Collections;


public class CameraController : MonoBehaviour
{
    public float move_speed;
    public float zoomSpeed;

    public static CameraController cameraInstance;

    //private GameController gameControllerInstance;
    private Vector3 offset;

    void Start()
    {
        cameraInstance = this;
    }

    private void LateUpdate()
    {
        float move_x = Input.GetAxisRaw("Horizontal");
        float move_y = Input.GetAxisRaw("Vertical");
        float cameraZoom = Input.GetAxisRaw("Mouse ScrollWheel");

        GetComponent<Camera>().orthographicSize += -cameraZoom * zoomSpeed;

        transform.position = transform.position + new Vector3(move_x * move_speed, move_y * move_speed);
    }
}

