#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Linq;
public static class BuildLocationsPreview
{
    public static void Build(){var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/LocationsPreview/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Locations preview build failed");}
}
#endif
