using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {
	[SerializeField] private float time = 1.0f;

	private void Start() {
		Destroy(gameObject, time);
	}
}
