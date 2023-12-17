using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public enum MaterialType { METAL, WOOD }

public class DynamicMeshController : MonoBehaviour {
    [SerializeField] private MaterialType materialType = MaterialType.METAL;
    [SerializeField] private float collisionVertexThreshold = 0.5f;
    [SerializeField] private float displacementForce = 0.1f;

    private Rigidbody rb = null;
    private MeshFilter meshFilter = null;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            throw new System.Exception("Cant find rigidbody on " + this);

        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            throw new System.Exception("Cant find meshFilter on " + this);
    }

    private void OnCollisionEnter(Collision col) {
        if (materialType == MaterialType.METAL) {
            MeshDisplace(col);
        }
    }

    private void MeshDisplace(Collision col) {
        if (col == null ||
            col.contacts.Length == 0 ||
            rb.velocity.magnitude < 2.0f) {
            return;
        }

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        Vector3 hitPoint = col.contacts[0].point;
        DebugController.Instance.SetPosition(hitPoint);
        Vector3 dir = (transform.position - hitPoint).normalized;

        float force = rb.velocity.magnitude * rb.mass;
        force = Mathf.Clamp(force, 0, 50f);

        for (int i = 0; i < vertices.Length; i++) {
            if (Vector3.Distance(transform.position + vertices[i], hitPoint) < collisionVertexThreshold) {
                vertices[i] += (dir * force) * displacementForce;
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = GetComponent<MeshRenderer>().material;
    }
}