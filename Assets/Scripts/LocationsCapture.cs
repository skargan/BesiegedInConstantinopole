#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
public class LocationsCapture:MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Begin(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-locations-capture")<0)return;var go=new GameObject("Location verification");DontDestroyOnLoad(go);go.AddComponent<LocationsCapture>();}
    string output;int passed;
    void Check(bool ok,string label){if(!ok)throw new Exception(label);passed++;}
    void Restore(VariableSingleton.CampaignSave state){typeof(VariableSingleton).GetMethod("RestoreState",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(VariableSingleton.Instance,new object[]{state});}
    IEnumerator Start(){
        AudioListener.volume=0;Application.runInBackground=true;
        output=Path.GetFullPath(Path.Combine(Application.dataPath,"../Captures"));Directory.CreateDirectory(output);VariableSingleton.TestSavePath=Path.Combine(output,"test-save.json");
        yield return null;FindObjectOfType<MainMenuUI>().NewGame();yield return new WaitForSeconds(.5f);
        var manager=FindObjectOfType<ManagerUI>();var c=CouncilUI.For(manager);
        VariableSingleton.ChangeBool("bCharWarrior0",true);VariableSingleton.ChangeBool("bCharCardinal7",true);
        int[] occupants={PalaceLocations.Visitor(0),PalaceLocations.Visitor(1),PalaceLocations.Visitor(2)};
        Check(occupants.Where(x=>x>=0).Distinct().Count()==occupants.Count(x=>x>=0),"Duplicate visitor across locations");
        Check(PalaceLocations.Visitor(0)==occupants[0]&&PalaceLocations.Visitor(1)==occupants[1],"Visitors rerolled");
        c.Palace();yield return Capture("palace");c.Garden();yield return Capture("garden");
        VariableSingleton.ChangeFloat("fHealth",.5f-VariableSingleton.GetFloatVariable("fHealth"));
        int turn=VariableSingleton.Turn;c.HagiaSophia();yield return Capture("hagia-sophia");
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth")-.51f)<.0001f,"First visit health");
        c.HagiaSophia();Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth")-.51f)<.0001f,"Repeated healing");
        Check(turn==VariableSingleton.Turn,"Visit consumed turn");
        var saved=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));
        VariableSingleton.SetInt("iLifeSanctuaryVisit",0);Restore(saved);PalaceLocations.VisitSanctuary();
        Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth")-.51f)<.0001f,"Reload healed twice");
        Check(PalaceLocations.Visitor(0)==occupants[0],"Reload changed visitor");
        saved.turn++;Restore(saved);PalaceLocations.VisitSanctuary();Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth")-.52f)<.0001f,"Next preparation turn failed");
        saved=VariableSingleton.Instance.CaptureState();saved.turn=VariableSingleton.EffectiveSiegeTurn;Restore(saved);PalaceLocations.VisitSanctuary();
        float health=VariableSingleton.GetFloatVariable("fHealth");saved=VariableSingleton.Instance.CaptureState();saved.turn++;Restore(saved);PalaceLocations.VisitSanctuary();Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth")-health-.01f)<.0001f,"Next siege turn failed");
        saved=VariableSingleton.Instance.CaptureState();saved.turn++;Restore(saved);VariableSingleton.ChangeFloat("fHealth",.995f-VariableSingleton.GetFloatVariable("fHealth"));PalaceLocations.VisitSanctuary();Check(VariableSingleton.GetFloatVariable("fHealth")==1,"Health cap");
        VariableSingleton.ChangeFloat("fHealth",-.1f);PalaceLocations.VisitSanctuary();Check(Mathf.Abs(VariableSingleton.GetFloatVariable("fHealth")-.9f)<.0001f,"Full-health visit not consumed");
        c.PalaceActions();yield return Capture("palace-actions");
        File.WriteAllText(Path.Combine(output,"checks.txt"),passed+" exported-player checks passed.");Application.Quit();
    }
    private IEnumerator Capture(string name)
    {
        yield return new WaitForSeconds(1);
        yield return null;
        File.WriteAllLines(Path.Combine(output,name+"-text.txt"),FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.name+" | "+t.rectTransform.rect+" | "+t.maxVisibleCharacters+" | "+string.Join(",",t.GetComponentsInParent<CanvasGroup>().Select(g=>g.name+":"+g.alpha))+" | "+t.text));
        // Render explicitly: a hidden Windows player can otherwise return a black back buffer.
        var cameraObject = new GameObject("Capture camera",typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic=true; camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(.06f,.07f,.08f); camera.nearClipPlane=.1f; camera.farClipPlane=100;
        var canvases=FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();
        var modes=canvases.Select(c=>c.renderMode).ToArray();
        var cameras=canvases.Select(c=>c.worldCamera).ToArray();
        var distances=canvases.Select(c=>c.planeDistance).ToArray();
        var target=new RenderTexture(1600,900,24);
        camera.targetTexture=target;
        foreach(var canvas in canvases) {canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;}
        Canvas.ForceUpdateCanvases();
        camera.Render();
        var previous=RenderTexture.active; RenderTexture.active=target;
        var shot=new Texture2D(1600,900,TextureFormat.RGB24,false);
        shot.ReadPixels(new Rect(0,0,1600,900),0,0);shot.Apply();
        File.WriteAllBytes(Path.Combine(output,name+".png"),shot.EncodeToPNG());
        RenderTexture.active=previous;
        for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
        camera.targetTexture=null;target.Release();Destroy(target);Destroy(shot);Destroy(cameraObject);
        yield return new WaitForSeconds(.3f);
    }
}
#endif
