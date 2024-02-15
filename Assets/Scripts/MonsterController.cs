using System;
using UnityEngine;

public class MonsterController : MonoBehaviour {
    [SerializeField] private LayerMask lm = 0;

    private new Camera camera = null;
    private bool isRendered = false;

    private void Start() {
        camera = Camera.main;
        if (camera == null)
            throw new Exception("Cant find main camera on " + this);
    }

    private bool IsObjectInCameraView() {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        bool viewCheck = GeometryUtility.TestPlanesAABB(planes, GetComponentInChildren<Collider>().bounds);
        Physics.Linecast(transform.position, camera.transform.position, out RaycastHit hit, lm);
        bool rayCheck = hit.collider == null;
        return viewCheck && rayCheck;
    }

    private void OnGUI() {
        GUI.Label(new Rect(10, 10, 100, 100), IsObjectInCameraView().ToString());
    }
}