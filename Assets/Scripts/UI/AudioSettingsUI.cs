using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class AudioSettingsUI
{
    static GameObject panel,overlay;
    public static bool Close(){if(panel==null)return false;if(overlay!=null)Object.Destroy(overlay);else Object.Destroy(panel);panel=null;overlay=null;return true;}
    public static void RefreshDisplay(){if(panel==null)return;foreach(var t in panel.GetComponentsInChildren<TMP_Text>()){if(t.text.StartsWith("Fullscreen"))t.text=Screen.fullScreen?"Fullscreen · selected":"Fullscreen";else if(t.text.StartsWith("Windowed"))t.text=Screen.fullScreen?"Windowed":"Windowed · selected";}}
    static void Heading(string text,float y)
    {
        var label=CampaignHUD.Label(panel.transform,text,new Vector2(.07f,y),new Vector2(.93f,y+.045f),23);label.color=ImperialTheme.Gold;
        var rule=CampaignHUD.Panel(panel.transform,text+" divider",new Vector2(.07f,y-.003f),new Vector2(.93f,y));rule.GetComponent<Image>().color=ImperialTheme.Gold;rule.GetComponent<Image>().raycastTarget=false;
    }
    static void RowLabel(string text,float y)
    {
        var label=CampaignHUD.Label(panel.transform,text,new Vector2(.07f,y),new Vector2(.40f,y+.07f),24);label.alignment=TextAlignmentOptions.MidlineLeft;
    }
    static Button Control(string text,float y,UnityEngine.Events.UnityAction action)
    {
        var button=CampaignHUD.MakeButton(panel.transform,text,new Vector2(.43f,y),new Vector2(.93f,y+.07f),action);ImperialTheme.Button(button);return button;
    }
    public static void Show(Transform parent)
    {
        Close();
        overlay=new GameObject("Settings overlay",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=overlay.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=400;var scaler=overlay.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
        var shade=CampaignHUD.Panel(overlay.transform,"Settings backdrop",Vector2.zero,Vector2.one);shade.GetComponent<Image>().color=new Color(0,.015f,.025f,.82f);
        var settings=Settings.Instance;
        panel=CampaignHUD.Panel(overlay.transform,"Settings",new Vector2(.23f,.08f),new Vector2(.77f,.92f));ImperialTheme.Panel(panel,false);
        var title=CampaignHUD.Label(panel.transform,"Settings",new Vector2(.08f,.91f),new Vector2(.92f,.98f),36);title.alignment=TextAlignmentOptions.Center;

        Heading("DISPLAY",.845f);RowLabel("Screen mode",.76f);
        Button full=null,window=null;
        full=CampaignHUD.MakeButton(panel.transform,"Fullscreen",new Vector2(.43f,.76f),new Vector2(.675f,.83f),()=>{settings.SetFullscreen(true);full.GetComponentInChildren<TMP_Text>().text="Fullscreen · selected";window.GetComponentInChildren<TMP_Text>().text="Windowed";});
        window=CampaignHUD.MakeButton(panel.transform,"Windowed",new Vector2(.69f,.76f),new Vector2(.93f,.83f),()=>{settings.SetFullscreen(false);full.GetComponentInChildren<TMP_Text>().text="Fullscreen";window.GetComponentInChildren<TMP_Text>().text="Windowed · selected";});ImperialTheme.Button(full);ImperialTheme.Button(window);RefreshDisplay();

        Heading("GAMEPLAY",.69f);RowLabel("Assault minigames",.61f);
        var assault=Control(PhasedAssaultUI.ActiveMode?"Active · On":"Assisted · neutral",.61f,()=>{});assault.name="Assault minigames";
        assault.onClick.AddListener(()=>{PhasedAssaultUI.ActiveMode=!PhasedAssaultUI.ActiveMode;assault.GetComponentInChildren<TMP_Text>().text=PhasedAssaultUI.ActiveMode?"Active · On":"Assisted · neutral";});
        RowLabel("Particles",.53f);var particles=Control(SceneParticles.Enabled?"On":"Off",.53f,()=>{});particles.name="Particles toggle";
        particles.onClick.AddListener(()=>{SceneParticles.Enabled=!SceneParticles.Enabled;particles.GetComponentInChildren<TMP_Text>().text=SceneParticles.Enabled?"On":"Off";});

        Heading("AUDIO",.46f);RowLabel("Music",.38f);var music=Control(settings.MusicEnabled?"On":"Off",.38f,()=>{});music.name="Music toggle";
        music.onClick.AddListener(()=>{settings.SetMusicEnabled(!settings.MusicEnabled);music.GetComponentInChildren<TMP_Text>().text=settings.MusicEnabled?"On":"Off";});
        RowLabel("Music volume",.30f);var value=CampaignHUD.Label(panel.transform,Mathf.RoundToInt(settings.MusicVolume*100)+"%",new Vector2(.84f,.30f),new Vector2(.93f,.37f),22);value.alignment=TextAlignmentOptions.MidlineRight;
        var track=CampaignHUD.Panel(panel.transform,"Music volume",new Vector2(.43f,.322f),new Vector2(.82f,.348f));track.GetComponent<Image>().color=ImperialTheme.Surface;
        var fill=CampaignHUD.Panel(track.transform,"Volume fill",Vector2.zero,Vector2.one);fill.GetComponent<Image>().color=new Color(.19f,.40f,.56f);
        var handle=CampaignHUD.Panel(track.transform,"Volume handle",Vector2.zero,Vector2.one);handle.GetComponent<Image>().color=ImperialTheme.Gold;
        var h=handle.GetComponent<RectTransform>();h.sizeDelta=new Vector2(22,10);
        var slider=track.AddComponent<Slider>();slider.fillRect=fill.GetComponent<RectTransform>();slider.handleRect=h;slider.targetGraphic=handle.GetComponent<Image>();slider.minValue=0;slider.maxValue=1;slider.SetValueWithoutNotify(settings.MusicVolume);
        slider.onValueChanged.AddListener(v=>{settings.SetMusicVolume(v);value.text=Mathf.RoundToInt(v*100)+"%";});
        RowLabel("Sound effects",.22f);var effects=Control(settings.SfxEnabled?"On":"Off",.22f,()=>{});effects.name="Sound effects toggle";
        effects.onClick.AddListener(()=>{settings.SetSfxEnabled(!settings.SfxEnabled);effects.GetComponentInChildren<TMP_Text>().text=settings.SfxEnabled?"On":"Off";});
        var help=CampaignHUD.Explanation(panel.transform,"Preferences are saved automatically.",new Vector2(.07f,.145f),new Vector2(.93f,.19f),19);help.alignment=TextAlignmentOptions.Center;
        ImperialTheme.Button(CampaignHUD.MakeButton(panel.transform,"Return",new Vector2(.31f,.04f),new Vector2(.69f,.115f),()=>Close()));
    }
}
