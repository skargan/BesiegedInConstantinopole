using UnityEngine;
using UnityEngine.UI;
using TMPro;
public static class CourtStandingSprites
{
    public static bool Has(int person)=>Resources.Load<Texture2D>("UI/Identity/Standing/"+person)!=null;
    public static void Draw(Transform painting,int person,int place,UnityEngine.Events.UnityAction talk)
    {
        var texture=Resources.Load<Texture2D>("UI/Identity/Standing/"+person);if(texture==null)return;
        Vector2 min=place==0?new Vector2(.29f,.025f):place==1?new Vector2(.44f,.02f):new Vector2(.40f,.05f);
        Vector2 max=place==0?new Vector2(.47f,.57f):place==1?new Vector2(.62f,.61f):new Vector2(.58f,.64f);
        var button=CampaignHUD.MakeButton(painting,"",min,max,talk);button.name="Speak to "+GardenConversations.Name(person);button.GetComponent<Image>().color=Color.clear;
        var border=button.transform.Find("Imperial goldwork");if(border!=null)border.gameObject.SetActive(false);
        var figure=new GameObject("Standing figure",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));figure.transform.SetParent(button.transform,false);
        var fit=figure.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=(float)texture.width/texture.height;
        var image=figure.GetComponent<RawImage>();image.texture=texture;image.raycastTarget=false;
        var label=CampaignHUD.Panel(button.transform,"Name",new Vector2(-.15f,-.035f),new Vector2(1.15f,.07f));ImperialTheme.Panel(label);label.GetComponent<Image>().raycastTarget=false;
        var name=CampaignHUD.Label(label.transform,GardenConversations.Name(person),new Vector2(.02f,.05f),new Vector2(.98f,.95f),19);name.enableAutoSizing=true;name.fontSizeMin=13;name.fontSizeMax=19;name.alignment=TextAlignmentOptions.Center;
        if(CourtConversationData.SomethingNew(person)){
            var bubble=CampaignHUD.Panel(button.transform,"New conversation",new Vector2(.65f,.78f),new Vector2(.99f,.94f));ImperialTheme.Panel(bubble);bubble.GetComponent<Image>().raycastTarget=false;
            var mark=CampaignHUD.Label(bubble.transform,"···",Vector2.zero,Vector2.one,30);mark.alignment=TextAlignmentOptions.Center;mark.color=EmperorAttributeBadge.Colour(0);
        }
    }
}
