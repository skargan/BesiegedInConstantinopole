using UnityEngine;

using UnityEngine.UI;

using UnityEngine.SceneManagement;

using TMPro;



// Shared campaign status and modal screens. The existing phase components own the turn flow.

public class CampaignHUD : MonoBehaviour

{

    public static CampaignHUD Instance { get; private set; }

    private Canvas canvas;

    private TMP_Text status;



    private Button back;

    public bool HasModal => modal != null;

    private GameObject modal;

    private GameObject pausedReport;

    private bool ended;
    public bool HasEnded => ended;



    private void Awake()

    {

        Instance = this;

        var root = new GameObject("Campaign HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        root.transform.SetParent(transform, false);

        canvas = root.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder = 100;

        var scaler = root.GetComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution = new Vector2(1600, 900);

        scaler.matchWidthOrHeight = 0.5f;

        var bar = Panel(root.transform, "Campaign Status", new Vector2(0, .88f), Vector2.one);

        ImperialTheme.Panel(bar);

        status = Label(bar.transform, "", new Vector2(.889f,.33f), new Vector2(.988f,.56f), 19);
        status.alignment=TextAlignmentOptions.Center;status.enableAutoSizing=true;status.fontSizeMin=15;status.fontSizeMax=19;status.enableWordWrapping=false;
        CampaignStatusStrip.Create(bar.transform);
        back=MakeButton(bar.transform,"Return",new Vector2(.889f,.065f),new Vector2(.988f,.30f),()=>ControlSingleton.Instance.GoBack());
        var menu=MakeButton(bar.transform,"Campaign menu",new Vector2(.889f,.60f),new Vector2(.988f,.95f),TogglePause);
        foreach(var control in new[]{back,menu}) {
            ImperialTheme.Button(control);
            var label=control.GetComponentInChildren<TMP_Text>();label.fontSizeMin=14;label.fontSizeMax=19;label.margin=new Vector4(6,0,6,0);
        }

    }



    private void Start() { CheckEnding(); }

    private void Update()

    {

        if (!VariableSingleton.IsInitialized) return;

        back.gameObject.SetActive(ControlSingleton.Instance != null && ControlSingleton.Instance.CanGoBack && !HasModal);

        status.text = VariableSingleton.GetDate().ToString("d MMM yyyy");


    }



    public void CheckEnding()

    {

        if (ended || !VariableSingleton.IsInitialized) return;

        string reason = null;

        if(MehmedAssassination.Victory) reason="Mehmed is dead. The Ottoman commanders abandon the siege. Constantinople endures.";
        else if(CouncilActions.Int("iSurrenderEnding")>0) reason=SurrenderTerms.EndingText;
        else if (VariableSingleton.Instance.EnduringWall.CityLost) reason = "The last wall has fallen. Constantinople is lost.";

        else if (VariableSingleton.GetFloatVariable("fHealth") <= 0) reason = DailyLifeEvents.I("iCombatAssassinationFatal")==1 ? "Assassinated in the palace. The Emperor dies of wounds sustained fighting his attacker." : DailyLifeEvents.I("iLifePoisonEnding")==1 ? "Assassinated by poison. The Emperor dies after eating a poisoned loaf." : "The Emperor has died.";

        else if (VariableSingleton.GetFloatVariable("fTroopRep") <= 0) reason = "Your army has lost the will to defend the city.";

        else if (VariableSingleton.GetFloatVariable("fPeopleRep") <= 0) reason = "The people have lost faith in your rule.";

        else if (VariableSingleton.GetFloatVariable("fNobleRep") <= 0) reason = "The nobility has withdrawn its support.";

        else if (VariableSingleton.GetFloatVariable("fPriestRep") <= 0) reason = "The Church has turned against your rule.";

        else if (VariableSingleton.GetIntVariable("iBalkans") + VariableSingleton.GetIntVariable("iBazouks") +

            VariableSingleton.GetIntVariable("iJanissaries") == 0) reason = "The Ottoman field army has been destroyed. The siege is broken.";

        else if (VariableSingleton.CompletedSiegeDays >= 240) reason = "After 240 days, the Ottoman siege collapses. Constantinople endures.";

        if(!MehmedAssassination.Victory && VariableSingleton.GetIntVariable("iDeposed")>0)reason="Giovanni has seized the imperial throne. Your reign ends; the defence continues under another emperor.";
        if (reason == null) return;

        if(MehmedAssassination.Victory || reason=="The Ottoman field army has been destroyed. The siege is broken." || reason=="After 240 days, the Ottoman siege collapses. Constantinople endures.") { PreparationCampaign.HonourVictory(); Achievements.Unlock(0); Achievements.Victory(); }
        bool ionianVictory=MehmedAssassination.Victory || reason=="The Ottoman field army has been destroyed. The siege is broken." || reason=="After 240 days, the Ottoman siege collapses. Constantinople endures.";
        reason+=IonianDiplomacy.EndingReflection(ionianVictory);
        Achievements.CampaignEnded();
        SaveGames.End("Campaign ended",reason+"\n\nSiege days survived: "+VariableSingleton.CompletedSiegeDays);
        ended = true;

        ShowModal("Campaign ended", reason + "\n\nSiege days survived: " + VariableSingleton.CompletedSiegeDays +

            "\nYour next attempt begins with what you learned.", false);

    }



    public void TogglePause()

    {

        if (ended) return;

        if (modal != null && modal.name == "Campaign Modal")

        {

            Destroy(modal); modal = pausedReport; pausedReport = null;

            if (modal != null) modal.SetActive(true);

            return;

        }

        if (modal != null) { pausedReport = modal; pausedReport.SetActive(false); modal = null; }

        ShowPauseMenu();

    }



    public void DismissReport()

    {

        if(modal!=null && !ended){Destroy(modal);modal=null;}

    }

    public void ShowReport(string title,string body,UnityEngine.Events.UnityAction next)

    {

        if(ended) return;

        if(modal!=null) Destroy(modal);

        modal=Panel(canvas.transform,"Event outcome",Vector2.zero,Vector2.one);

        Label(modal.transform,title,new Vector2(.10f,.83f),new Vector2(.90f,.94f),36);

        var viewport=Panel(modal.transform,"Outcome scroll",new Vector2(.10f,.21f),new Vector2(.88f,.81f));

        viewport.AddComponent<RectMask2D>();

        var text=Label(viewport.transform,body,Vector2.zero,Vector2.one,25);

        var scroll=viewport.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.scrollSensitivity=35;

        scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=text.rectTransform;

        var r=text.rectTransform;r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);

        Canvas.ForceUpdateCanvases();

        r.sizeDelta=new Vector2(0,Mathf.Max(viewport.GetComponent<RectTransform>().rect.height,text.GetPreferredValues(body,viewport.GetComponent<RectTransform>().rect.width,9999).y+20));

        r.anchoredPosition=Vector2.zero;

        AddScrollbar(scroll,modal.transform,new Vector2(.89f,.21f),new Vector2(.90f,.81f));

        MakeButton(modal.transform,"Continue",new Vector2(.34f,.07f),new Vector2(.66f,.16f),next);

    }



    public void MarkReportAdvance(string hint)
    {
        if(modal==null)return;
        var button=modal.GetComponentInChildren<Button>();if(button!=null)TurnActionButton.Mark(button,hint);
    }

    private void ShowPauseMenu()
    {
        // Keep the report/ending layout independent of the compact pause menu.
        modal=Panel(canvas.transform,"Campaign pause backdrop",Vector2.zero,Vector2.one);
        modal.name="Campaign Modal";
        modal.GetComponent<Image>().color=new Color(0,.015f,.025f,.82f);
        var card=Panel(modal.transform,"Campaign menu card",new Vector2(.31f,.18f),new Vector2(.69f,.82f));ImperialTheme.Panel(card,false);
        var title=Label(card.transform,"Campaign menu",new Vector2(.10f,.83f),new Vector2(.90f,.95f),34);title.alignment=TextAlignmentOptions.Center;
        var version=Label(card.transform,CouncilUI.BuildLabel,new Vector2(.06f,.025f),new Vector2(.94f,.085f),16);version.alignment=TextAlignmentOptions.Center;version.enableAutoSizing=true;version.fontSizeMin=12;version.fontSizeMax=16;
        var mode=Explanation(card.transform,SaveGames.Ironman?"Ironman · achievements enabled":"Normal campaign · achievements disabled",new Vector2(.08f,.73f),new Vector2(.92f,.83f),21);mode.alignment=TextAlignmentOptions.Center;
        ImperialTheme.Button(MakeButton(card.transform,"Resume",new Vector2(.12f,.59f),new Vector2(.88f,.69f),TogglePause));
        ImperialTheme.Button(MakeButton(card.transform,"Save / load",new Vector2(.12f,.47f),new Vector2(.88f,.57f),()=>SaveMenu.Show(canvas.transform)));
        ImperialTheme.Button(MakeButton(card.transform,"Settings",new Vector2(.12f,.35f),new Vector2(.88f,.45f),()=>AudioSettingsUI.Show(canvas.transform)));
        ImperialTheme.Button(MakeButton(card.transform,"Development Options",new Vector2(.12f,.23f),new Vector2(.88f,.33f),()=>{TogglePause();AssaultPractice.Show(FindObjectOfType<ManagerUI>(true));}));
        ImperialTheme.Button(MakeButton(card.transform,"Return to main menu",new Vector2(.12f,.11f),new Vector2(.88f,.21f),()=>{if(SaveGames.CanCheckpointCouncil)VariableSingleton.Instance.Save();SceneManager.LoadScene("Menu");}));
    }

    private void ShowModal(string title, string body, bool resume)

    {

        if (modal != null) Destroy(modal);

        modal = Panel(canvas.transform, "Campaign Modal", Vector2.zero, Vector2.one);

        string artKey=resume?null:EndingArtwork.Current;
        bool illustrated=!string.IsNullOrEmpty(artKey);
        if(illustrated){
            var frame=Panel(modal.transform,"Ending illustration",new Vector2(.035f,.07f),new Vector2(.50f,.91f));
            var image=new GameObject("Cause artwork",typeof(RectTransform),typeof(RawImage));image.transform.SetParent(frame.transform,false);
            var rect=(RectTransform)image.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(10,10);rect.offsetMax=new Vector2(-10,-10);
            var raw=image.GetComponent<RawImage>();raw.texture=Resources.Load<Texture2D>("UI/Endings/"+artKey);raw.raycastTarget=false;
            var fit=image.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=(float)raw.texture.width/raw.texture.height;
        }
        Label(modal.transform, title, new Vector2(illustrated?.54f:.12f,.83f), new Vector2(.94f,.96f), 42);

        var viewport=Panel(modal.transform,"Campaign outcome scroll",new Vector2(illustrated?.54f:.12f,.42f),new Vector2(illustrated?.93f:.86f,.82f));
        viewport.AddComponent<RectMask2D>();
        var outcome=Label(viewport.transform,body,Vector2.zero,Vector2.one,25);
        var scroll=viewport.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.scrollSensitivity=40;scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=outcome.rectTransform;
        var r=outcome.rectTransform;r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);
        Canvas.ForceUpdateCanvases();r.sizeDelta=new Vector2(0,Mathf.Max(viewport.GetComponent<RectTransform>().rect.height,outcome.GetPreferredValues(body,viewport.GetComponent<RectTransform>().rect.width,9999).y+20));r.anchoredPosition=Vector2.zero;
        AddScrollbar(scroll,modal.transform,new Vector2(illustrated?.94f:.87f,.42f),new Vector2(illustrated?.95f:.88f,.82f));

        if (resume) MakeButton(modal.transform, "Resume", new Vector2(.3f,.3f), new Vector2(.7f,.39f), TogglePause);

        else MakeButton(modal.transform,"Start a new campaign",new Vector2(illustrated?.55f:.3f,.3f),new Vector2(illustrated?.94f:.7f,.39f),()=>SaveMenu.NewCampaign(canvas.transform));

        if(resume){
            MakeButton(modal.transform,"Save / load",new Vector2(.08f,.04f),new Vector2(.48f,.13f),()=>SaveMenu.Show(canvas.transform));
            MakeButton(modal.transform,"Settings",new Vector2(.52f,.04f),new Vector2(.92f,.13f),()=>AudioSettingsUI.Show(canvas.transform));
        }
        MakeButton(modal.transform, "Return to main menu", new Vector2(illustrated?.55f:.3f,.17f), new Vector2(illustrated?.94f:.7f,.26f), () => {if(SaveGames.CanCheckpointCouncil)VariableSingleton.Instance.Save();SceneManager.LoadScene("Menu");});

    }



    public static void AddScrollbar(ScrollRect scroll,Transform parent,Vector2 min,Vector2 max)

    {

        var track=Panel(parent,"Scroll track",min,max);

        track.GetComponent<Image>().color=new Color(.08f,.10f,.12f);

        var handle=Panel(track.transform,"Scroll handle",Vector2.zero,Vector2.one);

        handle.GetComponent<Image>().color=new Color(.66f,.54f,.32f);

        var bar=track.AddComponent<Scrollbar>();bar.handleRect=handle.GetComponent<RectTransform>();

        bar.targetGraphic=handle.GetComponent<Image>();bar.direction=Scrollbar.Direction.BottomToTop;

        scroll.verticalScrollbar=bar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;

    }



    public static GameObject Panel(Transform parent, string name, Vector2 min, Vector2 max)

    {

        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));

        obj.transform.SetParent(parent, false);

        var rect = obj.GetComponent<RectTransform>();

        rect.anchorMin = min; rect.anchorMax = max;

        rect.offsetMin = rect.offsetMax = Vector2.zero;

        obj.GetComponent<Image>().color = ImperialTheme.Navy;
        if(name=="Event outcome" || name=="Campaign Modal" || name=="Achievements") ImperialTheme.Panel(obj);

        return obj;

    }

    // Secondary prose has a consistent role, independent of the chosen font family.
    public static TMP_Text Explanation(Transform parent, string text, Vector2 min, Vector2 max, float size)
    {
        var label = Label(parent, text, min, max, size);
        label.fontStyle |= FontStyles.Italic;
        return label;
    }

    public static TMP_Text Label(Transform parent, string text, Vector2 min, Vector2 max, float size)

    {

        var obj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));

        obj.transform.SetParent(parent, false);

        var rect = obj.GetComponent<RectTransform>();

        rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;

        var label = obj.GetComponent<TextMeshProUGUI>();

        label.font = ImperialTheme.Font;

        label.text = text; label.fontSize = size; label.color = new Color(.94f,.88f,.73f);

        label.enableWordWrapping = true; label.raycastTarget = false;

        return label;

    }

    public static Button MakeTurnButton(Transform parent,string text,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action)
    {
        var button=MakeButton(parent,text,min,max,action);TurnActionButton.Mark(button);return button;
    }

    public static Button MakeButton(Transform parent, string text, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)

    {

        var obj = Panel(parent, text, min, max);

        obj.GetComponent<Image>().color = new Color(.30f,.14f,.21f,1);

        var button = obj.AddComponent<Button>();

        var colours=button.colors;

        colours.highlightedColor=new Color(1f,.89f,.64f);colours.pressedColor=new Color(.72f,.59f,.38f);

        colours.selectedColor=new Color(1f,.89f,.64f);colours.disabledColor=new Color(.45f,.45f,.45f);

        button.colors=colours;

        var border=obj.AddComponent<Outline>();border.effectColor=new Color(.65f,.52f,.29f,.6f);border.effectDistance=new Vector2(1,-1);

        var label = Label(obj.transform, text, new Vector2(.03f,.03f), new Vector2(.97f,.97f), 25);

        label.alignment = TextAlignmentOptions.Center;

        label.enableAutoSizing = true; label.fontSizeMin = 18; label.fontSizeMax = 25;

        ButtonAudio.Attach(button);
        button.onClick.AddListener(action);

        return button;

    }

}

