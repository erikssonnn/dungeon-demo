using System;
using UnityEngine;
using Logger = erikssonn.Logger;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class PostProcessing : MonoBehaviour {
    [Header("Pixelate: ")]
    [SerializeField] private Vector2Int pixelSize = new Vector2Int(320, 420);
    [SerializeField] private Vector2Int screenSize = new Vector2Int(1080, 4400);
    [Header("ColorGrading: ")]
    [SerializeField] private int colorPrecision = 32;

    [Header("Dither")]
    [SerializeField] private Texture2D ditherTexture = null;
    [SerializeField] private Color ditherColor = Color.white;

    private Camera cam = null;
    private Material pixelateMat = null;
    private Material colorMat = null;
    private Material ditherMat = null;

    private void Start() {
        pixelateMat = new Material(Shader.Find("Hidden/pixelate"));
        ditherMat = new Material(Shader.Find("Hidden/dither"));
        colorMat = new Material(Shader.Find("Hidden/colorgrading"));
    }

    private void Awake() {
        cam = Camera.main;
        if (cam == null) {
            throw new Exception("Main camera not found!");
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (colorMat == null || pixelateMat == null || ditherMat == null) {
            Graphics.Blit(source, destination);
            throw new Exception("Cant find one of the postprocessing shader materials");
        }

        RenderTexture tempTexture = RenderTexture.GetTemporary(source.width, source.height);
        // RenderTexture tempTexture2 = RenderTexture.GetTemporary(source.width, source.height);

        // ditherMat.SetTexture("_DitherPattern", ditherTexture);
        // ditherMat.SetColor("_Color", ditherColor);
        // Graphics.Blit(source, tempTexture, ditherMat);

        colorMat.SetFloat("_Colors", colorPrecision);
        Graphics.Blit(source, tempTexture, colorMat);

        pixelateMat.SetInt("_PixelSize", GetPixelSizeForScreen());
        Graphics.Blit(tempTexture, destination, pixelateMat);
        
        RenderTexture.ReleaseTemporary(tempTexture);
        // RenderTexture.ReleaseTemporary(tempTexture2);
    }

    private int GetPixelSizeForScreen()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float screenValue = Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);
        int strength = Mathf.RoundToInt(Mathf.Lerp(pixelSize.x, pixelSize.y, Normalize(screenValue, screenSize.x, screenSize.y)));
        // Debug.Log("screenValue: " + screenValue + ", strength: " + strength);
        return strength;
    }

    private static float Normalize(float value, float min, float max)
    {
        return Mathf.Clamp01((value - min) / (max - min));
    }
}