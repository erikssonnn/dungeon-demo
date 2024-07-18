using UnityEngine;

[ExecuteInEditMode]
public class MotionBlur : MonoBehaviour {
    public Shader shader;

    public float blurStrength = 2.2f;
    public float blurWidth = 1.0f;

    private Material material = null;
    private bool isOpenGL;

    private Material GetMaterial() {
        if (material == null) {
            material = new Material(shader) {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        return material;
    }

    private void Start() {
        if (shader == null) {
            shader = Shader.Find("Hidden/RadialBlur");
        }

        isOpenGL = SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL");
    }

    private void OnRenderImage(RenderTexture source, RenderTexture dest) {
        float imageWidth = 1;
        float imageHeight = 1;
        if (isOpenGL) {
            imageWidth = source.width;
            imageHeight = source.height;
        }

        GetMaterial().SetFloat("_BlurStrength", blurStrength);
        GetMaterial().SetFloat("_BlurWidth", blurWidth);
        GetMaterial().SetFloat("_imgHeight", imageWidth);
        GetMaterial().SetFloat("_imgWidth", imageHeight);

        Graphics.Blit(source, dest, GetMaterial());
    }
}