#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildMedicalRecoveryPreview
{
    public static void Build()
    {
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
            locationPathName="Builds/MedicalRecoveryPreview/Besieged.exe",
            target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development
        });
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Medical recovery preview build failed: "+report.summary.result);
        UnityEngine.Debug.Log("MEDICAL_RECOVERY_BUILD_COMPLETE");
    }
}
#endif
