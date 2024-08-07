using System.Linq;
using UnityEngine;

public enum InteractType {
    DOOR,
    CONTAINER,
    LOOT
}

[System.Serializable]
public class Loot {
    public GameObject obj;
    public int spawnRate;
    public int value;
}

public class InteractObject : MonoBehaviour {
    [SerializeField] private Loot[] lootTable = null;
    [SerializeField] private GameObject spawnPoint = null;
    [SerializeField] private InteractType interactType = InteractType.CONTAINER;

    private bool looted = false;

    public void Activate() {
        if (interactType == InteractType.CONTAINER) {
            LootContainer();
        }

        if (interactType == InteractType.LOOT) {
            PickupLoot();
        }
    }

    public bool ShouldShowInteractUi() {
        if (interactType == InteractType.DOOR)
            return true;
        if (interactType == InteractType.LOOT)
            return true;

        return !looted;
    }

    private void PickupLoot() {
        Debug.Log("Picked up " + this.gameObject.name);
        if (lootTable[0] == null) {
            throw new System.Exception("No loot in lootTable on pickup!");
        }

        int value = lootTable[0].value + Random.Range(-Mathf.RoundToInt(lootTable[0].value * 0.25f), Mathf.RoundToInt(lootTable[0].value * 0.25f));
        // ScoreController.Instance.UpdateScore(value);
        Destroy(gameObject);
    }

    private void LootContainer() {
        if (looted) {
            return;
        }

        looted = true;
        int totalSpawnRate = lootTable.Sum(loot => loot.spawnRate);
        int randomValue = Random.Range(0, totalSpawnRate);

        foreach (Loot loot in lootTable) {
            if (randomValue < loot.spawnRate) {
                if (loot.obj == null) {
                    print("NO LOOT SORRY");
                    return;
                }

                GameObject newLoot = Instantiate(loot.obj, transform, true);
                newLoot.transform.SetPositionAndRotation(spawnPoint.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
                break;
            }

            randomValue -= loot.spawnRate;
        }
    }
}