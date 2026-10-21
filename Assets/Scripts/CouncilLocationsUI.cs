using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    private int conversationLocation=1;
    private static readonly Sprite[] locationPaintings=new Sprite[3];
    public void Palace()=>Location(0);
    public void HagiaSophia()=>Location(2);
    private void Location(int place)
    {
        string[] titles={"The Palace","The palace garden","Hagia Sophia"};
        string[] art={"palace-interior","palace-garden","hagia-sophia"};
        string subtitle=place==2?PalaceLocations.VisitSanctuary():place==0?"Attend to your affairs, or step outside to meet your court.":"A little distance from the council table. Conversation costs no major action.";
        Open(titles[place],subtitle);
        body.parent.gameObject.SetActive(false);
        // Fit the entire painting; hotspots use its own coordinates at every window aspect ratio.
        var region=CampaignHUD.Panel(screen.transform,"Location region",new Vector2(.025f,.115f),new Vector2(.975f,.80f));region.GetComponent<Image>().color=Color.clear;
        var holder=CampaignHUD.Panel(region.transform,"Location painting",Vector2.zero,Vector2.one);
        var picture=holder.GetComponent<Image>();var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Locations/"+art[place]);
        if(texture!=null){if(locationPaintings[place]==null)locationPaintings[place]=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));picture.sprite=locationPaintings[place];picture.color=Color.white;var fit=holder.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=(float)texture.width/texture.height;}
        picture.preserveAspect=true;
        if(place==0){
            LocationButton(holder.transform,"To the garden",.82f,.42f,.98f,.56f,Garden);
            LocationButton(holder.transform,"Hagia Sophia",.33f,.58f,.49f,.70f,HagiaSophia);
            LocationButton(holder.transform,"Imperial affairs",.39f,.10f,.64f,.24f,PalaceActions);
        }else if(place==1){
            LocationButton(holder.transform,"Return to the Palace",.02f,.45f,.24f,.59f,Palace);
            LocationButton(holder.transform,"Hagia Sophia",.62f,.55f,.80f,.67f,HagiaSophia);
        }else{
            LocationButton(holder.transform,"Return to the Palace",.03f,.08f,.27f,.22f,Palace);
            LocationButton(holder.transform,"Walk in the garden",.73f,.08f,.97f,.22f,Garden);
        }
        int visitor=PalaceLocations.Visitor(place);
        if(visitor>=0)LocationButton(holder.transform,GardenConversations.Name(visitor)+"\nSpeak",.05f,.07f,.33f,.24f,()=>{conversationLocation=place;GardenTalk(visitor,0);});
        if(place==2&&visitor>=0){ // Keep the visitor separate from the return doorway.
            var last=holder.transform.GetChild(holder.transform.childCount-1).GetComponent<RectTransform>();last.anchorMin=new Vector2(.37f,.08f);last.anchorMax=new Vector2(.65f,.24f);
        }
        if(place==0&&visitor>=0){var last=holder.transform.GetChild(holder.transform.childCount-1).GetComponent<RectTransform>();last.anchorMin=new Vector2(.03f,.07f);last.anchorMax=new Vector2(.31f,.24f);}
    }
    private void LocationButton(Transform parent,string title,float x,float y,float right,float top,UnityEngine.Events.UnityAction action)
    {
        CampaignHUD.MakeButton(parent,title,new Vector2(x,y),new Vector2(right,top),action);
    }
}
