using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SmgController : MonoBehaviour {
    [Header("ASSIGNABLES: ")]
    [SerializeField] private LayerMask lm = 0;
    [SerializeField] private GameObject blood = null;
    [SerializeField] private GameObject line = null;

    [SerializeField] private GameObject muzzleFlashPrefab = null;
    [SerializeField] private GameObject muzzleFlashOrigin = null;

    [Header("TWEAKABLES: ")]
    [SerializeField] private int startAmmo = 200;

    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private int magazineSize = 71;
    [SerializeField] private int damage = -5;

    private Animator anim = null;
    private int ammo = 0;
    private int ammoReserve = 0;
    private UiController uiController = null;
    private Camera cam = null;
    private float nextFire = 0.0f;
    private bool reloading = false;

    private void Start() {
        ammoReserve = startAmmo;
        ammo = magazineSize;

        uiController = UiController.Instance;
        cam = Camera.main;
        anim = GetComponent<Animator>();
        UpdateAmmoUi();
    }

    private void Update() {
        Shoot();
        Reload();
    }

    private void Reload() {
        if (ammo >= magazineSize)
            return;
        if (!Input.GetKeyDown(KeyCode.R))
            return;

        anim.SetTrigger("reloading");
        reloading = true;
    }

    public void AReloadReady() {
        int countToFillMag = magazineSize - ammo;
        if (countToFillMag <= ammoReserve) {
            ammo += countToFillMag;
            ammoReserve -= countToFillMag;
        } else {
            ammo += ammoReserve;
            ammoReserve = 0;
        }

        anim.ResetTrigger("reloading");
        reloading = false;
        UpdateAmmoUi();
    }

    private void UpdateAmmoUi() {
        uiController.ammoText.text = ammo + "/" + ammoReserve;
    }

    private void Shoot() {
        if (reloading)
            return;
        
        if (!Input.GetMouseButton(0)) {
            anim.SetBool("fire", false);
            return;
        }

        if (nextFire > Time.time)
            return;
        if (ammo <= 0) {
            anim.SetBool("fire", false);
            //play funny sound
            return;
        }

        GameObject newLine = Instantiate(line);
        newLine.transform.position = muzzleFlashOrigin.transform.position;
        newLine.transform.rotation = cam.transform.rotation;

        nextFire = Time.time + fireRate;
        
        GameObject flash = Instantiate(muzzleFlashPrefab);
        Vector3 rot = muzzleFlashOrigin.transform.eulerAngles + new Vector3(0, 0, Random.Range(-180, 180));
        flash.transform.SetPositionAndRotation(muzzleFlashOrigin.transform.position, Quaternion.Euler(rot));
        flash.transform.SetParent(muzzleFlashOrigin.transform, true);
        Destroy(flash, fireRate);
        
        ammo--;
        anim.SetBool("fire", true);
        // ScreenShake.Instance.StartCoroutine(ScreenShake.Instance.Shake(0.5f, 0.1f));
        UpdateAmmoUi();

        Ray forwardRay = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(forwardRay.origin, forwardRay.direction, Color.green);
        if (!Physics.Raycast(forwardRay, out RaycastHit hit, Mathf.Infinity, lm))
            return;
        
        Instantiate(blood, hit.point, Quaternion.LookRotation(transform.position - cam.transform.position));
        MonsterController monsterController = hit.transform.GetComponentInParent<MonsterController>();
        if (monsterController == null)
            return;
        monsterController.UpdateHealth(damage);
    }
}