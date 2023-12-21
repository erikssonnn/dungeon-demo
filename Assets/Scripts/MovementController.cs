using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {
    public float movementSpeed;
    public float mouseSensitivity = 2;
    public float jumpForce;
    public float maxStamina = 5.0f;

    private readonly float minX = -90f;
    private readonly float maxX = 90f;

    private float mouseX = 0f;
    private float mouseY = 0f;

    private float verticalVelocity = 0;
    private float speed;
    private float runSpeed;

    private CharacterController cc;
    private Transform cam = null;

    private float timeSinceSprint = 0.0f;
    private float stamina = 0.0f;
    private UiController ui = null;

    private void Start() {
        //Right now we lock mouse in here
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        runSpeed = movementSpeed * 1.5f;
        stamina = maxStamina;
        speed = movementSpeed;
        cc = GetComponent<CharacterController>();
        ui = UiController.Instance;

        if (Camera.main == null)
            throw new Exception("Cant find main camera!");
        cam = Camera.main.transform;
    }

    private void Movement() {
        speed = movementSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && (Input.GetAxis("Horizontal") > 0.0f ||Input.GetAxis("Vertical") > 0.0f) && stamina > 0.0f) {
            timeSinceSprint = 1.0f;
            stamina -= Time.fixedDeltaTime;
            speed = runSpeed;
        }

        if (!(Input.GetKey(KeyCode.LeftShift) && (Input.GetAxis("Horizontal") > 0.0f || Input.GetAxis("Vertical") > 0.0f))) {
            timeSinceSprint += Time.fixedDeltaTime;
            timeSinceSprint = Mathf.Clamp(timeSinceSprint, 1.0f, 15.0f);
            stamina += 0.05f * timeSinceSprint * Time.fixedDeltaTime;
            stamina = Mathf.Clamp(stamina, 0.0f, maxStamina);
        }

        ui.staminaBar.fillAmount = stamina / maxStamina;
        
        float horizontal = Input.GetAxis("Horizontal") * speed;
        float vertical = Input.GetAxis("Vertical") * speed;

        Vector3 forwardMovement = transform.forward * vertical;
        Vector3 rightMovement = transform.right * horizontal;
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
        Vector3 dir = forwardMovement + upMovement + rightMovement;
        cc.Move(dir * Time.deltaTime);
    }

    private void CameraRotation() {
        mouseX += Input.GetAxis("Mouse Y") * mouseSensitivity * 0.1f;
        mouseY += Input.GetAxis("Mouse X") * mouseSensitivity * 0.1f;

        mouseX = Mathf.Clamp(mouseX, minX, maxX);
        cam.eulerAngles = new Vector3(-mouseX, mouseY, 0);
        transform.eulerAngles = new Vector3(0, cam.eulerAngles.y, 0);
    }

    private void Update() {
        Movement();
        CameraRotation();
    }
}