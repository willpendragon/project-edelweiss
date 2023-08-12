using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToonSketch.Core
{
    [DisallowMultipleComponent, ExecuteInEditMode, ImageEffectAllowedInSceneView]
    [AddComponentMenu("ToonSketch/Post-Process Effects", -1)]
    [RequireComponent(typeof(Camera))]
    public class ToonSketchPostProcessing : MonoBehaviour
    {
        public PostProcessProfile profile;

        private PostProcessRenderer m_Renderer;
        private MaterialFactory m_MaterialFactory;
        private RenderTextureFactory m_RenderTextureFactory;
        private PostProcessContext m_Context;
        private Camera m_Camera;
        private PostProcessProfile m_PreviousProfile;
        private bool m_RenderingInSceneView = false;

        private void OnEnable()
        {
            // Set up our rendering components
            m_MaterialFactory = new MaterialFactory();
            m_RenderTextureFactory = new RenderTextureFactory();
            m_Context = new PostProcessContext();
            m_Context.materialFactory = m_MaterialFactory;
            m_Context.renderTextureFactory = m_RenderTextureFactory;
            UpdateProfile();
        }

        private void UpdateProfile()
        {
            // Add renderer entity if one doesn't exist yet
            if (m_Renderer == null)
                m_Renderer = new PostProcessRenderer();
            // Set previous profile flag
            m_PreviousProfile = profile;
            m_Context.profile = profile;
        }

        private void OnPreCull()
        {
            // Initialize the camera and update context data
            m_Camera = GetComponent<Camera>();
            if (profile == null || m_Camera == null)
                return;
            // Handle switching profiles
            if (m_PreviousProfile != profile)
                UpdateProfile();
            // Update camera settings
#if UNITY_EDITOR
            m_RenderingInSceneView = UnityEditor.SceneView.currentDrawingSceneView != null
                && UnityEditor.SceneView.currentDrawingSceneView.camera == m_Camera;
#endif
            m_Context.camera = m_Camera;
            if (m_Context.profile.outlineEffect)
                m_Context.camera.depthTextureMode = DepthTextureMode.DepthNormals;
        }

        private void OnPostRender()
        {
            if (profile == null || m_Camera == null)
                return;
            if (!m_RenderingInSceneView)
                m_Context.camera.ResetProjectionMatrix();
        }

        [ImageEffectTransformsToLDR]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (profile == null || m_Camera == null || m_Renderer == null)
            {
                Graphics.Blit(source, destination);
                return;
            }
            if (m_Renderer != null)
                m_Renderer.Render(m_Context, source, destination);
            else
                Graphics.Blit(source, destination);
            m_RenderTextureFactory.ReleaseAll();
        }

        private void OnDisable()
        {
            if (m_Camera != null)
                m_Camera.depthTextureMode = DepthTextureMode.None;
            if (m_Renderer != null)
                m_Renderer.Dispose();
            m_Renderer = null;
            m_MaterialFactory.Dispose();
            m_RenderTextureFactory.Dispose();
        }
    }
}