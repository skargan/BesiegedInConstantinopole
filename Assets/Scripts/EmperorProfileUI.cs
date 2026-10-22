using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmperorProfileUI:MonoBehaviour
{
    IEnumerator Start(){yield return null;if(EmperorProfile.NeedsOpening)Setup();Destroy(this);}
    static GameObject Root(string name)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=170;
        var s=go.GetComponent<CanvasScaler>();s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;s.referenceResolution=new Vector2(1600,900);s.matchWidthOrHeight=.5f;
        return go;
    }
    static TMP_Text Text(Transform p,string text,Vector2 a,Vector2 b,int size=26)
    {
        var t=CampaignHUD.Label(p,text,a,b,size);t.enableAutoSizing=true;t.fontSizeMin=18;t.fontSizeMax=size;t.alignment=TextAlignmentOptions.Center;return t;
    }
    public static void Setup()
    {
        var root=Root("What kind of Emperor am I?");var panel=CampaignHUD.Panel(root.transform,"The Emperor",Vector2.zero,Vector2.one);ImperialTheme.Panel(panel);
        Text(panel.transform,"WHAT KIND OF EMPEROR AM I?",new Vector2(.08f,.86f),new Vector2(.92f,.96f),39);
        Text(panel.transform,"You are Constantine XI Palaiologos. You have inherited a title whose reach is greater than your treasury, and a City whose dead are more numerous than its defenders. Men will ask whether you are worthy of the emperors before you. The living will have a harder question: what can you do for them now?\n\nBefore the envoys arrive and the petitions begin, you have a moment without witnesses. What have the years taught you to rely upon: the words that move men, the patience that understands them, or the courage to stand beside them?",new Vector2(.12f,.56f),new Vector2(.88f,.84f),27);
        var values=new[]{3,3,3};var cards=new TMP_Text[3];var minus=new Button[3];var plus=new Button[3];
        var remain=Text(panel.transform,"",new Vector2(.15f,.43f),new Vector2(.85f,.54f),27);Button confirm=null;
        System.Action refresh=()=>{int left=16-values[0]-values[1]-values[2];remain.text="Health 100%  ·  Allocate seven points  ·  Remaining: "+left;for(int i=0;i<3;i++){cards[i].text=EmperorProfile.Names[i]+"\n<size=150%>"+values[i]+" / 10</size>";minus[i].interactable=values[i]>3;plus[i].interactable=left>0&&values[i]<10;}confirm.interactable=EmperorProfile.Valid(values);};
        for(int i=0;i<3;i++){int k=i;float x=.10f+i*.275f;var card=CampaignHUD.Panel(panel.transform,EmperorProfile.Names[i],new Vector2(x,.20f),new Vector2(x+.25f,.42f));ImperialTheme.Frame(card);EmperorAttributeBadge.Draw(card.transform,i,new Vector2(.39f,.65f),new Vector2(.61f,.98f));cards[i]=Text(card.transform,"",new Vector2(.12f,.31f),new Vector2(.88f,.64f),27);cards[i].color=EmperorAttributeBadge.Colour(i);minus[i]=CampaignHUD.MakeButton(card.transform,"−",new Vector2(.08f,.07f),new Vector2(.43f,.30f),()=>{if(values[k]>3)values[k]--;refresh();});plus[i]=CampaignHUD.MakeButton(card.transform,"+",new Vector2(.57f,.07f),new Vector2(.92f,.30f),()=>{if(16-values[0]-values[1]-values[2]>0&&values[k]<10)values[k]++;refresh();});}
        confirm=CampaignHUD.MakeButton(panel.transform,"Face the day",new Vector2(.34f,.06f),new Vector2(.66f,.15f),()=>{if(EmperorProfile.Allocate(values)){VariableSingleton.Instance.Save();Destroy(root);}});refresh();
    }
    public static void Show(System.Action close)
    {
        var root=Root("The Emperor's reflection");var panel=CampaignHUD.Panel(root.transform,"Reflection",new Vector2(.08f,.12f),new Vector2(.92f,.86f));ImperialTheme.Panel(panel);
        // A full-screen raycast shield prevents actions behind the sheet.
        var shield=CampaignHUD.Panel(root.transform,"Reflection shade",Vector2.zero,Vector2.one);shield.GetComponent<Image>().color=new Color(0,0,0,.75f);shield.transform.SetAsFirstSibling();
        Text(panel.transform,"CONSTANTINE XI PALAIOLOGOS",new Vector2(.06f,.83f),new Vector2(.94f,.97f),36);
        Text(panel.transform,"The mirror offers neither counsel nor flattery.",new Vector2(.08f,.73f),new Vector2(.92f,.84f),24);
        for(int i=0;i<4;i++){float x=.06f+i*.225f;var card=CampaignHUD.Panel(panel.transform,"Attribute "+i,new Vector2(x,.38f),new Vector2(x+.205f,.74f));ImperialTheme.Frame(card);var value=Text(card.transform,i==0?"Health\n"+VariableSingleton.GetPercentFloat("fHealth"):EmperorProfile.Names[i-1]+"\n"+EmperorProfile.Value(i-1)+" / 10",new Vector2(.04f,.06f),new Vector2(.96f,i==0?.94f:.50f),30);if(i>0){value.color=EmperorAttributeBadge.Colour(i-1);EmperorAttributeBadge.Draw(card.transform,i-1,new Vector2(.29f,.53f),new Vector2(.71f,.97f));}}
        Text(panel.transform,"Attributes open different conversations. Martial skill also governs personal combat; fencing is part of it.\n\n"+Progress(0)+"   ·   "+Progress(1)+"   ·   "+Progress(2)+"\nTraining awards experience, not an instant level. Later levels require more XP. Quest decisions award XP once; rereading them awards nothing.",new Vector2(.10f,.14f),new Vector2(.90f,.35f),24);
        System.Action leave=()=>{ControlSingleton.Instance.ClearHistory();Destroy(root);close?.Invoke();};
        ControlSingleton.Instance.ClearHistory();ControlSingleton.Instance.LogToEsc(root,root);ControlSingleton.Instance.ReturnAction=leave;
        CampaignHUD.MakeButton(panel.transform,"Return to the Palace",new Vector2(.32f,.025f),new Vector2(.68f,.12f),()=>leave());
    }
    static string Progress(int i)=>EmperorProfile.Names[i]+(EmperorProfile.Value(i)>=10?": mastered":": "+EmperorProfile.XP(i)+" / "+EmperorProfile.NextLevel(i)+" XP");
}
