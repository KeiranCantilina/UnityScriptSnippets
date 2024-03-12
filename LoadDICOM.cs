using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DICOMParser;
using DICOMViews;
using System.IO;
using Threads;
using System.Threading;
using System;


public class LoadDICOM : MonoBehaviour
{
    //
    private string folderpath;
   // public string foldername;
    public ImageStack imagestack;
    private Texture2D texture;
    private int sliceindex;
    public SliceType sliceType;
    private SliceType oldSliceType;
    public GameObject displayCanvas;
    private ThreadGroupState ThreadState;
    private string DebugMessage = "Ready to Load";
    private int processingStage = 0;

    // Start is called before the first frame update
    void Start()
    {
        sliceindex = 144;
        folderpath = Application.dataPath + "\\Datasets\\1156_DICOM";
        oldSliceType = sliceType;
    }

    // Update is called once per frame
    void Update()
    {
        
        // Refresh texture display if slice type is changed
        if(oldSliceType != sliceType)
        {
            oldSliceType = sliceType;
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
        if(processingStage != 0 && processingStage != 6)
        {
            imagestack.WriteDebug(DebugMessage + "\n" + "Progress: " + 100 * ThreadState.Progress / ThreadState.TotalProgress + "%");
        }
        else
        {
            imagestack.WriteDebug(DebugMessage);
        }
        
    }

    // Triggered by button in UI
    public void LoadFile()
    {
        processingStage = 1;
        DebugMessage = "Loading files...";
        ThreadState = imagestack.StartParsingFiles(folderpath);
        ThreadState.ThreadsFinished += IncrementProcessCounter;
    }

    public void StartPreProcess()
    {
        DebugMessage = "Preprocessing...";
        ThreadState = imagestack.StartPreprocessData();
        ThreadState.ThreadsFinished += IncrementProcessCounter;
    }

    public void StartCreatingTextures()
    {
        DebugMessage = "Creating Textures...";
        ThreadState = imagestack.StartCreatingTextures();
        ThreadState.ThreadsFinished += IncrementProcessCounter;
    }

    // Triggered by changing slice type or index (or at end of load file process)
    public void StartGetTexture2D()
    {
        texture =  imagestack.GetTexture2D(sliceType, sliceindex);
        displayCanvas.GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
        texture.Apply();
        DebugMessage = "Showing " + sliceType + " View, Slice No. " + sliceindex;
    }

    public void incrementSliceIndex()
    {
        sliceindex++;
        processingStage = 6;
    }

    public void decrementSliceIndex()
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
}
