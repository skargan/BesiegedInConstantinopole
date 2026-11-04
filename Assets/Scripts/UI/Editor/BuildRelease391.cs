#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildRelease391 {
 public static void Build(){
  ValidateStory.Run(); PhasedAssaultTests.Run();
  PlayerSettings.productName=GameIdentity.Title;PlayerSettings.bundleVersion="39.1";
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Version-39.1/Saving Constantinople.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("39.1 build failed");
 }
}
#endif
