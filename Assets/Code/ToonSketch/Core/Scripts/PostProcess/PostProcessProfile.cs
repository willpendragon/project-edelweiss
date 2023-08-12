using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToonSketch.Core
{
    [CreateAssetMenu(menuName = "ToonSketch/Post-Process Profile")]
    public class PostProcessProfile : ScriptableObject
    {
        // LoFi Shading
        public bool loFiShadeEffect = false;
        [Range(-10f, 10f)]
        public float loFiShadeOffset = 5f;
        [Range(0f, 1f)]
        public float loFiShadeThreshold1 = 0.8f;
        [Range(0f, 1f)]
        public float loFiShadeThreshold2 = 0.6f;
        [Range(0f, 1f)]
        public float loFiShadeThreshold3 = 0.4f;
        [Range(0f, 1f)]
        public float loFiShadeThreshold4 = 0.2f;

        // Color Crush
        public bool colorCrushEffect = false;
        public Texture2D colorCrushTexture;

        // Outline Style
        [HideInInspector]
        public float outlineStyle = 0f;

        // Outlines
        public bool outlineEffect = false;
        [Range(0f, 2f)]
        public float outlineNormalSensitivity = 1f;
        [Range(0f, 2f)]
        public float outlineDepthSensitivity = 0.5f;
        [Range(0.2f, 5f)]
        public float outlineMinDistance = 1f;
        [Range(0f, 5f)]
        public float outlineMaxDistance = 2f;

        // Sketch
        public bool sketchEffect = false;
        public bool sketchMultiEffect = false;
        [Range(3, 8)]
        public int sketchAngles = 6;
        [Range(2, 16)]
        public int sketchSamples = 8;
        [Range(0f, 1f)]
        public float sketchSharpness = 0.8f;
        [Range(0f, 1f)]
        public float sketchDetail = 0.8f;
        [Range(0f, 1f)]
        public float sketchWeight = 0.8f;

        // Canvas Effect
        public bool canvasEffect = false;
        public Texture2D canvasTexture;
        [Range(0f, 1f)]
        public float canvasWeight = 0.1f;
        [Range(0f, 1f)]
        public float canvasJitterSpeed = 0.05f;

        // Composite
        [Range(0f, 1f)]
        public float colorAmount = 1f;
        [Range(0f, 1f)]
        public float loFiAmount = 1f;
        [Range(0f, 1f)]
        public float outlineAmount = 1f;
        [Range(0f, 1f)]
        public float canvasAmount = 1f;
    }
}