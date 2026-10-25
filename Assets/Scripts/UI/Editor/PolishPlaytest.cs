using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

public static class PolishPlaytest
{
    static T[] All<T>() where T:Object => Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None);
    static GameObject Named(string name) => All<Transform>().First(t=>t.gameObject.scene.IsValid() && t.name==name).gameObject;
    static void Rect(GameObject obj,float x,float y,float xx,float yy)
    {
        var r=obj.GetComponent<RectTransform>(); r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(xx,yy);
        r.offsetMin=r.offsetMax=Vector2.zero;r.localScale=Vector3.one;
    }
    static void Caption(Button button,string text)
    {
        var label=button.GetComponentInChildren<TMP_Text>(true);
        if(label==null) label=CampaignHUD.Label(button.transform,text,Vector2.zero,Vector2.one,24);
        label.text=text;label.alignment=TextAlignmentOptions.Center;
        label.margin=new Vector4(8,4,8,4);
    }
    public static void RunAndBuild() { Run(); ConfigurePlaytest.Build(); }
    public static void Run()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Continue Game.unity");
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        foreach(var text in All<TMP_Text>())
        {
            text.font=font;text.fontStyle=FontStyles.Normal;text.enableAutoSizing=true;
            text.fontSizeMin=20;text.fontSizeMax=28;text.fontSize=26;
            text.enableWordWrapping=true;text.raycastTarget=false;
        }
        foreach(var button in All<Button>())
        {
            var img=button.GetComponent<Image>();
            if(img!=null) {img.color=new Color(.23f,.27f,.28f);img.sprite=null;}
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.92f,.72f);
            colors.selectedColor=colors.highlightedColor;colors.disabledColor=new Color(.45f,.45f,.45f,.75f);button.colors=colors;
            var text=button.GetComponentInChildren<TMP_Text>(true);if(text!=null) text.color=new Color(.96f,.92f,.82f);
            if(button.name=="ButtonBack") Caption(button,"Return");
        }
        var variableSettings=new SerializedObject(Object.FindObjectOfType<VariableSingleton>(true));
        variableSettings.FindProperty("turnsToSiege").intValue=23;variableSettings.ApplyModifiedPropertiesWithoutUndo();
        var runner=Object.FindObjectOfType<DialogueRunner>(true);
        while(runner.onDialogueStart.GetPersistentEventCount()>0) UnityEventTools.RemovePersistentListener(runner.onDialogueStart,0);
        var eventUI=Object.FindObjectOfType<EventUI>(true);
        foreach(var b in eventUI.GetComponentsInChildren<Button>(true))
        {
            if(b.name!="ButtonToContinue") continue;
            while(b.onClick.GetPersistentEventCount()>0) UnityEventTools.RemovePersistentListener(b.onClick,0);
            UnityEventTools.AddPersistentListener(b.onClick,eventUI.ContinueToAssault);b.interactable=true;
            Caption(b,"Continue");Rect(b.gameObject,.76f,.015f,.97f,.10f);
        }
        Named("ResourceUI").SetActive(false);
        var manager=Object.FindObjectOfType<ManagerUI>(true);
        Rect(manager.gameObject,.50f,.07f,.94f,.93f);
        string[] buttons={"ButtonToTroops","ButtonToSiege","ButtonToDiplomacy","ButtonToSpy","ButtonToPeople","ButtonToContinue"};
        for(int i=0;i<buttons.Length;i++)
        {
            var btn=manager.GetComponentsInChildren<Button>(true).First(b=>b.name==buttons[i]);
            Rect(btn.gameObject,.07f,.73f-i*.125f,.93f,.835f-i*.125f);
        }
        // Make the primary screens occupy the available area, leaving room for the persistent header.
        Rect(Object.FindObjectOfType<EventUI>(true).gameObject,0,.015f,1,.865f);
        var eventScreen=Object.FindObjectOfType<DialogueController>(true).gameObject;
        Rect(eventScreen,0,.04f,1,.98f);
        foreach(var group in eventScreen.GetComponentsInChildren<LayoutGroup>(true)) group.enabled=false;
        foreach(var fit in eventScreen.GetComponentsInChildren<ContentSizeFitter>(true)) fit.enabled=false;
        var lv=Object.FindObjectOfType<LineView>(true);
        Rect(lv.gameObject,.03f,.07f,.97f,.97f);
        foreach(var text in lv.GetComponentsInChildren<TMP_Text>(true))
        {
            if(text.name=="Text") {Rect(text.gameObject,.045f,.13f,.955f,.94f);text.color=new Color(.95f,.91f,.82f);text.alignment=TextAlignmentOptions.TopLeft;}
        }
        if(lv.transform.Find("Continue with mouse")==null)
        {
            var next=CampaignHUD.MakeButton(lv.transform,"Continue with mouse",new Vector2(.67f,.025f),new Vector2(.955f,.105f),()=>{});
            UnityEventTools.AddPersistentListener(next.onClick,lv.OnContinueClicked);
        }
        var options=Object.FindObjectOfType<OptionListButtons>(true);
        Rect(options.gameObject,.025f,.07f,.975f,.97f);
        var serial=new SerializedObject(options);
        var panel=(RectTransform)serial.FindProperty("optionPanel").objectReferenceValue;
        Rect(panel.gameObject,.46f,.06f,.985f,.975f);
        if(options.transform.Find("Choice scroll hint")==null)
        {
            var hint=CampaignHUD.Label(options.transform,"Scroll for more choices",new Vector2(.46f,.005f),new Vector2(.985f,.05f),18);
            hint.name="Choice scroll hint";
        }
        var last=(TextMeshProUGUI)serial.FindProperty("lastLineText").objectReferenceValue;
        Rect(last.gameObject,.025f,.035f,.43f,.965f);
        last.fontSizeMin=18;last.fontSizeMax=24;last.color=new Color(.95f,.91f,.82f);last.alignment=TextAlignmentOptions.TopLeft;
        foreach(var img in options.GetComponentsInChildren<Image>(true).Concat(lv.GetComponentsInChildren<Image>(true)))
            if(img.name=="Background") {img.color=new Color(.10f,.13f,.145f,.99f);img.sprite=null;}
        var military=Named("Military UI");
        foreach(var button in military.GetComponentsInChildren<Button>(true))
        {
            if(button.name!="Button") continue;
            bool recruit=false;
            for(int i=0;i<button.onClick.GetPersistentEventCount();i++)
                if(button.onClick.GetPersistentMethodName(i)=="GetSelectionIndex") recruit=true;
            if(recruit) Caption(button,"Recruit");
            else if(button.GetComponentInChildren<ValueUpdater>(true)==null)
            {Caption(button,"Training\nUnavailable");button.interactable=false;}
        }
        if(military.GetComponent<Image>()!=null) military.GetComponent<Image>().color=new Color(.10f,.14f,.16f,.97f);
        Rect(military,.30f,.04f,.98f,.94f);
        foreach(var group in military.GetComponentsInChildren<LayoutGroup>(true)) group.enabled=false;
        foreach(var fitter in military.GetComponentsInChildren<ContentSizeFitter>(true)) fitter.enabled=false;
        for(int row=0;row<6;row++)
        {
            var unit=military.transform.Find("Unit "+(row+1));
            Rect(unit.gameObject,.015f,.16f+(5-row)*.13f,.985f,.275f+(5-row)*.13f);
            var group=unit.Find("Group");Rect(group.gameObject,0,0,1,1);
            var cells=group.Cast<Transform>().ToArray();
            foreach(Transform child in group) if(child.name=="Spacer") child.gameObject.SetActive(true);
            float[] left={.00f,.26f,.40f,.54f,.77f};float[] right={.25f,.39f,.52f,.75f,1f};
            for(int col=0;col<cells.Length && col<5;col++)
            {
                var cell=cells[col];Rect(cell.gameObject,left[col],.05f,right[col],.95f);
                var nestedButton=cell.GetComponentInChildren<Button>(true);
                if(nestedButton!=null && nestedButton.transform!=cell) Rect(nestedButton.gameObject,.02f,.05f,.98f,.95f);
                var image=cell.GetComponent<Image>();if(image!=null){image.sprite=null;image.color=new Color(.15f,.19f,.20f,.98f);}
                foreach(var t in cell.GetComponentsInChildren<TMP_Text>(true))
                {
                    if(t.transform!=cell) Rect(t.gameObject,.03f,.03f,.97f,.97f);
                    t.fontSizeMin=18;t.fontSizeMax=26;t.fontSize=24;t.color=new Color(.95f,.91f,.82f);t.alignment=TextAlignmentOptions.Center;
                }
            }
        }
        Rect(Named("Conscription").transform.gameObject,.02f,.065f,.98f,.14f);
        foreach(var t in Named("Conscription").transform.GetComponentsInChildren<TMP_Text>(true)) {t.fontSizeMin=20;t.fontSizeMax=24;}
        var selected=Object.FindObjectOfType<RecruitationSelection>(true);
        var overlay=selected.transform.parent;
        Rect(overlay.gameObject,.1f,.15f,.9f,.85f);
        Rect(selected.gameObject,.03f,.03f,.97f,.97f);
        var selectionGroup=selected.transform.Find("Group");Rect(selectionGroup.gameObject,0,0,1,1);
        foreach(var layout in overlay.GetComponentsInChildren<LayoutGroup>(true)) layout.enabled=false;
        foreach(Transform child in selectionGroup)
        {
            if(child.name=="Spacer") child.gameObject.SetActive(true);
            else if(child.name=="Name Field") Rect(child.gameObject,.05f,.80f,.95f,.95f);
            else if(child.name=="Number Input") Rect(child.gameObject,.05f,.39f,.95f,.77f);
            else if(child.name=="Cost Field") Rect(child.gameObject,.05f,.20f,.95f,.36f);
            else if(child.name=="Buy Button") Rect(child.gameObject,.10f,.02f,.90f,.16f);
        }
        var buy=Named("Buy Button");
        Rect(buy.transform.parent.gameObject,.1f,.02f,.9f,.16f);
        Rect(buy,0,0,1,1);
        foreach(var b in overlay.GetComponentsInChildren<Button>(true)) if(b.name=="ButtonBack") b.gameObject.SetActive(false);
        var number=selectionGroup.Find("Number Input");
        var field=number.GetComponentInChildren<TMP_InputField>(true);Rect(field.gameObject,.32f,.40f,.68f,.95f);
        var adjustments=number.GetComponentsInChildren<Button>(true);
        for(int i=0;i<adjustments.Length;i++) Rect(adjustments[i].gameObject,.02f+i*.163f,.04f,.17f+i*.163f,.32f);
        foreach(var t in overlay.GetComponentsInChildren<TMP_Text>(true)){t.fontSizeMin=20;t.fontSizeMax=28;}
        Caption(Named("Buy Button").GetComponent<Button>(),"Recruit troops");
        // Detailed repair preview gets its own wide area, rather than a tiny numeric cell.
        var defense=Object.FindObjectOfType<DefenseActions>(true);
        foreach(var group in defense.GetComponentsInChildren<LayoutGroup>(true)) group.enabled=false;
        foreach(var fit in defense.GetComponentsInChildren<ContentSizeFitter>(true)) fit.enabled=false;
        var ds=new SerializedObject(defense);
        var repairOptions=(GameObject)ds.FindProperty("Options").objectReferenceValue;
        var existingPreview=repairOptions.transform.Find("Repair Preview");
        var repairText=existingPreview!=null ? existingPreview.GetComponent<TMP_Text>() : CampaignHUD.Label(repairOptions.transform,"",Vector2.zero,Vector2.one,24);
        repairText.name="Repair Preview";repairText.font=font;repairText.color=new Color(.95f,.91f,.82f);repairText.enableAutoSizing=true;
        ds.FindProperty("repairCostDisplay").objectReferenceValue=repairText;ds.ApplyModifiedPropertiesWithoutUndo();
        Rect(repairText.gameObject,.04f,.43f,.96f,.68f);repairText.fontSizeMin=20;repairText.fontSizeMax=25;
        var name=(TMP_Text)ds.FindProperty("selectionName").objectReferenceValue;Rect(name.gameObject,.04f,.77f,.96f,.94f);
        Rect(((Button)ds.FindProperty("repairButton").objectReferenceValue).gameObject,.04f,.23f,.96f,.39f);
        Rect(((Button)ds.FindProperty("abandonButton").objectReferenceValue).gameObject,.04f,.03f,.96f,.19f);
        Caption(((Button)ds.FindProperty("repairButton").objectReferenceValue),"Repair — end turn");
        Caption(((Button)ds.FindProperty("abandonButton").objectReferenceValue),"Abandon wall — end turn");
        foreach(var b in defense.GetComponentsInChildren<Button>(true))
            for(int i=0;i<b.onClick.GetPersistentEventCount();i++) if(b.onClick.GetPersistentMethodName(i)=="GetSelection") Caption(b,"Inspect wall");
        var old=repairOptions.transform.Find("Repair Cost");if(old!=null)old.gameObject.SetActive(false);
        foreach(var button in All<Button>()) if(button.name=="ButtonBack") button.gameObject.SetActive(false);
        foreach(var component in All<Component>())
            if(component!=null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        // Theme menu fonts too; preserve its existing illustration and layout.
        scene=EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");
        foreach(var text in All<TMP_Text>()) {text.font=font;text.enableAutoSizing=true;text.fontSizeMin=22;text.fontSizeMax=Mathf.Max(28,text.fontSize);}
        foreach(var component in All<Component>())
            if(component!=null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("UI_POLISH_COMPLETE");
    }
}

