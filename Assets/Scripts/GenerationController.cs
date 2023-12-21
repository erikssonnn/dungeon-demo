using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class GenerationController : MonoBehaviour {
    [Header("TWEAK: ")]
    [SerializeField] private int maxSections = 10;
    [SerializeField] private int maxRetries = 10;
    [SerializeField] private bool debug = false;

    [Header("ASSIGN: ")] 
    [SerializeField] private GameObject[] sections = null;
    [SerializeField] private GameObject endSection = null;
    [SerializeField] private GameObject startSection = null;

    private List<GameObject> spawnedSections = new List<GameObject>();
    private int cornersInRow = 0;
    private int retries = 0;

    private void Start() {
        NullCheck();
        //Generate();
    }

    private void OnDrawGizmos() {
        if (!debug)
            return;
        
        if (spawnedSections.Count == 0)
            return;

        for (int i = 0; i < spawnedSections.Count; i++) {
            Color col = Color.green;

            Section section1 = spawnedSections[i].GetComponent<Section>();
            if (section1.origin == null)
                continue;

            for (int k = 0; k < spawnedSections.Count; k++) {
                if (i == k)
                    continue;
                
                Section section2 = spawnedSections[k].GetComponent<Section>();
                if (section2.origin == null)
                    continue;

                float dist = Vector3.Distance(spawnedSections[i].transform.TransformPoint(section1.origin.transform.localPosition),
                    spawnedSections[k].transform.TransformPoint(section2.origin.transform.localPosition));
                if (dist < ((section1.size / 2f) + (section2.size / 2f))) {
                    col = Color.red;
                }
            }

            Gizmos.color = col;
            Gizmos.DrawWireSphere(section1.origin.transform.position, section1.size / 2f);
        }
    }

    private void NullCheck() {
        if (sections == null || sections.Length == 0) {
            throw new SystemException("sections is empty or not assigned!");
        }
    }

    public void Generate() {
        ClearLog();
        Clear();
        PlaceStartSection();

        GameObject direction = GetRandomDirection();
        if (direction == null) {
            throw new Exception("Placed start section, but no direction was found!");
        }

        PlaceAllSections();
    }

    public void Clear() {
        foreach (GameObject section in spawnedSections) {
            DestroyImmediate(section);
        }

        spawnedSections.Clear();
        
        int childCount = transform.childCount;
        if (childCount == 0)
            return;
        
        for (int i = childCount - 1; i >= 0; i--) {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    private void PlaceStartSection() {
        GameObject startObject = Instantiate(startSection, transform, true);

        startObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(new Vector3(-90, 0, 0)));
        spawnedSections.Add(startObject);
    }

    private GameObject GetRandomSection() {
        float totalSpawnRate = 0f;

        foreach (GameObject sectionObj in sections) {
            totalSpawnRate += sectionObj.GetComponent<Section>().spawnRate;
        }

        float randomValue = Random.Range(0f, totalSpawnRate);

        foreach (GameObject sectionObj in sections) {
            if (randomValue <= sectionObj.GetComponent<Section>().spawnRate) {
                return sectionObj;
            }

            randomValue -= sectionObj.GetComponent<Section>().spawnRate;
        }

        return sections[0];
    }
    
    private void PlaceAllSections() {
        for (int i = 0; i < maxSections; i++) {
            GameObject direction = GetRandomDirection();
            if (direction == null) {
                if(debug) Debug.LogWarning("Probably placed endSection, and no more directions were found, breaking loop");
                break;
            }

            GameObject selectedSection = endSection;
            if (i < maxSections - 1) {
                selectedSection = GetRandomSection();
                if (selectedSection.GetComponent<Section>().name == "room" && i < (maxSections / 2)) {
                    selectedSection = sections[0];
                }
            }
            
            if (selectedSection.name == "2_corner" || selectedSection.name == "3_corner") {
                cornersInRow++;
            } else {
                cornersInRow = 0;
            }

            if (cornersInRow > 3) {
                selectedSection = sections[0];
                if(debug) Debug.LogWarning("Tried to place four corners in a row, changed to corridor");
            }

            GameObject newSection = Instantiate(selectedSection, transform, true);
            newSection.transform.name = selectedSection.name + ("(" + spawnedSections.Count + ")");

            Vector3 pos = spawnedSections.Count == 0 ? Vector3.zero : direction.transform.position;
            Vector3 rot = spawnedSections.Count == 0
                ? new Vector3(-90, 0, 0)
                : direction.transform.eulerAngles + newSection.GetComponent<Section>().rotation;
            newSection.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));

            if (!CanPlaceSection(newSection)) {
                DestroyImmediate(newSection);

                GameObject endObject = Instantiate(endSection, transform, true);
                endObject.transform.name = endSection.name + ("(" + spawnedSections.Count + ")");
                endObject.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
                spawnedSections.Add(endObject);
                DestroyImmediate(direction);

                if(debug) print("collided with map at: " + endObject.transform.name);
                continue;
            }

            spawnedSections.Add(newSection);
            DestroyImmediate(direction);

            GameObject newDirection = GetRandomDirection();
            if (newDirection != null) continue;
            if(debug) Debug.LogWarning("GENERATION IS COMPLETE (No more directions found)");
            break;
        }

        if (sections.Length < maxSections - 1 && retries < maxRetries) {
            retries++;
            Generate();
            return;
        }
        
        retries = 0;
        PlaceFinalEndSections();
    }

    private void PlaceFinalEndSections() {
        GameObject[] directions = GameObject.FindGameObjectsWithTag("Direction");
        if (directions.Length == 0) {
            return;
        }

        if(debug) print("Placing " + directions.Length + " endSections");
        foreach (GameObject dir in directions) {
            GameObject newSection = Instantiate(sections[5], transform, true); // try to place room
            newSection.transform.name = sections[5].name + ("(" + spawnedSections.Count + ")");

            Vector3 pos = spawnedSections.Count == 0 ? Vector3.zero : dir.transform.position;
            Vector3 rot = spawnedSections.Count == 0
                ? new Vector3(-90, 0, 0)
                : dir.transform.eulerAngles + newSection.GetComponent<Section>().rotation;
            newSection.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));

            // if u cant place room, place end
            if (!CanPlaceSection(newSection)) {
                DestroyImmediate(newSection);

                GameObject endObject = Instantiate(endSection, transform, true);
                endObject.transform.name = endSection.name + ("(" + spawnedSections.Count + ")");
                endObject.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
                spawnedSections.Add(endObject);
                continue;
            }
            
            spawnedSections.Add(newSection);
        }

        foreach (GameObject dir in directions) {
            DestroyImmediate(dir);
        }
    }

    private static GameObject GetRandomDirection() {
        return GameObject.FindWithTag("Direction");
    }

    private static void ClearLog() {
        Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        Type type = assembly.GetType("UnityEditor.LogEntries");
        type.GetMethod("Clear")!.Invoke(new object(), null);
    }

    private bool CanPlaceSection(GameObject newSectionObj) {
        if (spawnedSections.Count < 2) {
            return true;
        }

        Section newSection = newSectionObj.GetComponent<Section>();
        if (newSection.origin == null)
            return true;

        foreach (GameObject spawnedSection in spawnedSections) {
            Section section = spawnedSection.GetComponent<Section>();
            if (section.origin == null)
                continue;

            float dist = Vector3.Distance(spawnedSection.transform.TransformPoint(section.origin.transform.localPosition),
                newSectionObj.transform.TransformPoint(newSection.origin.transform.localPosition));
            if (dist < ((newSection.size / 2f) + (section.size / 2f))) {
                return false;
            }
        }

        return true;
    }
}