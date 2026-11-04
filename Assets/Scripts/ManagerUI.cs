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
        WallDefenceLayout.Apply(siegeUI,this);
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
        foreach(var child in troopsUI.GetComponentsInChildren<Transform>(true))
        {
            if(child.name=="Military UI") recruitmentPanel=child;
            if(child.name=="Conscription") child.gameObject.SetActive(false);
        }
        if(recruitmentPanel.Find("Recruitment logistics")==null)
            CampaignHUD.MakeButton(recruitmentPanel,"Recruitment logistics",new Vector2(.58f,.93f),new Vector2(.98f,.995f),()=>{troopsUI.SetActive(false);CouncilUI.For(this).Training();});
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
            if(i==2){
                var fleet=label.GetComponentInParent<UnityEngine.UI.Button>();
                if(fleet==null){
                    var target=label.transform.parent.gameObject;
                    if(target.GetComponent<UnityEngine.UI.Image>()==null)target.AddComponent<UnityEngine.UI.Image>();
                    fleet=target.AddComponent<UnityEngine.UI.Button>();
                }
                // Replace the old serialized upgrade action as well as runtime listeners.
                fleet.onClick=new UnityEngine.UI.Button.ButtonClickedEvent();fleet.interactable=true;
                fleet.name="Military fleet management";
                fleet.onClick.AddListener(()=>{
                    troopsUI.SetActive(false);var council=CouncilUI.For(this);council.Fleet();
                    ControlSingleton.Instance.ReturnAction=()=>{council.Hide();BringUpTroopsUI();};
                });
                TurnActionButton.Unmark(fleet);ImperialTheme.Button(fleet);
                var graphic=fleet.GetComponent<UnityEngine.UI.Image>();if(graphic!=null)graphic.raycastTarget=true;
            }
        }
        if(recruitmentPanel.Find("Quality and roles")==null)
        {
            var help=CampaignHUD.MakeButton(recruitmentPanel,"Lethality & unit roles",new Vector2(.02f,.93f),new Vector2(.54f,.995f),()=>CampaignHUD.Instance.ShowReport("Lethality and unit roles",MilitaryExplanation,()=>CampaignHUD.Instance.DismissReport()));
            help.name="Quality and roles";
        }
        if(recruitmentPanel.Find("Specialist recruitment")==null)
        {
            // Use the space vacated by the retired Conscription row, below all six units.
            var link=CampaignHUD.MakeButton(recruitmentPanel,"Specialists · engineers, monks, agents and envoys",new Vector2(.02f,.065f),new Vector2(.98f,.14f),()=>{troopsUI.SetActive(false);CouncilUI.For(this).Specialists();});
            link.name="Specialist recruitment";
            link.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().ignoreLayout=true;
            ImperialTheme.Button(link);
        }
        ControlSingleton.Instance.LogToEsc(this.gameObject, troopsUI);
    }

    public const string MilitaryExplanation =
        "<b>Lethality</b>\nLethality measures the killing power of each soldier. More men give you greater strength; training and equipment make each man more effective. For the same troop type in the same conditions, ×1 provides twice the killing power of ×0.5. This is its contribution to combat, not a guaranteed number of kills. Deployment, losses, commanders and the phase of battle determine how much of that strength reaches the enemy. The displayed value includes training and relevant technologies.\n\n"+
        "<b>Infantry — hold the line</b>\nInfantry meets the attackers who reach the walls and bears the brunt of the fighting. These men usually suffer the largest share of close-combat losses. Keep enough at the threatened wall to withstand repeated attacks; better training helps them kill more of the enemy once battle is joined.\n\n"+
        "<b>Archers — break the approach</b>\nArchers are your principal source of damage while the enemy approaches. Their volleys thin the ranks before hand-to-hand fighting begins, leaving fewer attackers for the infantry to face. Protect them from bombardment: every archer lost weakens the volleys still to come.\n\n"+
        "<b>Cavalry — strike beyond the gates</b>\nMounted troops serve in raids and charges when events offer the opportunity. To use them in a wall assault, enable Dismount cavalry under Garrison. Fighting on foot halves their displayed lethality; Dismounted Champions restores its full value. Cavalry kept mounted remains outside the wall-assault force.\n\n"+
        "<b>Varangian Guard — close the breach</b>\nThe Guard is your reserve for a failing defence. When attackers threaten to break through, committing the Varangians can save the line, at the cost of guardsmen. The battle shows how many are needed. Preserve this small force for the moments when the rest of the garrison cannot hold.\n\n"+
        "<b>Cannons — tear open the ranks</b>\nCannons strike the approaching enemy and need ammunition to fire. They can also shatter arrow shields, restoring the effectiveness of your archers. Artillery technologies and the gunner’s expertise strengthen their fire.\n\n"+
        "<b>Warships — sustain the defence</b>\nThe fleet follows its own orders: bringing supplies, raiding or defending the harbour. Its usefulness depends on the mission, surviving ships and command. Review these under Fleet.\n\n"+
        "<b>Improving your troops</b>\nPaid drills improve infantry, archers and cavalry after five days; the men remain available for battle while training. Open Upgrades to review their training, technologies and combined lethality.";

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

    private void Start() {
        CouncilUI.For(this).BuildManagementMenu();
        if(repairAction!=null)TurnActionButton.MarkBoundActions(repairAction.transform,"ApproveRepairs");
        if(recruitmentAction!=null)TurnActionButton.MarkBoundActions(recruitmentAction.transform,"ApproveTroopRecruitment");
    }

    private void OnEnable()
    {
        date.text = VariableSingleton.GetDate().ToString("d MMM yyyy");
        if (VariableSingleton.IsInitialized)
        {
            OttomanDirector.PrepareTurn();
            // Include the public report in the same checkpoint that preserves its notice.
            if (!AssaultPractice.Active && !string.IsNullOrEmpty(VariableSingleton.ArrivalNotice))
                DecisionJournal.Record("daily-"+VariableSingleton.Turn,"Daily council report",VariableSingleton.ArrivalNotice);
            if (SaveGames.PendingResume==null && !AssaultPractice.Active && string.IsNullOrEmpty(SaveGames.LastError)) VariableSingleton.Instance.Save();
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
