#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class BuildBisevoPreview
{
    public static void Build()
    {
        foreach(var image in Directory.GetFiles("Assets/Resources/UI/Maps/Bisevo","*.png")) {
            AssetDatabase.ImportAsset(image,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(image);importer.textureType=TextureImporterType.Default;
            importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;
            importer.filterMode=FilterMode.Bilinear;importer.alphaIsTransparency=true;importer.SaveAndReimport();
        }
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/BisevoPreview/Saving Constantinople.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development });
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Bisevo preview export failed");
    }
}
#endif
