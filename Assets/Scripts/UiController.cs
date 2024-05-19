using System;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour {
    public Image healthBar = null;
    public Image interactImage = null;
    public Text ammoText = null;
    public Text gunText = null;
    public Text versionText = null;

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
        if (healthBar == null)
            throw new Exception("staminaBar image is null");
        if (interactImage == null)
            throw new Exception("interactImage image is null");
    }
}