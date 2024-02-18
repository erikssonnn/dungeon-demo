using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class MonsterController : MonoBehaviour {
    [SerializeField] private LayerMask lm = 0;
    [SerializeField] private float spawnCheckInterval = 0.0f;

    private new Camera camera = null;
    private bool spawned = false;
    private float spawnChance = 0.0f;
    private new SkinnedMeshRenderer renderer = null;
    private GenerationController generationController = null;
    private float spawnTimer = 0.0f;

    private void Start() {
        NullChecker();
        spawned = false;
        renderer.enabled = false;
    }

    private void NullChecker() {
        generationController = FindObjectOfType<GenerationController>();
        if (generationController == null)
            throw new Exception("Cant find GenerationController.cs");

        renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer == null)
            throw new Exception("Cant find MeshRenderer on " + this);

        camera = Camera.main;
        if (camera == null)
            throw new Exception("Cant find main camera on " + this);
    }

    private bool IsObjectInCameraView() {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        bool viewCheck = GeometryUtility.TestPlanesAABB(planes, GetComponentInChildren<Collider>().bounds);
        Physics.Linecast(transform.position, camera.transform.position, out RaycastHit hit, lm);
        bool rayCheck = hit.collider == null;
        return viewCheck && rayCheck;
    }

    private void Update() {
        if (spawned)
            return;

        IncreaseSpawnChance();
        spawnTimer += Time.fixedDeltaTime;

        if (spawnTimer > spawnCheckInterval)
            CheckIfToSpawn();
    }

    private void IncreaseSpawnChance() {
        spawnChance += (0.01f * Time.fixedDeltaTime);
        spawnChance = Mathf.Clamp(spawnChance, 0, 100);
    }

    private void CheckIfToSpawn() {
        print("CHECK TO SPAWN");
        spawnTimer = 0.0f;
        
        float r = Random.value;
        if (r <= (spawnChance * 0.01f)) {
            Debug.Log(r + " <= " + (spawnChance * 0.01f));
            Spawn();
        }
    }

    private Vector3 GetSpawnLocation() {
        return generationController.GetRandomMapSection().transform.position;
    }

    private void Spawn() {
        spawned = true;
        spawnChance = 0.0f;
        Debug.Log("TRYING TO SPAWN MONSTER...");

        Vector3 position = GetSpawnLocation();
        transform.position = position;
        if (IsObjectInCameraView()) {
            print("monster in view, check another position");
            Spawn();
            return;
        }

        renderer.enabled = true;
        Debug.LogWarning("SPAWNED MONSTER COMPLETE AT " + position);
    }

    private void OnGUI() {
        string str = IsObjectInCameraView() + "\n" +
                     transform.position + "\n" +
                     "Spawn chance: " + spawnChance + "\n" +
                     "Spawn timer: " + spawnTimer + "\n"
            ;
        GUI.Label(new Rect(10, 10, 500, 100), str);
    }
}