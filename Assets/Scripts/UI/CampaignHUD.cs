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

    private TMP_Text[] resources = new TMP_Text[9];

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

        status = Label(bar.transform, "", new Vector2(.02f,.67f), new Vector2(.72f,.96f), 20);

        for(int i=0;i<9;i++)

        {

            float x=.02f+i*.085f;

            var card=Panel(bar.transform,"Resource",new Vector2(x,.08f),new Vector2(x+.08f,.63f));

            card.GetComponent<Image>().color=ImperialTheme.Surface;ImperialTheme.Frame(card);

            resources[i]=Label(card.transform,"",new Vector2(.08f,.03f),new Vector2(.98f,.97f),24);

        }

        back=MakeButton(bar.transform,"Return",new Vector2(.81f,.12f),new Vector2(.98f,.48f),()=>ControlSingleton.Instance.GoBack());

        MakeButton(bar.transform,"Campaign menu",new Vector2(.81f,.55f),new Vector2(.98f,.91f),TogglePause);

    }



    private void Start() { CheckEnding(); }

    private void Update()

    {

        if (!VariableSingleton.IsInitialized) return;

        back.gameObject.SetActive(ControlSingleton.Instance != null && ControlSingleton.Instance.CanGoBack && !HasModal);

        status.text = "CONSTANTINOPLE   /   " + VariableSingleton.GetDate().ToString("d MMM yyyy") +

            "   /   " + CouncilUI.BuildLabel + "   /   " + (VariableSingleton.HasSiegeStarted() ? "Siege day " + VariableSingleton.SiegeDay : "Preparations");

        string[] names={"GOLD","INFANTRY","ARCHERS","POPULATION","HEALTH","ARMY MORALE","MATERIALS","PROVISIONS","SPIES"};

        string[] values={VariableSingleton.GetIntVariable("iMoney").ToString("N0"),VariableSingleton.GetIntVariable("iInfantry").ToString("N0"),VariableSingleton.GetIntVariable("iArchers").ToString("N0"),PopulationAccounting.People.ToString("N0"),VariableSingleton.GetPercentFloat("fHealth"),VariableSingleton.GetPercentFloat("fTroopRep"),VariableSingleton.GetIntVariable("iMaterials").ToString("N0"),VariableSingleton.GetIntVariable("iProvisions").ToString("N0"),VariableSingleton.GetIntVariable("iSpies").ToString("N0")};

        for(int i=0;i<9;i++) resources[i].text="<size=13><color=#B9AB88>"+names[i]+"</color></size>\n"+values[i];

    }



    public void CheckEnding()

    {

        if (ended || !VariableSingleton.IsInitialized) return;

        string reason = null;

        if(CouncilActions.Int("iSurrenderEnding")>0) reason=SurrenderTerms.EndingText;
        else if (VariableSingleton.Instance.EnduringWall.CityLost) reason = "The last wall has fallen. Constantinople is lost.";

        else if (VariableSingleton.GetFloatVariable("fHealth") <= 0) reason = DailyLifeEvents.I("iCombatAssassinationFatal")==1 ? "Assassinated in the palace. The Emperor dies of wounds sustained fighting his attacker." : DailyLifeEvents.I("iLifePoisonEnding")==1 ? "Assassinated by poison. The Emperor dies after eating a poisoned loaf." : "The Emperor has died.";

        else if (VariableSingleton.GetFloatVariable("fTroopRep") <= 0) reason = "Your army has lost the will to defend the city.";

        else if (VariableSingleton.GetFloatVariable("fPeopleRep") <= 0) reason = "The people have lost faith in your rule.";

        else if (VariableSingleton.GetFloatVariable("fNobleRep") <= 0) reason = "The nobility has withdrawn its support.";

        else if (VariableSingleton.GetFloatVariable("fPriestRep") <= 0) reason = "The Church has turned against your rule.";

        else if (VariableSingleton.GetIntVariable("iBalkans") + VariableSingleton.GetIntVariable("iBazouks") +

            VariableSingleton.GetIntVariable("iJanissaries") == 0) reason = "The Ottoman field army has been destroyed. The siege is broken.";

        else if (VariableSingleton.CompletedSiegeDays >= 240) reason = "After 240 days, the Ottoman siege collapses. Constantinople endures.";

        if(VariableSingleton.GetIntVariable("iDeposed")>0)reason="Giovanni has seized the imperial throne. Your reign ends; the defence continues under another emperor.";
        if (reason == null) return;

        if(reason=="The Ottoman field army has been destroyed. The siege is broken." || reason=="After 240 days, the Ottoman siege collapses. Constantinople endures.") { PreparationCampaign.HonourVictory(); Achievements.Unlock(0); Achievements.Victory(); }
        SaveGames.End("Campaign ended",reason+"\n\nSiege days survived: "+VariableSingleton.CompletedSiegeDays);
        ended = true;

        ShowModal("Campaign ended", reason + "\n\nSiege days survived: " + VariableSingleton.CompletedSiegeDays +

            MurderMystery.Epilogue()+"\nYour next attempt begins with what you learned.", false);

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

        ShowModal("Campaign menu", (SaveGames.Ironman?"Ironman · one rolling save. Completed decisions and turns are saved automatically. Achievements enabled.":"Normal campaign · named saves available at the council. Achievements disabled.")+"\nLocal archives: "+SaveGames.Root, true);

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



    private void ShowModal(string title, string body, bool resume)

    {

        if (modal != null) Destroy(modal);

        modal = Panel(canvas.transform, "Campaign Modal", Vector2.zero, Vector2.one);

        Label(modal.transform, title, new Vector2(.12f,.83f), new Vector2(.88f,.96f), 42);

        var viewport=Panel(modal.transform,"Campaign outcome scroll",new Vector2(.12f,.42f),new Vector2(.86f,.82f));
        viewport.AddComponent<RectMask2D>();
        var outcome=Label(viewport.transform,body,Vector2.zero,Vector2.one,25);
        var scroll=viewport.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.scrollSensitivity=40;scroll.viewport=viewport.GetComponent<RectTransform>();scroll.content=outcome.rectTransform;
        var r=outcome.rectTransform;r.anchorMin=new Vector2(0,1);r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,1);
        Canvas.ForceUpdateCanvases();r.sizeDelta=new Vector2(0,Mathf.Max(viewport.GetComponent<RectTransform>().rect.height,outcome.GetPreferredValues(body,viewport.GetComponent<RectTransform>().rect.width,9999).y+20));r.anchoredPosition=Vector2.zero;
        AddScrollbar(scroll,modal.transform,new Vector2(.87f,.42f),new Vector2(.88f,.82f));

        if (resume) MakeButton(modal.transform, "Resume", new Vector2(.3f,.3f), new Vector2(.7f,.39f), TogglePause);

        else MakeButton(modal.transform,"Start a new campaign",new Vector2(.3f,.3f),new Vector2(.7f,.39f),()=>SaveMenu.NewCampaign(canvas.transform));

        if(resume){
            MakeButton(modal.transform,"Save / load",new Vector2(.08f,.04f),new Vector2(.48f,.13f),()=>SaveMenu.Show(canvas.transform));
            MakeButton(modal.transform,"Settings",new Vector2(.52f,.04f),new Vector2(.92f,.13f),()=>AudioSettingsUI.Show(canvas.transform));
        }
        MakeButton(modal.transform, "Return to main menu", new Vector2(.3f,.17f), new Vector2(.7f,.26f), () => {if(SaveGames.CanCheckpointCouncil)VariableSingleton.Instance.Save();SceneManager.LoadScene("Menu");});

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

