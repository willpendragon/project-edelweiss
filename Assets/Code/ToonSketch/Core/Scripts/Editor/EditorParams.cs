using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ToonSketch.Core
{
    public class EditorParams
    {
        public const string module = "Core";
        public const string version = "2.0";

        public static class Styles
        {
            public static GUIContent hatchingText = new GUIContent("Hatch Shading?", "Enable the hatch shading effect?");
            public static GUIContent hatch0Text = new GUIContent("Brightest Hatching", "Hatching texture for given brightness (RGB)");
            public static GUIContent hatch1Text = new GUIContent("Brighter Hatching", "Hatching texture for given brightness (RGB)");
            public static GUIContent hatch2Text = new GUIContent("Bright Hatching", "Hatching texture for given brightness (RGB)");
            public static GUIContent hatch3Text = new GUIContent("Dark Hatching", "Hatching texture for given brightness (RGB)");
            public static GUIContent hatch4Text = new GUIContent("Darker Hatching", "Hatching texture for given brightness (RGB)");
            public static GUIContent hatch5Text = new GUIContent("Darkest Hatching", "Hatching texture for given brightness (RGB)");
            public static GUIContent hatchScaleText = new GUIContent("Hatch Scale", "Hatch line scaling");
            public static GUIContent hatchThresholdText = new GUIContent("Hatch Threshold", "Threshold for weighting luminance values");
            public static GUIContent hatchStrengthText = new GUIContent("Hatch Strength", "Strength of the hatching lines' appearance");
            public static GUIContent hatchUVsText = new GUIContent("Hatch UVs", "Source to use for hatching UVs");
        }

        public class Properties
        {
            public MaterialProperty hatchShading = null;
            public MaterialProperty hatch0Texture = null;
            public MaterialProperty hatch1Texture = null;
            public MaterialProperty hatch2Texture = null;
            public MaterialProperty hatch3Texture = null;
            public MaterialProperty hatch4Texture = null;
            public MaterialProperty hatch5Texture = null;
            public MaterialProperty hatchScale = null;
            public MaterialProperty hatchThreshold = null;
            public MaterialProperty hatchStrength = null;
            public MaterialProperty hatchUVs = null;
        }
    }
}