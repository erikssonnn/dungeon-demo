using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class GunInfo {
    public string name = "temp";

    [Header("General")]
    public PickupType ammoType = PickupType.BULLETS_SMALL;
    public float reloadSpeed = 1;
    public float fireRate = 0.1f;
    public int magazineSize = 71;
    public int damage = -5;
    
    [Header("Bullets")]
    [Range(1, 10)] public int bulletsPerShot = 1;
    [Range(0.1f, 5.0f)] public float bulletsPerShotSpread = 1;
    
    [Header("Screenshake")]
    public bool screenshake = false;
    public float screenshakeAmount = 15f;
    public float screenshakeTime = 0.1f;
    
    [Header("Spread")]
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
    [SerializeField] private ParticleSystem cartridgeParticleSystem = null;

    private HandController handController = null;
    
    private Animator anim = null;
    private int ammo = 0;
    private UiController uiController = null;
    private Camera cam = null;
    private float nextFire = 0.0f;
    private bool reloading = false;
    private bool onehit = false;
    private float oldSpeed = 0.0f;
    private float spread = 0.0f;
    private int ammoTypeIndex = 0;

    public bool Onehit {
        get => onehit;
        set => onehit = value;
    }
    
    private void Start() {
        ammo = gunInfo.magazineSize;
        ammoTypeIndex = (int)gunInfo.ammoType;

        uiController = UiController.Instance;
        cam = Camera.main;
        handController = GetComponentInParent<HandController>();
        anim = GetComponent<Animator>();
        oldSpeed = anim.speed;
        UpdateGunUi();
    }

    private void Update() {
        Spread();
        Shoot();
        Reload();
    }
    
    // private void OnDrawGizmos() {
    //     if (cam == null)
    //         cam = Camera.main;
    //     Gizmos.color = Color.magenta;
    //     for (int i = -gunInfo.bulletsPerShot; i < gunInfo.bulletsPerShot; i++) {
    //         for (int k = -gunInfo.bulletsPerShot; k < gunInfo.bulletsPerShot; k++) {
    //             Vector3 dir = (cam.transform.forward + cam.transform.rotation * new Vector3(k * (gunInfo.bulletsPerShotSpread * 0.1f), i * (gunInfo.bulletsPerShotSpread * 0.1f), 0)).normalized;
    //             Gizmos.DrawRay(cam.transform.position, (GetSpreadDirection() + dir) * 1000);
    //         }
    //     }
    // }

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
        if (handController.Ammos[ammoTypeIndex].amount <= 0)
            return;

        anim.SetTrigger("reloading");
        anim.speed = gunInfo.reloadSpeed;
        reloading = true;
    }

    public void AReloadReady() {
        int countToFillMag = gunInfo.magazineSize - ammo;
        if (countToFillMag <= handController.Ammos[ammoTypeIndex].amount) {
            ammo += countToFillMag;
            handController.Ammos[ammoTypeIndex].amount -= countToFillMag;
        } else {
            ammo += handController.Ammos[ammoTypeIndex].amount;
            handController.Ammos[ammoTypeIndex].amount = 0;
        }

        anim.ResetTrigger("reloading");
        reloading = false;
        anim.speed = oldSpeed;
        UpdateGunUi();
    }

    public void UpdateGunUi() {
        if (uiController == null)
            return;
        uiController.ammoText.text = ammo + "/" + handController.Ammos[ammoTypeIndex].amount;
        uiController.gunText.text = gunInfo.name;
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
        if (nextFire > Time.time) {
            anim.SetBool("fire", false);
            return;
        }
        if (ammo <= 0) {
            anim.SetBool("fire", false);
            //play funny sound
            return;
        }

        if (cartridgeParticleSystem != null) {
            cartridgeParticleSystem.Play();
        }

        nextFire = Time.time + gunInfo.fireRate;

        //flash
        GameObject flash = Instantiate(muzzleFlashPrefab);
        Vector3 rot = muzzleFlashOrigin.transform.eulerAngles + new Vector3(0, 0, Random.Range(-180, 180));
        flash.transform.SetPositionAndRotation(muzzleFlashOrigin.transform.position, Quaternion.Euler(rot));
        flash.transform.SetParent(muzzleFlashOrigin.transform, true);
        Destroy(flash, 0.05f);

        ammo--;
        anim.SetBool("fire", true);
        UpdateGunUi();
        
        if (gunInfo.screenshake) {
            ScreenShake.Instance.StartCoroutine(ScreenShake.Instance.Shake(gunInfo.screenshakeAmount, gunInfo.screenshakeTime));
        }

        if (gunInfo.bulletsPerShot == 1) {
            GameObject newLine = Instantiate(line);
            newLine.transform.position = muzzleFlashOrigin.transform.position;
            newLine.transform.eulerAngles = cam.transform.eulerAngles;
            newLine.transform.SetParent(cam.transform, true);
                
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
            return;
        }
        
        for (int i = -gunInfo.bulletsPerShot; i < gunInfo.bulletsPerShot; i++) {
            for (int k = -gunInfo.bulletsPerShot; k < gunInfo.bulletsPerShot; k++) {
                GameObject newLine = Instantiate(line);
                newLine.transform.position = muzzleFlashOrigin.transform.position;
                newLine.transform.eulerAngles = cam.transform.eulerAngles;
                newLine.transform.SetParent(cam.transform, true);
                
                Vector3 dir = (cam.transform.forward + cam.transform.rotation * new Vector3(k * (gunInfo.bulletsPerShotSpread * 0.1f), i * (gunInfo.bulletsPerShotSpread * 0.1f), 0)).normalized;
                Ray forwardRay = new Ray(cam.transform.position, GetSpreadDirection() + dir);
                if (!Physics.Raycast(forwardRay, out RaycastHit hit, Mathf.Infinity, lm))
                    continue;
                newLine.transform.LookAt(hit.point);
                if ((1 << LayerMask.NameToLayer("Monster") & (1 << hit.transform.gameObject.layer)) == 0)
                    continue;
            
                GameObject newBlood = Instantiate(blood, hit.point, Quaternion.identity);
                newBlood.transform.LookAt(cam.transform.position);
                newBlood.transform.SetParent(hit.transform, true);
            
                MonsterController monsterController = hit.transform.GetComponentInParent<MonsterController>();
                if (monsterController == null)
                    continue;
                monsterController.UpdateHealth(Onehit ? -10000 : gunInfo.damage);
            }
        }
    }
}