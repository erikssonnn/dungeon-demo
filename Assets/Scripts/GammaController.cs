using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = erikssonn.Logger;

public class GammaController : MonoBehaviour {
	[SerializeField] private UnityEngine.UI.Slider slider = null;
	[SerializeField] private Settings settings = null;

	public void UpdateGammaValue() {
		float val = 1.0f / (float)slider.value;
		Color col = new Color(val, val, val, 1);
		RenderSettings.ambientLight = col;
		settings.gammaColor = col;
	}

	private void Start() {
		if (SceneManager.GetActiveScene().buildIndex == 0) {
			if (settings.gammaColor != Color.blue) {
				SceneManager.LoadScene(1);
			}
		}
		// if (SceneManager.GetActiveScene().buildIndex == 1) {
		// 	Logger.Print("Loaded main scene, set gamma");
		// 	RenderSettings.ambientLight = settings.gammaColor;
		// }
	}
}
