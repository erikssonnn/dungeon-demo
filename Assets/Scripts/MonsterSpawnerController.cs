using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnerController : MonoBehaviour {
	[SerializeField] private GameObject monsterPrefab = null;

	private int maxSpawnCount = 0;
	private int spawnInterval = 0;
	private float spawnTimer = 0.0f;
	
	private GenerationController generationController = null;
	private List<GameObject> spawnedMonsters = new List<GameObject>();

	private void Start() {
		generationController = FindObjectOfType<GenerationController>();
		if (generationController == null)
			throw new Exception("Cant find GenerationController");

		maxSpawnCount = generationController.spawnedSections.Count / 2;
		if (maxSpawnCount == 0)
			maxSpawnCount = 1;

		spawnInterval = 2;
		spawnedMonsters.Clear();
	}

	private void Update() {
		MonsterSpawnerCheck();
	}

	private void MonsterSpawnerCheck() {
		if (spawnedMonsters.Count >= maxSpawnCount)
			return;
		
		spawnTimer += Time.deltaTime;
		if (spawnTimer > spawnInterval) {
			spawnTimer = 0;
			SpawnMonster();
		}
	}

	private void SpawnMonster() {
		print("spawn monster");
	}
}
