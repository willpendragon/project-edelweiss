using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ToonSketchHatchedOutlineShaderGUI : ShaderGUI
{
    private ToonSketch.Shared.EditorParams.Properties basicProperties = new ToonSketch.Shared.EditorParams.Properties();
    private ToonSketch.Core.EditorParams.Properties coreProperties = new ToonSketch.Core.EditorParams.Properties();
    private MaterialEditor materialEditor;
    private bool firstTimeApply = true;

    private void Header()
    {
        ToonSketch.Shared.EditorUtils.Header(
            ToonSketch.Core.EditorParams.module,
            ToonSketch.Core.EditorParams.version,
            "Sketch Shader with Outlines"
        );
    }

    public void FindProperties(MaterialProperty[] properties)
    {
        basicProperties.styleMode = FindProperty("_Style", properties);
        basicProperties.blendMode = FindProperty("_Blend", properties);
        basicProperties.cullMode = FindProperty("_Cull", properties);
        basicProperties.albedoTexture = FindProperty("_MainTex", properties);
        basicProperties.albedoColor = FindProperty("_Color", properties);
        basicProperties.alphaCutoff = FindProperty("_Cutoff", properties);
        basicProperties.rampTextureEnable = FindProperty("_Ramp", properties);
        basicProperties.rampTexture = FindProperty("_RampTex", properties);
        basicProperties.rampThreshold = FindProperty("_RampThreshold", properties);
        basicProperties.rampCutoff = FindProperty("_RampCutoff", properties);
        basicProperties.bumpEnable = FindProperty("_Bump", properties);
        basicProperties.bumpTexture = FindProperty("_BumpMap", properties);
        basicProperties.bumpStrength = FindProperty("_BumpScale", properties);
        basicProperties.specularEnable = FindProperty("_Specular", properties);
        basicProperties.specularTexture = FindProperty("_SpecularTex", properties);
        basicProperties.specularColor = FindProperty("_SpecularColor", properties);
        basicProperties.specularType = FindProperty("_SpecularType", properties);
        basicProperties.smoothnessType = FindProperty("_SmoothnessType", properties);
        basicProperties.smoothness = FindProperty("_Smoothness", properties);
        basicProperties.glossyReflections = FindProperty("_GlossyReflections", properties);
        basicProperties.specularThreshold = FindProperty("_SpecularThreshold", properties);
        basicProperties.specularCutoff = FindProperty("_SpecularCutoff", properties);
        basicProperties.specularIntensity = FindProperty("_SpecularIntensity", properties);
        basicProperties.rimEnable = FindProperty("_RimLighting", properties);
        basicProperties.rimColor = FindProperty("_RimColor", properties);
        basicProperties.rimType = FindProperty("_RimType", properties);
        basicProperties.rimColoring = FindProperty("_RimColoring", properties);
        basicProperties.rimMin = FindProperty("_RimMin", properties);
        basicProperties.rimMax = FindProperty("_RimMax", properties);
        basicProperties.rimIntensity = FindProperty("_RimIntensity", properties);
        basicProperties.ignoreIndirect = FindProperty("_IgnoreIndirect", properties);
        basicProperties.outlineColor = FindProperty("_OutlineColor", properties);
        basicProperties.outlineWidth = FindProperty("_OutlineWidth", properties);
        basicProperties.outlineSaturation = FindProperty("_OutlineSaturation", properties);
        basicProperties.outlineBrightness = FindProperty("_OutlineBrightness", properties);
        basicProperties.outlineAngle = FindProperty("_OutlineAngle", properties);
        coreProperties.hatchShading = FindProperty("_HatchShading", properties);
        coreProperties.hatch0Texture = FindProperty("_Hatch0", properties);
        coreProperties.hatch1Texture = FindProperty("_Hatch1", properties);
        coreProperties.hatch2Texture = FindProperty("_Hatch2", properties);
        coreProperties.hatch3Texture = FindProperty("_Hatch3", properties);
        coreProperties.hatch4Texture = FindProperty("_Hatch4", properties);
        coreProperties.hatch5Texture = FindProperty("_Hatch5", properties);
        coreProperties.hatchScale = FindProperty("_HatchScale", properties);
        coreProperties.hatchThreshold = FindProperty("_HatchThreshold", properties);
        coreProperties.hatchStrength = FindProperty("_HatchStrength", properties);
        coreProperties.hatchUVs = FindProperty("_HatchUVs", properties);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        FindProperties(properties);
        this.materialEditor = materialEditor;
        Material material = materialEditor.target as Material;
        if (firstTimeApply)
        {
            MaterialChanged(material);
            firstTimeApply = false;
        }
        ShaderPropertiesGUI(material);
    }

    public void ShaderPropertiesGUI(Material material)
    {
        Header();
        EditorGUIUtility.labelWidth = 0f;
        EditorGUI.BeginChangeCheck();
        {
            ToonSketch.Shared.EditorUtils.ModePopup(materialEditor, basicProperties);
            ToonSketch.Shared.EditorUtils.MainSettings(materialEditor, basicProperties);
            ToonSketch.Shared.EditorUtils.RampSettings(materialEditor, basicProperties);
            ToonSketch.Shared.EditorUtils.BumpSettings(materialEditor, basicProperties);
            ToonSketch.Shared.EditorUtils.SpecularSettings(materialEditor, basicProperties);
            ToonSketch.Shared.EditorUtils.RimSettings(materialEditor, basicProperties);
            ToonSketch.Shared.EditorUtils.LightSettings(materialEditor, basicProperties);
            ToonSketch.Shared.EditorUtils.OutlineSettings(materialEditor, basicProperties);
            ToonSketch.Core.EditorUtils.HatchSettings(materialEditor, coreProperties);
            ToonSketch.Shared.EditorUtils.AdvancedSettings(materialEditor, basicProperties);
        }
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obj in basicProperties.blendMode.targets)
                MaterialChanged((Material)obj);
        }
    }

    public static void MaterialChanged(Material material)
    {
        ToonSketch.Shared.EditorUtils.MaterialChanged(material);
    }
}