using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = erikssonn.Logger;

[Serializable]
public class MeleeDisableMonobehaviours {
    public MonoBehaviour script = null;
    [FormerlySerializedAs("wasOn")] [HideInInspector] public bool scriptWasEnabled = false;
    public bool useGameObject = false;
    [HideInInspector] public bool gameObjectWasEnabled = false;
}

public class MeleeController : MonoBehaviour {
    #region Variable struct
    [Serializable]
    public struct MeleeVariables {
        [Tooltip("Enable visual in unity Editor, useful for when changing variables")]
        public bool showDebug;
        [Tooltip("The range of the melee attack")]
        public float range;
        [Tooltip("Amount of raycasts to detect enemies when using melee, high number means more possibilities to hit enemies")] [Range(1.0f, 10.0f)]
        public int rayAmount;
        [Tooltip("The spread of the 'rayAmount' raycasts. High number means more distance between each ray, these two go hand in hand")] [Range(0.5f, 5.0f)]
        public float raySpread;
    }
    #endregion

    [SerializeField] private LayerMask lm = new LayerMask();
    [SerializeField] private MeleeDisableMonobehaviours[] monobehaviours = null;

    [SerializeField] private MeleeVariables meleeVar = new MeleeVariables();
    [SerializeField] private int damage = 100;
    [SerializeField] private Animator meleeAnim = null;
    
    private Vector3 origin = Vector3.zero;
    private Camera cam = null;
    private bool meleeing = false;

    private void Start() {
        cam = Camera.main;
    }

    private void OnDrawGizmos() {
        if (cam == null)
            cam = Camera.main;
        if (!meleeVar.showDebug) 
            return;
        Gizmos.color = Color.magenta;
        for (int i = -meleeVar.rayAmount; i < meleeVar.rayAmount; i++) {
            for (int j = -meleeVar.rayAmount; j < meleeVar.rayAmount; j++) {
                Vector3 dir = (cam.transform.forward + cam.transform.rotation * new Vector3(j * (meleeVar.raySpread * 0.1f), i * (meleeVar.raySpread * 0.1f), 0)).normalized;
                Gizmos.DrawRay(origin, dir * meleeVar.range);
            }
        }
    }

    private void Update() {
        origin = cam.transform.position;
        MeleeInitiator();
    }

    private void StartMelee() {
        StartCoroutine(ResetMelee());
        float temp = 69.0f; // lmao
        Transform hitObj = null;
        RaycastHit lateHit = new RaycastHit();

        for (int i = -meleeVar.rayAmount; i < meleeVar.rayAmount; i++) {
            for (int j = -meleeVar.rayAmount; j < meleeVar.rayAmount; j++) {
                Vector3 dir = (cam.transform.forward + cam.transform.rotation * new Vector3(j * (meleeVar.raySpread * 0.1f), i * (meleeVar.raySpread * 0.1f), 0)).normalized;

                Ray ray = new Ray(origin, dir);

                if (!Physics.Raycast(ray, out RaycastHit hit, meleeVar.range, lm))
                    continue;
                float dist = Vector3.Distance(origin, hit.point);

                if (i < -meleeVar.rayAmount + 1) {
                    temp = dist;
                }

                if (!(dist < temp))
                    continue;
                temp = dist;
                lateHit = hit;
                hitObj = lateHit.transform;
            }
        }

        if (hitObj == null) {
            return;
        }

        StartCoroutine(ScreenShake.Instance.Shake(4f, 0.2f));
        
        MonsterController hitMonster = hitObj.transform.GetComponentInParent<MonsterController>();
        if (hitMonster == null) {
            return;
        }

        hitMonster.UpdateHealth(-damage);
    }

    private IEnumerator ResetMelee() {
        yield return new WaitForSeconds(0.2f);
        meleeing = false;
        foreach (MeleeDisableMonobehaviours behaviour in monobehaviours) {
            if (behaviour.useGameObject) {
                if (behaviour.gameObjectWasEnabled) {
                    behaviour.script.gameObject.SetActive(true);
                }
            } else {
                if (behaviour.scriptWasEnabled) {
                    behaviour.script.enabled = true;
                }
            }
        }
        meleeAnim.SetBool("Meleeing", meleeing);
    }

    private void MeleeInitiator() {
        if (!Input.GetKeyDown(KeyCode.Q) || meleeing)
            return;
        meleeing = true;
        
        foreach (MeleeDisableMonobehaviours behaviour in monobehaviours) {
            if (behaviour.useGameObject) {
                behaviour.gameObjectWasEnabled = behaviour.script.gameObject.activeInHierarchy;
                behaviour.script.gameObject.SetActive(false);
            } else {
                behaviour.scriptWasEnabled = behaviour.script.enabled;
                behaviour.script.enabled = false;
            }
        }
        
        meleeAnim.SetBool("Meleeing", meleeing);
        StartMelee();
    }
}