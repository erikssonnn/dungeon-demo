using UnityEngine;

public class InventoryController : MonoBehaviour {
    [Header("ASSIGNABLE: ")]
    [SerializeField] private GameObject inventoryUi;
    [SerializeField] private GameObject[] inventorySlots;
    
    private bool visable = false;

    private void Update() {
        if (!Input.GetKeyDown(KeyCode.Tab)) return;
        visable = !visable;
        inventoryUi.SetActive(visable);
    }
}