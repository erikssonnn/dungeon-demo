using System;
using UnityEngine;

public class VersionController : MonoBehaviour {
	public const string Version = "pre-release 0.1";
	private static VersionController instance;

	public static VersionController Instance {
		get {
			instance = FindObjectOfType<VersionController>();
			if (instance != null) 
				return instance;
			GameObject obj = new GameObject("VersionController");
			instance = obj.AddComponent<VersionController>();
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

	private void OnEnable() {
		UiController.Instance.versionText.text = Version;
	}
}
