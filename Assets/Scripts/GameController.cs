using System;
using UnityEngine;

public enum Gamemode { SCORE, TIME, LIMITLESS }

public class GameController : MonoBehaviour {
	[Header("Player")]
	[SerializeField] private int startHealth = 0;

	[Header("Gamemode")]
	[SerializeField] private Gamemode gamemode = Gamemode.SCORE;
	[SerializeField] private int scoreGoal = 0;
	
	private int health = 0;
	private bool god = false;
	private int score = 0;
	private float timer = 0;
	
	private UiController uiController;

	public bool God {
		get => god;
		set => god = value;
	}

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
		uiController = UiController.Instance;
		if (uiController == null)
			throw new Exception("Cant find uiController instance");
		
		UpdateHealth(startHealth);
		UpdateGamemodeText();
	}

	private void Update() {
		if (gamemode != Gamemode.TIME) 
			return;
		timer += Time.deltaTime;
		UpdateGamemodeText();
	}

	private void UpdateGamemodeText() {
		uiController.gamemodeText.text = gamemode switch {
			Gamemode.SCORE => "Score: " + score + "/" + scoreGoal,
			Gamemode.TIME => "Time: " + timer + "s",
			Gamemode.LIMITLESS => "Score (Limitless): " + score,
			_ => throw new Exception("Gamemode switch default case hit!")
		};
	}

	public void UpdateScore(int amount) {
		score += amount;
		UpdateGamemodeText();
	}
	
	public void UpdateHealth(int amount) {
		if (god)
			return;
		
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
