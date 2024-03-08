using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DICOMParser;
using DICOMViews;
using System.IO;


public class LoadDICOM : MonoBehaviour
{
    //
    private string folderpath;
   // public string foldername;
    public ImageStack imagestack;
    private Texture2D texture;
    private int sliceindex;
    public SliceType sliceType;
    public GameObject displayCanvas;

    // Start is called before the first frame update
    void Start()
    {
        sliceindex = 144;
        folderpath = Application.dataPath + "\\Datasets\\1156_DICOM";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadFile()
    {
        imagestack.StartParsingFiles(folderpath);
        
        //imagestack.StartPreprocessData();
        //imagestack.StartCreatingTextures();
    }

    public void startPreProcess()
    {
        imagestack.StartPreprocessData();
    }

    public void startCreatingTextures()
    {
        imagestack.StartCreatingTextures();
    }

    public void GetTexture2D()
    {
        texture =  imagestack.GetTexture2D(sliceType, sliceindex);
        displayCanvas.GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
        texture.Apply();
    }

    public void incrementSliceIndex()
    {
        sliceindex++;
    }

    public void decrementSliceIndex()
    {
        if(sliceindex > 0)
        {
            sliceindex--;
        }
        else
        {
            sliceindex = 0;
        }
    }
}
