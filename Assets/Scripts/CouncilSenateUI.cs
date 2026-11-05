using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A view over existing civic measures; selecting a delegation never spends resources.
public partial class CouncilUI
{
    int senateSelection;
    static Sprite senatePainting;
    static readonly string[] SenateGroups={"Officers","Townspeople","Noble households","Clergy","Galata delegation"};
    void ShowSenate()
    {
        Open("The Senate", "");
        foreach(Transform child in screen.transform) child.gameObject.SetActive(false);
        ControlSingleton.Instance.ReturnAction=()=>{Hide();manager.gameObject.SetActive(true);};
        var region=CampaignHUD.Panel(screen.transform,"Senate scene",Vector2.zero,Vector2.one);
        region.GetComponent<Image>().color=ImperialTheme.Navy;
        var painting=CampaignHUD.Panel(region.transform,"Senate painting",Vector2.zero,Vector2.one);
        var texture=Resources.Load<Texture2D>("UI/MidnightMosaic/Scenes/senate-landmarks");
        var image=painting.GetComponent<Image>();image.raycastTarget=false;
        if(texture!=null){if(senatePainting==null)senatePainting=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));image.sprite=senatePainting;image.color=Color.white;var fit=painting.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=(float)texture.width/texture.height;}
        // Coordinates are attached to the fitted artwork, so letterboxing cannot move targets.
        for(int i=0;i<5;i++){
            int choice=i;
            var group=LocationHotspot.Create(painting.transform,SenateGroups[i],SenateHotspots.Groups[i],()=>{senateSelection=choice;ShowSenate();});
            if(i==4){var flag=LocationHotspot.Create(painting.transform,"Galata delegation",SenateHotspots.Flag,()=>{senateSelection=4;ShowSenate();});var peers=new[]{group,flag};group.OutlinePeers=peers;flag.OutlinePeers=peers;}
        }
        LocationHotspot.Create(painting.transform,"Research tower",SenateHotspots.Tower,()=>{Technologies();ControlSingleton.Instance.ReturnAction=ShowSenate;});
        LocationHotspot.Create(painting.transform,"Shipyard",SenateHotspots.Shipyard,()=>Shipyard(ShowSenate));
        LocationHotspot.Create(painting.transform,"Hagia Sophia",SenateHotspots.Cathedral,()=>{HagiaSophia();ControlSingleton.Instance.ReturnAction=ShowSenate;});
        // Later sibling wins this small overlap, leaving the rest of the envoy selectable.
        LocationHotspot.Create(painting.transform,"Bribe the podestà",SenateHotspots.Hand,()=>{
            GalataMenu(0);ControlSingleton.Instance.ReturnAction=ShowSenate;
            var back=screen.transform.Find("Galata desk/Galata return").GetComponent<Button>();
            back.onClick.RemoveAllListeners();back.onClick.AddListener(ShowSenate);
            back.GetComponentInChildren<TMP_Text>().text="Return to Senate";
        });
        var header=CampaignHUD.Panel(screen.transform,"Senate heading",new Vector2(0,.885f),Vector2.one);header.GetComponent<Image>().color=new Color(.012f,.035f,.052f,.94f);
        var title=MedicalText(screen.transform,"THE SENATE",new Vector2(.20f,.94f),new Vector2(.80f,.995f),30,TextAlignmentOptions.Center);title.name="Senate title";
        var back=CampaignHUD.MakeButton(screen.transform,"Return",new Vector2(.83f,.94f),new Vector2(.98f,.995f),()=>ControlSingleton.Instance.GoBack());ImperialTheme.Button(back);
        for(int i=0;i<5;i++){
            int choice=i;float x=.02f+i*.194f;
            var button=CampaignHUD.MakeButton(screen.transform,CouncilActions.FactionNames[i]+" · "+VariableSingleton.GetPercentFloat(CouncilActions.FactionKeys[i]),new Vector2(x,.89f),new Vector2(x+.188f,.935f),()=>{senateSelection=choice;ShowSenate();});
            button.name="Senate delegation "+i;ImperialTheme.Button(button);
            var label=button.GetComponentInChildren<TMP_Text>();label.fontSize=21;label.enableAutoSizing=true;label.fontSizeMin=16;label.fontSizeMax=21;label.margin=new Vector4(4,0,4,0);
            if(i==senateSelection)label.color=ImperialTheme.Gold;
            CampaignChoiceTooltip.Attach(button,CouncilActions.FactionDescriptions[i]);
        }
        var panel=CampaignHUD.Panel(screen.transform,"Senate selected delegation",new Vector2(.02f,.015f),new Vector2(.98f,.16f));ImperialTheme.Panel(panel,false);
        int selected=Mathf.Clamp(senateSelection,0,4);
        MedicalText(panel.transform,SenateGroups[selected],new Vector2(.02f,.67f),new Vector2(.61f,.96f),25);
        MedicalText(panel.transform,CouncilActions.FactionDescriptions[selected],new Vector2(.02f,.08f),new Vector2(.60f,.65f),21);
        float support=VariableSingleton.GetFloatVariable(CouncilActions.FactionKeys[selected]);
        string gain=Mathf.Max(0,Mathf.Min(3,(1-support)*100)).ToString("0.#");
        var action=CampaignHUD.MakeButton(panel.transform,(selected==4?"Send a gift":CouncilActions.FactionOrders[selected])+"\n300 gold · +"+gain+" points",new Vector2(.63f,selected==4?.46f:.17f),new Vector2(.98f,.93f),()=>ReviewFaction(selected));
        if(selected==4){var giftLabel=action.GetComponentInChildren<TMP_Text>();giftLabel.text="Send gift · 300 gold · +"+gain+" points";giftLabel.enableAutoSizing=true;giftLabel.fontSizeMax=21;giftLabel.fontSizeMin=15;giftLabel.enableWordWrapping=false;}
        action.name="Senate review action";action.interactable=CouncilActions.Int("iMoney")>=300&&support<1;ImperialTheme.Button(action);
        if(selected==4){var diplomacy=CampaignHUD.MakeButton(panel.transform,"Galata relations",new Vector2(.63f,.06f),new Vector2(.98f,.40f),Galata);ImperialTheme.Button(diplomacy);}
    }
}
