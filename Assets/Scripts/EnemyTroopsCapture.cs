#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class EnemyTroopsCapture:MonoBehaviour
{
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Begin(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-enemy-troops-capture")<0)return;var go=new GameObject("Enemy troop verification");DontDestroyOnLoad(go);go.AddComponent<EnemyTroopsCapture>();}
 string output;int passed;
 void Check(bool ok,string label){if(!ok)throw new Exception(label);passed++;}
 string Visible()=>string.Join("\n",FindObjectsOfType<TMPro.TMP_Text>().Select(t=>t.text));
 IEnumerator Start(){
  AudioListener.volume=0;Application.runInBackground=true;
  output=Path.GetFullPath(Path.Combine(Application.dataPath,"../EnemyTroopsCaptures"));Directory.CreateDirectory(output);VariableSingleton.TestSavePath=Path.Combine(output,"test-save.json");
  yield return null;FindObjectOfType<MainMenuUI>().NewGame();yield return new WaitForSeconds(.5f);
  var c=CouncilUI.For(FindObjectOfType<ManagerUI>());
  VariableSingleton.SetInt("iSpies",1);VariableSingleton.SetInt("iBalkans",12345);VariableSingleton.SetInt("iBazouks",23456);VariableSingleton.SetInt("iJanissaries",3456);
  int money=CouncilActions.Int("iMoney");int turn=VariableSingleton.Turn;
  c.Spies();yield return null;
  var card=GameObject.Find("Intelligence report 1");Check(card!=null&&card.GetComponent<Button>()!=null,"Spy card not clickable");card.GetComponent<Button>().onClick.Invoke();yield return Capture("enemy-troops-known");
  Check(Visible().Contains("12,345")&&Visible().Contains("23,456")&&Visible().Contains("3,456"),"Current counts missing");
  Check(Visible().Contains("Janissaries")&&Visible().Contains("Bashi-bazouks"),"Types missing");
  Check(Visible().Contains(AI.Instance.TroopAssaultRating(2)+" points"),"Scene assault rating missing");
  Check(Visible().Contains(AI.Instance.TroopLethality(2).ToString("0.###")+" per attacker"),"Scene lethality missing");
  ControlSingleton.Instance.ReturnAction();yield return null;Check(GameObject.Find("Intelligence report 1")!=null,"Return to spies failed");
  c.Overview();yield return null;card=GameObject.Find("Enemy report 0");Check(card!=null&&card.GetComponent<Button>()!=null,"Overview card not clickable");card.GetComponent<Button>().onClick.Invoke();yield return null;
  ControlSingleton.Instance.ReturnAction();yield return null;Check(GameObject.Find("Enemy report 0")!=null,"Return to overview failed");
  VariableSingleton.SetInt("iSpies",0);c.EnemyTroops(c.Overview);yield return Capture("enemy-troops-unknown");
  Check(!Visible().Contains("12,345")&&!Visible().Contains("23,456"),"Hidden counts leaked");Check(Visible().Contains("Remaining: unknown"),"Unknown status missing");
  VariableSingleton.SetInt("iSpies",1);VariableSingleton.SetInt("iJanissaries",0);c.EnemyTroops(c.Overview);yield return null;Check(Visible().Contains("Available wave: 0"),"Zero-force wave incorrect");
  Check(CouncilActions.Int("iMoney")==money&&VariableSingleton.Turn==turn,"Inspection changed campaign");
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
