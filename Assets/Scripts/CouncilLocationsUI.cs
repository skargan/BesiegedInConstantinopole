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
        string[] art={"palace-mirror","palace-garden","hagia-sophia"};
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
            LocationHotspot.Create(holder.transform,"The Emperor’s mirror",new[]{new Vector2(.075f,.31f),new Vector2(.24f,.31f),new Vector2(.24f,.65f),new Vector2(.20f,.82f),new Vector2(.14f,.84f),new Vector2(.08f,.68f)},EmperorSheet);
            LocationHotspot.Create(holder.transform,"To the garden",GardenDoor(),Garden);
            LocationHotspot.Create(holder.transform,"Hagia Sophia",PalaceCathedral(),HagiaSophia);
            LocationHotspot.Create(holder.transform,"Imperial affairs",new[]{new Vector2(.30f,.43f),new Vector2(.59f,.46f),new Vector2(.90f,.28f),new Vector2(.66f,.14f),new Vector2(.32f,.31f)},PalaceActions);
        }else if(place==1){
            LocationHotspot.Create(holder.transform,"Return to the Palace",new[]{new Vector2(.015f,.46f),new Vector2(.080f,.48f),new Vector2(.081f,.77f),new Vector2(.043f,.79f),new Vector2(.014f,.77f)},Palace);
            LocationHotspot.Create(holder.transform,"Hagia Sophia",new[]{new Vector2(.665f,.669f),new Vector2(.665f,.70f),new Vector2(.679f,.70f),new Vector2(.684f,.714f),new Vector2(.695f,.72f),new Vector2(.707f,.712f),new Vector2(.712f,.702f),new Vector2(.724f,.696f),new Vector2(.724f,.672f)},HagiaSophia);
        }else{
            LocationButton(holder.transform,"Return to the Palace",.03f,.08f,.27f,.22f,Palace);
            LocationButton(holder.transform,"Walk in the garden",.73f,.08f,.97f,.22f,Garden);
        }
        int visitor=PalaceLocations.Visitor(place);
        if(visitor>=0&&CourtStandingSprites.Has(visitor)){CourtStandingSprites.Draw(holder.transform,visitor,place,()=>{conversationLocation=place;GardenTalk(visitor,0);});return;}
        if(visitor>=0){
            bool hasSprite=CourtStandingSprites.Has(visitor);
            var speak=CampaignHUD.MakeButton(holder.transform,GardenConversations.Name(visitor)+(CourtConversationData.SomethingNew(visitor)?" · ◆":"")+"\nSpeak",new Vector2(.05f,.07f),new Vector2(.33f,.24f),()=>{conversationLocation=place;GardenTalk(visitor,0);});
            var label=speak.GetComponentInChildren<TMPro.TMP_Text>();label.rectTransform.anchorMin=new Vector2(.30f,.04f);label.rectTransform.anchorMax=new Vector2(.97f,.96f);label.enableAutoSizing=true;label.fontSizeMin=17;label.fontSizeMax=25;
            CharacterPortraits.Draw(speak.transform,visitor,new Vector2(.02f,.06f),new Vector2(.28f,.94f));
        }
        if(place==2&&visitor>=0){ // Keep the visitor separate from the return doorway.
            var last=holder.transform.GetChild(holder.transform.childCount-1).GetComponent<RectTransform>();last.anchorMin=new Vector2(.37f,.08f);last.anchorMax=new Vector2(.65f,.24f);
        }
        if(place==0&&visitor>=0){var last=holder.transform.GetChild(holder.transform.childCount-1).GetComponent<RectTransform>();last.anchorMin=new Vector2(.03f,.07f);last.anchorMax=new Vector2(.31f,.24f);}
    }
    private Vector2[] GardenDoor()
    {
        var points=new System.Collections.Generic.List<Vector2>{new Vector2(.844f,.411f),new Vector2(.844f,.765f)};
        for(int i=1;i<=24;i++){float a=Mathf.PI-i*Mathf.PI/24;points.Add(new Vector2(.897f+Mathf.Cos(a)*.053f,.765f+Mathf.Sin(a)*.085f));}
        points.Add(new Vector2(.950f,.393f));return points.ToArray();
    }
    private Vector2[] PalaceCathedral()=>new[]{new Vector2(.366f,.687f),new Vector2(.367f,.724f),new Vector2(.372f,.726f),new Vector2(.375f,.710f),new Vector2(.389f,.717f),new Vector2(.390f,.732f),new Vector2(.399f,.743f),new Vector2(.409f,.747f),new Vector2(.420f,.742f),new Vector2(.427f,.728f),new Vector2(.428f,.718f),new Vector2(.436f,.718f),new Vector2(.436f,.733f),new Vector2(.440f,.735f),new Vector2(.443f,.720f),new Vector2(.447f,.705f),new Vector2(.447f,.677f),new Vector2(.424f,.676f),new Vector2(.416f,.672f),new Vector2(.391f,.674f),new Vector2(.390f,.688f),new Vector2(.378f,.693f)};
    private void LocationButton(Transform parent,string title,float x,float y,float right,float top,UnityEngine.Events.UnityAction action)
    {
        CampaignHUD.MakeButton(parent,title,new Vector2(x,y),new Vector2(right,top),action);
    }
}
