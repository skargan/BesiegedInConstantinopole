using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CityMapView:MonoBehaviour
{
    public static readonly string[] WallKeys={"fOuterWall","fMiddleWall","fInnerWall"};
    public static readonly string[] WallNames={"Theodosian Wall","Constantinian Wall","Severan Wall"};
    private ManagerUI manager;
    private GameObject returnTo;
    private GameObject detail;
    private bool closing;
    private static Sprite painting;
    public bool InCampaign=>manager!=null;
    public static CityMapView Show(Transform canvas,ManagerUI campaign=null,GameObject origin=null)
    {
        foreach(var old in FindObjectsOfType<CityMapView>(true))Destroy(old.gameObject);
        var root=CampaignHUD.Panel(canvas,"Map of the City",Vector2.zero,Vector2.one);ImperialTheme.Panel(root);
        var view=root.AddComponent<CityMapView>();view.manager=campaign;view.returnTo=origin;
        if(campaign!=null){var bounds=root.GetComponent<RectTransform>();bounds.anchorMax=new Vector2(1,.87f);}
        CampaignHUD.Label(root.transform,"MAP OF THE CITY",new Vector2(.035f,.92f),new Vector2(.78f,.99f),32);
        CampaignHUD.Explanation(root.transform,campaign==null?"Explore the City's defensive lines. Enter a campaign to inspect their condition and order repairs.":"Hover over a wall to reveal its outline. Select it to inspect its condition and repair orders.",new Vector2(.035f,.865f),new Vector2(.965f,.925f),21);
        var region=CampaignHUD.Panel(root.transform,"City map region",new Vector2(.025f,.10f),new Vector2(.975f,.855f));region.GetComponent<Image>().color=Color.clear;
        var holder=CampaignHUD.Panel(region.transform,"City map painting",Vector2.zero,Vector2.one);
        var image=holder.GetComponent<Image>();var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Locations/city-map");
        if(texture!=null){if(painting==null)painting=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));image.sprite=painting;image.color=Color.white;image.preserveAspect=true;var fit=holder.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=(float)texture.width/texture.height;}
        // Coordinates follow the supplied painting. Both western ramparts belong to the same defensive line.
        view.AddWall(holder.transform,0,Strip(new[]{P(242,890),P(213,866),P(215,830),P(237,790),P(254,750),P(273,710),P(291,670),P(310,630),P(327,590),P(347,550),P(365,510),P(382,470),P(400,430),P(417,390),P(434,350),P(451,310),P(475,270),P(494,230),P(504,192),P(487,152),P(488,134)},8));
        view.AddWall(holder.transform,0,Strip(new[]{P(130,862),P(138,830),P(154,790),P(169,750),P(184,710),P(207,670),P(222,630),P(244,590),P(258,550),P(281,510),P(295,470),P(322,430),P(345,390),P(363,350),P(371,310),P(405,270),P(427,230),P(448,190),P(475,150)},7));
        view.AddWall(holder.transform,1,Strip(new[]{P(850,260),P(840,291),P(822,323),P(805,351),P(790,379),P(807,417),P(827,456),P(849,496),P(872,538),P(897,585),P(925,632),P(953,677),P(982,717),P(1004,748)},9));
        view.AddWall(holder.transform,2,Strip(new[]{P(1363,407),P(1346,463),P(1325,523),P(1294,585),P(1269,642),P(1259,698),P(1280,748)},9));
        CampaignHUD.MakeButton(root.transform,"Return",new Vector2(.79f,.02f),new Vector2(.965f,.085f),view.Close);
        view.WireReturn();return view;
    }
    private static Vector2 P(float x,float y)=>new Vector2(x/1672f,1-y/941f);
    private static Vector2[] Strip(Vector2[] line,float pixelWidth)
    {
        var a=new List<Vector2>();var b=new List<Vector2>();
        for(int i=0;i<line.Length;i++){
            Vector2 previous=line[Mathf.Max(0,i-1)],next=line[Mathf.Min(line.Length-1,i+1)];
            var tangent=new Vector2((next.x-previous.x)*1672,(next.y-previous.y)*941).normalized;
            var normal=new Vector2(-tangent.y*pixelWidth/1672,tangent.x*pixelWidth/941);
            a.Add(line[i]+normal);b.Add(line[i]-normal);
        }
        b.Reverse();a.AddRange(b);return a.ToArray();
    }
    private void AddWall(Transform holder,int index,Vector2[] shape)
    {
        var target=LocationHotspot.Create(holder,WallNames[index],shape,()=>Inspect(index));
        target.gameObject.AddComponent<CityMapWall>().Index=index;
    }
    private void WireReturn()
    {
        var controls=ControlSingleton.Instance;
        if(controls==null)return;
        controls.ClearHistory();controls.LogToEsc(returnTo==null?gameObject:returnTo,gameObject);controls.ReturnAction=Close;
    }
    public void Inspect(int index)
    {
        if(index<0||index>=WallKeys.Length)return;
        if(manager!=null){gameObject.SetActive(false);manager.InspectWallFromMap(WallKeys[index],()=>{if(this!=null){gameObject.SetActive(true);WireReturn();}});return;}
        if(detail!=null)Destroy(detail);
        detail=CampaignHUD.Panel(transform,"Wall information",new Vector2(.25f,.28f),new Vector2(.75f,.72f));ImperialTheme.Panel(detail);
        CampaignHUD.Label(detail.transform,WallNames[index],new Vector2(.06f,.74f),new Vector2(.94f,.95f),32);
        string[] descriptions={"The outer defensive line. The City's first stand is made here.","The second defensive line. The defenders can withdraw here if the outer line is abandoned.","The final defensive line. Losing this position ends the defence of the City."};
        CampaignHUD.Explanation(detail.transform,descriptions[index]+"\n\nEnter or continue a campaign to see current integrity, repair costs and available orders.",new Vector2(.07f,.25f),new Vector2(.93f,.72f),24);
        CampaignHUD.MakeButton(detail.transform,"Back to the map",new Vector2(.20f,.06f),new Vector2(.80f,.20f),()=>{Destroy(detail);detail=null;});
        if(ControlSingleton.Instance!=null)ControlSingleton.Instance.ReturnAction=()=>{if(detail!=null){Destroy(detail);detail=null;WireReturn();}else Close();};
    }
    public void Close()
    {
        if(closing)return;closing=true;if(ControlSingleton.Instance!=null)ControlSingleton.Instance.ClearHistory();Destroy(gameObject);
    }
}
public class CityMapWall:MonoBehaviour {public int Index;}

