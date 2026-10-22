using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManagerUI : UIWindowFam
{
    [SerializeField] GameObject diplomacyUI;
    [SerializeField] GameObject siegeUI;
    [SerializeField] GameObject troopsUI;
    [SerializeField] GameObject spiesUI;
    [SerializeField] GameObject peopleUI;

    [SerializeField] GameObject restPhaseUI;

    [SerializeField] TMP_Text date;


    //*****************************
    public void BringUpDiplomacy()
    {
        CouncilUI.For(this).Diplomacy();
    }

    //*****************************
    public void BringUpSiegeUI()
    {
        SwitchPhase(siegeUI);ImperialTheme.LegacySurface(siegeUI);SceneBackdrop.Apply(siegeUI,SceneBackdrop.Phase("walls"));
        if(siegeUI.transform.Find("Special actions")==null)
            CampaignHUD.MakeButton(siegeUI.transform,"Special actions",new Vector2(.66f,.82f),new Vector2(.94f,.91f),()=>{siegeUI.SetActive(false);CouncilUI.For(this).WallSpecialActions();});
        if(siegeUI.transform.Find("Special actions")==null)
            CampaignHUD.MakeButton(siegeUI.transform,"Special actions",new Vector2(.66f,.82f),new Vector2(.94f,.91f),()=>{siegeUI.SetActive(false);CouncilUI.For(this).WallSpecialActions();});
        if(siegeUI.transform.Find("Garrison dispositions")==null)CampaignHUD.MakeButton(siegeUI.transform,"Garrison dispositions",new Vector2(.04f,.82f),new Vector2(.31f,.91f),()=>DefencePosture.Show(this,null));
        ControlSingleton.Instance.LogToEsc(this.gameObject, siegeUI);
    }

    [SerializeField] GameObject repairAction;
    public void OrderRepairs()
    {
        repairAction.SetActive(true);
        ControlSingleton.Instance.LogToEsc(siegeUI, repairAction);
    }

    public void ApproveRepairs()
    {
        ActionResults.Instance.LastAction = 0;
        ControlSingleton.Instance.GoBack();
        ControlSingleton.Instance.GoBack();
        ContinueToRest();
    }


    //*****************************
    public void BringUpTroopsUI()
    {
        SwitchPhase(troopsUI);ImperialTheme.LegacySurface(troopsUI);SceneBackdrop.Apply(troopsUI,"army");
        Transform recruitmentPanel=troopsUI.transform;
        foreach(var child in troopsUI.GetComponentsInChildren<Transform>(true)) if(child.name=="Military UI") recruitmentPanel=child;
        if(recruitmentPanel.Find("Ongoing training")==null)
            CampaignHUD.MakeButton(recruitmentPanel,"Ongoing training",new Vector2(.58f,.93f),new Vector2(.98f,.995f),()=>{troopsUI.SetActive(false);CouncilUI.For(this).Training();});
        foreach(var caption in troopsUI.GetComponentsInChildren<TMP_Text>(true))
        {
            if(caption.text.Trim()=="Quality") caption.text="Specialist";
            if(caption.text.Trim()=="Recruit") caption.text="Train / build";
            if(caption.text.Contains("Unavailable") && (caption.text.Contains("Training") || caption.text.Contains("Equipment")))
            {
                var unused=caption.GetComponentInParent<UnityEngine.UI.Button>();
                if(unused!=null) {
                    unused.gameObject.SetActive(true);unused.interactable=true;
                    caption.text="Upgrades";unused.onClick.RemoveAllListeners();
                    unused.onClick.AddListener(()=>{troopsUI.SetActive(false);CouncilUI.For(this).TroopUpgrades();});
                }
            }
        }
        var upgrades=troopsUI.GetComponentsInChildren<UnityEngine.UI.Button>(true).Where(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Upgrades").OrderByDescending(b=>b.transform.position.y).ToArray();
        for(int i=0;i<upgrades.Length;i++) {int selected=Mathf.Min(i,3);var b=upgrades[i];b.onClick.RemoveAllListeners();b.onClick.AddListener(()=>{troopsUI.SetActive(false);CouncilUI.For(this).TroopUpgrades(selected);});}
        var recruitment=troopsUI.GetComponentsInChildren<UnityEngine.UI.Button>(true).Where(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Train / build").OrderByDescending(b=>b.transform.position.y).ToArray();
        for(int i=0;i<recruitment.Length;i++) recruitment[i].GetComponentInChildren<TMP_Text>().text=i>=4?"Build":"Train";
        var specialists=troopsUI.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.text=="Specialist" || t.gameObject.name=="Specialist lethality").OrderByDescending(t=>t.transform.position.y).ToArray();
        var combat=FindObjectOfType<AssaultSystem>(true);
        for(int i=0;i<specialists.Length;i++) {
            var label=specialists[i];label.gameObject.name="Specialist lethality";label.fontSize=20;
            label.text=i==0?"Lethality\n×"+combat.GuardLethality.ToString("0.##"):i==1?"Lethality\n×"+combat.CannonLethality.ToString("0.##"):"Fleet\norders";
        }
        if(recruitmentPanel.Find("Quality and roles")==null)
        {
            var help=CampaignHUD.MakeButton(recruitmentPanel,"Lethality & unit roles",new Vector2(.02f,.93f),new Vector2(.54f,.995f),()=>CampaignHUD.Instance.ShowReport("Quality and unit roles",MilitaryExplanation,()=>CampaignHUD.Instance.DismissReport()));
            help.name="Quality and roles";
        }
        ControlSingleton.Instance.LogToEsc(this.gameObject, troopsUI);
    }

    public const string MilitaryExplanation =
        "<b>Lethality · ×0.5, ×1 and higher</b>\n<i>Higher quality increases a unit's contribution to combat. The displayed figure includes relevant technology upgrades.</i>\n\n"+
        "<b>Infantry</b>\n<i>At ×0.5, 1,000 infantry contribute about 500 enemy casualties to the melee calculation; at ×1, about 1,000. Commander bonuses, other troops and the number of attackers affect the final result.</i>\n\n"+
        "<b>Archers</b>\n<i>At ×1, 1,000 archers can stop about 1,000 attackers before melee. At ×0.5, their contribution would be about 500, before other bonuses.</i>\n\n"+
        "<b>Cavalry</b>\n<i>Cavalry contributes half its lethality while fighting dismounted at the walls. Dismounted Champions restores its full contribution.</i>\n\n"+
        "<b>Varangian Guard, cannons and warships</b>\n<i>These units have no separate quality rating. Varangians use their own combat strength when deployed. Cannons use artillery strength, modified by technologies and advisers. Warships contribute according to fleet orders.</i>\n\n"+
        "<b>Improving your forces</b>\n<i>Paid drills raise training over five days. Roman Tactics, Upgraded Crossbows and Cataphract Armour add 25% to the trained value. Open Upgrades to see the training bar, active technologies and combined lethality.</i>";

    [SerializeField] GameObject recruitmentAction;
    public void OrderTroopRecruitment()
    {
        recruitmentAction.SetActive(true);
        ControlSingleton.Instance.LogToEsc(troopsUI, recruitmentAction);
    }

    public void ApproveTroopRecruitment()
    {
        ActionResults.Instance.LastAction = 1;
        ControlSingleton.Instance.GoBack();
        ControlSingleton.Instance.GoBack();
        ContinueToRest();
    }






    //*****************************
    public void BringUpSpiesUI()
    {
        CouncilUI.For(this).Spies();
    }

    public void BringUpPeopleUI()
    {
        CouncilUI.For(this).Factions();
    }

    public void ContinueToRest()
    {
        VariableSingleton.ChangePercentFloat("fHealth", Random.Range(0.01f, 0.1f));
        ActionResults.Instance.Message = "You rest while the city prepares. The council will hear the next petition.";
        CompleteAction();
    }

    public void CompleteAction()
    {
        CouncilUI.For(this).Hide();
        ControlSingleton.Instance.ClearHistory();
        foreach (var panel in new[] { diplomacyUI, siegeUI, troopsUI, spiesUI, peopleUI, repairAction, recruitmentAction })
            if (panel != null) panel.SetActive(false);
        restPhaseUI.SetActive(true);
        gameObject.SetActive(false);
    }



    //*****************************
    // Refresh

    private void Start() { CouncilUI.For(this).BuildManagementMenu(); }

    private void OnEnable()
    {
        date.text = VariableSingleton.GetDate().ToString("d MMM yyyy");
        if (VariableSingleton.IsInitialized)
        {
            if (VariableSingleton.LastCheckpointTurn != VariableSingleton.Turn) VariableSingleton.Instance.Save();
            if (CampaignHUD.Instance != null)
            {
                CampaignHUD.Instance.CheckEnding();
                if (!CampaignHUD.Instance.HasEnded && !string.IsNullOrEmpty(VariableSingleton.ArrivalNotice))
                {
                    string arrivals=VariableSingleton.ArrivalNotice; VariableSingleton.ArrivalNotice="";
                    CampaignHUD.Instance.ShowReport("Daily council report",arrivals,()=>CampaignHUD.Instance.DismissReport());
                }
            }
        }
    }
}
