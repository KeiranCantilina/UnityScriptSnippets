﻿using System;
using UnityEngine;

namespace DICOMParser
{
    /// <summary>
    /// This class contains Color32 => Color32 functions that can be applied when processing the pixels contained inside a DICOM file.
    /// </summary>
    public class TransferFunction
    {
        private TransferFunction() { }

        /// <summary>
        /// Dynamic alpha calculation based on grey rgb(x,x,x) value, where x is the intensity.
        /// </summary>
        /// <param name="isoColor">input color (assumed r=g=b)</param>
        /// <returns>input color with calculated alpha value</returns>
        public static Color32 DYN_ALPHA(Color32 isoColor)
        {

            if (isoColor.r < 120)
            {
                double dynAlpha = 240 * Math.Max(isoColor.r - 15, 0) / 255d;
                isoColor.a = (byte) dynAlpha;
            }

            return isoColor;
        }

        /// <summary>
        /// Identity function color32 -> color32 
        /// </summary>
        /// <param name="isoColor"> input color </param>
        /// <returns>unchanged input color</returns>
        public static Color32 Identity(Color32 isoColor)
        {
            return isoColor;
        }

        public static Color32 GRAYSCALE(Color32 isoColor)
        {
            isoColor.r = (byte)((isoColor.r)*(230)/255);
            //isoColor.a = isoColor.r;
            return isoColor;
        }

        private float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }
    }

}
