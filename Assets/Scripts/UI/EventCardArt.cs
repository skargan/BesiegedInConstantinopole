using UnityEngine;
using UnityEngine.UI;

public static class EventCardArt
{
    public static void Refresh(Transform parent,string node,string narrative)
    {
        string category="civic";string text=narrative.ToLowerInvariant();
        if(node=="foodShortage")category="civic";
        else if(node.StartsWith("shah") || text.Contains("traitor") || text.Contains("spy") || text.Contains("spies") || text.Contains("envoy") || text.Contains("sultan") || text.Contains("messenger")) category="intrigue";
        else if(text.Contains("cannon") || text.Contains("assault") || text.Contains("fortification") || text.Contains("wall") || text.Contains("soldier"))category="military";
        var frame=parent.Find("Event artwork");
        if(frame==null) {
            var obj=CampaignHUD.Panel(parent,"Event artwork",new Vector2(.025f,.69f),new Vector2(.43f,.965f));frame=obj.transform;
            var picture=new GameObject("Illustration",typeof(RectTransform),typeof(RawImage));picture.transform.SetParent(frame,false);var pr=picture.GetComponent<RectTransform>();pr.anchorMin=Vector2.zero;pr.anchorMax=Vector2.one;pr.offsetMin=pr.offsetMax=Vector2.zero;picture.GetComponent<RawImage>().raycastTarget=false;obj.GetComponent<Image>().raycastTarget=false;
            var border=obj.AddComponent<Outline>();border.effectColor=new Color(.64f,.48f,.24f);border.effectDistance=new Vector2(2,-2);
        }
        var raw=frame.GetComponentInChildren<RawImage>();raw.texture=Resources.Load<Texture2D>("EventArt/"+category+"-r11");
        if(raw.texture==null){frame.gameObject.SetActive(false);return;}
        frame.gameObject.SetActive(true);ImperialTheme.Frame(frame.gameObject);Canvas.ForceUpdateCanvases();
        var rect=frame.GetComponent<RectTransform>().rect;float h=Mathf.Min(1,rect.height/rect.width*raw.texture.width/raw.texture.height);
        float centre=category=="intrigue"?.75f:category=="military"?.60f:.55f;
        raw.uvRect=new Rect(0,Mathf.Clamp(centre-h/2,0,1-h),1,h);
    }
}
