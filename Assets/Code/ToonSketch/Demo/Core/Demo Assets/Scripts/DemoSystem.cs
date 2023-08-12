using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToonSketch.Core.Demo
{
    public class DemoSystem : MonoBehaviour
    {
        private enum AnimationState
        {
            Idle,
            Walk,
            Run,
            Salute
        }

        public DemoCamera mainCam;
        public ToonSketchPostProcessing camEffects;
        public Animator charAnim;
        public MeshRenderer[] meshRenderers;
        public SkinnedMeshRenderer[] skinRenderers;
        public Light directionalLight;
        public DemoOrbit[] spotLights;
        public Light pointLight;
        public Texture2D[] rampTextures;
		public PostProcessProfile[] effectProfiles;
        public bool hideGUI = false;

        private List<Material> materials;
        private List<Texture> ramps;
        private bool directionalLightOn;
        private bool spotLightsOn;
        private bool spotLightsOrbit;
        private bool pointLightOn;
        private bool hatchingOn;
        private int currentRamp;
		private bool softDiffuse;
		private int currentProfile;

        private void Awake()
        {
            CacheMaterials();
            SetAnimation(AnimationState.Idle);
            SetOrbitCam();
            SetDirectionalLight(true);
            SetSpotLights(true);
            SetSpotLightsOrbit(true);
            SetPointLight(false);
            SetHatching(true);
			SetRampTexture(0);
			SetDiffuseStyle(true);
			SetEffectProfile(0);
        }

        private void CacheMaterials()
        {
            materials = new List<Material>();
            foreach (MeshRenderer renderer in meshRenderers)
                foreach (Material material in renderer.materials)
                    materials.Add(material);
            foreach (SkinnedMeshRenderer renderer in skinRenderers)
                foreach (Material material in renderer.materials)
                    materials.Add(material);
            ramps = new List<Texture>();
            foreach (Material material in materials)
                ramps.Add(material.GetTexture("_RampTex"));
        }

        private void SetAnimation(AnimationState value)
        {
            switch (value)
            {
                case AnimationState.Idle:
                    charAnim.SetFloat("speed", 0f);
                    charAnim.SetBool("salute", false);
                    break;
                case AnimationState.Walk:
                    charAnim.SetFloat("speed", 0.5f);
                    charAnim.SetBool("salute", false);
                    break;
                case AnimationState.Run:
                    charAnim.SetFloat("speed", 1.5f);
                    charAnim.SetBool("salute", false);
                    break;
                case AnimationState.Salute:
                    charAnim.SetFloat("speed", 0f);
                    charAnim.SetBool("salute", true);
                    break;
            }
        }

        private void SetOrbitCam()
        {
            mainCam.gameObject.SetActive(true);
            mainCam.orbitSpeed = 20f;
            mainCam.autoOrbit = true;
        }

        private void SetStaticCam()
        {
            mainCam.gameObject.SetActive(true);
            mainCam.orbitSpeed = 0f;
            mainCam.autoOrbit = true;
        }

        private void SetFreeCam()
        {
            mainCam.gameObject.SetActive(true);
            mainCam.orbitSpeed = 20f;
            mainCam.autoOrbit = false;
        }

        private void SetDirectionalLight(bool value)
        {
            directionalLightOn = value;
            directionalLight.gameObject.SetActive(directionalLightOn);
        }

        private void SetSpotLights(bool value)
        {
            spotLightsOn = value;
            foreach (DemoOrbit light in spotLights)
                light.gameObject.SetActive(spotLightsOn);
        }

        private void SetSpotLightsOrbit(bool value)
        {
            spotLightsOrbit = value;
            foreach (DemoOrbit light in spotLights)
                light.orbiting = spotLightsOrbit;
        }

        private void SetPointLight(bool value)
        {
            pointLightOn = value;
            pointLight.gameObject.SetActive(pointLightOn);
        }

        private void SetHatching(bool value)
        {
            hatchingOn = value;
            foreach (Material material in materials)
            {
                material.SetFloat("_HatchShading", (hatchingOn) ? 1f : 0f);
                if (hatchingOn)
                    material.EnableKeyword("_TS_HATCHING_ON");
                else
                    material.DisableKeyword("_TS_HATCHING_ON");
            }
        }

		private void SetRampTexture(int value)
		{
            currentRamp = value % (rampTextures.Length + 1);
            if (currentRamp > 0)
                foreach (Material material in materials)
                    material.SetTexture("_RampTex", rampTextures[currentRamp - 1]);
            else
                for (int i = 0; i < materials.Count; i++)
                    materials[i].SetTexture("_RampTex", ramps[i]);
		}

		private void SetDiffuseStyle(bool value)
		{
			softDiffuse = value;
			foreach (Material material in materials)
				material.SetFloat("_Style", (softDiffuse) ? 0 : 1);
		}

		private void SetEffectProfile(int value)
		{
			if (effectProfiles.Length == 0)
				return;
			currentProfile = value % effectProfiles.Length;
			camEffects.profile = effectProfiles[currentProfile];
		}

        private void OnGUI()
        {
            if (hideGUI)
                return;
            int width = 200;
            int x = 10;
            int y = 10;
            // Animations
            GUI.Box(new Rect(x, y, width, 120), "Animation");
            x += 10;
            y += 30;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Idle"))
            {
                SetAnimation(AnimationState.Idle);
            }
            y += 20;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Walk"))
            {
                SetAnimation(AnimationState.Walk);
            }
            y += 20;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Run"))
            {
                SetAnimation(AnimationState.Run);
            }
            y += 20;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Salute"))
            {
                SetAnimation(AnimationState.Salute);
            }
            x -= 10;
            y += 40;
            // Cameras
            GUI.Box(new Rect(x, y, width, 100), "Camera");
            x += 10;
            y += 30;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Orbit Cam"))
            {
                SetOrbitCam();
            }
            y += 20;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Static Cam"))
            {
                SetStaticCam();
            }
            y += 20;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Free Cam"))
            {
                SetFreeCam();
            }
            x -= 10;
            y += 40;
            // Lights
            GUI.Box(new Rect(x, y, width, 120), "Lights");
            x += 10;
            y += 30;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Toggle Directional Light"))
            {
                SetDirectionalLight(!directionalLightOn);
            }
            y += 20;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Toggle Spotlights"))
            {
                SetSpotLights(!spotLightsOn);
            }
            y += 20;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Toggle Spotlight Orbit"))
            {
                SetSpotLightsOrbit(!spotLightsOrbit);
            }
            y += 20;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Toggle Point Light"))
            {
                SetPointLight(!pointLightOn);
            }
            x -= 10;
            y += 40;
            // Materials
            GUI.Box(new Rect(x, y, width, 100), "Materials");
            x += 10;
            y += 30;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Toggle Hatching"))
            {
                SetHatching(!hatchingOn);
            }
            y += 20;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Diffuse Style: " + (softDiffuse ? "Soft" : "Hard")))
            {
                SetDiffuseStyle(!softDiffuse);
            }
            y += 20;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Switch Ramp Texture"))
            {
				SetRampTexture(currentRamp + 1);
            }
            x -= 10;
            y += 40;
            // Effects
            GUI.Box(new Rect(x, y, width, 60), "Effects");
            x += 10;
            y += 30;
            if (GUI.Button(new Rect(x, y, width - 20, 20), "Switch Effects Profile"))
            {
				SetEffectProfile(currentProfile + 1);
            }
        }
    }
}