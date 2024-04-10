using System;
using UnityEditor.Experimental.GraphView;
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
    [SerializeField] private float spread = 0.5f;

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
        DebugShowRay();
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

    private Vector3 GetSpreadDirection() {
        Vector3 direction = cam.transform.forward;
        direction.x += Random.Range(-spread, spread);
        direction.y += Random.Range(-spread, spread);
        direction.z += Random.Range(-spread, spread);

        return direction;
    }

    private void DebugShowRay() {
        Vector3 direction = GetSpreadDirection();

        Ray forwardRay = new Ray(cam.transform.position, direction);
        Debug.DrawRay(forwardRay.origin, direction * 1000, Color.green);
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
        newLine.transform.rotation = Quaternion.identity;

        LineRenderer ren = newLine.GetComponent<LineRenderer>();
        ren.positionCount = 2;
        ren.SetPosition(0, muzzleFlashOrigin.transform.position);
        ren.SetPosition(1, GetSpreadDirection() * 100);

        nextFire = Time.time + fireRate;

        //flash
        GameObject flash = Instantiate(muzzleFlashPrefab);
        Vector3 rot = muzzleFlashOrigin.transform.eulerAngles + new Vector3(0, 0, Random.Range(-180, 180));
        flash.transform.SetPositionAndRotation(muzzleFlashOrigin.transform.position, Quaternion.Euler(rot));
        flash.transform.SetParent(muzzleFlashOrigin.transform, true);
        Destroy(flash, fireRate);

        ammo--;
        anim.SetBool("fire", true);
        // ScreenShake.Instance.StartCoroutine(ScreenShake.Instance.Shake(0.5f, 0.1f));
        UpdateAmmoUi();

        Ray forwardRay = new Ray(cam.transform.position, GetSpreadDirection());
        if (!Physics.Raycast(forwardRay, out RaycastHit hit, Mathf.Infinity, lm))
            return;

        ren.SetPosition(1, hit.point);
        if ((1 << LayerMask.NameToLayer("Monster") & (1 << hit.transform.gameObject.layer)) == 0)
            return;

        Instantiate(blood, hit.point, Quaternion.LookRotation(transform.position - cam.transform.position));
        MonsterController monsterController = hit.transform.GetComponentInParent<MonsterController>();
        if (monsterController == null)
            return;
        monsterController.UpdateHealth(damage);
    }
}