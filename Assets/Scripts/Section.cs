using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Section : MonoBehaviour{
	public int index;
	public new string name;
	public GameObject origin;
	public float size;
	public Vector3 rotation;
	public float spawnRate;

	[SerializeField] private GameObject[] optionalProps;

	private void Start() {
		if (name != "room") 
			return;
		foreach (GameObject t in optionalProps) {
			if (Random.value < 0.5f) {
				t.SetActive(true);
			}
		}
	}
}
