#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildCartographyPatch
{
    public static void Build()
    {
        WarTheatre.Validate(WarTheatre.Data);
        DiplomaticRepliesSmokeTest.Run();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/Version-36.9/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Cartography build failed: "+report.summary.result);
        UnityEngine.Debug.Log("CARTOGRAPHY_BUILD_COMPLETE");
    }
}
#endif
