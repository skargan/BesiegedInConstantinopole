#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class BuildEaglePreview
{
    public static void Build(){var path="Assets/Resources/UI/Maps/Landmarks/nameless-eagle-frames.png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var texture=AssetImporter.GetAtPath(path) as TextureImporter;if(texture==null)throw new Exception("Eagle texture importer missing");texture.textureType=TextureImporterType.Default;texture.mipmapEnabled=false;texture.npotScale=TextureImporterNPOTScale.None;texture.maxTextureSize=2048;texture.textureCompression=TextureImporterCompression.Uncompressed;texture.alphaIsTransparency=true;texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Bilinear;texture.SaveAndReimport();var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/TheodoroPreview/Saving Constantinople.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Eagle preview build failed");}
}
#endif
