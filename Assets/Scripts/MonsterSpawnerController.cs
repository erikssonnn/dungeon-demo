using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterSpawnerController : MonoBehaviour {
	[SerializeField] private GameObject monsterPrefab = null;
	[SerializeField] private GameObject portalPrefab = null;
	[SerializeField] private int distribution = 0;
	[SerializeField] private LayerMask lm = 0;

	private int maxSpawnCount = 0;
	private float spawnInterval = 0;
	private float spawnTimer = 0.0f;
	private List<GameObject> spawnedMonsters = new List<GameObject>();

	private Vector3 previousPos = Vector3.zero;
	private Transform player = null;

	private void Start() {
		player = FindObjectOfType<MovementController>().transform;
		maxSpawnCount = FindObjectOfType<RoomGenerationController>().GetMapSize();
		
		if (maxSpawnCount == 0)
			maxSpawnCount = 1;

		spawnInterval = 111111111.5f;
		spawnedMonsters.Clear();
	}

	private void Update() {
		MonsterSpawnerCheck();
	}

	// private void DebugSpawnMonster() {
	// 	/*
	// 	 * TODO: DEBUG REMOVE ON RELEASE
	// 	 */
	// 	Ray forwardRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
	// 	if (!Physics.Raycast(forwardRay, out RaycastHit hit, Mathf.Infinity))
	// 		return;
	// 	if (Input.GetKeyDown(KeyCode.M)) {
	// 		SpawnMonster(hit.point);
	// 	}
	// 	if (Input.GetKeyDown(KeyCode.N)) {
	// 		DecalController.Instance.SpawnDecal(hit.point, forwardRay.direction.normalized, -0.5f);
	// 	}
	// }

	private void MonsterSpawnerCheck() {
		if (spawnedMonsters.Count >= maxSpawnCount)
			return;
		
		spawnTimer += Time.deltaTime;
		if (spawnTimer > spawnInterval) {
			spawnTimer = 0;
			CheckPosition();
		}
	}

	private bool TooCloseToPlayer(Vector3 pos) {
		return Vector3.Distance(pos, player.position) < 5;
	}

	private void CheckPosition() {
		Vector3 rayPos = new Vector3(Random.Range(0, 100), 100, Random.Range(0, 100));
		Ray ray = new Ray(rayPos, Vector3.down * 1000);

		if (!Physics.Raycast(ray, out RaycastHit hit, 1000, lm))
			return;
		
		Vector3 pos = new Vector3(hit.point.x, 0, hit.point.z);
		if (TooCloseToPlayer(pos)) {
			CheckPosition();
			return;
		}

		if (Vector3.Distance(pos, previousPos) < distribution) {
			CheckPosition();
			return;
		}

		SpawnMonster(pos);
	}

	public void SpawnMonster(Vector3 pos) {
		Instantiate(portalPrefab, pos, Quaternion.identity);
		
		GameObject newMonster = Instantiate(monsterPrefab, pos, Quaternion.identity);
		newMonster.transform.SetParent(transform, true);
		spawnedMonsters.Add(newMonster);
		
		previousPos = pos;
	}
}
