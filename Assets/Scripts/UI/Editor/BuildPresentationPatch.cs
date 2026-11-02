#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildPresentationPatch
{
    public static void Build()
    {
        PlayerSettings.bundleVersion="36.14";
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/Version-36.14/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Presentation build failed: "+report.summary.result);
        UnityEngine.Debug.Log("PRESENTATION_BUILD_COMPLETE");
    }
}
#endif
