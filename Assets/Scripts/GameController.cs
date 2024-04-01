using System;
using UnityEngine;

public class GameController : MonoBehaviour {
	[SerializeField] private int startHealth = 0;
	
	private int health = 0;
	private static GameController instance;

	public static GameController Instance {
		get {
			instance = FindObjectOfType<GameController>();
			if (instance != null) return instance;
			GameObject obj = new GameObject("GameController");
			instance = obj.AddComponent<GameController>();
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
		UpdateHealth(startHealth);
	}

	public void UpdateHealth(int amount) {
		health += amount;

		float percentageHealth = (float)health / (float)startHealth;
		UiController.Instance.healthBar.fillAmount = percentageHealth;

		if (health <= 0)
			Die();
	}

	private void Die() {
		Debug.LogError("PLAYER DIED!");
		Debug.Break();
	}
}
