using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    PlayerController player;
    Rigidbody2D rigidbody2d;
    [SerializeField]
    Transform cardContainer;

    [SerializeField]
    Camera camera;
    int cameraSpeed = 50;
    bool freeCamera = false;

    private void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        cardContainer.position = new Vector3(transform.position.x, transform.position.y - 1.57f, cardContainer.position.z);
        if (Input.GetKeyDown(KeyCode.F)) freeCamera = !freeCamera;
        if (freeCamera) Move();
        else transform.position = new Vector3(GameController.instance.player.transform.position.x, GameController.instance.player.transform.position.y, transform.position.z);

        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            if(camera.orthographicSize > 4) 
            {
                camera.orthographicSize--;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            camera.orthographicSize++;
        }

    }

    void Move() 
    {
        //Mouse Movement
        /*if(Input.mousePosition.x <= 0) 
        {
            rigidbody2D.AddForce(new Vector2(-1*cameraSpeed, 0));          
        }

        if(Input.mousePosition.y <= 0) 
        {
            rigidbody2D.AddForce(new Vector2(0, -1*cameraSpeed));
        }

        if(Input.mousePosition.x >= Screen.width - 1) 
        {
            rigidbody2D.AddForce(new Vector2(1 * cameraSpeed, 0));
        }

        if (Input.mousePosition.y >= Screen.height - 1)
        {
            rigidbody2D.AddForce(new Vector2(0, 1 * cameraSpeed));
        }*/

        rigidbody2d.AddForce(new Vector2(Input.GetAxis("Horizontal") * cameraSpeed * camera.orthographicSize/2, Input.GetAxis("Vertical") * cameraSpeed * camera.orthographicSize/2));
        rigidbody2d.velocity = Vector2.zero;

        if (Input.GetKeyUp(KeyCode.Space)) transform.position = player.transform.position;
    }

    public void SetPlayer(PlayerController player) 
    {
        this.player = player;
    }
}
