using System;
using UnityEngine;
using Logger = erikssonn.Logger;

public class HandController : MonoBehaviour {
    [SerializeField] private GunController[] guns = null;

    private int currentIndex = 0;

    private void Start() {
        SetGun(0);
    }

    private void Update() {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        int oldIndex = currentIndex;
    
        if (scroll > 0.0f) {
            currentIndex++;
            if (currentIndex > guns.Length - 1)
                currentIndex = guns.Length - 1;
        } else if (scroll < 0.0f) {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = 0;
        }
    
        if (oldIndex == currentIndex)
            return;
        SetGun(currentIndex);
    }

    private void SetGun(int index) {
        foreach (GunController gun in guns) {
            gun.gameObject.SetActive(false);
        }

        guns[index].gameObject.SetActive(true);
        guns[index].UpdateGunUi();
        currentIndex = index;
    }
}