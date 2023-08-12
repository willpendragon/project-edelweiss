using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToonSketch.Core
{
    public class PostProcessRenderer
    {
        private Texture2D source2DLUT = null;
        private Texture2D converted2DLUT = null;
        private float canvasTimePassed;

        static class Uniforms
        {
            // LoFi Shading
            internal static readonly int _LoFiShadeOffset = Shader.PropertyToID("_LoFiShadeOffset");
            internal static readonly int _LoFiShadeThreshold1 = Shader.PropertyToID("_LoFiShadeThreshold1");
            internal static readonly int _LoFiShadeThreshold2 = Shader.PropertyToID("_LoFiShadeThreshold2");
            internal static readonly int _LoFiShadeThreshold3 = Shader.PropertyToID("_LoFiShadeThreshold3");
            internal static readonly int _LoFiShadeThreshold4 = Shader.PropertyToID("_LoFiShadeThreshold4");

            // Color Crush
            internal static readonly int _ColorCrushTex = Shader.PropertyToID("_ColorCrushTex");
            internal static readonly int _ColorCrushScale = Shader.PropertyToID("_ColorCrushScale");
            internal static readonly int _ColorCrushDim = Shader.PropertyToID("_ColorCrushDim");
            internal static readonly int _ColorCrushOffset = Shader.PropertyToID("_ColorCrushOffset");

            // Outlines
            internal static readonly int _OutlineNormalSensitivity = Shader.PropertyToID("_OutlineNormalSensitivity");
            internal static readonly int _OutlineDepthSensitivity = Shader.PropertyToID("_OutlineDepthSensitivity");
            internal static readonly int _OutlineMinDistance = Shader.PropertyToID("_OutlineMinDistance");
            internal static readonly int _OutlineMaxDistance = Shader.PropertyToID("_OutlineMaxDistance");

            // Sketch
            internal static readonly int _SketchAngles = Shader.PropertyToID("_SketchAngles");
            internal static readonly int _SketchSamples = Shader.PropertyToID("_SketchSamples");
            internal static readonly int _SketchDetail = Shader.PropertyToID("_SketchDetail");
            internal static readonly int _SketchSharpness = Shader.PropertyToID("_SketchSharpness");
            internal static readonly int _SketchWeight = Shader.PropertyToID("_SketchWeight");

            // Canvas Effect
            internal static readonly int _CanvasTex = Shader.PropertyToID("_CanvasTex");
            internal static readonly int _CanvasOffset = Shader.PropertyToID("_CanvasOffset");
            internal static readonly int _CanvasWeight = Shader.PropertyToID("_CanvasWeight");

            // Composite
            internal static readonly int _LoFiAmount = Shader.PropertyToID("_LoFiAmount");
            internal static readonly int _ColorAmount = Shader.PropertyToID("_ColorAmount");
            internal static readonly int _OutlineAmount = Shader.PropertyToID("_OutlineAmount");
            internal static readonly int _CanvasAmount = Shader.PropertyToID("_CanvasAmount");
        }

        const string k_ShaderString = "Hidden/ToonSketch/PostProcessEffects";

        public void Render(PostProcessContext context, RenderTexture source, RenderTexture destination)
        {
            var material = context.materialFactory.Get(k_ShaderString);
            material.shaderKeywords = null;

            // LoFi Shading
            if (context.profile.loFiShadeEffect)
            {
                material.EnableKeyword("_LOFISHADE_ON");
                material.SetFloat(Uniforms._LoFiShadeOffset, context.profile.loFiShadeOffset);
                material.SetFloat(Uniforms._LoFiShadeThreshold1, context.profile.loFiShadeThreshold1);
                material.SetFloat(Uniforms._LoFiShadeThreshold2, context.profile.loFiShadeThreshold2);
                material.SetFloat(Uniforms._LoFiShadeThreshold3, context.profile.loFiShadeThreshold3);
                material.SetFloat(Uniforms._LoFiShadeThreshold4, context.profile.loFiShadeThreshold4);
                material.SetFloat(Uniforms._LoFiAmount, context.profile.loFiAmount);
            }

            // Color Crush
            if (context.profile.colorCrushEffect)
            {
                material.EnableKeyword("_COLORCRUSH_ON");
                if (converted2DLUT == null || context.profile.colorCrushTexture != source2DLUT)
                {
                    if (context.profile.colorCrushTexture == null)
                        IdentityLUT();
                    else
                    {
                        if (!ConvertLUT(context.profile.colorCrushTexture))
                            IdentityLUT();
                        else
                            source2DLUT = context.profile.colorCrushTexture;
                    }
                }
                converted2DLUT.wrapMode = TextureWrapMode.Clamp;
                float lutSize = converted2DLUT.width;
                float lutSquare = Mathf.Sqrt(lutSize);
                material.SetTexture(Uniforms._ColorCrushTex, converted2DLUT);
                material.SetFloat(Uniforms._ColorCrushScale, (lutSquare - 1) / lutSize);
                material.SetFloat(Uniforms._ColorCrushDim, lutSquare);
                material.SetFloat(Uniforms._ColorCrushOffset, 1 / (2 * lutSize));
            }
            else
            {
                if (converted2DLUT != null)
                    DisposeLUT();
            }

            // Outlines
            if (context.profile.outlineEffect)
            {
                material.EnableKeyword("_OUTLINE_ON");
                material.SetFloat(Uniforms._OutlineNormalSensitivity, context.profile.outlineNormalSensitivity);
                material.SetFloat(Uniforms._OutlineDepthSensitivity, context.profile.outlineDepthSensitivity);
                material.SetFloat(Uniforms._OutlineMinDistance, context.profile.outlineMinDistance);
                material.SetFloat(Uniforms._OutlineMaxDistance, context.profile.outlineMaxDistance);
                material.SetFloat(Uniforms._OutlineAmount, context.profile.outlineAmount);
            }

            // Sketch
            if (context.profile.sketchEffect)
            {
                material.EnableKeyword("_SKETCH_ON");
                if (context.profile.sketchMultiEffect)
                {
                    material.EnableKeyword("_SKETCH_MULTI_ON");
                    material.SetInt(Uniforms._SketchAngles, context.profile.sketchAngles);
                }
                material.SetInt(Uniforms._SketchSamples, context.profile.sketchSamples);
                material.SetFloat(Uniforms._SketchDetail, context.profile.sketchDetail);
                material.SetFloat(Uniforms._SketchSharpness, context.profile.sketchSharpness);
                material.SetFloat(Uniforms._SketchWeight, context.profile.sketchWeight);
                material.SetFloat(Uniforms._OutlineAmount, context.profile.outlineAmount);
            }

            // Canvas Effect
            if (context.profile.canvasEffect)
            {
                material.EnableKeyword("_CANVAS_ON");
                material.SetTexture(Uniforms._CanvasTex, context.profile.canvasTexture);
                material.SetFloat(Uniforms._CanvasWeight, context.profile.canvasWeight);
                Vector2 offset = Vector2.zero;
                if (context.profile.canvasJitterSpeed > 0f)
                {
                    canvasTimePassed += Time.deltaTime;
                    if (canvasTimePassed >= context.profile.canvasJitterSpeed)
                    {
                        offset.x = Random.value;
                        offset.y = Random.value;
                        canvasTimePassed = 0f;
                    }
                }
                material.SetVector(Uniforms._CanvasOffset, offset);
                material.SetFloat(Uniforms._CanvasAmount, context.profile.canvasAmount);
            }

            // Composite
            material.SetFloat(Uniforms._ColorAmount, context.profile.colorAmount);

            Graphics.Blit(source, destination, material);
        }

        private bool ValidLUTDimensions(Texture2D texture)
        {
            if (texture == null)
                return false;
            int height = texture.height;
            if (height != Mathf.FloorToInt(Mathf.Sqrt(texture.width)))
                return false;
            if (height != 16)
                return false;
            return true;
        }

        private void IdentityLUT()
        {
            int dim = 16;
            Color[] colors = new Color[dim * dim * dim * dim];
            float oneOverDim = 1.0f / (1.0f * dim - 1.0f);

            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    for (int x = 0; x < dim; x++)
                    {
                        for (int y = 0; y < dim; y++)
                        {
                            colors[x + i * dim + y * dim * dim + j * dim * dim * dim] =
                                new Color(x * oneOverDim, y * oneOverDim, (j * dim + i) / (dim * dim - 1.0f), 1);
                        }
                    }
                }
            }
            
            if (converted2DLUT != null)
                Object.DestroyImmediate(converted2DLUT);
            converted2DLUT = new Texture2D(dim * dim, dim * dim, TextureFormat.ARGB32, false);
            converted2DLUT.SetPixels(colors);
            converted2DLUT.Apply();
            source2DLUT = null;
        }

        private bool ConvertLUT(Texture2D texture)
        {
            if (texture == null)
                return false;
            int dim = texture.width * texture.height;
            dim = texture.height;
            if (!ValidLUTDimensions(texture))
                return false;
            Color[] original = texture.GetPixels();
            Color[] colors = new Color[dim * dim * dim * dim];

            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    for (int x = 0; x < dim; x++)
                    {
                        for (int y = 0; y < dim; y++)
                        {
                            float b = (i + j * dim * 1.0f) / dim;
                            int bi0 = Mathf.FloorToInt(b);
                            int bi1 = Mathf.Min(bi0 + 1, dim - 1);
                            float f = b - bi0;

                            int index = x + (dim - y - 1) * dim * dim;

                            Color col1 = original[index + bi0 * dim];
                            Color col2 = original[index + bi1 * dim];

                            colors[x + i * dim + y * dim * dim + j * dim * dim * dim] =
                                Color.Lerp(col1, col2, f);
                        }
                    }
                }
            }

            if (converted2DLUT != null)
                Object.DestroyImmediate(converted2DLUT);
            converted2DLUT = new Texture2D(dim * dim, dim * dim, TextureFormat.ARGB32, false);
            converted2DLUT.SetPixels(colors);
            converted2DLUT.Apply();
            return true;
        }

        private void DisposeLUT()
        {
            if (converted2DLUT != null)
                Object.DestroyImmediate(converted2DLUT);
            converted2DLUT = null;
            source2DLUT = null;
        }

        public void Dispose()
        {
            DisposeLUT();
        }
    }
}