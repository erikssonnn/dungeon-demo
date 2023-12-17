using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class GenerationController : MonoBehaviour {
    [Header("TWEAK: ")] 
    [SerializeField] private int maxSections = 10;
    [SerializeField] private float maxDistance = 2f;
    [SerializeField] private int maxRetries = 10;

    [Header("ASSIGN: ")] 
    [SerializeField] private Section[] sections = null;
    [SerializeField] private Section endSection = null;
    [SerializeField] private Section startSection = null;
    
    [SerializeField] private List<Section> spawnedSections = new List<Section>();
    [SerializeField] private List<GameObject> spawnedSectionObjects = new List<GameObject>();

    private int cornersInRow = 0;
    private int retries = 0;
    
    private void Start() {
        NullCheck();
        Generate();
    }

    // private void OnDrawGizmos() {
    //     if (spawnedSectionObjects.Count == 0)
    //         return;
    //
    //     for (int i = 0; i < spawnedSectionObjects.Count; i++) {
    //         GameObject section1 = spawnedSectionObjects[i];
    //         Collider col1 = section1.transform.GetComponent<Collider>();
    //         if (col1 == null) {
    //             continue;
    //         }
    //
    //         bool intersects = false;
    //
    //         for (int k = 0; k < spawnedSectionObjects.Count; k++) {
    //             if (i == k)
    //                 continue;
    //
    //             GameObject section2 = spawnedSectionObjects[k];
    //             Collider col2 = section2.transform.GetComponent<Collider>();
    //
    //             if (col2 == null)
    //                 continue;
    //
    //             if (col1.bounds.Intersects(col2.bounds)) {
    //                 intersects = true;
    //                 break;
    //             }
    //         }
    //
    //         Gizmos.color = intersects ? Color.red : Color.green;
    //         Gizmos.DrawWireCube(section1.transform.position, col1.bounds.size);
    //     }
    // }

    private void OnDrawGizmos() {
        if (spawnedSectionObjects.Count == 0)
            return;
    
        for (int i = 0; i < spawnedSectionObjects.Count; i++) {
            Color col = Color.green;
            
            if(spawnedSectionObjects[i].transform.childCount < 2)
                continue;
    
            for (int k = 0; k < spawnedSectionObjects.Count; k++) {
                if(i == k)
                    continue; 
                if(spawnedSectionObjects[k].transform.childCount < 2)
                    continue;
                
                float dist = Vector3.Distance(spawnedSectionObjects[i].transform.TransformPoint(spawnedSectionObjects[i].transform.GetChild(1).transform.localPosition), 
                    spawnedSectionObjects[k].transform.TransformPoint(spawnedSectionObjects[k].transform.GetChild(1).transform.localPosition));
                if (dist < maxDistance) {
                    col = Color.red;
                }
            }
    
            Gizmos.color = col;
            Gizmos.DrawWireSphere(spawnedSectionObjects[i].transform.GetChild(1).transform.position, maxDistance / 2f);
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
        
        for (int i = 0; i < maxSections; i++) {
            Section selectedSection = i == maxSections - 1 ? endSection : sections[Random.Range(0, sections.Length)];

            if (selectedSection.name == "2_corner" || selectedSection.name == "3_corner") {
                cornersInRow++;
            } else {
                cornersInRow = 0;
            }

            if (cornersInRow > 3) {
                selectedSection = sections[0];
                Debug.LogWarning("Tried to place four corners in a row, changed to corridor");
            }
        
            GameObject newSection = Instantiate(selectedSection.prefab, transform, true);
            newSection.transform.name = selectedSection.name + ("(" + i + ")");

            Vector3 pos = spawnedSectionObjects.Count == 0 ? Vector3.zero : spawnedSectionObjects[spawnedSectionObjects.Count - 1].transform.GetChild(0).position;
            Vector3 rot = spawnedSectionObjects.Count == 0
                ? new Vector3(-90, 0, 0)
                : spawnedSectionObjects[spawnedSectionObjects.Count - 1].transform.GetChild(0).eulerAngles + spawnedSections[spawnedSections.Count - 1].rotation;
            newSection.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));

            if (!CanPlaceSection(newSection)) {
                DestroyImmediate(newSection);

                if (retries < maxRetries) {
                    retries++;
                    continue;
                }

                GameObject endObject = Instantiate(endSection.prefab, transform, true);
                endObject.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
                selectedSection = endSection;
                spawnedSections.Add(selectedSection);
                spawnedSectionObjects.Add(endObject);
            
                print("collided with sections, breaking loop, retries: " + retries);
                return;
            }

            retries = 0;
        
            spawnedSections.Add(selectedSection);
            spawnedSectionObjects.Add(newSection);
        }
    }

    public void Clear() {
        foreach (GameObject section in spawnedSectionObjects) {
            DestroyImmediate(section);
        }

        spawnedSections.Clear();
        spawnedSectionObjects.Clear();
    }

    private void PlaceStartSection() {
        GameObject startObject = Instantiate(startSection.prefab, transform, true);

        startObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(new Vector3(-90, 0, 0)));
        spawnedSections.Add(startSection);
        spawnedSectionObjects.Add(startObject);
    }
    
    public void ClearLog() {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    private bool CanPlaceSection(GameObject newSectionObj) {
        if (spawnedSectionObjects.Count < 2) {
            return true;
        }
        
        if(newSectionObj.transform.childCount < 2)
            return true;

        for (int i = 0; i < spawnedSectionObjects.Count; i++) {
            if(spawnedSectionObjects[i].transform.childCount < 2)
                continue;
            
            float dist = Vector3.Distance(spawnedSectionObjects[i].transform.TransformPoint(spawnedSectionObjects[i].transform.GetChild(1).transform.localPosition), 
                newSectionObj.transform.TransformPoint(newSectionObj.transform.GetChild(1).transform.localPosition));
            if (dist < maxDistance) {
                return false;
            }
        }
        
        return true;
    }
}