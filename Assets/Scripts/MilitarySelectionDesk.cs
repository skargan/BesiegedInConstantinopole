using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Reuse the scene's existing recruitment callbacks, including serialized setup.
public sealed class MilitarySelectionDesk:MonoBehaviour
{
    static readonly string[] Names={"Infantry","Archers","Cavalry","Varangian Guard","Cannons","Warships"};
    static readonly string[] Keys={"iInfantry","iArchers","iCavalry","iVarangians","iCannons","iShips"};
    Button[] recruitment,upgrades;Transform pane;ManagerUI manager;int selected;
    public static void Apply(GameObject root,ManagerUI owner)
    {
        var view=root.GetComponent<MilitarySelectionDesk>()??root.AddComponent<MilitarySelectionDesk>();view.manager=owner;
        if(view.pane!=null)DestroyImmediate(view.pane.gameObject);
        view.recruitment=root.GetComponentsInChildren<Button>(true).Where(b=>new[]{"Train","Build"}.Contains(b.GetComponentInChildren<TMP_Text>()?.text)).OrderByDescending(b=>b.transform.position.y).ToArray();
        view.upgrades=root.GetComponentsInChildren<Button>(true).Where(b=>b.GetComponentInChildren<TMP_Text>()?.text=="Upgrades").OrderByDescending(b=>b.transform.position.y).ToArray();
        if(view.recruitment.Length!=6)return;
        foreach(Transform child in root.transform)if(child.name!="Scene artwork")child.gameObject.SetActive(false);
        view.pane=CampaignHUD.Panel(root.transform,"Military selection desk",new Vector2(.48f,.09f),new Vector2(.98f,.94f)).transform;
        ImperialTheme.Panel(view.pane.gameObject,false);view.Draw();
    }
    void Draw()
    {
        foreach(Transform child in pane){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        Text("MILITARY & RECRUITMENT",.04f,.91f,.96f,.99f,28);
        for(int i=0;i<6;i++){int unit=i;float top=.90f-i*.08f;
            var button=CampaignHUD.MakeButton(pane,Names[i]+"     "+CouncilActions.Int(Keys[i]).ToString("N0"),new Vector2(.04f,top-.069f),new Vector2(.96f,top),()=>{selected=unit;Draw();});
            if(i==selected){var colours=button.colors;colours.normalColor=new Color(1f,.78f,.36f);button.colors=colours;}
            button.name="Select military unit "+i;
        }
        Text(Names[selected],.04f,.35f,.96f,.42f,27);
        var combat=FindObjectOfType<AssaultSystem>(true);
        Text(selected<3?"Lethality ×"+TroopTraining.Lethality(selected).ToString("0.##"):selected==5?"Manage missions, harbour defence and shipyards in Fleet management.":"Lethality ×"+(selected==3?combat.GuardLethality:combat.CannonLethality).ToString("0.##"),.04f,.255f,.96f,.35f,21);
        CampaignHUD.MakeButton(pane,selected<4?"Train "+Names[selected]:"Build "+Names[selected],new Vector2(.04f,.18f),new Vector2(.48f,.25f),()=>recruitment[selected].onClick.Invoke());
        CampaignHUD.MakeButton(pane,"Upgrades",new Vector2(.52f,.18f),new Vector2(.96f,.25f),()=>{if(upgrades.Length>selected)upgrades[selected].onClick.Invoke();});
        CampaignHUD.MakeButton(pane,"Lethality & unit roles",new Vector2(.04f,.105f),new Vector2(.48f,.17f),()=>CampaignHUD.Instance.ShowReport("Lethality and unit roles",ManagerUI.MilitaryExplanation,()=>CampaignHUD.Instance.DismissReport()));
        CampaignHUD.MakeButton(pane,selected==5?"Fleet management":"Recruitment logistics",new Vector2(.52f,.105f),new Vector2(.96f,.17f),()=>{gameObject.SetActive(false);if(selected==5)CouncilUI.For(manager).Fleet();else CouncilUI.For(manager).Training();});
        CampaignHUD.MakeButton(pane,"Specialists",new Vector2(.04f,.025f),new Vector2(.48f,.095f),()=>{gameObject.SetActive(false);CouncilUI.For(manager).Specialists();});
        CampaignHUD.MakeButton(pane,"Return",new Vector2(.52f,.025f),new Vector2(.96f,.095f),()=>ControlSingleton.Instance.GoBack());
    }
    void Text(string s,float x,float y,float right,float top,int size){var t=CampaignHUD.Label(pane,s,new Vector2(x,y),new Vector2(right,top),size);t.enableAutoSizing=true;t.fontSizeMin=17;t.fontSizeMax=size;t.alignment=TextAlignmentOptions.MidlineLeft;}
}
