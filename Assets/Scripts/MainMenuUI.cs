using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.SceneManagement;

public class MainMenuUI : UIWindowFam

{

    [SerializeField] GameObject optionsDialogue;

    [SerializeField] GameObject loadDialogue;



    private void Start()
    {
        var font=ImperialTheme.Font;
        foreach(var title in transform.root.GetComponentsInChildren<TMPro.TMP_Text>(true)) if(title.text.ToUpperInvariant().Contains("BESIEGED IN CONSTANTINOPLE")) title.gameObject.SetActive(false);
        var shade=CampaignHUD.Panel(transform.root,"Menu shade",Vector2.zero,Vector2.one);shade.GetComponent<UnityEngine.UI.Image>().color=new Color(.06f,.025f,.04f,.42f);shade.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
        transform.SetAsLastSibling();
        var bounds=GetComponent<RectTransform>();bounds.anchorMin=new Vector2(.57f,.06f);bounds.anchorMax=new Vector2(.95f,.94f);bounds.offsetMin=bounds.offsetMax=Vector2.zero;
        var panel=GetComponent<UnityEngine.UI.Image>();if(panel!=null){ImperialTheme.Panel(gameObject);}
        var border=gameObject.AddComponent<UnityEngine.UI.Outline>();border.effectColor=new Color(.64f,.49f,.25f);border.effectDistance=new Vector2(2,-2);
        var heading=CampaignHUD.Label(transform,"BESIEGED\n<size=72%>IN CONSTANTINOPLE</size>",new Vector2(.04f,.76f),new Vector2(.96f,.97f),47);heading.font=font;heading.alignment=TMPro.TextAlignmentOptions.Center;heading.characterSpacing=2;
        var subtitle=CampaignHUD.Explanation(transform,"A City. An Empire. A last stand.",new Vector2(.06f,.70f),new Vector2(.94f,.77f),23);subtitle.font=font;subtitle.alignment=TMPro.TextAlignmentOptions.Center;
        var rule=CampaignHUD.Panel(transform,"Gold rule",new Vector2(.15f,.695f),new Vector2(.85f,.697f));rule.GetComponent<UnityEngine.UI.Image>().color=new Color(.64f,.49f,.25f);
        var original=GetComponentsInChildren<UnityEngine.UI.Button>(true);
        var quick=CampaignHUD.MakeButton(transform,VariableSingleton.HasPreparationPreset?"Repeat preparation":"Repeat preparation\n<size=65%>Reach the siege to unlock</size>",Vector2.zero,Vector2.one,QuickStart);quick.interactable=VariableSingleton.HasPreparationPreset;
        var achievement=CampaignHUD.MakeButton(transform,"Achievements",Vector2.zero,Vector2.one,()=>Achievements.Show(transform.root));
        var settingsButton=CampaignHUD.MakeButton(transform,"Settings",Vector2.zero,Vector2.one,()=>AudioSettingsUI.Show(transform.root));
        var ordered=new System.Collections.Generic.List<UnityEngine.UI.Button>();foreach(var item in original)if(item.transform.parent==transform)ordered.Add(item);
        ordered.Insert(Mathf.Min(2,ordered.Count),quick);ordered.Insert(Mathf.Max(0,ordered.Count-1),achievement);ordered.Insert(Mathf.Max(0,ordered.Count-1),settingsButton);
        for(int i=0;i<ordered.Count;i++) {
            var button=ordered[i];float top=.66f-i*.081f;var rect=button.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.09f,top-.065f);rect.anchorMax=new Vector2(.91f,top);rect.offsetMin=rect.offsetMax=Vector2.zero;
            var background=button.GetComponent<UnityEngine.UI.Image>();if(background!=null){ImperialTheme.Button(button);}
            var outline=button.GetComponent<UnityEngine.UI.Outline>()??button.gameObject.AddComponent<UnityEngine.UI.Outline>();outline.effectColor=new Color(.64f,.49f,.25f,.8f);outline.effectDistance=new Vector2(1,-1);
            var caption=button.GetComponentInChildren<TMPro.TMP_Text>();if(caption!=null){caption.font=font;caption.fontSize=32;caption.fontSizeMin=23;caption.fontSizeMax=32;caption.enableAutoSizing=true;caption.color=new Color(.94f,.88f,.73f);}
            if(button.name=="ButtonContinue")button.interactable=VariableSingleton.HasSave();
        }
        var version=CampaignHUD.Label(transform,CouncilUI.BuildLabel,new Vector2(.06f,.025f),new Vector2(.94f,.08f),17);version.alignment=TMPro.TextAlignmentOptions.Center;
    }

    public void QuickStart()
    {
        if(!VariableSingleton.HasPreparationPreset) return;
        var confirm=CampaignHUD.Panel(transform.root,"Repeat preparation",new Vector2(.18f,.2f),new Vector2(.82f,.8f));ImperialTheme.Panel(confirm);
        CampaignHUD.Label(confirm.transform,"Restart at the siege",new Vector2(.06f,.78f),new Vector2(.94f,.96f),36);
        CampaignHUD.Label(confirm.transform,"Load the preparation snapshot saved when you last reached the siege. Your prepared army, resources, decisions and relationships return to that point. You skip the preparation months.\n\nChoose New campaign instead if you want different preparation decisions. This restart will become your current campaign checkpoint.",new Vector2(.06f,.25f),new Vector2(.94f,.76f),26);
        CampaignHUD.MakeButton(confirm.transform,"Cancel",new Vector2(.06f,.06f),new Vector2(.45f,.20f),()=>Destroy(confirm));
        CampaignHUD.MakeButton(confirm.transform,"Restart at siege",new Vector2(.52f,.06f),new Vector2(.94f,.20f),()=>{VariableSingleton.QuickStartRequested=true;VariableSingleton.ContinueRequested=false;SceneManager.LoadScene("Continue Game");});
    }

    public void NewGame()

    {

        VariableSingleton.QuickStartRequested = false;

        VariableSingleton.ContinueRequested = false;

        SceneManager.LoadScene("Continue Game");

    }



    public void Load()

    {

        if (!VariableSingleton.HasSave()) return;

        VariableSingleton.ContinueRequested = true;

        SceneManager.LoadScene("Continue Game");

    }



    public void Options()

    {

        CampaignGuide.Show(transform.root);

    }

    

    public void Quit()

    {

        Application.Quit();

    }

}

