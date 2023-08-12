using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ToonSketch.Core
{
    public class EditorUtils
    {
        public static void HatchSettings(MaterialEditor materialEditor, EditorParams.Properties properties)
        {
            Shared.EditorUtils.Section("Hatch Shading Settings");
            materialEditor.ShaderProperty(properties.hatchShading, EditorParams.Styles.hatchingText);
            EditorGUILayout.Space();
            if (properties.hatchShading != null && properties.hatchShading.floatValue == 1)
            {
                // Set default textures
                MaterialProperty[] texProps = new MaterialProperty[6] {
                    properties.hatch0Texture, properties.hatch1Texture,
                    properties.hatch2Texture, properties.hatch3Texture,
                    properties.hatch4Texture, properties.hatch5Texture };
                string[] guids = AssetDatabase.FindAssets("hatch t:texture2D", new[] { "Assets/ToonSketch/Core" });
                for (int i = 0; i < 6; i++)
                    if (texProps[i].textureValue == null) texProps[i].textureValue = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[i]));
                // GUI output
                materialEditor.TexturePropertySingleLine(EditorParams.Styles.hatch0Text, properties.hatch0Texture);
                materialEditor.TexturePropertySingleLine(EditorParams.Styles.hatch1Text, properties.hatch1Texture);
                materialEditor.TexturePropertySingleLine(EditorParams.Styles.hatch2Text, properties.hatch2Texture);
                materialEditor.TexturePropertySingleLine(EditorParams.Styles.hatch3Text, properties.hatch3Texture);
                materialEditor.TexturePropertySingleLine(EditorParams.Styles.hatch4Text, properties.hatch4Texture);
                materialEditor.TexturePropertySingleLine(EditorParams.Styles.hatch5Text, properties.hatch5Texture);
                materialEditor.ShaderProperty(properties.hatchScale, EditorParams.Styles.hatchScaleText);
                materialEditor.ShaderProperty(properties.hatchThreshold, EditorParams.Styles.hatchThresholdText);
                materialEditor.ShaderProperty(properties.hatchStrength, EditorParams.Styles.hatchStrengthText);
                materialEditor.ShaderProperty(properties.hatchUVs, EditorParams.Styles.hatchUVsText);
            }
        }
    }
}