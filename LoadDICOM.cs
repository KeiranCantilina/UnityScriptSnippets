using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DICOMParser;
using DICOMViews;
//using System.IO;
using Threads;
//using System.Threading;
using System;
using Microsoft.MixedReality.Toolkit.UI;



public class LoadDICOM : MonoBehaviour
{
    //
    private string folderpath;
   // public string foldername;
    public ImageStack imagestack;
    private Texture2D texture;
    private int sliceindex;
    private int oldsliceindex;
    public SliceType sliceType;
    private SliceType oldSliceType;
    public GameObject displayCanvas;
    private ThreadGroupState ThreadState;
    private string DebugMessage = "Ready to Load";
    private int processingStage = 0;
    public PinchSlider slider;
    private int maxIndex = 0;
    public bool doneLoading;
    public bool compress;
    private Vector3 DefaultView;

    // Start is called before the first frame update
    void Start()
    {
        doneLoading = false;
        sliceindex = 0;
        oldsliceindex = 0;
        folderpath = Application.dataPath + "\\StreamingAssets";
        oldSliceType = sliceType;
        DefaultView = this.transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        
        // Refresh texture display if slice type or slice index is changed
        if(oldSliceType != sliceType || oldsliceindex != sliceindex)
        {
            oldSliceType = sliceType;
            oldsliceindex = sliceindex;
            processingStage = 6;
        }

        // Process Pseudo-Queue
        if(processingStage == 2)
        {
            processingStage = 3;
            StartPreProcess();
        }
        else if(processingStage == 4){
            processingStage = 5;
            StartCreatingTextures();
        }
        else if(processingStage == 6)
        {
            processingStage = 0;
            StartGetTexture2D();
        }

        // Info panel
        if(processingStage != 0 && processingStage != 6)
        {
            imagestack.WriteDebug(DebugMessage + "\n" + "Progress: " + 100 * ThreadState.Progress / ThreadState.TotalProgress + "%");
        }
        else
        {
            imagestack.WriteDebug(DebugMessage);
        }

        // Grab slider value for index (but only if files are loaded)
        if (doneLoading)
        {
            sliceindex = Convert.ToInt32(Math.Ceiling(slider.SliderValue * maxIndex));
        }
    }

    // Triggered by button in UI
    public void LoadFile()
    {
        if (!doneLoading)
        {
            processingStage = 1;
            DebugMessage = "Loading files...";
            ThreadState = imagestack.StartParsingFiles(folderpath);
            ThreadState.ThreadsFinished += IncrementProcessCounter;
        }
        else
        {
            DebugMessage = "Files already loaded!";
        }
        
    }

    public void StartPreProcess()
    {
        DebugMessage = "Preprocessing...";
        ThreadState = imagestack.StartPreprocessData();
        ThreadState.ThreadsFinished += IncrementProcessCounter;
    }

    public void StartCreatingTextures()
    {
        DebugMessage = "Rendering...";
        ThreadState = imagestack.StartCreatingTextures();
        ThreadState.ThreadsFinished += IncrementProcessCounter;
    }

    // Triggered by changing slice type or index (or at end of load file process)
    public void StartGetTexture2D()
    {
        if (sliceType == SliceType.Transversal)
        {
            maxIndex = imagestack.GetMaxValue(SliceType.Transversal);
            this.transform.localEulerAngles = new Vector3(180, 0, 0);
        }
        else if (sliceType == SliceType.Sagittal)
        {
            maxIndex = imagestack.GetMaxValue(SliceType.Sagittal);
            this.transform.localEulerAngles = DefaultView;
        }
        else
        {
            maxIndex = imagestack.GetMaxValue(SliceType.Frontal);
            this.transform.localEulerAngles = DefaultView;
        }

        doneLoading = true;
        texture =  imagestack.GetTexture2D(sliceType, sliceindex);
        displayCanvas.GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
        if (texture != null)
        {
            texture.Apply();
        }
        DebugMessage = "Showing " + sliceType + " View, Slice No. " + sliceindex;
    }

    public void IncrementSliceIndex()
    {
        if (sliceindex < maxIndex)
        {
            sliceindex++;
            processingStage = 6;
        }
        else
        {
            sliceindex = maxIndex;
        }
    }

    public void DecrementSliceIndex()
    {
        if(sliceindex > 0)
        {
            sliceindex--;
            processingStage = 6;
        }
        else
        {
            sliceindex = 0;
        }
    }
    private void IncrementProcessCounter(object sender, EventArgs e)
    {
        processingStage++;
    }

    public void SetViewTransverse()
    {
        sliceType = SliceType.Transversal;

    }
    public void SetViewSagittal()
    {
        sliceType = SliceType.Sagittal;
    }
    public void SetViewFrontal()
    {
        sliceType = SliceType.Frontal;
    }
}
