using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugController : MonoBehaviour {
    [SerializeField] private GameObject debugObject = null;

    public static DebugController Instance;

    private Camera cam = null;
    private bool debug = false;
    private bool paused = false;

    private CharacterController characterController = null;
    private ConsoleController consoleController = null;
    public bool Paused => paused; 
    
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start() {
        cam = Camera.main;
        if (cam == null) {
            throw new Exception("Cant find main camera");
        }
        characterController = FindObjectOfType<CharacterController>();
        if (characterController == null) {
            throw new Exception("Cant find CharacterController");
        }
        consoleController = FindObjectOfType<ConsoleController>();
        if (consoleController == null) {
            throw new Exception("Cant find ConsoleController");
        }
    }

    public void SetPosition(Vector3 pos) {
        GameObject newDebugObject = Instantiate(debugObject, pos, Quaternion.identity);
        Destroy(newDebugObject, 5f);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Pause();
        }
    }

    private void Pause() {
        paused = !paused;
        if (!paused && consoleController.ConsoleActive) 
            consoleController.ToggleConsole();

        FindObjectOfType<MovementController>().enabled = !paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
        Time.timeScale = paused ? 0.0f : 1.0f;
    }

    public void ToggleDebug(bool onOff) {
        debug = onOff;

        if (debug) {
            cam.cullingMask |= 1 << LayerMask.NameToLayer("Debug");
        }
        else {
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer("Debug"));
        }
    }

    private void OnGUI() {
        if (!debug)
            return;
        string debugString = "vel: " + characterController.velocity.magnitude.ToString("F2");
        GUI.Label(new Rect(10, 10, 100, 100), debugString);
    }
}