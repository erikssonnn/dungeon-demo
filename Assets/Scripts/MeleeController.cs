using System;
using UnityEngine;

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
    [SerializeField] private MonoBehaviour[] scripts = null;

    [SerializeField] private MeleeVariables meleeVar = new MeleeVariables();
    [SerializeField] private float damage = 100.0f;

    private Vector3 origin = Vector3.zero;
    private Camera cam = null;
    private bool meleeing = false;

    private void Start() {
        cam = Camera.main;
        origin = cam.transform.position;
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
        MeleeInitiator();
    }

    // to be added by animation event
    public void StartMelee() {
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
                hitObj = hit.transform;
            }
        }

        if (hitObj == null)
            return;
        StartCoroutine(ScreenShake.Instance.Shake(4f, 0.2f));
        // add enemy hit here use lateHit
    }

    // to be added by animation event
    public void ResetMelee() {
        foreach (MonoBehaviour script in scripts) {
            script.enabled = true;
        }
    }

    private void MeleeInitiator() {
        if (!Input.GetKeyDown(KeyCode.F) || meleeing)
            return;
        
        meleeing = true;
        foreach (MonoBehaviour script in scripts) {
            script.enabled = false;
        }
    }
}