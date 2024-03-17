using System;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class PostProcessing : MonoBehaviour {
    [SerializeField] private int pixelSize = 16;
    [SerializeField] private int colorPrecision = 32;

    private Camera cam = null;
    private Material pixelateMat = null;
    private Material colorMat = null;

    private void Start() {
        pixelateMat = new Material(Shader.Find("Hidden/pixelate"));
        colorMat = new Material(Shader.Find("Hidden/colorgrading"));
    }

    private void Awake() {
        cam = Camera.main;
        if (cam == null) {
            throw new Exception("Main camera not found!");
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (colorMat == null || pixelateMat == null) {
            Graphics.Blit(source, destination);
            throw new Exception("Cant find colorMat or pixelateMat");
        }

        RenderTexture tempTexture = RenderTexture.GetTemporary(source.width, source.height);

        colorMat.SetFloat("_Colors", colorPrecision);
        Graphics.Blit(source, tempTexture, colorMat);

        pixelateMat.SetInt("_PixelSize", pixelSize);
        Graphics.Blit(tempTexture, destination, pixelateMat);
        RenderTexture.ReleaseTemporary(tempTexture);
    }
}