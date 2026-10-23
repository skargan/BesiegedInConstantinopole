using UnityEditor;
using UnityEditor.Build.Reporting;
public static class Campaign200Build{
 public static void Build(){ValidateStory.Run();var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Menu.unity","Assets/Scenes/Continue Game.unity"},locationPathName="Builds/QA200/Besieged.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(r.summary.result!=BuildResult.Succeeded)throw new System.Exception("QA200 build failed");}
}
