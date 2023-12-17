using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class SpellController : MonoBehaviour {
    [Header("ASSIGNABLE: ")] 
    [SerializeField] private Transform targetObject = null;

    [SerializeField] private float range = 0.0f;
    [SerializeField] private LayerMask lm = 0;
    [SerializeField] private float pullForce = 0.0f;
    [SerializeField] private float pushForce = 0.0f;

    private Camera cam = null;
    private Rigidbody hitObject = null;
    private Transform lookAtObject = null;

    private void Start() {
        NullChecker();
    }

    private void NullChecker() {
        cam = Camera.main;
        if (cam == null)
            throw new Exception("No camera found!");
    }

    private void Update() {
        SpellCasting();
        Hold();
    }

    private void SpellCasting() {
        Ray forwardRay = new Ray(cam.transform.position, cam.transform.forward);

        Debug.DrawRay(forwardRay.origin, forwardRay.direction * range, Color.red);
        if (Physics.Raycast(forwardRay, out RaycastHit hit, range, lm)) {
            if (Input.GetMouseButtonDown(1)) {
                hitObject = hit.transform.GetComponent<Rigidbody>();
            }
        }

        if (hitObject == null) {
            return;
        }
        
        lookAtObject = hit.transform;

        // throw
        if (Input.GetMouseButtonDown(0)) {
            Throw();
        }

        // release
        if (Input.GetMouseButtonUp(1)) {
            hitObject.constraints = RigidbodyConstraints.None;
            hitObject = null;
        }
    }

    private void Throw() {
        Rigidbody tempObject = hitObject;
        hitObject = null;

        tempObject.constraints = RigidbodyConstraints.None;
        tempObject.velocity += cam.transform.forward * pushForce;
    }

    private void Hold() {
        if (hitObject == null) return;

        hitObject.constraints = RigidbodyConstraints.FreezeRotation;
        Vector3 dir = (targetObject.position - hitObject.position).normalized;
        float dist = Vector3.Distance(targetObject.position, hitObject.position);
        dist = Mathf.Clamp(dist, 0.0f, pullForce);

        hitObject.velocity = Vector3.Lerp(hitObject.velocity, dir * (pullForce * dist), Time.deltaTime * 50);
    }

    private void OnGUI() {
        GUI.Label(new Rect(10, 0, 200, 20), "HOLDING: " + (hitObject == null ? "NULL" : hitObject.ToString()));
        GUI.Label(new Rect(10, 20, 200, 20), "VELOCITY: " + (hitObject == null ? "NULL" : hitObject.GetComponent<Rigidbody>().velocity.magnitude.ToString("F3")));
        GUI.Label(new Rect(10, 40, 200, 20), "LOOKING: " + (lookAtObject == null ? "NULL" : lookAtObject.ToString()));
    }
}