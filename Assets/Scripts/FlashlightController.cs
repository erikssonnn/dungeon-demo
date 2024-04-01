using System;
using UnityEngine;

public class FlashlightController : MonoBehaviour {
	// [SerializeField] private GameObject flashLightObjects = null;
	// [SerializeField] private float maxBattery = 90.0f;
	//
	// private bool active = false;
	// private float battery = 0.0f;
	// private UiController ui = null;
	//
	// private void Start() {
	// 	battery = maxBattery;
	// 	NullChecker();
	// }
	//
	// private void NullChecker() {
	// 	active = false;
	// 	ui = UiController.Instance;
	// 	if (flashLightObjects == null) {
	// 		throw new Exception("flashLightObjects is null on " + this);
	// 	}
	// }
	//
	// private void Update() {
	// 	if (active) {
	// 		battery -= Time.fixedDeltaTime;
	// 		ui.batteryBar.fillAmount = battery / maxBattery;
	// 		
	// 		if (battery <= 0.0f) {
	// 			flashLightObjects.SetActive(false);
	// 		}
	// 	}
	// 	
	// 	if (Input.GetMouseButtonDown(1)) {
	// 		ToggleFlashlight();
	// 	}
	// }
	//
	// private void ToggleFlashlight() {
	// 	if (battery <= 0.0f) {
	// 		// play funny sound
	// 		return;
	// 	}
	// 	
	// 	active = !active;
	// 	flashLightObjects.SetActive(active);
	// }
}
