using UnityEngine;

public class PostProcessing : MonoBehaviour {
    [SerializeField] private Material mat;
    [SerializeField] private int blockCount;

    private Camera cam = null;
    private Vector2 referenceResolution = new Vector2(1920, 1080);

    private void Awake() {
        cam = Camera.main;
        if (cam == null) {
            throw new System.Exception("Main camera not found!");
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture dest) {
        float k = cam.aspect;
        Vector2 count = new Vector2(Screen.width / blockCount, Screen.height / (blockCount / k));
        Vector2 size = new Vector2(1.0f / count.x, 1.0f / count.y);

        mat.SetVector("block_count", count);
        mat.SetVector("block_size", size);
        mat.SetTexture("main_tex", source);

        Graphics.Blit(source, dest, mat);
    }
}