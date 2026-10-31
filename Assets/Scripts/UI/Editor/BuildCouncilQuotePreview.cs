#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildCouncilQuotePreview
{
    public static void Build()
    {
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = Environment.GetCommandLineArgs().Contains("-release363") ? "Builds/Version-36.3/Besieged.exe" : "Builds/CouncilQuotePreview/Besieged.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Council quote preview build failed: " + report.summary.result);
        UnityEngine.Debug.Log("COUNCIL_QUOTE_BUILD_COMPLETE");
    }
}
#endif
