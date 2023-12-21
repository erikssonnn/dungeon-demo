using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Viewbob : MonoBehaviour {
    [Header("Viewbobbing: ")]
    [Tooltip("The strength of Bob. No but just the amount each view'bob' moves")]
    [SerializeField] private float bobStrength = 0.5f;
    [Tooltip("how fast the viewbobbing moves, should be in sync with movement sounds")]
    [SerializeField] private float bobSpeed = 2f;

    private Vector3 origin = Vector3.zero;
    private Vector3 dest = Vector3.zero;
    private float time = -1.0f;
    private bool down = false;

    // private AudioController am = null;
    private CharacterController cc;

    private void Start() {
        // am = FindObjectOfType<AudioController>();
        cc = GetComponentInParent<CharacterController>();
        origin = transform.localPosition;
    }

    private void LateUpdate() {
        if (cc.isGrounded && cc.enabled) {
            Bobbing();
        }
    }

    private void Bobbing() {
        float vel = cc.velocity.magnitude;

        time = Mathf.PingPong(Time.time * bobSpeed, 2.0f) - 1.0f;
        dest = (vel * 2.0f) * new Vector3(0, -Mathf.Sin(time * time * (bobStrength * 0.001f)), Mathf.Sin(time * (bobStrength * 0.001f)));

        if (transform.localPosition.y <= -0.04f && !down) {
            down = true;
            // am.PlayAudio(am.GetStepSound(), -0.1f, 0.3f, null);
        } else if (transform.localPosition.y > -0.03f) {
            down = false;
        }

        transform.localPosition = origin + dest;
    }
}

