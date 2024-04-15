using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class GunInfo {
    public string name = "temp";
    public int startAmmo = 200;
    public float reloadSpeed = 1;

    public float fireRate = 0.1f;
    public int magazineSize = 71;
    public int damage = -5;
    public float spreadTangent = 0.01f;
    public float spreadMultiplier = 2.0f;
    public float maxSpread = 0.5f;
}

public class GunController : MonoBehaviour {
    [Header("GUN INFO: ")]
    [SerializeField] private GunInfo gunInfo;
    
    [Header("ASSIGNABLES: ")]
    [SerializeField] private LayerMask lm = 0;

    [SerializeField] private GameObject blood = null;
    [SerializeField] private GameObject line = null;

    [SerializeField] private GameObject muzzleFlashPrefab = null;
    [SerializeField] private GameObject muzzleFlashOrigin = null;
    

    private Animator anim = null;
    private int ammo = 0;
    private int ammoReserve = 0;
    private UiController uiController = null;
    private Camera cam = null;
    private float nextFire = 0.0f;
    private bool reloading = false;
    private bool onehit = false;
    private float oldSpeed = 0.0f;
    private float spread = 0.0f;

    public bool Onehit {
        get => onehit;
        set => onehit = value;
    }


    private void Start() {
        ammoReserve = gunInfo.startAmmo;
        ammo = gunInfo.magazineSize;

        uiController = UiController.Instance;
        cam = Camera.main;
        anim = GetComponent<Animator>();
        oldSpeed = anim.speed;
        UpdateGunUi();
    }

    private void Update() {
        Spread();
        Shoot();
        Reload();
    }

    private void Spread() {
        if (Input.GetMouseButton(0)) {
            spread += gunInfo.spreadTangent * gunInfo.spreadMultiplier * Time.deltaTime;
            if(spread > gunInfo.maxSpread)
                spread = gunInfo.maxSpread;
            return;
        }
        
        spread -= gunInfo.spreadTangent * gunInfo.spreadMultiplier * 5 * Time.deltaTime;
        if(spread < gunInfo.spreadTangent)
            spread = gunInfo.spreadTangent;
    }

    private void Reload() {
        if (ammo >= gunInfo.magazineSize)
            return;
        if (!Input.GetKeyDown(KeyCode.R))
            return;
        if (ammoReserve <= 0)
            return;

        anim.SetTrigger("reloading");
        anim.speed = gunInfo.reloadSpeed;
        reloading = true;
    }

    public void AReloadReady() {
        int countToFillMag = gunInfo.magazineSize - ammo;
        if (countToFillMag <= ammoReserve) {
            ammo += countToFillMag;
            ammoReserve -= countToFillMag;
        } else {
            ammo += ammoReserve;
            ammoReserve = 0;
        }

        anim.ResetTrigger("reloading");
        reloading = false;
        anim.speed = oldSpeed;
        UpdateGunUi();
    }

    public void UpdateGunUi() {
        if (uiController == null)
            return;
        uiController.ammoText.text = ammo + "/" + ammoReserve;
        uiController.gunText.text = name;
    }

    private Vector3 GetSpreadDirection() {
        Vector3 direction = cam.transform.forward;
        direction.x += Random.Range(-spread, spread);
        direction.y += Random.Range(-spread, spread);
        direction.z += Random.Range(-spread, spread);

        return direction;
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
        newLine.transform.eulerAngles = cam.transform.eulerAngles;
        newLine.transform.SetParent(cam.transform, true);

        nextFire = Time.time + gunInfo.fireRate;

        //flash
        GameObject flash = Instantiate(muzzleFlashPrefab);
        Vector3 rot = muzzleFlashOrigin.transform.eulerAngles + new Vector3(0, 0, Random.Range(-180, 180));
        flash.transform.SetPositionAndRotation(muzzleFlashOrigin.transform.position, Quaternion.Euler(rot));
        flash.transform.SetParent(muzzleFlashOrigin.transform, true);
        Destroy(flash, gunInfo.fireRate);

        ammo--;
        anim.SetBool("fire", true);
        ScreenShake.Instance.StartCoroutine(ScreenShake.Instance.Shake(0.5f, 0.1f));
        UpdateGunUi();

        Ray forwardRay = new Ray(cam.transform.position, GetSpreadDirection());
        if (!Physics.Raycast(forwardRay, out RaycastHit hit, Mathf.Infinity, lm))
            return;

        newLine.transform.LookAt(hit.point);
        if ((1 << LayerMask.NameToLayer("Monster") & (1 << hit.transform.gameObject.layer)) == 0)
            return;
        
        GameObject newBlood = Instantiate(blood, hit.point, Quaternion.identity);
        newBlood.transform.LookAt(cam.transform.position);
        newBlood.transform.SetParent(hit.transform, true);
        
        MonsterController monsterController = hit.transform.GetComponentInParent<MonsterController>();
        if (monsterController == null)
            return;
        monsterController.UpdateHealth(Onehit ? -10000 : gunInfo.damage);
    }
}