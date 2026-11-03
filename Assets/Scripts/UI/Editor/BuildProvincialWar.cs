#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class BuildProvincialWar
{
    public static void Build()
    {
        ProvincialWarRules.Validate(ProvincialWarRules.Data);
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/ProvincialPreview/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Provincial build failed");
        Debug.Log("PROVINCIAL_BUILD_COMPLETE");
    }
}
#endif
