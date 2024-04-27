using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwayController : MonoBehaviour {
    [Header("Movement sway: ")]
    [SerializeField] private float movementAmount = 0.2f;
    [SerializeField] private float movementSpeed = 2f;

    [Header("Rotation sway: ")]
    [SerializeField] private float rotationAmount = 25f;
    [SerializeField] private float rotationSpeed = 12f;

    private Vector3 startPos = Vector3.zero;
    private Vector3 desiredPos = Vector3.zero;

    private Vector3 desiredRot = Vector3.zero;
    private Vector3 startRot = Vector3.zero;

    private Vector3 right = Vector3.zero;
    private Vector3 forward = Vector3.zero;
    private float mouseX = 0f;
    private float mouseY = 0f;
    
    private float horizontal;
    private float vertical;
    
    private void Start() {
        startPos = transform.localPosition;
        desiredPos = startPos;

        startRot = transform.localEulerAngles;
        desiredRot = startRot;
    }

    private void LateUpdate() {
        MovementSway();
        CameraSway();
    }

    private void CameraSway() {
        mouseY = Input.GetAxis("Mouse X") * (rotationAmount * 0.1f);
        mouseX = Input.GetAxis("Mouse Y") * (rotationAmount * 0.1f);

        desiredRot = new Vector3(mouseX, mouseY, right.x * -100f);
        Quaternion dest = Quaternion.Euler(startRot + desiredRot);

        float step = rotationSpeed * Time.fixedDeltaTime;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, dest, step);
    }

    private void MovementSway() {
        float step = movementSpeed * Time.fixedDeltaTime;
        float targetHorizontal = Input.GetAxis("Horizontal");
        float targetVertical = Input.GetAxis("Vertical");

        horizontal = Mathf.Lerp(horizontal, targetHorizontal, step);
        vertical = Mathf.Lerp(vertical, targetVertical, step);

        right = horizontal * Vector3.right * (movementAmount * 0.1f);
        forward = vertical * Vector3.forward * (movementAmount * 0.1f);

        desiredPos = right + forward;

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPos + desiredPos, step);
    }
}
