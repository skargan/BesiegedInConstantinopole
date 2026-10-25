using UnityEditor;
using UnityEngine;
using System.Linq;
public static class ValidateStory
{
    public static void Run()
    {
        foreach(var file in System.IO.Directory.GetFiles("Assets/Yarn/Test","*.yarn",System.IO.SearchOption.AllDirectories)) {
            string text=System.IO.File.ReadAllText(file);
            if(System.Text.RegularExpressions.Regex.IsMatch(text,@"(?<!\\)\[Cost:"))throw new System.Exception("Unescaped cost markup in "+file);
            if(System.Text.RegularExpressions.Regex.IsMatch(text,@"<<Change\s+\w+\s+\$\w+"))throw new System.Exception("Uninterpolated numeric Yarn command in "+file);
        }
        string path="Assets/Yarn/Test/NewProject.yarnproject";
        AssetDatabase.ImportAsset("Assets/Yarn/Test/Story_events_YARN_11.yarn",ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Yarn/Test/Random_events_recurring.yarn",ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Yarn/Test/Shaherezade.yarn",ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Yarn/Test/FoodShortage.yarn",ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
        var project=AssetDatabase.LoadAssetAtPath<Yarn.Unity.YarnProject>(path);
        Debug.Log("STORY_NODES "+string.Join(",",project.NodeNames));
        if(!project.NodeNames.Contains("random22") || !project.NodeNames.Contains("random8a")) throw new System.Exception("Missing recurring events");
        if(!project.NodeNames.Contains("shah3")) throw new System.Exception("Missing Shaherezade chain");
        if(!project.NodeNames.Contains("story0")) throw new System.Exception("Missing story0");
    }
}