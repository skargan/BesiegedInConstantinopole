using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public partial class CouncilUI
{
 static readonly string[] OrderKeys={"iInfantry","iArchers","iCavalry","iVarangians","iCannons","iShips"};
 public void RecruitmentOrder(int unit,Action back)
 {
  string key=OrderKeys[unit];var item=VariableSingleton.Instance.RecruitmentPricelist.First(x=>x.GetVariableName()==key);int price=TrainingPool.UnitPrice(key,item.GetCost());int quantity=1;
  var pane=MilitaryCanvas((unit>=4?"Build ":"Train ")+TrainingPool.Name(key),MilitarySelectionDesk.Art[unit],"");screen.name="Military order screen";screen.transform.Find("Scene heading").gameObject.SetActive(false);ControlSingleton.Instance.ReturnAction=()=>{Hide();back();};
  CompactText(pane,TrainingPool.Name(key),.05f,.87f,.95f,.98f,30);
  CompactText(pane,price.ToString("N0")+" gold each Â· "+TrainingPool.Duration(key)+" days"+(key=="iVarangians"?"\n"+CourtServiceBenefits.GuardAvailability:"")+"\nReady "+VariableSingleton.GetDate().AddDays(TrainingPool.Duration(key)).ToString("d MMM yyyy"),.05f,.72f,.95f,.87f,23);
  var count=CompactText(pane,"",.05f,.58f,.95f,.72f,32);count.alignment=TextAlignmentOptions.Center;
  var terms=CompactText(pane,"",.05f,.20f,.95f,.47f,22);Button confirm=null;
  Action refresh=()=>{quantity=Mathf.Clamp(quantity,0,TrainingPool.MaximumOrder(key,price));count.text="Quantity Â· "+quantity.ToString("N0");bool allowed=TrainingPool.CanPlace(key,quantity,price,out var reason);terms.text="Total: "+((long)quantity*price).ToString("N0")+" gold\nAvailable manpower: "+RecruitmentLogistics.AvailableManpower.ToString("N0")+(unit>=4?"\nMaterials: "+quantity*SpecialistPersonnel.MaterialsPerUnit(key)+" Â· Engineers: "+quantity*SpecialistPersonnel.EngineersPerUnit(key)+" Â· Crew: "+quantity*SpecialistPersonnel.OrdinaryManpowerPerUnit(key):"")+(allowed?"":"\n"+reason);if(confirm!=null)confirm.interactable=allowed;};
  var slider=ImperialAllocationSlider.Create(pane,quantity,TrainingPool.MaximumOrder(key,price),v=>{quantity=Mathf.RoundToInt(v);refresh();});
  var sliderRect=slider.GetComponent<RectTransform>();sliderRect.anchorMin=new Vector2(.16f,.49f);sliderRect.anchorMax=new Vector2(.84f,.56f);
  sliderRect.offsetMin=sliderRect.offsetMax=Vector2.zero;
  CampaignHUD.MakeButton(pane,"−",new Vector2(.05f,.49f),new Vector2(.14f,.57f),()=>{slider.value=quantity-1;});
  CampaignHUD.MakeButton(pane,"+",new Vector2(.86f,.49f),new Vector2(.95f,.57f),()=>{slider.value=quantity+1;});

  bool placed=false;confirm=CampaignHUD.MakeButton(pane,"Confirm order",new Vector2(.05f,.105f),new Vector2(.95f,.185f),()=>{if(placed)return;if(!TrainingPool.Place(key,quantity,price)){refresh();return;}placed=true;SaveGames.Checkpoint();Hide();back();CampaignHUD.Instance.ShowReport("Order commissioned",quantity+" "+TrainingPool.Name(key)+" Â· "+((long)quantity*price).ToString("N0")+" gold.\nReady "+VariableSingleton.GetDate().AddDays(TrainingPool.Duration(key)).ToString("d MMM yyyy"),()=>CampaignHUD.Instance.DismissReport());});
  CampaignHUD.MakeButton(pane,"Cancel",new Vector2(.25f,.025f),new Vector2(.75f,.09f),()=>ControlSingleton.Instance.GoBack());refresh();
 }
 public void Shipyard(Action back=null)
 {
  Action previous=()=>MilitarySelectionDesk.ShowUnit(manager,4);var pane=MilitaryCanvas("The Imperial Shipyard","shipyard","");screen.name="Shipyard screen";ControlSingleton.Instance.ReturnAction=()=>{Hide();previous();};
  CompactText(pane,"THE IMPERIAL SHIPYARD",.05f,.89f,.95f,.985f,28);
  CompactText(pane,CouncilActions.Int("iShips")+" warships in service",.05f,.80f,.95f,.89f,24);
  CampaignHUD.MakeButton(pane,"Build warships",new Vector2(.05f,.68f),new Vector2(.95f,.79f),()=>RecruitmentOrder(5,()=>Shipyard(previous)));
  CompactText(pane,"ORDERS IN PROGRESS",.05f,.58f,.95f,.67f,23);
  var lines=TrainingPool.Orders.Where(o=>o.unit=="iShips").Select(o=>o.quantity+" warships Â· "+(o.kind==TrainingPool.ShipRepair?"repairing":"building")+"\nReady "+TrainingPool.ReadyDate(o).ToString("d MMM yyyy")+" Â· "+TrainingPool.Remaining(o)+" days").ToArray();
  CompactText(pane,lines.Length==0?"No shipyard orders in progress.":string.Join("\n\n",lines.Take(4)),.05f,.19f,.95f,.58f,22);
  CampaignHUD.MakeButton(pane,"All orders & recruitment logistics",new Vector2(.05f,.105f),new Vector2(.95f,.18f),()=>{Training();ControlSingleton.Instance.ReturnAction=()=>Shipyard(previous);});
  CampaignHUD.MakeButton(pane,"Return",new Vector2(.25f,.025f),new Vector2(.75f,.09f),()=>ControlSingleton.Instance.GoBack());
 }
}
