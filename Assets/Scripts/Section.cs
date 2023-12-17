using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Section", fileName = "new Section")]
public class Section : ScriptableObject {
	public new string name;
	public GameObject prefab;
	public Vector3 rotation;
}
