#if UNITY_EDITOR
using System;using System.Linq;using UnityEditor;using UnityEditor.Build.Reporting;
public static class BuildLithuania {
 public static void Build(){PlayerSettings.bundleVersion="36.20";
 var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Version-36.20/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
 if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Merchant build failed");}
}
#endif
