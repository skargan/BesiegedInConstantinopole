#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Linq;
public static class BuildEmperorPreview
{
    public static void Build()
    {
        AssetDatabase.Refresh();
        foreach(var name in new[]{"charisma","intellect","martial"}){string path="Assets/Resources/UI/Identity/Attributes/"+name+".png";var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)throw new System.Exception("Attribute art missing: "+name);importer.maxTextureSize=512;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.filterMode=UnityEngine.FilterMode.Bilinear;importer.SaveAndReimport();}
        AssetDatabase.ImportAsset("Assets/Yarn/Test/Emperor_health.yarn",ImportAssetOptions.ForceUpdate);AssetDatabase.ImportAsset("Assets/Yarn/Test/NewProject.yarnproject",ImportAssetOptions.ForceUpdate);
        var project=AssetDatabase.LoadAssetAtPath<Yarn.Unity.YarnProject>("Assets/Yarn/Test/NewProject.yarnproject");
        for(int i=0;i<5;i++)if(project==null||!project.NodeNames.Contains("emperorHealth"+i))throw new System.Exception("Compiled health event missing: "+i);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/EmperorPreview/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Emperor preview build failed");
    }
}
#endif
