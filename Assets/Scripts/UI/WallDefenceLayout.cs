using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A compact presentation adapter; the existing wall action component owns every rule.
public static class WallDefenceLayout
{
    static readonly string[] Keys = {"fOuterWall", "fMiddleWall", "fInnerWall", "fMoat"};
    static readonly string[] Names = {"Theodosian Wall", "Constantinian Wall", "Severan Wall", "Moat"};

    public static void Apply(GameObject root, ManagerUI manager)
    {
        var legacy = root.transform.Find("Defenses");
        if(legacy != null) legacy.gameObject.SetActive(false);
        var actions = root.GetComponentInChildren<DefenseActions>(true);
        if(actions == null) return;
        var panel = root.transform.Find("Wall command panel");
        if(panel == null)
        {
            var obj = new GameObject("Wall command panel",typeof(RectTransform),typeof(Image));
            obj.transform.SetParent(root.transform,false); panel=obj.transform;
            Place(panel,.02f,.025f,.44f,.91f);
            ImperialTheme.Panel(obj,false);
            Label(panel,"Defence of the walls",.05f,.925f,.95f,.985f,28);
            for(int i=0;i<Keys.Length;i++)
            {
                int index=i; float top=.91f-i*.085f;
                if(i<3)
                {
                    var button=CampaignHUD.MakeButton(panel,Names[i],new Vector2(.04f,top-.074f),new Vector2(.96f,top),()=>actions.GetSelection(Keys[index]));
                    button.name="Wall row "+Keys[i];
                }
                else
                {
                    var row=new GameObject("Wall row "+Keys[i],typeof(RectTransform),typeof(Image));row.transform.SetParent(panel,false);
                    Place(row.transform,.04f,top-.074f,.96f,top);ImperialTheme.Panel(row,false);
                    Label(row.transform,Names[i],.03f,.22f,.97f,.96f,22);
                }
            }
            var frame=root.transform.Find("Options Frame");
            if(frame!=null)
            {
                frame.SetParent(panel,false);Place(frame,.04f,.215f,.96f,.56f);
                var image=frame.GetComponent<Image>();if(image!=null){image.sprite=null;image.color=Color.clear;}
                foreach(var layout in frame.GetComponentsInChildren<LayoutGroup>(true))layout.enabled=false;
                foreach(var fitter in frame.GetComponentsInChildren<ContentSizeFitter>(true))fitter.enabled=false;
                var outerFrame=frame.Find("Imperial goldwork");if(outerFrame!=null)outerFrame.gameObject.SetActive(false);
            }
            CampaignHUD.MakeButton(panel,"Garrison",new Vector2(.04f,.12f),new Vector2(.49f,.195f),()=>DefencePosture.Show(manager,null));
            CampaignHUD.MakeButton(panel,"Special actions",new Vector2(.51f,.12f),new Vector2(.96f,.195f),()=>{root.SetActive(false);CouncilUI.For(manager).WallSpecialActions();});
            CampaignHUD.MakeButton(panel,"Return",new Vector2(.04f,.025f),new Vector2(.96f,.10f),()=>ControlSingleton.Instance.GoBack());
        }
        // Conditions refresh whenever the player returns from another council screen.
        for(int i=0;i<Keys.Length;i++)
        {
            var row=panel.Find("Wall row "+Keys[i]);if(row==null)continue;
            var label=row.GetComponentInChildren<TMP_Text>(true);
            label.text=Names[i]+"  ·  "+VariableSingleton.GetPercentFloat(Keys[i]);
            label.enableAutoSizing=true;label.fontSizeMin=16;label.fontSizeMax=23;label.alignment=TextAlignmentOptions.Center;
            Place(label.transform,.025f,.20f,.975f,.97f);
            var bar=row.Find("Integrity colour");
            if(bar==null){var obj=new GameObject("Integrity colour",typeof(RectTransform),typeof(Image));obj.transform.SetParent(row,false);bar=obj.transform;bar.GetComponent<Image>().raycastTarget=false;}
            float integrity=Mathf.Clamp01(VariableSingleton.GetFloatVariable(Keys[i]));
            Place(bar,.04f,.08f,.04f+.92f*integrity,.145f);
            bar.GetComponent<Image>().color=i==3?new Color(.28f,.66f,.74f):Color.Lerp(new Color(.72f,.30f,.24f),new Color(.43f,.72f,.57f),integrity);
        }
        actions.GetSelection("f"+VariableSingleton.Instance.EnduringWall.GetCodeName());
    }

    public static void StyleOptions(GameObject options,TMP_Text title,TMP_Text cost,Button repair,Button abandon)
    {
        Place(options.transform,0,0,1,1);
        // The legacy cost text lived inside two fixed-size grey cells.
        // Keep its serialized reference, but place it directly in the detail area.
        cost.transform.SetParent(options.transform,false);
        var oldCostCells=options.transform.Find("Repair Cost");
        if(oldCostCells!=null)oldCostCells.gameObject.SetActive(false);
        foreach(var image in options.GetComponentsInChildren<Image>(true))
            if(image.GetComponent<Button>()==null && image.GetComponentInParent<Slider>(true)==null){image.sprite=null;image.color=Color.clear;}
        var border=options.transform.Find("Imperial goldwork");if(border!=null)border.gameObject.SetActive(false);
        Place(title.transform,.025f,.85f,.975f,.99f);
        title.font=ImperialTheme.Font;title.color=ImperialTheme.Ivory;title.enableAutoSizing=true;title.fontSizeMin=17;title.fontSizeMax=25;title.alignment=TextAlignmentOptions.Center;
        Place(cost.transform,.03f,.225f,.97f,.585f);
        cost.font=ImperialTheme.Font;cost.color=ImperialTheme.Ivory;cost.enableAutoSizing=true;cost.fontSizeMin=14;cost.fontSizeMax=19;cost.alignment=TextAlignmentOptions.Center;
        Place(repair.transform,.02f,.025f,.55f,.205f);Place(abandon.transform,.59f,.025f,.98f,.205f);
        repair.GetComponentInChildren<TMP_Text>(true).text="Order repairs";
        abandon.GetComponentInChildren<TMP_Text>(true).text="Abandon wall";
        ImperialTheme.Button(repair);ImperialTheme.Button(abandon);
        TurnActionButton.Mark(repair);TurnActionButton.Mark(abandon);
        foreach(var button in new[]{repair,abandon})foreach(var text in button.GetComponentsInChildren<TMP_Text>(true)){text.enableAutoSizing=true;text.fontSizeMin=16;text.fontSizeMax=22;}
    }

    public static Slider EngineerSlider(Transform parent, out TMP_Text caption, UnityEngine.Events.UnityAction<float> changed)
    {
        caption=CampaignHUD.Label(parent,"Engineers",new Vector2(.025f,.735f),new Vector2(.975f,.835f),20);
        caption.name="Engineer assignment caption";
        caption.alignment=TextAlignmentOptions.Center;caption.enableAutoSizing=true;caption.fontSizeMin=14;caption.fontSizeMax=20;caption.raycastTarget=false;
        var root=new GameObject("Engineer assignment",typeof(RectTransform),typeof(Image),typeof(Slider));root.transform.SetParent(parent,false);
        Place(root.transform,.045f,.605f,.955f,.725f);
        root.GetComponent<Image>().color=Color.clear; // broad click/drag target around a slim track
        var track=CampaignHUD.Panel(root.transform,"Track",new Vector2(.015f,.39f),new Vector2(.985f,.61f));
        track.GetComponent<Image>().color=ImperialTheme.Surface;track.GetComponent<Image>().raycastTarget=false;
        var fillArea=new GameObject("Fill area",typeof(RectTransform));fillArea.transform.SetParent(root.transform,false);Place(fillArea.transform,.015f,.39f,.985f,.61f);
        var fill=CampaignHUD.Panel(fillArea.transform,"Gold fill",Vector2.zero,Vector2.one);fill.GetComponent<Image>().color=ImperialTheme.Gold;fill.GetComponent<Image>().raycastTarget=false;
        var handleArea=new GameObject("Handle area",typeof(RectTransform));handleArea.transform.SetParent(root.transform,false);Place(handleArea.transform,.015f,.5f,.985f,.5f);
        var handle=CampaignHUD.Panel(handleArea.transform,"Gold seal",Vector2.zero,Vector2.zero);
        var handleRect=handle.GetComponent<RectTransform>();handleRect.sizeDelta=new Vector2(15,15);handleRect.localRotation=Quaternion.Euler(0,0,45);
        handle.GetComponent<Image>().color=ImperialTheme.Gold;handle.GetComponent<Image>().raycastTarget=false;
        var centre=CampaignHUD.Panel(handle.transform,"Ivory inset",new Vector2(.30f,.30f),new Vector2(.70f,.70f));centre.GetComponent<Image>().color=ImperialTheme.Ivory;centre.GetComponent<Image>().raycastTarget=false;
        var slider=root.GetComponent<Slider>();slider.wholeNumbers=true;slider.minValue=1;slider.maxValue=10;slider.fillRect=fill.GetComponent<RectTransform>();slider.handleRect=handleRect;slider.targetGraphic=handle.GetComponent<Image>();
        slider.onValueChanged.AddListener(changed);StyleEngineerSlider(slider);return slider;
    }

    public static void StyleEngineerSlider(Slider slider)
    {
        var colours=slider.colors;colours.normalColor=ImperialTheme.Gold;colours.highlightedColor=ImperialTheme.Ivory;colours.pressedColor=Color.white;colours.selectedColor=ImperialTheme.Ivory;colours.disabledColor=new Color(.42f,.38f,.29f,.65f);slider.colors=colours;
    }

    static void Label(Transform parent,string text,float left,float bottom,float right,float top,float size)
    {
        var obj=new GameObject(text,typeof(RectTransform),typeof(TextMeshProUGUI));obj.transform.SetParent(parent,false);Place(obj.transform,left,bottom,right,top);
        var label=obj.GetComponent<TextMeshProUGUI>();label.text=text;label.font=ImperialTheme.Font;label.color=ImperialTheme.Ivory;label.alignment=TextAlignmentOptions.Center;label.enableAutoSizing=true;label.fontSizeMin=18;label.fontSizeMax=size;label.raycastTarget=false;
    }
    static void Place(Transform target,float left,float bottom,float right,float top)
    {
        var rect=target as RectTransform;if(rect==null)return;
        rect.anchorMin=new Vector2(left,bottom);rect.anchorMax=new Vector2(right,top);rect.offsetMin=rect.offsetMax=Vector2.zero;
    }
}
