#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildInfirmaryDesk {
    public static void Build(){
        PlayerSettings.bundleVersion="36.16";
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/Version-36.16/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Infirmary build failed");
        UnityEngine.Debug.Log("INFIRMARY_BUILD_COMPLETE");
    }
}
#endif
