#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildRelease43
{
 public static void Build(){SaveGameTests.Run();PlayerSettings.productName=GameIdentity.Title;PlayerSettings.bundleVersion="43";WarTheatre.Validate(WarTheatre.Data);var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Version-43/Saving Constantinople.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Version43 build failed");}
}
#endif
