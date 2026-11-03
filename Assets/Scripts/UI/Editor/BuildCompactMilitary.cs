#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildCompactMilitary
{
    public static void Build()
    {
        ProvincialWarRules.Validate(ProvincialWarRules.Data);
        PlayerSettings.bundleVersion="36.17";
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/Version-36.17/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Military presentation build failed");
    }
}
#endif
