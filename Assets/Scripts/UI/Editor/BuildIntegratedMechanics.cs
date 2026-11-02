#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildIntegratedMechanics
{
    public static void Build()
    {
        WarTheatre.Validate(WarTheatre.Data);
        if(CountryDiplomacy.Definitions.Length!=25)throw new Exception("Diplomatic catalogue incomplete.");
        if(MonasticResearch.Rules.ProgressPerSecret!=100||MonasticResearch.Rules.ProgressPerMonkPerDay!=1)throw new Exception("Unexpected monastic playtest balance.");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/Version-36.11/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Integrated mechanics build failed: "+report.summary.result);
        UnityEngine.Debug.Log("INTEGRATED_MECHANICS_BUILD_COMPLETE");
    }
}
#endif
