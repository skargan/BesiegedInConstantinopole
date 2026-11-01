#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildHourglassPatch
{
    public static void Build()
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/UI/MidnightMosaic/turn-hourglass.png");
        importer.textureType=TextureImporterType.Default;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.maxTextureSize=512;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.filterMode=UnityEngine.FilterMode.Bilinear;importer.SaveAndReimport();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/Version-36.6/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Hourglass build failed: "+report.summary.result);
        UnityEngine.Debug.Log("HOURGLASS_BUILD_COMPLETE");
    }
}
#endif
