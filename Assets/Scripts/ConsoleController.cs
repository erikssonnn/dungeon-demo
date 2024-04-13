using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class ConsoleController : MonoBehaviour {
    [SerializeField] private GameObject console = null;
    [SerializeField] private Text output = null;
    [SerializeField] private Text inputText= null;
    private static readonly string[] commands = { "debug", "noclip"};
    private bool active = false;
    private InputKey input = new InputKey();
    private MovementController mc = null;
    private string lastCommand = "";
    private bool debug = false;
    private string inputString = "";

    public bool ConsoleActive => active;

    private void Start() {
        mc = FindObjectOfType<MovementController>();
        input = InputController.Instance.InputKey;
        console.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(input.CONSOLE)) {
            if (!DebugController.Instance.Paused)
                return;
            ToggleConsole();
        }

        if (Input.GetKeyDown(input.PREV_COMMAND)) {
            inputString = lastCommand;
        }

        if (active && Input.GetKeyDown(input.ENTER)) {
            Enter();
        }

        if (active) {
            foreach(char c in Input.inputString) {
                if (c == '\b') {
                    if (inputString.Length > 0) {
                        inputString = inputString.Substring(0, inputString.Length - 1);
                    }
                } else if (c == '\u0020') {
                    inputString += " ";
                } else {
                    inputString += c;
                }
            }
            inputText.text = inputString + "_";
        }
    }

    public void ToggleConsole() {
        active = !active;
        console.SetActive(active);
        EventSystem.current.SetSelectedGameObject(inputText.gameObject);
    }

    private static bool IsValidCommand(string str) {
        foreach(string command in commands) {
            if(command.Trim() == str) {
                return true;
            }
        }
        return false;
    }

    private void PrintConsole(string str) {
        var h = output.preferredHeight;

        if(h > output.rectTransform.rect.height) {
            output.text = "<--CLEARED CONSOLE-->\n";
        }

        output.text += str;
    }

    private void Enter() {
        string inputStr = inputText.text.Replace("\n", "").Replace("_", "").Trim();
        lastCommand = inputStr;
        string[] splitStr = inputStr.Split(' ');
        string commandStr = splitStr[0].Trim();

        if (!IsValidCommand(commandStr)) {
            PrintConsole(inputStr + " <-- INVALID COMMAND\n");
            inputString = "";
            return;
        }

        if (splitStr.Length < 2) {
            PrintConsole(inputStr + " <-- COMMAND NEEDS VALUE\n");
            inputString = "";
            return;
        }

        int value = -1;
        if (int.TryParse(splitStr[1], out value)) {
            value = int.Parse(splitStr[1]);
        } else {
            PrintConsole(inputStr + " <-- INVALID VALUE\n");
            inputString = "";
            return;
        }

        if (IsValidCommand(commandStr) && value != -1) {
            PrintConsole(inputStr + "\n");
            ExecuteCommand(commandStr, value);
        }

        inputString = "";
    }

    private void ExecuteCommand(string command, int value) { 
        bool b = value == 1 ? true : false;
        switch (command) {
            case "debug":
                debug = b;
                DebugController.Instance.ToggleDebug(debug);
                // Camera cam = Camera.main;
                // cam.cullingMask = value == 1 ? cam.cullingMask |= (1 << LayerMask.NameToLayer("debug")) : cam.cullingMask &= ~(1 << LayerMask.NameToLayer("debug"));
                break;
            case "noclip":
                mc.enabled = !b;
                mc.GetComponent<CharacterController>().enabled = !b;
                var v = FindObjectsOfType<SwayController>();
                for (int k = 0; k < v.Length; k++) {
                    v[k].enabled = !b;
                }
                FindObjectOfType<FreeCameraController>().enabled = b;
                break;
            default:
                UnityEngine.Debug.LogError("SWITCH CASE FELL BACK TO DEFAULT");
                break;
        }
    }
}
