#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Linq;
public static class BuildCityMapPreview
{
    public static void Build(){
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset("Assets/Yarn/Test/NewProject.yarnproject",ImportAssetOptions.ForceUpdate);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/CityMapPreview/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("City map preview build failed");
    }
}
#endif
