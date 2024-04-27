using System;
using UnityEngine;
using Logger = erikssonn.Logger;

public class HandController : MonoBehaviour {
    [SerializeField] private GunController[] guns = null;
    [SerializeField] private GameObject[] gunPrefabs = null;

    private int currentIndex = 0;

    private void Start() {
        SetGun(0, null);
    }

    // private void Update() {
    //     float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
    //     int oldIndex = currentIndex;
    //
    //     if (scroll > 0.0f) {
    //         currentIndex++;
    //         if (currentIndex > guns.Length - 1)
    //             currentIndex = guns.Length - 1;
    //     } else if (scroll < 0.0f) {
    //         currentIndex--;
    //         if (currentIndex < 0)
    //             currentIndex = 0;
    //     }
    //
    //     if (oldIndex == currentIndex)
    //         return;
    //     SetGun(currentIndex);
    // }

    public void SetGun(int index, GameObject refGun) {
        foreach (GunController gun in guns) {
            gun.gameObject.SetActive(false);
        }

        if (index == currentIndex) {
            return;
        }
        
        DropGun();
        if (refGun != null) {
            Destroy(refGun);
        }
        guns[index].gameObject.SetActive(true);
        guns[index].UpdateGunUi();
        currentIndex = index;
    }

    private void DropGun() {
        GameObject droppedGun = Instantiate(gunPrefabs[currentIndex]);
        droppedGun.transform.SetPositionAndRotation(transform.position - new Vector3(0, 1, 0), Quaternion.identity);
    }
}