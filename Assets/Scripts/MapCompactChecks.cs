#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class MapCompactChecks
{
    static int count;
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);count++;}
    static GameObject Map=>GameObject.Find("War Beyond the Walls map");
    static void Click(string name){Map.GetComponentsInChildren<Button>().First(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==name)).onClick.Invoke();}
    public static IEnumerator Run(string output)
    {
        count=0;
        var originalNameMode=DiplomaticNames.Mode;DiplomaticNames.Mode=DiplomaticNames.Style.Short;
        foreach(var name in DiplomaticNames.All){
            Check(!string.IsNullOrEmpty(name.Short)&&!string.IsNullOrEmpty(name.Full)&&!string.IsNullOrEmpty(name.Original),"Complete naming profile: "+name.Id);
            uint[] missing;Check(ImperialTheme.Font.HasCharacters(name.Display,out missing,true,true),"Native-script glyph coverage: "+name.Id);
        }
        foreach(string id in new[]{"naples","sicily","sardinia","sardinia_part2"}){
            var realm=WarTheatre.DiplomaticPower(WarTheatre.Find(id));
            Check(CountryDiplomacy.Index(realm.group)==CountryDiplomacy.Index("naples"),"Shared Alfonso diplomacy: "+id);
        }
        Check(WarTheatreUI.TerritoryTitle(WarTheatre.Find("sicily"))=="Sicily","Sicilian identity preserved");
        Check(WarTheatreUI.TerritoryTitle(WarTheatre.Find("sardinia_part2"))=="Sardinia","Sardinian possessions retain territorial identity");
        Check(new[]{"naples","sicily","sardinia"}.Select(id=>WarTheatreUI.TerritorySummary(WarTheatre.Find(id))).Distinct().Count()==3,"Three distinct territorial descriptions");
        foreach(var region in WarTheatre.Data.regions)Check(WarTheatreUI.CourtReligion(region)!="Unclassified","Every current region has a court mapping: "+region.id);
        foreach(string id in new[]{"bosnia","lithuania","gazaria","cyprus","athens","ionian_tocco"})Check(WarTheatreUI.CourtReligion(WarTheatre.Find(id))=="Catholic","Catholic ruling court "+id);
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900)}){
            Screen.SetResolution(size.x,size.y,false);yield return new WaitForSecondsRealtime(.4f);WarTheatreUI.Show();yield return null;
            string words=string.Join("\n",Map.GetComponentsInChildren<TMP_Text>().Select(t=>t.text));
            Check(!words.Contains("Hover for gold")&&!words.Contains("Historical atlas")&&!words.Contains("Frontiers circa"),"Map removes tutorial and atlas clutter");
            var region=Map.GetComponentsInChildren<TerritoryGraphic>().First(t=>t.name=="ionian_tocco");
            region.OnPointerEnter(new PointerEventData(EventSystem.current));region.OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});yield return null;
            words=string.Join("\n",Map.GetComponentsInChildren<TMP_Text>().Select(t=>t.text));
            Check(words.Contains("Ionian Lordships")&&!words.Contains("No war commitment")&&!words.Contains("nearby Corfu")&&!words.Contains("All possessions"),"Country detail is concise");
            Check(Map.GetComponentsInChildren<Image>().Any(i=>i.name.Contains("Attitudes/")),"Country detail shows shared attitude art");
            foreach(string id in new[]{"naples","sicily","sardinia"}){
                Map.GetComponentsInChildren<TerritoryGraphic>().First(t=>t.name==id).OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});yield return null;
                Check(Map.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==WarTheatreUI.TerritoryTitle(WarTheatre.Find(id))),"Territory heading rendered: "+id);
                Check(Map.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==WarTheatreUI.TerritorySummary(WarTheatre.Find(id))),"Territory description rendered: "+id);
                Check(Map.GetComponentsInChildren<Button>().Any(b=>b.GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="Alfonso’s court at Naples")),"Alfonso destination rendered: "+id);
            }
            var selectedHeading=Map.GetComponentsInChildren<RealmNameHeading>().First();
            foreach(var mode in new[]{DiplomaticNames.Style.Full,DiplomaticNames.Style.Original,DiplomaticNames.Style.Short}){
                Map.GetComponentsInChildren<Button>().First(b=>b.name=="Realm naming selector").onClick.Invoke();yield return null;
                Check(DiplomaticNames.Mode==mode,"Naming selector reaches "+mode);
                Check(selectedHeading.GetComponent<TMP_Text>().text==DiplomaticNames.Name("sardinia"),"Selected territorial name refreshes: "+mode);
            }
            yield return Shot(output,"selected-sardinia-"+size.x);
            DiplomaticNames.Mode=DiplomaticNames.Style.Original;
            foreach(string id in new[]{"wallachia","moldavia","georgia","karaman","theodoro"}){
                Map.GetComponentsInChildren<TerritoryGraphic>().First(t=>t.name==id).OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});yield return null;Canvas.ForceUpdateCanvases();
                var heading=Map.GetComponentInChildren<RealmNameHeading>().GetComponent<TMP_Text>();
                Check(!heading.isTextOverflowing,"Original name fits: "+id);
                yield return Shot(output,"original-"+id+"-"+size.x);
            }
            DiplomaticNames.Mode=DiplomaticNames.Style.Short;
            foreach(int company in MercenaryContracts.Offers){
                MercenaryUI.Show(company);yield return null;Canvas.ForceUpdateCanvases();var brokers=GameObject.Find("Mercenary brokers");
                Check(brokers.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Port of departure:")),"Explicit mercenary departure port");
                Check(brokers.transform.GetComponentsInChildren<RectTransform>().Any(t=>t.name=="Company description"),"Company description present");
                Check(!brokers.GetComponentsInChildren<Button>().Any(b=>b.name=="Company historical background"),"Retired history control absent");
                foreach(var text in brokers.GetComponentsInChildren<TMP_Text>())Check(!text.isTextOverflowing,"Mercenary text fits: "+text.text);
                if(company==MercenaryContracts.Offers[0])yield return Shot(output,"mercenary-brokers-"+size.x);
                MercenaryUI.Close();yield return null;
            }
            var ui=Map.GetComponent<WarTheatreUI>();
            foreach(string expected in new[]{"Ottoman provinces","Non-Ottoman Powers","Court religion","All territories"}){
                Map.GetComponentsInChildren<Button>().First(b=>b.name=="Map view selector").onClick.Invoke();yield return null;
                Check(Map.GetComponentsInChildren<TMP_Text>().Any(t=>t.text==expected),"View selector reaches "+expected);
                var shapes=Map.GetComponentsInChildren<TerritoryGraphic>();
                if(expected=="Ottoman provinces")Check(!shapes.Any(t=>t.name=="venice")&&shapes.Any(t=>t.name=="thrace"),"Ottoman filter");
                if(expected=="Non-Ottoman Powers")Check(shapes.Any(t=>t.name=="venice")&&!shapes.Any(t=>t.name=="thrace"),"Non-Ottoman filter");
                if(expected=="Court religion"){
                    Check(!Map.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Unclassified")),"No unclassified legend when all courts mapped");
                    foreach(string id in new[]{"bosnia","lithuania","gazaria","cyprus","athens","ionian_tocco"}){
                        var shape=shapes.Single(t=>t.name==id);Check(Mathf.Abs(shape.color.r-.88f)<.001f&&Mathf.Abs(shape.color.g-.64f)<.001f,"Catholic colour rendered "+id);
                    }
                    yield return Shot(output,"court-religion-"+size.x);
                }
            }
            var passage=Map.GetComponentsInChildren<MaritimeMapMedallions>().First(m=>m.Passage);
            Check(!passage.GetComponentsInChildren<TMP_Text>(true).First().gameObject.activeSelf,"Passage caption hidden until hover");
            passage.OnPointerEnter(new PointerEventData(EventSystem.current));Check(passage.GetComponentsInChildren<TMP_Text>().Any(),"Passage caption appears on hover");
            passage.Activate();yield return null;Check(GameObject.Find("Maritime status card")!=null,"Passage opens centred inspection");
            yield return Shot(output,"passage-"+size.x);
            var popup=GameObject.Find("Maritime status");popup.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>().text=="Return").onClick.Invoke();yield return null;
            Map.GetComponentsInChildren<MaritimeMapMedallions>().First(m=>!m.Passage).Activate();yield return null;
            Check(GameObject.Find("Maritime status card")!=null,"Sea control opens centred inspection");
            popup=GameObject.Find("Maritime status");popup.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMP_Text>().text=="Return").onClick.Invoke();yield return null;
            Map.GetComponentsInChildren<TerritoryGraphic>().First(t=>t.name=="ionian_tocco").OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});yield return null;
            yield return Shot(output,"compact-map-"+size.x);
            Click("Diplomacy");yield return null;Check(Map==null,"Diplomacy navigation closes map");WarTheatreUI.Show();yield return null;Click("Return");yield return null;Check(Map==null,"Return closes map");
        }
        DiplomaticNames.Mode=originalNameMode;
        File.WriteAllText(Path.Combine(output,"map-checks.txt"),"COMPACT_MAP_CHECKS "+count+" passed");
    }
    static IEnumerator Shot(string output,string name){
        Canvas.ForceUpdateCanvases();var obj=new GameObject("Map review camera",typeof(Camera));var camera=obj.GetComponent<Camera>();camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=ImperialTheme.Navy;
        var canvases=UnityEngine.Object.FindObjectsOfType<Canvas>().Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);var previous=RenderTexture.active;
        camera.targetTexture=target;foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=10;}Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());
        RenderTexture.active=previous;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}camera.targetTexture=null;RenderTexture.ReleaseTemporary(target);UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(obj);yield return null;
    }
}
#endif
