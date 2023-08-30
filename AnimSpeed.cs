using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI;

public class AnimSpeed : MonoBehaviour
{
    public Animator anim;
    public PinchSlider slider;


    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("AnimSpeed", slider.SliderValue);
    }
}