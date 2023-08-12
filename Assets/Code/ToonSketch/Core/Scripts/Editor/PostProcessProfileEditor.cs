using System;
using UnityEngine;
using UnityEditor;

namespace ToonSketch.Core
{
    [CustomEditor(typeof(PostProcessProfile))]
    [CanEditMultipleObjects]
    public class PostProcessProfileEditor : Editor
    {
        public enum OutlineStyle
        {
            None,
            Standard,
            Sketched,
            SketchedAngle
        }

        public static readonly GUIContent[] OutlineStyleNames = {
             new GUIContent("Disabled", "Disable outlines"),
             new GUIContent("Standard", "Standard edge detection"),
             new GUIContent("Sketched (Fixed)", "Sketch effect edges with fixed angles"),
             new GUIContent("Sketched (Angles)", "Sketch effect edges with custom angles")
        };

        private static class Styles
        {
            public static string titleHeadingText = "Non-Photorealistic Effects";
            public static string loFiHeadingText = "LoFi Shading";
            public static string colorCrushHeadingText = "Color Crushing";
            public static string outlineHeadingText = "Outline Drawing";
            public static string canvasHeadingText = "Canvas Texturing";
            public static string blendHeadingText = "Final Blending";
            public static GUIContent loFiEnableText = new GUIContent("LoFi Shading?", "Enable loFi shading effect?");
            public static GUIContent loFiOffsetText = new GUIContent("Hatch Offset", "Offset of hatching lines");
            public static GUIContent loFiThreshold1Text = new GUIContent("Brightest Threshold", "Luminance threshold for given brightness");
            public static GUIContent loFiThreshold2Text = new GUIContent("Bright Threshold", "Luminance threshold for given brightness");
            public static GUIContent loFiThreshold3Text = new GUIContent("Dark Threshold", "Luminance threshold for given brightness");
            public static GUIContent loFiThreshold4Text = new GUIContent("Darkest Threshold", "Luminance threshold for given brightness");
            public static GUIContent colorCrushEnableText = new GUIContent("Color Crushing?", "Enable a color look up table to limit palette?");
            public static GUIContent colorCrushTextureText = new GUIContent("Palette Texture", "The look up table texture to use for the color palette (LUT)");
            public static GUIContent outlineEnableText = new GUIContent("Outline Drawing?", "Enable outlines to be drawn around objects?");
            public static GUIContent outlineStyleText = new GUIContent("Outline Style", "The style to use for outline effects");
            public static GUIContent outlineNormalSensitivityText = new GUIContent("Normal Sensitivity", "The sensitivity to use for detecting edges by surface normals");
            public static GUIContent outlineDepthSensitivityText = new GUIContent("Depth Sensitivity", "The sensitivity to use for detecting edges by object depth");
            public static GUIContent outlineDistanceText = new GUIContent("Sample Distance", "The minimum and maximum sample distances to use for edge detection");
            public static GUIContent outlineMinDistanceText = new GUIContent("Min", "The minimum sample distances to use for edge detection");
            public static GUIContent outlineMaxDistanceText = new GUIContent("Max", "The maximum sample distances to use for edge detection");
            public static GUIContent sketchAnglesText = new GUIContent("Angles", "The number of offset angles to use for edge detection");
            public static GUIContent sketchSamplesText = new GUIContent("Samples", "The number of samples to use for edge detection");
            public static GUIContent sketchSharpnessText = new GUIContent("Sharpness", "The sharpness of the drawn sketch");
            public static GUIContent sketchDetailText = new GUIContent("Detail", "The amount of details rendered in the sketch");
            public static GUIContent sketchWeightText = new GUIContent("Weight", "The weight of the lines drawn in the sketch");
            public static GUIContent canvasEnableText = new GUIContent("Canvas Texturing?", "Enable canvas effect to be textured over output?");
            public static GUIContent canvasTextureText = new GUIContent("Canvas Texture", "The texture to use for the canvas texture effect (RGB)");
            public static GUIContent canvasWeightText = new GUIContent("Weight", "The weight of the canvas texture on the output image");
            public static GUIContent canvasJitterSpeedText = new GUIContent("Jitter Speed", "The speed at which the canvas texture will jitter and change shape");
            public static GUIContent colorAmountText = new GUIContent("Base Color", "The blend amount to use for the base color");
            public static GUIContent loFiAmountText = new GUIContent("LoFi Shading", "The blend amount to use for the loFi shading");
            public static GUIContent outlineAmountText = new GUIContent("Outlines", "The blend amount to use for the outlines");
            public static GUIContent canvasAmountText = new GUIContent("Canvas", "The blend amount to use for the canvas texture");
            public static readonly GUIContent[] outlineStyleNames = OutlineStyleNames;
        }

        SerializedProperty loFiShadeEffect = null;
        SerializedProperty loFiShadeOffset = null;
        SerializedProperty loFiShadeThreshold1 = null;
        SerializedProperty loFiShadeThreshold2 = null;
        SerializedProperty loFiShadeThreshold3 = null;
        SerializedProperty loFiShadeThreshold4 = null;
        SerializedProperty colorCrushEffect = null;
        SerializedProperty colorCrushTexture = null;
        SerializedProperty outlineStyle = null;
        SerializedProperty outlineEffect = null;
        SerializedProperty outlineNormalSensitivity = null;
        SerializedProperty outlineDepthSensitivity = null;
        SerializedProperty outlineMinDistance = null;
        SerializedProperty outlineMaxDistance = null;
        SerializedProperty sketchEffect = null;
        SerializedProperty sketchMultiEffect = null;
        SerializedProperty sketchAngles = null;
        SerializedProperty sketchSamples = null;
        SerializedProperty sketchSharpness = null;
        SerializedProperty sketchDetail = null;
        SerializedProperty sketchWeight = null;
        SerializedProperty canvasEffect = null;
        SerializedProperty canvasTexture = null;
        SerializedProperty canvasWeight = null;
        SerializedProperty canvasJitterSpeed = null;
        SerializedProperty colorAmount = null;
        SerializedProperty loFiAmount = null;
        SerializedProperty outlineAmount = null;
        SerializedProperty canvasAmount = null;

        bool m_FirstTimeApply = true;

        private void OnEnable()
        {
            loFiShadeEffect = serializedObject.FindProperty("loFiShadeEffect");
            loFiShadeOffset = serializedObject.FindProperty("loFiShadeOffset");
            loFiShadeThreshold1 = serializedObject.FindProperty("loFiShadeThreshold1");
            loFiShadeThreshold2 = serializedObject.FindProperty("loFiShadeThreshold2");
            loFiShadeThreshold3 = serializedObject.FindProperty("loFiShadeThreshold3");
            loFiShadeThreshold4 = serializedObject.FindProperty("loFiShadeThreshold4");
            colorCrushEffect = serializedObject.FindProperty("colorCrushEffect");
            colorCrushTexture = serializedObject.FindProperty("colorCrushTexture");
            outlineStyle = serializedObject.FindProperty("outlineStyle");
            outlineEffect = serializedObject.FindProperty("outlineEffect");
            outlineNormalSensitivity = serializedObject.FindProperty("outlineNormalSensitivity");
            outlineDepthSensitivity = serializedObject.FindProperty("outlineDepthSensitivity");
            outlineMinDistance = serializedObject.FindProperty("outlineMinDistance");
            outlineMaxDistance = serializedObject.FindProperty("outlineMaxDistance");
            sketchEffect = serializedObject.FindProperty("sketchEffect");
            sketchMultiEffect = serializedObject.FindProperty("sketchMultiEffect");
            sketchAngles = serializedObject.FindProperty("sketchAngles");
            sketchSamples = serializedObject.FindProperty("sketchSamples");
            sketchSharpness = serializedObject.FindProperty("sketchSharpness");
            sketchDetail = serializedObject.FindProperty("sketchDetail");
            sketchWeight = serializedObject.FindProperty("sketchWeight");
            canvasEffect = serializedObject.FindProperty("canvasEffect");
            canvasTexture = serializedObject.FindProperty("canvasTexture");
            canvasWeight = serializedObject.FindProperty("canvasWeight");
            canvasJitterSpeed = serializedObject.FindProperty("canvasJitterSpeed");
            colorAmount = serializedObject.FindProperty("colorAmount");
            loFiAmount = serializedObject.FindProperty("loFiAmount");
            outlineAmount = serializedObject.FindProperty("outlineAmount");
            canvasAmount = serializedObject.FindProperty("canvasAmount");
        }

        private void GetHeading(int headingNum)
        {
            switch (headingNum)
            {
                case 0:
                    Shared.EditorUtils.Header(EditorParams.module, EditorParams.version, Styles.titleHeadingText);
                    break;
                case 1:
                    Shared.EditorUtils.Section(Styles.loFiHeadingText);
                    break;
                case 2:
                    Shared.EditorUtils.Section(Styles.colorCrushHeadingText);
                    break;
                case 3:
                    Shared.EditorUtils.Section(Styles.outlineHeadingText);
                    break;
                case 4:
                    Shared.EditorUtils.Section(Styles.canvasHeadingText);
                    break;
                case 5:
                    Shared.EditorUtils.Section(Styles.blendHeadingText);
                    break;
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_FirstTimeApply)
            {
                ApplySettings();
                m_FirstTimeApply = false;
            }
            serializedObject.Update();
            GetHeading(0);
            EditorGUIUtility.labelWidth = 0f;
            EditorGUI.BeginChangeCheck();
            {
                LoFiSettings();
                ColorCrushSettings();
                OutlineSettings();
                CanvasSettings();
                BlendSettings();
            }
            if (EditorGUI.EndChangeCheck())
                ApplySettings();
        }

        private void LoFiSettings()
        {
            GetHeading(1);
            EditorGUILayout.PropertyField(loFiShadeEffect, Styles.loFiEnableText);
            EditorGUILayout.Space();
            if (loFiShadeEffect != null && loFiShadeEffect.boolValue)
            {
                EditorGUILayout.PropertyField(loFiShadeOffset, Styles.loFiOffsetText);
                EditorGUILayout.PropertyField(loFiShadeThreshold1, Styles.loFiThreshold1Text);
                EditorGUILayout.PropertyField(loFiShadeThreshold2, Styles.loFiThreshold2Text);
                EditorGUILayout.PropertyField(loFiShadeThreshold3, Styles.loFiThreshold3Text);
                EditorGUILayout.PropertyField(loFiShadeThreshold4, Styles.loFiThreshold4Text);
            }
        }

        private void ColorCrushSettings()
        {
            GetHeading(2);
            EditorGUILayout.PropertyField(colorCrushEffect, Styles.colorCrushEnableText);
            EditorGUILayout.Space();
            if (colorCrushEffect != null && colorCrushEffect.boolValue)
            {
                EditorGUILayout.PropertyField(colorCrushTexture, Styles.colorCrushTextureText);
            }
        }

        private void OutlineSettings()
        {
            GetHeading(3);
            OutlineStylePopup();
            EditorGUILayout.Space();
            switch ((OutlineStyle)outlineStyle.floatValue)
            {
                case OutlineStyle.Standard:
                    float minDistance = outlineMinDistance.floatValue;
                    float maxDistance = outlineMaxDistance.floatValue;
                    EditorGUILayout.PropertyField(outlineNormalSensitivity, Styles.outlineNormalSensitivityText);
                    EditorGUILayout.PropertyField(outlineDepthSensitivity, Styles.outlineDepthSensitivityText);
                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUILayout.MinMaxSlider(Styles.outlineDistanceText, ref minDistance, ref maxDistance, 0.2f, 5f);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (minDistance != outlineMinDistance.floatValue)
                            outlineMinDistance.floatValue = minDistance;
                        if (maxDistance != outlineMaxDistance.floatValue)
                            outlineMaxDistance.floatValue = maxDistance;
                        ApplySettings();
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUILayout.PropertyField(outlineMinDistance, Styles.outlineMinDistanceText);
                        EditorGUILayout.PropertyField(outlineMaxDistance, Styles.outlineMaxDistanceText);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        ApplySettings();
                    }
                    EditorGUILayout.EndHorizontal();
                    break;
                case OutlineStyle.Sketched:
                    EditorGUILayout.PropertyField(sketchSamples, Styles.sketchSamplesText);
                    EditorGUILayout.PropertyField(sketchSharpness, Styles.sketchSharpnessText);
                    EditorGUILayout.PropertyField(sketchDetail, Styles.sketchDetailText);
                    EditorGUILayout.PropertyField(sketchWeight, Styles.sketchWeightText);
                    break;
                case OutlineStyle.SketchedAngle:
                    EditorGUILayout.PropertyField(sketchAngles, Styles.sketchAnglesText);
                    EditorGUILayout.PropertyField(sketchSamples, Styles.sketchSamplesText);
                    EditorGUILayout.PropertyField(sketchSharpness, Styles.sketchSharpnessText);
                    EditorGUILayout.PropertyField(sketchDetail, Styles.sketchDetailText);
                    EditorGUILayout.PropertyField(sketchWeight, Styles.sketchWeightText);
                    break;
            }
        }

        public void OutlineStylePopup()
        {
            var style = (OutlineStyle)outlineStyle.floatValue;
            EditorGUI.BeginChangeCheck();
            {
                style = (OutlineStyle)EditorGUILayout.Popup(Styles.outlineStyleText, (int)style, Styles.outlineStyleNames);
            }
            if (EditorGUI.EndChangeCheck())
            {
                outlineStyle.floatValue = (float)style;
                ApplySettings();
            }
        }

        private void CanvasSettings()
        {
            GetHeading(4);
            EditorGUILayout.PropertyField(canvasEffect, Styles.canvasEnableText);
            EditorGUILayout.Space();
            if (canvasEffect != null && canvasEffect.boolValue)
            {
                EditorGUILayout.PropertyField(canvasTexture, Styles.canvasTextureText);
                EditorGUILayout.PropertyField(canvasWeight, Styles.canvasWeightText);
                EditorGUILayout.PropertyField(canvasJitterSpeed, Styles.canvasJitterSpeedText);
            }
        }

        private void BlendSettings()
        {
            GetHeading(5);
            EditorGUILayout.PropertyField(colorAmount, Styles.colorAmountText);
            if (loFiShadeEffect != null && loFiShadeEffect.boolValue)
                EditorGUILayout.PropertyField(loFiAmount, Styles.loFiAmountText);
            if ((OutlineStyle)outlineStyle.floatValue != OutlineStyle.None)
                EditorGUILayout.PropertyField(outlineAmount, Styles.outlineAmountText);
            if (canvasEffect != null && canvasEffect.boolValue)
                EditorGUILayout.PropertyField(canvasAmount, Styles.canvasAmountText);
        }

        private void ApplySettings()
        {
            switch ((OutlineStyle)outlineStyle.floatValue)
            {
                case OutlineStyle.None:
                    outlineEffect.boolValue = false;
                    sketchEffect.boolValue = false;
                    sketchMultiEffect.boolValue = false;
                    break;
                case OutlineStyle.Standard:
                    outlineEffect.boolValue = true;
                    sketchEffect.boolValue = false;
                    sketchMultiEffect.boolValue = false;
                    break;
                case OutlineStyle.Sketched:
                    outlineEffect.boolValue = false;
                    sketchEffect.boolValue = true;
                    sketchMultiEffect.boolValue = false;
                    break;
                case OutlineStyle.SketchedAngle:
                    outlineEffect.boolValue = false;
                    sketchEffect.boolValue = true;
                    sketchMultiEffect.boolValue = true;
                    break;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}