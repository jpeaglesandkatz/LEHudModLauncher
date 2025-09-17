using AssetsTools.NET;
using AssetsTools.NET.Extra;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEHuDModLauncher.Classlibs
{
    public static class NewUnityHelper
    {

        public static string ReadGameInfo(AssetsManager assetsManager, string gameDataPath)
        {
            AssetsFileInstance instance = null;
            AssetsManager _assetsManager = new AssetsManager();
            string EngineVersion, GameVer = "";
            try
            {
                string bundlePath = Path.Combine(gameDataPath, "globalgamemanagers");
                if (!File.Exists(bundlePath))
                    bundlePath = Path.Combine(gameDataPath, "mainData");

                if (!File.Exists(bundlePath))
                {
                    bundlePath = Path.Combine(gameDataPath, "data.unity3d");
                    
                    BundleFileInstance bundleFile = _assetsManager.LoadBundleFile(bundlePath);
                    instance = _assetsManager.LoadAssetsFileFromBundle(bundleFile, "globalgamemanagers");
                }
                else
                    instance = _assetsManager.LoadAssetsFile(bundlePath, true);
                if (instance == null)
                    return null;
                
                _assetsManager.LoadIncludedClassPackage();
                if (!instance.file.Metadata.TypeTreeEnabled)
                    _assetsManager.LoadClassDatabaseFromPackage(instance.file.Metadata.UnityVersion);


                List<AssetFileInfo> assetFiles = instance.file.GetAssetsOfType(AssetClassID.PlayerSettings);
                if (assetFiles.Count > 0)
                {
                    AssetFileInfo playerSettings = assetFiles.First();

                    AssetTypeValueField playerSettings_baseField = _assetsManager.GetBaseField(instance, playerSettings);
                    if (playerSettings_baseField != null)
                    {
                        AssetTypeValueField bundleVersion = playerSettings_baseField.Get("bundleVersion");
                        if (bundleVersion != null)
                        {
                            GameVer = bundleVersion.AsString;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            if (instance != null)
                instance.file.Close();
            _assetsManager.UnloadAll();
            return GameVer;
        }

    }
}
