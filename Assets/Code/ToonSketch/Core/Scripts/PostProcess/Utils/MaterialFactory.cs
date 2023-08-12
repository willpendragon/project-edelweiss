using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToonSketch.Core
{
    public sealed class MaterialFactory : IDisposable
    {
        Dictionary<string, Material> m_Materials;

        public MaterialFactory()
        {
            m_Materials = new Dictionary<string, Material>();
        }

        public Material Get(string shaderName)
        {
            Material material;

            if (!m_Materials.TryGetValue(shaderName, out material))
            {
                var shader = Shader.Find(shaderName);

                if (shader == null)
                    throw new ArgumentException(string.Format("Shader not found ({0})", shaderName));

                material = new Material(shader)
                {
                    name = string.Format("PostFX - {0}", shaderName.Substring(shaderName.LastIndexOf("/") + 1)),
                    hideFlags = HideFlags.DontSave
                };

                m_Materials.Add(shaderName, material);
            }

            return material;
        }

        public void Dispose()
        {
            var enumerator = m_Materials.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var material = enumerator.Current.Value;
                if (material != null)
                {
#if UNITY_EDITOR
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(material);
                    else
                        UnityEngine.Object.DestroyImmediate(material);
#else
                    UnityEngine.Object.Destroy(material);
#endif
                }
            }

            m_Materials.Clear();
        }
    }
}