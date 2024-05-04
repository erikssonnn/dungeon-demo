using System;
using UnityEngine;
using Logger = erikssonn.Logger;

public enum PickupType {
    BULLETS_SMALL,
    BULLETS_BIG,
    BULLETS_SLUGS
}

public class PickupObject : MonoBehaviour {
    [SerializeField] private PickupType pickupType = 0;
    [SerializeField] private int amount = 0;

    [Header("Bobbing settings: ")]
    [SerializeField] private float bobSpeed = 0.0f;

    [SerializeField] private float bobStrength = 0.0f;
    [SerializeField] private GameObject model = null;

    private Vector3 origin = Vector3.zero;
    private Vector3 dest = Vector3.zero;
    private float time = -1.0f;
    private HandController handController = null;
    
    private void Start() {
        handController = FindObjectOfType<HandController>();
        origin = model.transform.localPosition;
    }

    private void LateUpdate() {
        PickupBobbing();
    }

    private void PickupBobbing() {
        time = Mathf.PingPong(Time.time * bobSpeed, 2.0f) - 1.0f;
        dest = new Vector3(0.0f, time * bobStrength, 0.0f);

        model.transform.localPosition = origin + dest;
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) {
            return;
        }

        // TODO: rewrite this please, wtf even is this
        foreach (Ammo t in handController.Ammos) {
            if (t.ammoType == pickupType) {
                t.amount += amount;
            }
        }

        GunController[] guns = FindObjectsOfType<GunController>();
        foreach (GunController t in guns) {
            t.UpdateGunUi();
        }

        Destroy(gameObject);
    }
}