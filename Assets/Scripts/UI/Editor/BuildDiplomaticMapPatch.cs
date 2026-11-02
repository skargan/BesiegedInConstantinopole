#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildDiplomaticMapPatch
{
    public static void Build()
    {
        WarTheatre.Validate(WarTheatre.Data);
        if(CountryDiplomacy.Definitions.Length!=31)throw new Exception("Expected 31 diplomatic courts.");
        foreach(string id in new[]{"ragusa","ionian_tocco","florence","milan","siena","theodoro","crimea","mamluks","poland_lithuania","papacy"})
            if(!WarTheatre.Data.regions.Any(r=>r.group==id&&r.points.Length>=3))throw new Exception("No selectable map territory for "+id);
        DiplomaticRepliesSmokeTest.Run();
        PlayerSettings.bundleVersion="36.13";
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/Version-36.13/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Diplomatic map build failed: "+report.summary.result);
        UnityEngine.Debug.Log("DIPLOMATIC_MAP_BUILD_COMPLETE");
    }
}
#endif
