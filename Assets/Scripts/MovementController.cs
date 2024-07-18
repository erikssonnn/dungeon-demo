using System.Collections.Generic;
using UnityEngine;
using Logger = erikssonn.Logger;
using erikssonn;

public class MovementController : MonoBehaviour {
    [SerializeField] private float movementSpeed;
    [SerializeField] private float mouseSensitivity = 2;
    [SerializeField] private float jumpForce;
    [SerializeField] private float dashRate;
    [SerializeField] private float dashLength;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float fallAcceleration;
    [SerializeField] private float jumpAcceleration;

    private float mouseX = 0f;
    private float mouseY = 0f;

    private float verticalVelocity = 0;
    private float speed;
    private float timeSinceSprint = 1.0f;
    
    private CharacterController cc;
    private Camera cam = null;
    private UiController ui = null;

    private const float minX = -90f;
    private const float maxX = 90f;
    private float nextDash = 0.0f;
    private Vector3 dir = Vector3.zero;
    private List<Vector3> positioningList = new List<Vector3>();
    private float fallForce = 1.5f;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        speed = movementSpeed;
        ui = UiController.Instance;

        if (Camera.main == null) {
            throw new System.Exception("Cant find main camera");
        }

        cam = Camera.main;
        cc = GetComponent<CharacterController>();
    }

    private void Update() {
        Movement();
        CameraRotation();
        Dash();
    }

    private void AdditionalPositioning(Vector3 pos) {
        float step = dashSpeed * Time.deltaTime;
        float dist = Vector3.Distance(transform.position, pos); //Vector3.SqrMagnitude(transform.position - pos); //optimized

        if (dist > 0.1f) {
            transform.position = Vector3.MoveTowards(transform.position, pos, step);
            cam.fieldOfView = dist >= half ? Mathf.Lerp(cam.fieldOfView, endFov, step * 0.25f) : Mathf.Lerp(cam.fieldOfView, startFov, step * 0.5f);
        } else {
            positioningList.Remove(positioningList[0]);
            cam.fieldOfView = startFov;
            cc.enabled = true;
        }
    }

    private void Movement() {
        if (positioningList.Count > 0) {
            if (positioningList.Count > 1) {
                Logger.Print("Something is wrong, there should not be more than one", LogLevel.ERROR);
            }

            cc.enabled = false;
            verticalVelocity = 0.0f;
            AdditionalPositioning(positioningList[0]);
            return;
        }

        if (!(Input.GetKey(KeyCode.LeftShift) && (Input.GetAxis("Horizontal") > 0.0f || Input.GetAxis("Vertical") > 0.0f))) {
            timeSinceSprint += Time.fixedDeltaTime;
            timeSinceSprint = Mathf.Clamp(timeSinceSprint, 1.0f, 15.0f);
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forwardMovement = (transform.forward * vertical).normalized;
        Vector3 rightMovement = (transform.right * horizontal).normalized;
        Vector3 upMovement = transform.up * verticalVelocity;

        forwardMovement.y = 0;
        rightMovement.y = 0;

        // Jump and falling
        if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded) {
            verticalVelocity = jumpForce;
        }
        if (verticalVelocity >= 0)
        {
            verticalVelocity -= 25 * jumpAcceleration * Time.deltaTime;
        }
        if (fallForce > 0 && cc.isGrounded)
        {
            fallForce = 0.1f;
        }
        if(verticalVelocity < 0)
        {
            fallForce += (Mathf.Pow(fallAcceleration, 2f) * Time.deltaTime);
        }

        // apply vectors
        upMovement.y -= (fallForce * 9.81f);
        dir = forwardMovement + rightMovement;
        dir.Normalize();

        Vector3 totalMovement = dir * (Time.deltaTime * speed);
        totalMovement += upMovement * Time.deltaTime;

        cc.Move(totalMovement);
    }
    
    private void CameraRotation() {
        mouseX += Input.GetAxis("Mouse Y") * mouseSensitivity * 0.1f;
        mouseY += Input.GetAxis("Mouse X") * mouseSensitivity * 0.1f;

        mouseX = Mathf.Clamp(mouseX, minX, maxX);
        cam.transform.eulerAngles = new Vector3(-mouseX, mouseY, 0);
        transform.eulerAngles = new Vector3(0, cam.transform.eulerAngles.y, 0);
    }

    private void Dash() {
        RaycastHit hit;
        Vector3 newDir = new Vector3(dir.x, 0, dir.z).normalized;
        Ray ray = new Ray(transform.position, newDir);

        if (!Input.GetKeyDown(KeyCode.LeftShift) || !(Time.time > nextDash) || !(newDir.sqrMagnitude > 0.1f))
            return;
        nextDash = Time.time + dashRate;
        Vector3 pos;

        if (!Physics.Raycast(ray, out hit, (dashLength / 2))) {
            pos = transform.position + (ray.direction * (dashLength / 2));
        } else {
            pos = new Vector3(hit.point.x - newDir.x * cc.radius, transform.position.y, hit.point.z - newDir.z * cc.radius);
        }

        // startFov = cam.fieldOfView;
        positioningList.Add(pos);
        // endFov = cam.fieldOfView + dashFov;
        // half = Vector3.Distance(transform.position, pos) * 0.5f;
    }

    private void OnGUI() {
        GUI.Label(new Rect(10, 10, 100, 100), verticalVelocity.ToString("F2"));
    }
}