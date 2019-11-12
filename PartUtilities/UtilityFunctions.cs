using System;
using System.Collections.Generic;
using UnityEngine;

namespace JSIPartUtilities
{
	public static class JUtil
	{
		public static bool debugLoggingEnabled = true;

		public static void LogMessage (object caller, string line, params object[] list)
		{
			if (debugLoggingEnabled)
				Debug.Log (String.Format (caller.GetType ().Name + ": " + line, list));
		}

		public static void LogErrorMessage (object caller, string line, params object[] list)
		{
			Debug.LogError (String.Format (caller.GetType ().Name + ": " + line, list));
		}

		public static void ForceRightclickMenuRefresh ()
		{
			foreach (UIPartActionWindow thatWindow in UnityEngine.Object.FindObjectsOfType<UIPartActionWindow>()) {
				thatWindow.displayDirty = true;
			}
		}

		public static FlagBrowser CreateFlagSelectorWindow (object caller, FlagBrowser.FlagSelectedCallback selectedCallback, Callback dismissedCallback)
		{
			LogMessage (caller, "Creating flag selector window...");

			// I don't know the actual asset name for the flag prefab. There's probably a way to find it, but it's kind of tricky.
			// But FlagBrowserGUIButton class knows it!
			// So I create a dummy instance of it to get at the actual asset reference, and then replicate 
			// what it's doing to create a flag browser window.
			var sourceButton = new FlagBrowserGUIButton (null, null, null, null);
			FlagBrowser fb = (UnityEngine.Object.Instantiate ((UnityEngine.Object)sourceButton.FlagBrowserPrefab) as GameObject).GetComponent<FlagBrowser> ();
			fb.OnDismiss = dismissedCallback;
			fb.OnFlagSelected = selectedCallback;
			return fb;
		}

        internal static Dictionary<string, Shader> parsedShaders;
        static bool assetsLoaded = false;
        public static void LoadAssets()
        {
            assetsLoaded = true;
            String assetsPath = KSPUtil.ApplicationRootPath + "GameData/JSI/RPMBundles/";
            String shaderAssetBundleName = "rasterpropmonitor";
            parsedShaders = new Dictionary<string, Shader>();

            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                shaderAssetBundleName += "-windows";
            }
            else if (Application.platform == RuntimePlatform.LinuxPlayer)
            {
                shaderAssetBundleName += "-linux";
            }
            else if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                shaderAssetBundleName += "-osx";
            }
            shaderAssetBundleName += ".assetbundle";

            WWW www = new WWW("file://" + assetsPath + shaderAssetBundleName);

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log("Error loading AssetBundle: "+ www.error);
                return;
            }
            else if (www.assetBundle == null)
            {
                Debug.Log("Unable to load AssetBundle "+ www);
                return;
            }

            JUtil.parsedShaders.Clear();

            AssetBundle bundle = www.assetBundle;

            string[] assetNames = bundle.GetAllAssetNames();
            int len = assetNames.Length;

            Shader shader;
            for (int i = 0; i < len; i++)
            {
                if (assetNames[i].EndsWith(".shader"))
                {
                    shader = bundle.LoadAsset<Shader>(assetNames[i]);
                    if (!shader.isSupported)
                    {
                        Debug.Log("Shader " + shader.name+" - unsupported in this configuration: ");
                    }
                    JUtil.parsedShaders[shader.name] = shader;
                }
            }

            bundle.Unload(false);

             Debug.Log( "Found "+ JUtil.parsedShaders.Count + " RPM shaders");
        }

        internal static Shader LoadInternalShader(string shaderName)
        {
            if (!parsedShaders.ContainsKey(shaderName))
            {
                JUtil.LogErrorMessage(null, "Failed to find shader {0}", shaderName);
                return null;
            }
            else
            {
                return parsedShaders[shaderName];
            }
        }


        public static Material DrawLineMaterial()
        {
            if (!assetsLoaded)
                LoadAssets();

            var lineMaterial = new Material(LoadInternalShader("RPM/FontShader"));
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            return lineMaterial;
        }

    }
}

