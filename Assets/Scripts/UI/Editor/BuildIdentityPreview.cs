#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildIdentityPreview
{
    public static void Build()
    {
        PlayerSettings.productName = GameIdentity.Title;
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = "Builds/IdentityPreview/Saving Constantinople.exe",
            target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
        if (result.summary.result != BuildResult.Succeeded) throw new Exception("Identity preview build failed");
    }
}
#endif
