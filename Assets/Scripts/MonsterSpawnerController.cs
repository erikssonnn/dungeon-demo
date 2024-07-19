using System;
using System.Collections.Generic;
using erikssonn;
using UnityEngine;
using Logger = erikssonn.Logger;
using Random = UnityEngine.Random;

public class MonsterSpawnerController : MonoBehaviour {
	[SerializeField] private GameObject monsterPrefab = null;
	[SerializeField] private GameObject portalPrefab = null;
	[SerializeField] private int distribution = 0;
	[SerializeField] private LayerMask lm = 0;
	[SerializeField] private bool spawnMonsters = false;
	[SerializeField] private float spawnInterval = 1.5f;

	private int maxSpawnCount = 0;
	private float spawnTimer = 0.0f;
	private List<GameObject> spawnedMonsters = new List<GameObject>();

	private Vector3 previousPos = Vector3.zero;
	private Transform player = null;
	private RoomGenerationController roomGenerationController = null;
	private List<Vector3> spawnPositions = new List<Vector3>();

	private void Start() {
		player = FindObjectOfType<MovementController>().transform;
		roomGenerationController = FindObjectOfType<RoomGenerationController>();

		maxSpawnCount = roomGenerationController.GetMapSize();
		if (maxSpawnCount == 0)
			maxSpawnCount = 1;

		spawnedMonsters.Clear();

		if (spawnPositions.Count == 0) {
			SetSpawnPositions();
		}
	}

	public void RemoveListedMonster(GameObject monster) {
		if (!spawnedMonsters.Contains(monster)) {
			Logger.Print("Tried to remove monster that doesnt exist!", LogLevel.WARNING);
			return;
		}
		spawnedMonsters.Remove(monster);
	}

	private void Update() {
		if (spawnMonsters) {
			MonsterSpawnerCheck();
		}

		if (Input.GetMouseButtonDown(2)) {
			Logger.Print("DEBUG spawn monster");
			Ray forwardRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
			if (!Physics.Raycast(forwardRay, out RaycastHit hit, Mathf.Infinity, lm))
				return;
			SpawnMonster(hit.point);
		}
	}

	public void SetSpawnPositions() {
		if (roomGenerationController == null) {
			roomGenerationController = FindObjectOfType<RoomGenerationController>();
		}

		for (int i = 0; i < roomGenerationController.GetEligiblePositions().Count; i++) {
			spawnPositions.Add(roomGenerationController.GetEligiblePositions()[i]);
		}
	}

	private void MonsterSpawnerCheck() {
		if (spawnedMonsters.Count >= maxSpawnCount) {
			Logger.Print("Max monsterspawns reached");
			return;
		}
		
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
		// Logger.Print("spawnPos:_ " + spawnPositions.Count);
		Vector3 pos = spawnPositions[Random.Range(0, spawnPositions.Count)];
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
