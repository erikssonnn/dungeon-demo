using System;
using erikssonn;
using UnityEngine;
using Logger = erikssonn.Logger;

public enum Gamemode { SCORE, TIME, LIMITLESS }
public enum EndGameReason { DIED, TIME_WIN, SCORE_WIN }

public class GameController : MonoBehaviour {
	[Header("Player")]
	[SerializeField] private int startHealth = 0;

	[Header("Gamemode")]
	[SerializeField] private Gamemode gamemode = Gamemode.SCORE;
	[SerializeField] private int goal = 0; // can be either start time or goal score
	
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

		if (gamemode == Gamemode.TIME) {
			timer = goal;
		}
		
		UpdateHealth(startHealth);
		UpdateGamemodeText();
	}

	private void Update() {
		if (gamemode == Gamemode.SCORE) 
			return;
		timer += gamemode == Gamemode.LIMITLESS ? Time.deltaTime : -Time.deltaTime;
		TimeWinCheck();
		UpdateGamemodeText();
	}

	private void TimeWinCheck() {
		if (gamemode != Gamemode.TIME)
			return;
		if (timer <= 0) {
			EndGame(EndGameReason.TIME_WIN);
		}
	}

	private void UpdateGamemodeText() {
		uiController.gamemodeText.text = gamemode switch {
			Gamemode.SCORE => "Score: " + score + "/" + goal,
			Gamemode.TIME => "Time: " + timer.ToString("F1") + "s",
			Gamemode.LIMITLESS => "Score (Limitless): " + score,
			_ => throw new Exception("Gamemode switch default case hit!")
		};
	}

	public void UpdateScore(int amount) {
		if (gamemode == Gamemode.TIME)
			return;
		score += amount;
		UpdateGamemodeText();
		if (gamemode != Gamemode.SCORE)
			return;
		if (score >= goal) {
			EndGame(EndGameReason.SCORE_WIN);
		}
	}
	
	public void UpdateHealth(int amount) {
		if (god)
			return;
		
		health += amount;
		float percentageHealth = (float)health / (float)startHealth;
		UiController.Instance.healthBar.fillAmount = percentageHealth;

		if (amount < 0) {
			ScreenShake.Instance.StartCoroutine(ScreenShake.Instance.Shake(50, 0.25f));
		}

		if (health <= 0)
			Die();
	}

	private static void EndGame(EndGameReason reason) {
		Logger.Print(reason.ToString(), LogLevel.FATAL);
	}

	private static void Die() {
		EndGame(EndGameReason.DIED);
	}
}
