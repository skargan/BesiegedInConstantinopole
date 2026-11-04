#if UNITY_EDITOR
using System;using System.Linq;using UnityEditor;using UnityEditor.Build.Reporting;
public static class BuildEventPresentation{public static void Build(){PlayerSettings.bundleVersion="37";var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Version-37/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Event presentation build failed");}}
#endif

