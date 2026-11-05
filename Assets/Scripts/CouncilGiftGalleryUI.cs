using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    static readonly string[] GiftArt={"sweetmeats","silk","alexander","chess","astrolabe","feast","timber","portfolio"};
    static readonly string[] GiftTitles={"Casket of sweetmeats","Bursa silk & silver","Alexander manuscript","Ivory chess set","Jewelled astrolabe","Feast for the household","Pavilion materials","Intelligence portfolio"};
    Transform GiftSurface(string title,System.Action back)
    {
        Open(title,"");foreach(Transform child in screen.transform)child.gameObject.SetActive(false);
        ControlSingleton.Instance.ReturnAction=()=>back();
        var heading=CampaignHUD.Label(screen.transform,title,new Vector2(.035f,.91f),new Vector2(.965f,.985f),32);heading.alignment=TextAlignmentOptions.Center;
        return screen.transform;
    }
    static TMP_Text GiftText(Transform parent,string text,Vector2 min,Vector2 max,int size)
    {
        var label=CampaignHUD.Label(parent,text,min,max,size);label.enableAutoSizing=true;label.fontSizeMin=16;label.fontSizeMax=size;return label;
    }
    static void GiftPainting(Transform parent,int index,Vector2 min,Vector2 max)
    {
        var frame=CampaignHUD.Panel(parent,"Gift artwork "+index,min,max);ImperialTheme.Panel(frame,false);
        var image=new GameObject("Gift painting",typeof(RectTransform),typeof(RawImage),typeof(AspectRatioFitter));image.transform.SetParent(frame.transform,false);
        var rect=(RectTransform)image.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
        var texture=Resources.Load<Texture2D>("UI/Gifts/"+GiftArt[index]);var raw=image.GetComponent<RawImage>();raw.texture=texture;raw.raycastTarget=false;
        var fit=image.GetComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=texture==null?1:(float)texture.width/texture.height;
    }
    void ShowGiftGallery()
    {
        var surface=GiftSurface("Gifts for Mehmed",Sultan);
        for(int i=0;i<SultanGifts.Gifts.Length;i++)
        {
            int choice=i;var gift=SultanGifts.Gifts[i];float x=.025f+(i%4)*.24f,y=.89f-(i/4)*.40f;
            var card=CampaignHUD.Panel(surface,"Gift card "+i,new Vector2(x,y-.38f),new Vector2(x+.23f,y));ImperialTheme.Panel(card,false);
            GiftPainting(card.transform,i,new Vector2(.025f,.32f),new Vector2(.975f,.975f));
            var title=GiftText(card.transform,GiftTitles[i],new Vector2(.025f,.20f),new Vector2(.975f,.32f),23);title.alignment=TextAlignmentOptions.Center;
            var cost=GiftText(card.transform,gift.Cost+"\nFury −"+gift.Fury,new Vector2(.025f,.015f),new Vector2(.975f,.20f),20);cost.alignment=TextAlignmentOptions.Center;
            var select=card.AddComponent<UnityEngine.UI.Button>();select.targetGraphic=card.GetComponent<UnityEngine.UI.Image>();ImperialTheme.Button(select);select.onClick.AddListener(()=>ReviewSultanGift(choice));
            // The whole card is a review target. Affordability is enforced at confirmation.
        }
        CampaignHUD.MakeButton(surface,"Return",new Vector2(.79f,.015f),new Vector2(.965f,.075f),()=>ControlSingleton.Instance.GoBack());
    }
    void ShowGiftReview(int index)
    {
        var gift=SultanGifts.Gifts[index];var surface=GiftSurface("Gifts for Mehmed",SultanGiftMenu);
        GiftPainting(surface,index,new Vector2(.045f,.13f),new Vector2(.525f,.87f));
        var title=GiftText(surface,gift.Name,new Vector2(.57f,.72f),new Vector2(.95f,.86f),30);
        var description=GiftText(surface,gift.Description,new Vector2(.57f,.47f),new Vector2(.95f,.70f),25);description.fontStyle=FontStyles.Italic;
        GiftText(surface,gift.Cost+"\nSultan's fury −"+gift.Fury+" points",new Vector2(.57f,.32f),new Vector2(.95f,.46f),25);
        string unavailable=SultanGifts.GiftUnavailable(index);
        if(unavailable!=null)GiftText(surface,unavailable,new Vector2(.57f,.25f),new Vector2(.95f,.32f),20);
        var confirm=CampaignHUD.MakeTurnButton(surface,"Confirm — send gift",new Vector2(.57f,.15f),new Vector2(.95f,.24f),()=>{bool ok=SultanGifts.Send(index,out var report);Finish(ok,report);});confirm.interactable=unavailable==null;
        CampaignHUD.MakeButton(surface,"Choose another gift",new Vector2(.66f,.025f),new Vector2(.95f,.10f),SultanGiftMenu);
    }
}
