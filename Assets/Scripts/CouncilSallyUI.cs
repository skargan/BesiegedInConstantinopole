using UnityEngine;
public partial class CouncilUI {
 void FinishSally(bool ok,string report){
  if(ok&&SallyOut.LastInterception!=null){CampaignHUD.Instance.ShowReport("Cut off from the gates",SallyOut.LastInterception+"\n\n"+report,()=>{CampaignHUD.Instance.DismissReport();Finish(true,report);});}
  else Finish(ok,report);
 }

 void SallyOrders(int count=1,bool lead=false){
  Open("Sally out","Choose a cavalry contingent and its objective.");BodyHeight(750);
  int available=CouncilActions.Int("iCavalry");count=Mathf.Clamp(count,1,Mathf.Max(1,available));
  var row=Row(0,90);CampaignHUD.Label(row.transform,"Cavalry: "+count+" / "+available+" · Cut-off risk: "+SallyOut.CutOffChance(count)+"%",new Vector2(.03f,.1f),new Vector2(.7f,.9f),23);
  CampaignHUD.MakeButton(row.transform,"−",new Vector2(.72f,.1f),new Vector2(.83f,.9f),()=>SallyOrders(Mathf.Max(1,count-50),lead));
  CampaignHUD.MakeButton(row.transform,"+",new Vector2(.85f,.1f),new Vector2(.97f,.9f),()=>SallyOrders(Mathf.Min(available,count+50),lead));
  for(int i=0;i<6;i++){int mission=i;var r=Row(i+1,90);var b=CampaignHUD.MakeTurnButton(r.transform,SallyOut.Missions[i],new Vector2(.03f,.1f),new Vector2(.97f,.9f),()=>{
   if(mission==5){Open("Strike at Mehmed","His position is protected by 15,000 troops. Defeat them to reach him.");BodyHeight(240);for(int j=0;j<2;j++){bool personal=j==1;var choice=Row(j,110);CampaignHUD.MakeTurnButton(choice.transform,personal?"Lead the charge personally":"Send the contingent",new Vector2(.03f,.1f),new Vector2(.97f,.9f),()=>{string report;FinishSally(SallyOut.Resolve(count,mission,personal,out report),report);});}}
   else {string report;FinishSally(SallyOut.Resolve(count,mission,false,out report),report);}
  });b.interactable=available>0&&VariableSingleton.HasSiegeStarted()&&!SaveGames.Terminal;}
 }
}
