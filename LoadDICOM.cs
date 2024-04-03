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
    public ImageStack2 imagestack;
    private Texture2D texture;
    private int sliceindex;
    private int oldsliceindex;
    public ViewNumber viewNumber;
    private ViewNumber oldviewNumber;
    private string SliceName = "Axial";
    public GameObject displayCanvas;
    private ThreadGroupState ThreadState;
    private string DebugMessage = "Ready to Load";
    private int processingStage = 0;
    public PinchSlider slider;
    private int maxIndex = 0;
    public bool doneLoading;
    public bool compress;
    private Vector3 DefaultView;
    public SliceType View1SliceType;
    public int View1WindowCenter;
    public int View1WindowWidth;
    public SliceType View2SliceType;
    public int View2WindowCenter;
    public int View2WindowWidth;
    public SliceType View3SliceType;
    public int View3WindowCenter;
    public int View3WindowWidth;
    public SliceType View4SliceType;
    public int View4WindowCenter;
    public int View4WindowWidth;

    // Start is called before the first frame update
    void Start()
    {
        doneLoading = false;
        sliceindex = 0;
        oldsliceindex = 0;
        folderpath = Application.dataPath + "\\StreamingAssets";
        oldviewNumber = viewNumber;
        DefaultView = this.transform.localEulerAngles;
        
    }

    // Update is called once per frame
    void Update()
    {
        
        // Refresh texture display if slice type or slice index is changed
        if(oldviewNumber != viewNumber || oldsliceindex != sliceindex)
        {
            oldviewNumber = viewNumber;
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
        int[] windowWidths = { View1WindowWidth, View2WindowWidth, View3WindowWidth };
        int[] windowCenters = { View1WindowCenter, View2WindowCenter, View3WindowCenter };

    
        SliceType[] SliceTypes = { View1SliceType, View2SliceType, View3SliceType };

        ThreadState = imagestack.StartCreatingTextures(SliceTypes, windowWidths, windowCenters);
        ThreadState.ThreadsFinished += IncrementProcessCounter;
    }

    // Triggered by changing slice type or index (or at end of load file process)
    public void StartGetTexture2D()
    {
        if (viewNumber == ViewNumber.View1)
        {
            maxIndex = imagestack.GetMaxValue(ViewNumber.View1);
            SliceName = "View 1";
        }
        else if (viewNumber == ViewNumber.View2)
        {
            maxIndex = imagestack.GetMaxValue(ViewNumber.View2);
            SliceName = "View 2";
        }
        else
        {
            maxIndex = imagestack.GetMaxValue(ViewNumber.View3);
            SliceName = "View 3";
        }

        this.transform.localEulerAngles = ViewCorrection(viewNumber);

        doneLoading = true;
        texture =  imagestack.GetTexture2D(viewNumber, sliceindex);
        displayCanvas.GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
        if (texture != null)
        {
            texture.Apply();
        }
        DebugMessage = "Showing " + SliceName + ", Slice No. " + sliceindex;
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

    public void SetView1()
    {
        viewNumber = ViewNumber.View1;
        SliceName = "View 1";
    }
    public void SetView2()
    {
        viewNumber = ViewNumber.View2;
        SliceName = "View 2";

    }
    public void SetView3()
    {
        viewNumber = ViewNumber.View3;
        SliceName = "View 3";
    }

    private Vector3 ViewCorrection(ViewNumber view)
    {
        SliceType type;
        SliceType[] SliceTypes = { View1SliceType, View2SliceType, View3SliceType };

        switch (view)
        {
            case ViewNumber.View1:
                type = SliceTypes[0];
                break;
            case ViewNumber.View2:
                type = SliceTypes[1];
                break;
            case ViewNumber.View3:
                type = SliceTypes[2];
                break;
            default:
                type = SliceType.Sagittal;
                break;
        }

        if (type == SliceType.Transversal)
        {
            return new Vector3(180, 0, 0);
        }
        else
        {
            return DefaultView;
        }
    }
}
