using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class AudioSettingsUI
{
    static GameObject panel;
    public static void Close(){if(panel!=null)Object.Destroy(panel);panel=null;}
    public static void Show(Transform parent)
    {
        if(panel!=null) Object.Destroy(panel);
        var settings=Settings.Instance;
        panel=CampaignHUD.Panel(parent,"Settings",Vector2.zero,Vector2.one);
        ImperialTheme.Panel(panel);
        var title=CampaignHUD.Label(panel.transform,"Settings",new Vector2(.12f,.80f),new Vector2(.88f,.94f),40);title.alignment=TextAlignmentOptions.Center;
        var toggle=CampaignHUD.MakeButton(panel.transform,settings.MusicEnabled?"Music: On":"Music: Off",new Vector2(.30f,.61f),new Vector2(.70f,.72f),()=>{});
        toggle.onClick.AddListener(()=>{settings.SetMusicEnabled(!settings.MusicEnabled);toggle.GetComponentInChildren<TMP_Text>().text=settings.MusicEnabled?"Music: On":"Music: Off";});
        var value=CampaignHUD.Label(panel.transform,"Music volume: "+Mathf.RoundToInt(settings.MusicVolume*100)+"%",new Vector2(.25f,.46f),new Vector2(.75f,.56f),28);value.alignment=TextAlignmentOptions.Center;
        var track=CampaignHUD.Panel(panel.transform,"Music volume",new Vector2(.25f,.36f),new Vector2(.75f,.42f));track.GetComponent<Image>().color=ImperialTheme.Surface;
        var fill=CampaignHUD.Panel(track.transform,"Volume fill",Vector2.zero,Vector2.one);fill.GetComponent<Image>().color=new Color(.19f,.40f,.56f);
        var handle=CampaignHUD.Panel(track.transform,"Volume handle",Vector2.zero,Vector2.one);handle.GetComponent<Image>().color=new Color(.80f,.65f,.35f);
        var h=handle.GetComponent<RectTransform>();h.sizeDelta=new Vector2(22,8);
        var slider=track.AddComponent<Slider>();slider.fillRect=fill.GetComponent<RectTransform>();slider.handleRect=h;slider.targetGraphic=handle.GetComponent<Image>();slider.minValue=0;slider.maxValue=1;slider.SetValueWithoutNotify(settings.MusicVolume);
        slider.onValueChanged.AddListener(v=>{settings.SetMusicVolume(v);value.text="Music volume: "+Mathf.RoundToInt(v*100)+"%";});
        var effects=CampaignHUD.MakeButton(panel.transform,settings.SfxEnabled?"Sound effects: On":"Sound effects: Off",new Vector2(.30f,.24f),new Vector2(.70f,.33f),()=>{});
        effects.onClick.AddListener(()=>{settings.SetSfxEnabled(!settings.SfxEnabled);effects.GetComponentInChildren<TMP_Text>().text=settings.SfxEnabled?"Sound effects: On":"Sound effects: Off";});
        var help=CampaignHUD.Explanation(panel.transform,"Music and effects are independent. Both settings are saved.",new Vector2(.15f,.205f),new Vector2(.85f,.24f),18);help.alignment=TextAlignmentOptions.Center;
        CampaignHUD.MakeButton(panel.transform,"Return",new Vector2(.35f,.10f),new Vector2(.65f,.20f),Close);
    }
}
