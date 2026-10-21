using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RecruitationSelection : MonoBehaviour
{
    [SerializeField] GameObject superMenu;

    [SerializeField] TMP_Text displayName;
    [SerializeField] TMP_Text displayCost;
    [SerializeField] CostMultiplyingDisplay displayMultiplied;
    [SerializeField] ValueIncrementer costDeterminant;

    string troopVariableName;
    int troopCost;

    public void GetSelectionIndex(int newIndex)
    {
        superMenu.SetActive(true);
        ArrangeControls();

        troopVariableName = VariableSingleton.Instance.RecruitmentPricelist[newIndex].GetVariableName();
        displayName.text = VariableSingleton.Instance.RecruitmentPricelist[newIndex].GetName();

        troopCost = VariableSingleton.Instance.RecruitmentPricelist[newIndex].GetCost();
        displayCost.text = troopCost.ToString()+" gold / unit · Ready in "+TrainingPool.Duration(troopVariableName)+" days\n"+VariableSingleton.GetDate().AddDays(TrainingPool.Duration(troopVariableName)).ToString("d MMM yyyy");
        displayCost.fontStyle |= FontStyles.Italic;
        displayMultiplied.TroopCost = troopCost;

        costDeterminant.SetMaxAffordable(VariableSingleton.GetIntVariable("iMoney") / troopCost);


        LogRecruitationToEsc();
    }

    void ArrangeControls()
    {
        var panel=superMenu.GetComponent<UnityEngine.UI.Image>();
        if(panel!=null) {panel.sprite=null;panel.color=new Color(.08f,.10f,.12f,1);}
        foreach(var image in GetComponentsInChildren<UnityEngine.UI.Image>(true))
            if(image.GetComponent<UnityEngine.UI.Button>()==null && image.GetComponent<TMP_InputField>()==null)
                {image.sprite=null;image.color=new Color(.13f,.16f,.18f,1);}
        foreach(var label in GetComponentsInChildren<TMP_Text>(true)) label.color=new Color(.95f,.91f,.82f);
        var field=GetComponentInChildren<TMP_InputField>(true);
        if(field!=null && field.GetComponent<UnityEngine.UI.Image>()!=null) field.GetComponent<UnityEngine.UI.Image>().color=new Color(.07f,.09f,.11f);
        string[] order={"-100","-10","-","+","+10","+100"};
        foreach(var b in GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            var text=b.GetComponentInChildren<TMP_Text>();if(text==null) continue;
            int index=System.Array.IndexOf(order,text.text.Trim());if(index<0) continue;
            var rect=b.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.02f+index*.163f,.04f);rect.anchorMax=new Vector2(.17f+index*.163f,.32f);rect.offsetMin=rect.offsetMax=Vector2.zero;
        }
        Transform buy=null;
        foreach(var button in GetComponentsInChildren<UnityEngine.UI.Button>(true))
            if(button.name=="Buy Button") { buy=button.transform.parent;break; }
        if(buy!=null)
        {
            var rect=buy.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.42f,.02f);rect.anchorMax=new Vector2(.90f,.16f);rect.offsetMin=rect.offsetMax=Vector2.zero;
            if(buy.parent.Find("Cancel recruitment")==null)
            {
                var cancel=CampaignHUD.MakeButton(buy.parent,"Cancel",new Vector2(.10f,.02f),new Vector2(.38f,.16f),()=>ControlSingleton.Instance.GoBack());
                cancel.name="Cancel recruitment";
            }
        }
    }

    void LogRecruitationToEsc() 
    {
        ControlSingleton.Instance.LogToEsc(superMenu.transform.parent.gameObject, superMenu);  //first parameter irrelevant for overlay application
                                                                         //alternative implementation of LogToEsc recommended
    }

    public void CommitRecruitmentTransaction() 
    {
        int troopsOrdered = costDeterminant.CommitNumber();
        bool placed=TrainingPool.Place(troopVariableName,troopsOrdered,troopCost);
        

        ControlSingleton.Instance.GoBack();
        if(placed) CampaignHUD.Instance.ShowReport("Recruitment order placed",troopsOrdered.ToString("N0")+" "+TrainingPool.Name(troopVariableName)+" will be ready on "+VariableSingleton.GetDate().AddDays(TrainingPool.Duration(troopVariableName)).ToString("d MMM yyyy")+" ("+TrainingPool.Duration(troopVariableName)+" days).\n\nPaid "+((long)troopsOrdered*troopCost).ToString("N0")+" gold.\nView outstanding orders in the Ongoing training.",()=>CampaignHUD.Instance.DismissReport());
    }
}
