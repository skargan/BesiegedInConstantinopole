#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class BuildRelease39
{
    public static void Build()
    {
        PhasedAssaultTests.Run();
        Debug.Log("RELEASE39_ASSAULT_REGRESSION_PASSED");
        PlayerSettings.bundleVersion="39";
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/Version-39/Besieged.exe",
            target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Version 39 build failed: "+report.summary.result);
        Debug.Log("RELEASE39_BUILD_COMPLETE "+report.summary.totalSize+" bytes");
    }
}
#endif
