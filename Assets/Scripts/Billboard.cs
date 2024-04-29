using erikssonn;
using UnityEngine;
using Logger = erikssonn.Logger;

public class Billboard : MonoBehaviour {

	private Camera cam = null;

	private void Start() {
		cam = Camera.main;
		if (cam == null) {
			Logger.Print("Cant find the main camera on " + this, LogLevel.ERROR);
		}
	}

	private void LateUpdate() {
		if (cam == null)
			return;

		Vector3 dir = cam.transform.position - transform.position;
		if (dir == Vector3.zero)
			return;
		
		Quaternion rot = Quaternion.LookRotation(dir);
		Vector3 targetEulerAngles = rot.eulerAngles;
		transform.rotation = Quaternion.Euler(90.0f, targetEulerAngles.y, 0.0f);
	}
}
