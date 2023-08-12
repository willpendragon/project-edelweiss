using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToonSketch.Core
{
    public class PostProcessContext
    {
        public PostProcessProfile profile;
        public Camera camera;
        public MaterialFactory materialFactory;
        public RenderTextureFactory renderTextureFactory;

        public bool interrupted { get; private set; }

        public void Interrupt()
        {
            interrupted = true;
        }

        public PostProcessContext Reset()
        {
            profile = null;
            camera = null;
            materialFactory = null;
            renderTextureFactory = null;
            interrupted = false;
            return this;
        }

        public int width
        {
            get { return camera.pixelWidth; }
        }

        public int height
        {
            get { return camera.pixelHeight; }
        }
    }
}