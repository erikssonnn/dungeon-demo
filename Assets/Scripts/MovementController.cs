using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {
    [SerializeField] private float movementSpeed;
    [SerializeField] private float mouseSensitivity = 2;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxStamina;
    [SerializeField] private Vector3 crouchedPos;

    private float mouseX = 0f;
    private float mouseY = 0f;

    private float verticalVelocity = 0;
    private float speed;
    private float crouchSpeed;
    private float runSpeed;
    private float stamina = 0.0f;
    private float timeSinceSprint = 1.0f;

    private bool crouched = false;
    private bool canStandUp = false;
    private Vector3 cameraDefaultPos;
    private CharacterController cc;
    private Camera cam = null;
    private UiController ui = null;

    private const float minX = -90f;
    private const float maxX = 90f;
    private const float startHeight = 2.0f;
    private const float endHeight = 1.0f;


    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        speed = movementSpeed;
        stamina = maxStamina;
        runSpeed = movementSpeed * 1.9f;
        crouchSpeed = movementSpeed * 0.4f;
        ui = UiController.Instance;

        if (Camera.main == null) {
            throw new System.Exception("Cant find main camera");
        }
        cam = Camera.main;
        
        cameraDefaultPos = cam.transform.localPosition;
        cc = GetComponent<CharacterController>();
    }

    private void Movement() {
        if (Input.GetKeyDown(KeyCode.LeftControl) && canStandUp) {
            crouched = !crouched;
        }

        if (!crouched) {
            float dist = Vector3.Distance(cam.transform.localPosition, cameraDefaultPos);
            speed = movementSpeed;

            if (dist > 0.01f) {
                cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, cameraDefaultPos, 1.3f * Time.fixedDeltaTime);
                cc.height = Mathf.Lerp(startHeight, endHeight, 1.3f * Time.fixedDeltaTime);
            }

            if (Input.GetKey(KeyCode.LeftShift) && (Input.GetAxis("Horizontal") > 0.0f || Input.GetAxis("Vertical") > 0.0f) && stamina > 0.0f) {
                timeSinceSprint = 1.0f;
                stamina -= Time.fixedDeltaTime;
                speed = runSpeed;
            }
        } else {
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, crouchedPos, 1.3f * Time.fixedDeltaTime);
            cc.height = Mathf.Lerp(endHeight, startHeight, 1.3f * Time.fixedDeltaTime);
            speed = crouchSpeed;
        }
        
        if (!(Input.GetKey(KeyCode.LeftShift) && (Input.GetAxis("Horizontal") > 0.0f || Input.GetAxis("Vertical") > 0.0f))) {
            timeSinceSprint += Time.fixedDeltaTime;
            timeSinceSprint = Mathf.Clamp(timeSinceSprint, 1.0f, 15.0f);
            stamina +=  0.05f * timeSinceSprint * Time.fixedDeltaTime;
            stamina = Mathf.Clamp(stamina, 0.0f, maxStamina);
        }
        
        ui.staminaBar.fillAmount = stamina / maxStamina;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forwardMovement = (transform.forward * vertical).normalized;
        Vector3 rightMovement = (transform.right * horizontal).normalized;
        Vector3 upMovement = transform.up * verticalVelocity;

        forwardMovement.y = 0;
        rightMovement.y = 0;

        if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded) {
            verticalVelocity = jumpForce;
        }

        if (verticalVelocity > 0) {
            verticalVelocity -= 10 * Time.deltaTime;
        }

        upMovement.y -= 9.81f;
        Vector3 dir = forwardMovement + rightMovement;
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

    private void Update() {
        Movement();
        CameraRotation();
        canStandUp = CanStandUp();
    }

    private bool CanStandUp() {
        Ray forwardRay = new Ray(cam.transform.position, transform.up);
        if (Physics.Raycast(forwardRay, out RaycastHit hit, 0.5f)) {
            return false;
        }
        return true;
    }
}
