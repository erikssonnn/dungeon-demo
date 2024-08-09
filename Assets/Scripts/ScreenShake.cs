using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScreenShake : MonoBehaviour {
    [SerializeField] private Transform shakeOrigin = null;
    [SerializeField] private Transform shakeCam = null;
    [SerializeField] private float shakeRotationTangent = 2f;

    private Vector3 startPos = Vector3.zero;
    private Vector3 desiredPos = Vector3.zero;
    private Vector3 startRot = Vector3.zero;
    private Vector3 desiredRot = Vector3.zero;

    private Vector3 camStartPos = Vector3.zero;
    private Vector3 camDesiredPos = Vector3.zero;
    private Vector3 camStartRot = Vector3.zero;
    private Vector3 camDesiredRot = Vector3.zero;

    private static ScreenShake instance;

    public static ScreenShake Instance {
        get {
            instance = FindObjectOfType<ScreenShake>();
            if (instance != null) return instance;
            GameObject obj = new GameObject("ScreenShake");
            instance = obj.AddComponent<ScreenShake>();
            return instance;
        }
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
    }
    
    private void Start() {
        if(shakeOrigin == null || shakeCam == null) {
            Debug.Break();
            Debug.LogError("shakeOrigin is null");
        }
        startPos = shakeOrigin.localPosition;
        startRot = shakeOrigin.localEulerAngles;
        camStartPos = shakeCam.localPosition;
        camStartRot = shakeCam.localEulerAngles;
    }

    private void Update() {
        // TODO: DEBUG
        if (Input.GetKeyDown(KeyCode.O)) {
            StartCoroutine(Shake(5, 0.5f));
        }
    }

    public IEnumerator Shake(float shakeAmount, float shakeTime) {
        float time = 0.0f;

        float step = Time.deltaTime * (shakeAmount * 2.0f);
        float rotStep = Time.deltaTime * (shakeAmount * shakeRotationTangent);

        while (time < shakeTime) {
            float up = UnityEngine.Random.Range(-1.0f, 1.0f) * (shakeAmount * 0.1f);
            float right = UnityEngine.Random.Range(-1.0f, 1.0f) * (shakeAmount * 0.1f);

            desiredPos = new Vector3(right, up, 0);
            camDesiredPos = new Vector3(right, up, 0);

            shakeOrigin.localPosition = Vector3.MoveTowards(shakeOrigin.localPosition, desiredPos * 0.1f + startPos, step);
            shakeCam.localPosition = Vector3.MoveTowards(shakeCam.localPosition, camDesiredPos * 0.5f + camStartPos, step);

            desiredRot = new Vector3(right, up, 0) * (shakeRotationTangent * 0.1f);
            Quaternion dest = Quaternion.Euler(startRot + desiredRot);
            camDesiredRot = new Vector3(right, up, 0) * shakeRotationTangent;
            Quaternion camDest = Quaternion.Euler(camStartRot + camDesiredRot);

            shakeOrigin.localRotation = Quaternion.Slerp(shakeOrigin.localRotation, dest, rotStep);
            shakeCam.localRotation = Quaternion.Slerp(shakeCam.localRotation, camDest, rotStep);

            time += Time.deltaTime;
            yield return null;
        }

        shakeOrigin.localEulerAngles = startRot;
        shakeCam.localEulerAngles = camStartPos;
        StartCoroutine(ReturnHome(step));
        StartCoroutine(ReturnHomeCam(step));
    }

    private IEnumerator ReturnHome(float step) {
        float time = 0.0f;

        while (time < step) {
            time += Time.deltaTime;
            float dist = Vector3.Distance(shakeOrigin.localPosition, startPos);

            if (dist > 0.001f) {
                shakeOrigin.localPosition = Vector3.MoveTowards(shakeOrigin.localPosition, startPos, step * 0.1f);
                yield return null;
            }
        }

        shakeOrigin.localPosition = startPos;
    }

    private IEnumerator ReturnHomeCam(float step) {
        float time = 0.0f;

        while (time < step) {
            time += Time.deltaTime;
            float dist = Vector3.Distance(shakeCam.localPosition, camStartPos);

            if (dist > 0.001f) {
                shakeCam.localPosition = Vector3.MoveTowards(shakeCam.localPosition, camStartPos, step * 0.1f);
                yield return null;
            }
        }

        shakeCam.localPosition = camStartPos;
    }
}
