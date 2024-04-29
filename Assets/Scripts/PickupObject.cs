using System;
using UnityEngine;
using Logger = erikssonn.Logger;

public class PickupObject : MonoBehaviour {
	[SerializeField] private int index = 0;

	[Header("Bobbing settings: ")]
	[SerializeField] private float bobSpeed = 0.0f;
	[SerializeField] private float bobStrength = 0.0f;
	[SerializeField] private GameObject model = null;

	private Vector3 origin = Vector3.zero;
	private Vector3 dest = Vector3.zero;
	private float time = -1.0f;

	private void Start() {
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
	}
}
