using UnityEngine;

[System.Serializable]
public struct InputKey {
	public KeyCode ACTIVATE_1;
	public KeyCode ACTIVATE_2;
	public KeyCode JUMP;
	public KeyCode CONSOLE;
	public KeyCode RELOAD;
	public KeyCode ENTER;
	public KeyCode PREV_COMMAND;
	public KeyCode PAUSE;
}

public class InputController : MonoBehaviour {
	[SerializeField] private InputKey inputKeys;
	public static InputController Instance { get; private set; }
	public InputKey InputKey => inputKeys;

	void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(this);
		} else {
			Instance = this;
			DontDestroyOnLoad(this);
		}
	}
}
