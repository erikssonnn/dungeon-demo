using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PickupSpawnController : MonoBehaviour {
	[SerializeField] [Range(0.0f, 1.0f)] private float spawnChance = 0.1f;
	[SerializeField] private GameObject[] pickupObjects = null;
	
	private static PickupSpawnController instance;

	public static PickupSpawnController Instance {
		get {
			instance = FindObjectOfType<PickupSpawnController>();
			if (instance != null) return instance;
			GameObject obj = new GameObject("PickupSpawnController");
			instance = obj.AddComponent<PickupSpawnController>();
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

	public void CheckSpawnChance(Vector3 pos) {
		if (Random.value < spawnChance) {
			SpawnPickupObject(pos);
		}
	}

	private void SpawnPickupObject(Vector3 pos) {
		GameObject newPickup = Instantiate(pickupObjects[Random.Range(0, pickupObjects.Length)]);
		newPickup.transform.SetPositionAndRotation(pos, Quaternion.identity);
	}
}
