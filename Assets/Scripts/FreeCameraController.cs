using UnityEngine;

public class FreeCameraController : MonoBehaviour {
    [SerializeField] private float movementSpeed;
    [SerializeField] private float mouseSensitivity = 2;

    private float minX = -90f;
    private float maxX = 90f;

    private float mouseX = 0f;
    private float mouseY = 0f;

    private float speed;

    private void Start() {
        speed = movementSpeed;
    }

    public float mSpeed {
        set {
            movementSpeed = value;
            speed = movementSpeed;
        }
        get {
            return movementSpeed;
        }
    }

    private void Movement() {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");

        Vector3 forwardMovement = (transform.forward * vertical).normalized;
        Vector3 rightMovement = (transform.right * horizontal).normalized;

        Vector3 dir = forwardMovement + rightMovement;
        transform.position += dir * Time.deltaTime * speed;
    }

    private void CameraRotation() {
        mouseX += Input.GetAxis("Mouse Y") * mouseSensitivity * 0.5f;
        mouseY += Input.GetAxis("Mouse X") * mouseSensitivity * 0.5f;

        mouseX = Mathf.Clamp(mouseX, minX, maxX);
        Camera.main.transform.eulerAngles = new Vector3(-mouseX, mouseY, 0);
        transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
    }

    private void Update() {
        Movement();
        CameraRotation();
    }
}