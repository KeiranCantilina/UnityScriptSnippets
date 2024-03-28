using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class ContrastSlider : MonoBehaviour
{
    private Shader shader;
    public PinchSlider contrastSlider;
    public PinchSlider brightnessSlider;
    private Renderer rend;
    float contrast;
    float brightness;


    // Start is called before the first frame update
    void Start()
    {
        shader = Shader.Find("Mixed Reality Toolkit / StandardGreyscale");
        rend = this.GetComponent<MeshRenderer>();
        contrast = 1.0f;
        brightness = 0;
    }

    // Update is called once per frame
    void Update()
    {
        brightness = (brightnessSlider.SliderValue-0.5f)*2;
        contrast = contrastSlider.SliderValue*4;
        rend.sharedMaterial.SetFloat("_Contrast", contrast);
        rend.sharedMaterial.SetFloat("_Brightness", brightness);
    }
}
