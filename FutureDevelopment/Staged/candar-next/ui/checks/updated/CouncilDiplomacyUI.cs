using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class CouncilUI
{
    int diplomacyPage;
    enum DiplomaticOrder { RequestAid, Expedite, Dispatch, Recall, Gift, RenewAid, War, RenewWar }
    static readonly Color[] AttitudeColours={new Color(.79f,.25f,.23f),new Color(.78f,.44f,.24f),new Color(.74f,.67f,.43f),new Color(.40f,.70f,.49f),new Color(.26f,.79f,.59f)};

    Transform DiplomacyPage(string title,string introduction,Action back)
    {
        Open(title,introduction);ControlSingleton.Instance.ReturnAction=back;
        var scroll=body.parent.GetComponent<ScrollRect>();
        if(scroll.verticalScrollbar!=null)scroll.verticalScrollbar.gameObject.SetActive(false);
        body.parent.gameObject.SetActive(false);
        foreach(var label in screen.GetComponentsInChildren<TMP_Text>())if(label.transform.parent==screen.transform&&label.text==title)label.rectTransform.anchorMax=new Vector2(.72f,.98f);
        var count=CampaignHUD.Label(screen.transform,"Envoys: "+CountryDiplomacy.AvailableEnvoys+" / "+CountryDiplomacy.TotalEnvoys,new Vector2(.74f,.91f),new Vector2(.965f,.975f),24);
        count.alignment=TextAlignmentOptions.Right;
        var page=CampaignHUD.Panel(screen.transform,"Diplomatic page",new Vector2(.035f,.115f),new Vector2(.965f,.795f));
        page.GetComponent<Image>().color=Color.clear;page.GetComponent<Image>().raycastTarget=false;return page.transform;
    }
    TMP_Text DiplomacyText(Transform parent,string text,Vector2 min,Vector2 max,float size=22)
    {
        var label=CampaignHUD.Label(parent,text,min,max,size);label.enableAutoSizing=true;label.fontSizeMax=size;label.fontSizeMin=Mathf.Max(16,size-3);return label;
    }
    void DiplomacyFlag(Transform parent,int index,Vector2 min,Vector2 max)=>DiplomaticArtwork.DrawFlag(parent,CountryDiplomacy.Definitions[index].Id,min,max);
    void DiplomacyAttitude(Transform parent,int index,Vector2 min,Vector2 max)=>DiplomaticArtwork.DrawAttitude(parent,CountryDiplomacy.Attitude(index).ToString(),min,max);
    string AidStanding(int index)
    {
        int state=DailyLifeEvents.I("iAidState"+index);
        if(state==1)return "Aid due in "+DailyLifeEvents.I("iAidDays"+index)+" days";
        if(CountryDiplomacy.RenewalReady(index,false))return "Renewed aid request available";
        if(CountryDiplomacy.RenewalDays(index,false)>0)return "Aid renewal: "+CountryDiplomacy.RenewalDays(index,false)+" resident days remain";
        return state==2?"Aid delivered":state==3?"Aid appeal declined":state==4?"Relief expedition lost":"Initial aid request available";
    }
    void UnifiedDiplomaticRelations()
    {
        CountryDiplomacy.Ensure();int total=CountryDiplomacy.Definitions.Length+1,perPage=8,pages=(total+perPage-1)/perPage;
        diplomacyPage=Mathf.Clamp(diplomacyPage,0,pages-1);
        var stage=DiplomacyPage("Diplomatic relations","Choose a court. Aid appeals need no envoy; resident envoys open deeper negotiations.",Diplomacy);
        for(int n=0;n<perPage&&diplomacyPage*perPage+n<total;n++) {
            int index=diplomacyPage*perPage+n;float left=(n%2)*.505f,top=1-(n/2)*.222f;
            if(index==CountryDiplomacy.Definitions.Length) {
                var special=CandarPlainButton(stage,"Candar · İsmail Bey\n"+(CandarDiplomacy.Unlocked?"Secret negotiations":"Hostile · a guarded door"),new Vector2(left,top-.207f),new Vector2(left+.495f,top),CandarCourt);
                special.gameObject.name="Diplomatic court candar";continue;
            }
            var tile=CampaignHUD.MakeButton(stage,"",new Vector2(left,top-.207f),new Vector2(left+.495f,top),()=>DiplomaticPower(index));
            tile.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);ImperialTheme.Button(tile);
            tile.gameObject.name="Diplomatic court "+CountryDiplomacy.Definitions[index].Id;
            DiplomacyFlag(tile.transform,index,new Vector2(.025f,.50f),new Vector2(.145f,.94f));
            var courtName=DiplomacyText(tile.transform,CountryDiplomacy.Definitions[index].Name,new Vector2(.175f,.65f),new Vector2(.96f,.95f),25);courtName.gameObject.name="Court name";
            DiplomacyAttitude(tile.transform,index,new Vector2(.175f,.405f),new Vector2(.215f,.64f));
            var attitude=DiplomacyText(tile.transform,CountryDiplomacy.Attitude(index)+" · "+CountryDiplomacy.FinalChance(index).ToString("0.##")+"% acceptance",new Vector2(.225f,.405f),new Vector2(.96f,.64f),20);
            attitude.color=AttitudeColours[(int)CountryDiplomacy.Attitude(index)];attitude.gameObject.name="Court attitude text";
            var standing=DiplomacyText(tile.transform,CountryDiplomacy.EnvoyStatus(index)+"\n"+AidStanding(index),new Vector2(.035f,.03f),new Vector2(.96f,.39f),18);standing.gameObject.name="Court commitments";
        }
        var previous=CampaignHUD.MakeButton(stage,"‹ Previous courts",new Vector2(0,0),new Vector2(.27f,.09f),()=>{diplomacyPage--;UnifiedDiplomaticRelations();});previous.interactable=diplomacyPage>0;
        var pageLabel=DiplomacyText(stage,"Courts "+(diplomacyPage*perPage+1)+"–"+Mathf.Min(total,(diplomacyPage+1)*perPage)+" of "+total,new Vector2(.3f,0),new Vector2(.7f,.09f),20);pageLabel.alignment=TextAlignmentOptions.Center;
        var next=CampaignHUD.MakeButton(stage,"Next courts ›",new Vector2(.73f,0),new Vector2(1,.09f),()=>{diplomacyPage++;UnifiedDiplomaticRelations();});next.interactable=diplomacyPage<pages-1;
    }
    void UnifiedDiplomaticPower(int index)
    {
        if(index<0||index>=CountryDiplomacy.Definitions.Length){DiplomaticRelations();return;}
        var d=CountryDiplomacy.Definitions[index];var ally=CouncilActions.Allies[index];
        var stage=DiplomacyPage(d.Name,DiplomaticChronicles.All[index].Religion+" · "+CountryDiplomacy.EnvoyStatus(index),DiplomaticRelations);
        var heading=CampaignHUD.Panel(stage,"Relationship",new Vector2(0,.73f),Vector2.one);ImperialTheme.Panel(heading,false);
        DiplomacyFlag(heading.transform,index,new Vector2(.02f,.17f),new Vector2(.17f,.90f));
        DiplomacyAttitude(heading.transform,index,new Vector2(.19f,.60f),new Vector2(.24f,.89f));
        var relationship=DiplomacyText(heading.transform,CountryDiplomacy.Attitude(index)+" · "+CountryDiplomacy.FinalChance(index).ToString("0.##")+"% acceptance",new Vector2(.255f,.57f),new Vector2(.97f,.94f),27);relationship.color=AttitudeColours[(int)CountryDiplomacy.Attitude(index)];relationship.gameObject.name="Court attitude text";
        var breakdown=DiplomacyText(heading.transform,CountryDiplomacy.ChanceBreakdown(index),new Vector2(.19f,.08f),new Vector2(.97f,.56f),19);breakdown.gameObject.name="Court acceptance breakdown";
        var cargo=CampaignHUD.Panel(stage,"Aid and commitments",new Vector2(0,.40f),new Vector2(1,.705f));ImperialTheme.Panel(cargo,false);
        DiplomacyText(cargo.transform,AidStanding(index)+" · "+CountryDiplomacy.WarStatus(index),new Vector2(.025f,.72f),new Vector2(.975f,.96f),24);
        string delivery=DiplomaticStoryAid.DeliveredCargo(index)!=null?"Preparation aid received. A renewed appeal uses the ordinary package in its confirmation.":"Delivery: "+ally.Days+" calendar days. Transport vessels return home.";
        DiplomacyText(cargo.transform,d.Description+"\n\n"+AidRouting.Route(index)+" · "+AidRouting.Cargo(index)+"\n"+delivery,new Vector2(.025f,.06f),new Vector2(.975f,.71f),22);
        DiplomaticOrder aid=DailyLifeEvents.I("iAidState"+index)==1?DiplomaticOrder.Expedite:DiplomaticOrder.RequestAid;
        DiplomacyOrderButton(stage,index,aid,new Vector2(0,.18f),new Vector2(.48f,.36f));
        bool assigned=CountryDiplomacy.EnvoyState(index)!=CountryDiplomacy.EnvoyAssignment.None;
        DiplomacyOrderButton(stage,index,assigned?DiplomaticOrder.Recall:DiplomaticOrder.Dispatch,new Vector2(.52f,.18f),new Vector2(1,.36f));
        CampaignHUD.MakeButton(stage,"The realm and its interests",new Vector2(0,0),new Vector2(.48f,.13f),()=>{Chronicle(index);ControlSingleton.Instance.ReturnAction=()=>DiplomaticPower(index);});
        var advanced=CampaignHUD.MakeButton(stage,"Advanced diplomacy",new Vector2(.52f,0),new Vector2(1,.13f),()=>AdvancedDiplomacy(index));
        advanced.interactable=CountryDiplomacy.EnvoyState(index)==CountryDiplomacy.EnvoyAssignment.Resident;
        if(!advanced.interactable)advanced.GetComponentInChildren<TMP_Text>().text="Advanced diplomacy · envoy must be resident";
    }
    void AdvancedDiplomacy(int index)
    {
        var d=CountryDiplomacy.Definitions[index];
        var stage=DiplomacyPage(d.Name+" · Advanced diplomacy",CountryDiplomacy.Attitude(index)+" · "+CountryDiplomacy.FinalChance(index).ToString("0.##")+"% acceptance · "+CountryDiplomacy.EnvoyStatus(index),()=>DiplomaticPower(index));
        DiplomaticOrder[] orders={DiplomaticOrder.Gift,DiplomaticOrder.RenewAid,DiplomaticOrder.War,DiplomaticOrder.RenewWar};
        for(int n=0;n<orders.Length;n++) {
            int item=n;var order=orders[n];float x=n%2*.51f,top=1-n/2*.505f;
            var card=CampaignHUD.Panel(stage,OrderTitle(order),new Vector2(x,top-.485f),new Vector2(x+.49f,top));ImperialTheme.Panel(card,false);
            DiplomacyText(card.transform,OrderTitle(order),new Vector2(.04f,.78f),new Vector2(.96f,.96f),27);
            DiplomacyText(card.transform,OrderTerms(index,order),new Vector2(.04f,.25f),new Vector2(.96f,.76f),21);
            string reason=OrderReason(index,order);
            var review=CampaignHUD.MakeButton(card.transform,reason??"Review this action",new Vector2(.04f,.04f),new Vector2(.96f,.23f),()=>ReviewDiplomaticOrder(index,orders[item]));review.interactable=reason==null;
            var label=review.GetComponentInChildren<TMP_Text>();label.enableAutoSizing=true;label.fontSizeMin=16;label.fontSizeMax=21;
        }
    }
    static string OrderTitle(DiplomaticOrder order)
    {
        switch(order) {
            case DiplomaticOrder.RequestAid:return "Request aid";case DiplomaticOrder.Expedite:return "Expedite aid";
            case DiplomaticOrder.Dispatch:return "Send an envoy";case DiplomaticOrder.Recall:return "Recall the envoy";
            case DiplomaticOrder.Gift:return "Generous gifts";case DiplomaticOrder.RenewAid:return "Renew the aid appeal";
            case DiplomaticOrder.War:return "Request war against the Ottomans";default:return "Renew the war appeal";
        }
    }
    string OrderReason(int i,DiplomaticOrder order)
    {
        switch(order) {
            case DiplomaticOrder.RequestAid:return CountryDiplomacy.AidRequestReason(i);
            case DiplomaticOrder.Expedite:return DailyLifeEvents.I("iAidState"+i)!=1||DailyLifeEvents.I("iAidDays"+i)<=1?"No expedition that can arrive sooner":DailyLifeEvents.I("iMoney")<1000?"Requires 1,000 gold":null;
            case DiplomaticOrder.Dispatch:return CountryDiplomacy.DispatchReason(i);case DiplomaticOrder.Recall:return CountryDiplomacy.RecallReason(i);
            case DiplomaticOrder.Gift:return CountryDiplomacy.GiftReason(i);case DiplomaticOrder.RenewAid:return CountryDiplomacy.RenewReason(i,false);
            case DiplomaticOrder.War:return CountryDiplomacy.WarReason(i);default:return CountryDiplomacy.RenewReason(i,true);
        }
    }
    string OrderTerms(int i,DiplomaticOrder order)
    {
        var d=CountryDiplomacy.Definitions[i];
        switch(order) {
            case DiplomaticOrder.RequestAid:return "1,000 gold · "+CountryDiplomacy.FinalChance(i).ToString("0.##")+"% acceptance\n"+CouncilActions.Allies[i].Cargo+"\nArrival in "+d.AidDays+" calendar days. "+(d.AidBySea?"An available ship is required, but is not consumed.":"Land route.")+"\nA refusal spends the appeal cost.";
            case DiplomaticOrder.Expedite:return "1,000 gold · Bring the committed expedition up to five calendar days closer. Earliest arrival: tomorrow.";
            case DiplomaticOrder.Dispatch:return "Assign one available envoy. Outward journey: "+d.EnvoyDays+" calendar days. Goodwill improves only while resident; gifts and deeper negotiations then become available.";
            case DiplomaticOrder.Recall:return "Return journey: "+d.EnvoyDays+" calendar days. Aid and war commitments remain. Resident work and unfinished renewal progress pause until an envoy returns to this court.";
            case DiplomaticOrder.Gift:return "3,000 gold · Goodwill +"+Mathf.Min(10,95-CountryDiplomacy.Goodwill(i)).ToString("0.##")+" (maximum 95).\n50% chance to improve attitude by one level, up to Friendly. Gifts cannot create an alliance.";
            case DiplomaticOrder.RenewAid:case DiplomaticOrder.RenewWar:
                bool war=order==DiplomaticOrder.RenewWar;int days=CountryDiplomacy.RenewalDays(i,war);
                return "1,000 gold · Ten resident days to unlock one further "+(war?"war negotiation":"aid request at its normal 1,000-gold cost")+".\nProgress pauses while the envoy is absent. "+(days>0?days+" resident days remain.":CountryDiplomacy.RenewalReady(i,war)?"Your renewed request is ready.":"This permission applies only to "+(war?"war":"aid")+".");
            default:
                if(CountryDiplomacy.WarState(i)>=CountryDiplomacy.WarCommitment.Mobilising)return CountryDiplomacy.WarStatus(i)+"\n\n"+WarInterventions.Summary(i);
                return d.WarEligible?CountryDiplomacy.FinalChance(i).ToString("0.##")+"% acceptance · No additional gold charge.\nRequires Friendly relations and at least 75% acceptance. Success creates Allied status, then "+d.MobilisationDays+" days of mobilisation. "+WarInterventions.Terms(i):d.Id=="morea"?"The brothers' decisions determine Morean cooperation. Ordinary aid and resident diplomacy remain available here.":"This power offers relief and specialised agreements rather than a generic regional war commitment.";
        }
    }
    void DiplomacyOrderButton(Transform parent,int i,DiplomaticOrder order,Vector2 min,Vector2 max)
    {
        string reason=OrderReason(i,order);
        var b=CampaignHUD.MakeButton(parent,OrderTitle(order)+(reason!=null?"\n"+reason:" · review"),min,max,()=>ReviewDiplomaticOrder(i,order));b.interactable=reason==null;
        var text=b.GetComponentInChildren<TMP_Text>();text.enableAutoSizing=true;text.fontSizeMin=17;text.fontSizeMax=24;
    }
    void ReviewDiplomaticOrder(int i,DiplomaticOrder order)
    {
        Action back=()=>{if(order>=DiplomaticOrder.Gift)AdvancedDiplomacy(i);else DiplomaticPower(i);};
        var stage=DiplomacyPage(OrderTitle(order),CountryDiplomacy.Definitions[i].Name+" · Review before committing",back);
        DiplomacyText(stage,OrderTerms(i,order)+"\n\nThis uses your major action and advances the turn.",new Vector2(.04f,.28f),new Vector2(.96f,.94f),28);
        string reason=OrderReason(i,order);bool submitted=false;
        var confirm=CampaignHUD.MakeTurnButton(stage,reason??"Confirm · "+OrderTitle(order),new Vector2(.04f,.06f),new Vector2(.65f,.22f),()=>{
            if(submitted)return;submitted=true;string report;bool ok;
            switch(order) {
                case DiplomaticOrder.RequestAid:ok=CouncilActions.RequestAid(i,out report);break;
                case DiplomaticOrder.Expedite:ok=CouncilActions.HurryAid(i,out report);break;
                case DiplomaticOrder.Dispatch:ok=CountryDiplomacy.Dispatch(i,out report);break;
                case DiplomaticOrder.Recall:ok=CountryDiplomacy.Recall(i,out report);break;
                case DiplomaticOrder.Gift:ok=CountryDiplomacy.Gift(i,out report);break;
                case DiplomaticOrder.RenewAid:ok=CountryDiplomacy.Renew(i,false,out report);break;
                case DiplomaticOrder.War:ok=CountryDiplomacy.NegotiateWar(i,out report);break;
                default:ok=CountryDiplomacy.Renew(i,true,out report);break;
            }
            if(!ok)submitted=false;Finish(ok,report);
        });confirm.interactable=reason==null;
        CampaignHUD.MakeButton(stage,"Cancel",new Vector2(.69f,.06f),new Vector2(.96f,.22f),()=>back());
    }
}

// Small scalable theatrical-mask emblem. Its text label always accompanies its colour.
public sealed class DiplomaticMaskBadge : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();Rect rect=GetPixelAdjustedRect();float size=Mathf.Min(rect.width,rect.height),x=rect.center.x,y=rect.center.y;
        AddFace(mesh,new Vector2(x-size*.11f,y+size*.09f),size*.68f,new Color(color.r*.65f,color.g*.65f,color.b*.65f,color.a));
        AddFace(mesh,new Vector2(x+size*.1f,y-size*.07f),size*.78f,color);
    }
    static void Shape(VertexHelper mesh,Vector2 centre,Vector2[] points,Color tint)
    {
        int start=mesh.currentVertCount;mesh.AddVert(centre,tint,Vector2.zero);
        foreach(var p in points)mesh.AddVert(p,tint,Vector2.zero);
        for(int j=0;j<points.Length;j++)mesh.AddTriangle(start,start+1+j,start+1+(j+1)%points.Length);
    }
    static void AddFace(VertexHelper mesh,Vector2 c,float size,Color tint)
    {
        var p=new Vector2[16];for(int n=0;n<16;n++){float a=n*Mathf.PI*2/16;p[n]=c+new Vector2(Mathf.Cos(a)*size*.39f,Mathf.Sin(a)*size*.49f);}
        Shape(mesh,c,p,tint);Color dark=new Color(.035f,.06f,.08f,tint.a);
        for(int side=-1;side<=1;side+=2){Vector2 e=c+new Vector2(side*size*.16f,size*.11f);Shape(mesh,e,new[]{e+new Vector2(-size*.10f,0),e+new Vector2(0,size*.04f),e+new Vector2(size*.10f,0),e+new Vector2(0,-size*.04f)},dark);}
        Vector2 m=c+new Vector2(0,-size*.19f);Shape(mesh,m,new[]{m+new Vector2(-size*.16f,size*.035f),m+new Vector2(size*.16f,size*.035f),m+new Vector2(size*.10f,-size*.045f),m+new Vector2(-size*.10f,-size*.045f)},dark);
    }
}
