using UnityEngine;

namespace Serialization {
	[System.Serializable]
	public class ConsoleSaveAsset {
		public string key;
		public int value;
	}
	
	[CreateAssetMenu(menuName = "custom/ConsoleSave", fileName = "new ConsoleSave")]
	public class ConsoleSave : ScriptableObject {
		public ConsoleSaveAsset[] consoleSaves;
	}
}
