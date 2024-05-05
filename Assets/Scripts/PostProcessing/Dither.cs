using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class Dither : MonoBehaviour {
    public Shader ditherShader;

    [Range(0.0f, 1.0f)] public float spread = 0.5f;

    [Range(2, 16)] public int redColorCount = 2;
    [Range(2, 16)] public int greenColorCount = 2;
    [Range(2, 16)] public int blueColorCount = 2;
    [Range(0, 2)] public int bayerLevel = 0;
    [Range(-1, 1)] public float threshold = 0;
    
    private Material ditherMat;

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        ditherMat = new Material(ditherShader);
        ditherMat.hideFlags = HideFlags.HideAndDontSave;
        
        ditherMat.SetFloat("_Spread", spread);
        ditherMat.SetInt("_RedColorCount", redColorCount);
        ditherMat.SetInt("_GreenColorCount", greenColorCount);
        ditherMat.SetInt("_BlueColorCount", blueColorCount);
        ditherMat.SetInt("_BayerLevel", bayerLevel);
        ditherMat.SetFloat("_Threshold", threshold);

        int width = source.width;
        int height = source.height;
        
        RenderTexture dither = RenderTexture.GetTemporary(width, height, 0, source.format);
        Graphics.Blit(source, destination, ditherMat);
        RenderTexture.ReleaseTemporary(dither);
    }
}