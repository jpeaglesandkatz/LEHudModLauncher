using AssetsTools.NET;
using AssetsTools.NET.Extra;
using MelonLoader;

namespace LEHuDModLauncher.Classlibs;

public static class NewUnityHelper
{

    public static string? ReadGameInfo(AssetsManager assetsManager, string gameDataPath)
    {
        AssetsFileInstance instance;
        var _assetsManager = new AssetsManager();
        var gameVer = "";
        try
        {
            var bundlePath = Path.Combine(gameDataPath, "globalgamemanagers");
            if (!File.Exists(bundlePath))
                bundlePath = Path.Combine(gameDataPath, "mainData");

            if (!File.Exists(bundlePath))
            {
                bundlePath = Path.Combine(gameDataPath, "data.unity3d");

                var bundleFile = _assetsManager.LoadBundleFile(bundlePath);
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
                var playerSettings = assetFiles.First();

                if (_assetsManager.GetBaseField(instance, playerSettings) is { } playerSettingsBaseField)
                {
                    var bundleVersion = playerSettingsBaseField.Get("bundleVersion");
                    if (bundleVersion != null)
                    {
                        gameVer = bundleVersion.AsString;
                    }

                }
            }
        }
        catch (Exception ex)
        {
            return null;
        }
        instance.file.Close();
        _assetsManager.UnloadAll();
        return gameVer;
    }

}