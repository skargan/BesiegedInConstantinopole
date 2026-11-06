using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Purchase presentation only. SupplyEconomy owns validation and the saved turn allowance.
public partial class CouncilUI
{
    readonly int[] marketQuantities = {100,10,1};
    int marketStep = 1;

    public void Market()
    {
        Open("Treasury and market", "");
        // Keep the authored full-screen artwork; replace the generic scroll layout with one compact desk.
        foreach(Transform child in screen.transform)
            if(child.name!="Scene artwork" && child.name!="Imperial goldwork")child.gameObject.SetActive(false);
        var panel=CampaignHUD.Panel(screen.transform,"Market desk",new Vector2(.515f,.025f),new Vector2(.985f,.975f));
        ImperialTheme.Panel(panel,false);
        MarketText(panel.transform,"TREASURY AND MARKET",new Vector2(.04f,.93f),new Vector2(.96f,.985f),29);
        MarketText(panel.transform,"Daily food consumption: "+SharedProvisions.Format(SharedProvisions.CityDemand)+" provisions · Food remaining: "+SupplyEconomy.DaysLeft.ToString("N0")+" days",new Vector2(.04f,.845f),new Vector2(.96f,.93f),21);
        var stepPanel=CampaignHUD.Panel(panel.transform,"Market quantity step",new Vector2(.025f,.78f),new Vector2(.975f,.84f));
        ImperialTheme.Panel(stepPanel,false);
        MarketText(stepPanel.transform,"Quantity step",new Vector2(.025f,.09f),new Vector2(.42f,.91f),21);
        int[] steps={1,10,100};
        for(int i=0;i<steps.Length;i++) {
            int step=steps[i];float x=.44f+i*.18f;
            var button=CampaignHUD.MakeButton(stepPanel.transform,step.ToString(),new Vector2(x,.09f),new Vector2(x+.16f,.91f),()=>{marketStep=step;Market();});
            button.gameObject.name="Market step "+step;
            button.interactable=marketStep!=step;
            MarketButtonText(button,21);
        }
        string[] resources={"iProvisions","iMaterials","iAmmunition"},names={"Provisions","Materials","Ammunition"};
        for(int i=0;i<resources.Length;i++) {
            int index=i;string resource=resources[i];float top=.77f-i*.185f;
            var row=CampaignHUD.Panel(panel.transform,"Market "+resource,new Vector2(.025f,top-.175f),new Vector2(.975f,top));
            ImperialTheme.Panel(row,false);row.GetComponent<Image>().color=ImperialTheme.Surface;
            MarketText(row.transform,names[i]+"  "+(resource==SharedProvisions.City?SharedProvisions.Format(SharedProvisions.Stock(resource)):CouncilActions.Int(resource).ToString("N0")),new Vector2(.025f,.72f),new Vector2(.975f,.985f),26);
            string period=VariableSingleton.HasSiegeStarted()?"today":"this month";
            MarketText(row.transform,SupplyEconomy.Price(resource)+" gold each  ·  Available "+period+": "+MarketPurchaseLimits.Remaining(resource).ToString("N0")+" / "+MarketPurchaseLimits.Limit(resource).ToString("N0"),new Vector2(.025f,.49f),new Vector2(.975f,.72f),20);

            var fieldObject=CampaignHUD.Panel(row.transform,"Market quantity "+resource,new Vector2(.12f,.08f),new Vector2(.42f,.46f));
            ImperialTheme.Panel(fieldObject,false);
            var input=fieldObject.AddComponent<TMP_InputField>();
            var value=MarketText(fieldObject.transform,"",new Vector2(.04f,.06f),new Vector2(.96f,.94f),25);
            value.alignment=TextAlignmentOptions.Center;value.raycastTarget=false;
            input.textViewport=fieldObject.GetComponent<RectTransform>();input.textComponent=(TextMeshProUGUI)value;
            input.targetGraphic=fieldObject.GetComponent<Image>();input.contentType=TMP_InputField.ContentType.IntegerNumber;
            input.characterValidation=TMP_InputField.CharacterValidation.Digit;input.characterLimit=10;
            input.customCaretColor=true;input.caretColor=ImperialTheme.Ivory;input.selectionColor=new Color(.45f,.35f,.15f,.75f);
            input.text=marketQuantities[index].ToString();

            var buy=CampaignHUD.MakeButton(row.transform,"Buy",new Vector2(.55f,.08f),new Vector2(.975f,.46f),()=>{
                if(SupplyEconomy.Buy(resource,marketQuantities[index],out var report))VariableSingleton.Instance.Save();
                Market();notice.text=report;
            });
            buy.gameObject.name="Market buy "+resource;MarketButtonText(buy,22);
            Action refresh=()=>{
                bool valid=int.TryParse(input.text,out int quantity) && quantity>0;
                marketQuantities[index]=valid?quantity:0;
                buy.interactable=valid && SupplyEconomy.PurchaseUnavailable(resource,quantity)==null;
                buy.GetComponentInChildren<TMP_Text>(true).text=valid?"Buy · "+SupplyEconomy.Cost(resource,quantity).ToString("N0")+" gold":"Buy";
            };
            input.onValueChanged.AddListener(_=>refresh());
            var less=CampaignHUD.MakeButton(row.transform,"−",new Vector2(.025f,.08f),new Vector2(.105f,.46f),()=>input.text=Math.Max(1L,(long)marketQuantities[index]-marketStep).ToString());
            less.gameObject.name="Market decrease "+resource;MarketButtonText(less,25);
            var more=CampaignHUD.MakeButton(row.transform,"+",new Vector2(.435f,.08f),new Vector2(.515f,.46f),()=>input.text=Math.Min(int.MaxValue,Math.Max(0L,(long)marketQuantities[index])+marketStep).ToString());
            more.gameObject.name="Market increase "+resource;MarketButtonText(more,25);
            refresh();
        }
        MarketText(panel.transform,"Emergency levy\n+"+(SupplyEconomy.DailyNeed*100).ToString("N0")+" gold · Public morale −15",new Vector2(.04f,.14f),new Vector2(.59f,.22f),21);
        var levy=CampaignHUD.MakeTurnButton(panel.transform,"Collect tax",new Vector2(.62f,.145f),new Vector2(.96f,.215f),()=>{SupplyEconomy.EmergencyTax(out var report);Finish(true,report);});
        levy.gameObject.name="Market emergency levy";MarketButtonText(levy,22);
        notice.transform.SetParent(panel.transform,false);notice.gameObject.SetActive(true);
        var noticeRect=notice.rectTransform;noticeRect.anchorMin=new Vector2(.04f,.082f);noticeRect.anchorMax=new Vector2(.96f,.135f);noticeRect.offsetMin=noticeRect.offsetMax=Vector2.zero;
        notice.fontSize=19;notice.fontSizeMin=16;notice.fontSizeMax=19;notice.enableAutoSizing=true;notice.alignment=TextAlignmentOptions.Center;notice.text="";
        var back=CampaignHUD.MakeButton(panel.transform,"Return",new Vector2(.20f,.02f),new Vector2(.80f,.075f),()=>ControlSingleton.Instance.GoBack());
        back.gameObject.name="Market return";MarketButtonText(back,23);
        MarketMerchant.Attach(screen);
    }

    static TMP_Text MarketText(Transform parent,string text,Vector2 min,Vector2 max,int size)
    {
        var label=CampaignHUD.Label(parent,text,min,max,size);
        label.fontSizeMax=size;label.fontSizeMin=17;label.enableAutoSizing=true;
        label.overflowMode=TextOverflowModes.Truncate;label.alignment=TextAlignmentOptions.MidlineLeft;
        return label;
    }
    static void MarketButtonText(Button button,int size)
    {
        var label=button.GetComponentInChildren<TMP_Text>(true);
        label.fontSizeMax=size;label.fontSizeMin=17;label.enableAutoSizing=true;
    }
}
