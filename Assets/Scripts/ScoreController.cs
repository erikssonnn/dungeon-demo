using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour {
	[SerializeField] private Text scoreText = null;

	private int score = 0;
	private static ScoreController instance;

	public static ScoreController Instance {
		get {
			instance = FindObjectOfType<ScoreController>();
			if (instance != null) return instance;
			GameObject obj = new GameObject("ScoreController");
			instance = obj.AddComponent<ScoreController>();
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
		score = 0;
		if (scoreText == null) {
			throw new System.Exception("scoreText is not assigned!");
		}

		UpdateScore(0);
	}

	public void UpdateScore(int value) {
		score += value;
		if (score < 0)
			score = 0;

		scoreText.text = "$" + score;
	}
}
