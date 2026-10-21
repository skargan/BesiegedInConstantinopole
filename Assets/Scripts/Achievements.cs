using UnityEngine;
using UnityEngine.UI;
public static class Achievements
{
    public static readonly string[] Names={"Still We Stand","The Last Signature","The Sealed Bargain","A Season Against the World","The Unbroken Wall","The Emperor's Eyes","A Fleet on the Horizon","Master of the Workshops","Four Voices, One Crown","Bread for the City"};
    public static readonly string[] Descriptions={"Survive the siege or destroy the Ottoman field army.","Be assassinated by your own people while accepting surrender terms.","Conclude a surrender without being killed by your own people.","Hold Constantinople for 120 siege days.","Finish a campaign without abandoning the outer wall.","Build a network of eight agents.","Receive a foreign relief expedition.","Adopt every available technology.","Keep all four political blocs influential and the loyalists strongest.","Recover from famine and rebuild a month's food reserve."};
    public static void Unlock(int index) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(!string.IsNullOrEmpty(VariableSingleton.TestSavePath))return;
#endif
        PlayerPrefs.SetInt("BesiegedAchievement"+index,1);PlayerPrefs.Save();
    }
    public static void Show(Transform parent)
    {
        var panel=CampaignHUD.Panel(parent,"Achievements",new Vector2(.02f,.02f),new Vector2(.98f,.98f));panel.GetComponent<Image>().color=new Color(.12f,.075f,.10f,1);panel.transform.SetAsLastSibling();
        CampaignHUD.Label(panel.transform,"Achievements",new Vector2(.04f,.91f),new Vector2(.96f,.99f),33);
        for(int i=0;i<10;i++) {
            float top=.90f-i*.077f;
            string state=i>=3?"Planned":PlayerPrefs.GetInt("BesiegedAchievement"+i,0)==1?"Earned":"Locked";
            CampaignHUD.Label(panel.transform,Names[i]+" · "+state,new Vector2(.04f,top-.03f),new Vector2(.96f,top),22);
            CampaignHUD.Explanation(panel.transform,Descriptions[i],new Vector2(.04f,top-.069f),new Vector2(.96f,top-.031f),18);
        }
        CampaignHUD.MakeButton(panel.transform,"Return",new Vector2(.65f,.025f),new Vector2(.96f,.095f),()=>Object.Destroy(panel));
    }
}
