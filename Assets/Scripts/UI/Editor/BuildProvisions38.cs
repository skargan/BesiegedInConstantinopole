#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildProvisions38
{
    public static void Build()
    {
        PlayerSettings.bundleVersion="38";
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/Version-38/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Version 38 provisions build failed");
        Debug.Log("PROVISIONS38_BUILD_COMPLETE");
    }
}
#endif
