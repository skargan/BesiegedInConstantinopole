#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class BuildMaritimeMedallions
{
    public static void Build()
    {
        foreach(string id in new[]{"sea-imperial","sea-contested","sea-ottoman","strait-open","strait-interdicted"})
        {
            string path="Assets/Resources/UI/Maps/Maritime/"+id+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=1024;
            importer.SaveAndReimport();
            if(Resources.Load<Texture2D>("UI/Maps/Maritime/"+id)==null)throw new Exception("Missing medallion "+id);
        }
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/MaritimePreview/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Maritime build failed");
        Debug.Log("MARITIME_BUILD_COMPLETE");
    }
}
#endif
