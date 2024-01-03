using UnityEditor;
using UnityEngine;

public class InteractController : MonoBehaviour {
	[SerializeField] private float interactRange = 5.0f;
	[SerializeField] private LayerMask lm = 0;
	
	private Camera cam = null;
	private UiController ui = null;
	
	private void Start() {
		NullChecker();
	}

	private void NullChecker() {
		ui = UiController.Instance;
		
		cam = Camera.main;
		if (cam == null)
			throw new System.Exception("No camera found!");
	}
	
	private void Update() {
		RaycastCheck();
	}

	private void RaycastCheck() {
		Ray forwardRay = new Ray(cam.transform.position, cam.transform.forward);

		Debug.DrawRay(forwardRay.origin, forwardRay.direction * interactRange, Color.red);
		if (Physics.Raycast(forwardRay, out RaycastHit hit, interactRange, lm)) {
			InteractObject interactObject = hit.transform.GetComponent<InteractObject>();
			if (interactObject == null) {
				throw new System.Exception("No InteractObject script on InteractObject");
			}
			ui.interactImage.enabled = interactObject.ShouldShowInteractUi();
				
			if (Input.GetMouseButtonDown(0)) {
				interactObject.Activate();
			}
		} else {
			ui.interactImage.enabled = false;
		}
	}
}
