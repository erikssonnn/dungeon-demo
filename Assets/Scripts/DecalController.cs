using System;
using System.Collections.Generic;
using ch.sycoforge.Decal;
using UnityEngine;
using Random = UnityEngine.Random;

public class DecalController : MonoBehaviour {
    [Header("TWEAKABLE: ")]
    [SerializeField] private int maxDecals = 100;

    [SerializeField] private Vector2 sizeSpan = new Vector2(0.5f, 2f);

    [Header("ASSIGN: ")]
    [SerializeField] private GameObject decalPrefab = null;
    [SerializeField] private Material[] materials = null;

    private DecalRoot decalRoot = null;
    private List<GameObject> decals = new List<GameObject>();
    private static DecalController instance;

    public static DecalController Instance {
        get {
            instance = FindObjectOfType<DecalController>();
            if (instance != null) return instance;
            GameObject obj = new GameObject("DecalController");
            instance = obj.AddComponent<DecalController>();
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
        if(materials.Length <= 0)
            throw new Exception("No materials assigned on " + this);
        if (decalPrefab == null)
            throw new Exception("Cant find decalPrefab on " + this);

        decalRoot = FindObjectOfType<DecalRoot>();
        if (decalRoot == null)
            throw new Exception("Cant find decal root on " + this);
    }

    public void SpawnDecal(Vector3 pos, Vector3 dir, float distance) {
        if (decals.Count + 1 > maxDecals) {
            decals.RemoveAt(0);
        }

        GameObject decal = Instantiate(decalPrefab);
        decal.GetComponent<EasyDecal>().DecalMaterial = materials[Random.Range(0, materials.Length)];

        Quaternion rot = Quaternion.LookRotation(dir);
        rot *= Quaternion.Euler(-90, 0, 0);
        decal.transform.SetPositionAndRotation(pos + (dir * distance), rot);
        float ran = UnityEngine.Random.Range(sizeSpan.x, sizeSpan.y);
        decal.transform.localScale = new Vector3(ran, ran, ran);
        decal.transform.SetParent(decalRoot.transform, true);
        decals.Add(decal);
    }
}