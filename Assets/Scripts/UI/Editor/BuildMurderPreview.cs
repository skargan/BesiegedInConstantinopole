#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Linq;
public static class BuildMurderPreview
{
    public static void Build(){
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset("Assets/Yarn/Test/Brutal_murder.yarn",ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Yarn/Test/NewProject.yarnproject",ImportAssetOptions.ForceUpdate);
        var project=AssetDatabase.LoadAssetAtPath<Yarn.Unity.YarnProject>("Assets/Yarn/Test/NewProject.yarnproject");
        for(int stage=1;stage<=7;stage++)if(project==null||!project.NodeNames.Contains("murder"+stage))throw new System.Exception("Compiled Yarn is missing murder"+stage);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/MurderPreview/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Murder preview build failed");
    }
}
#endif
