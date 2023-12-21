using System;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour {
    public Image staminaBar = null;
    public Image batteryBar = null;

    private static UiController instance;

    public static UiController Instance {
        get {
            instance = FindObjectOfType<UiController>();
            if (instance != null) return instance;
            GameObject obj = new GameObject("UiController");
            instance = obj.AddComponent<UiController>();
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
        if (staminaBar == null)
            throw new Exception("staminaBar image is null");
        if (batteryBar == null)
            throw new Exception("batteryBar image is null");
    }
}