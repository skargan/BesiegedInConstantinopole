using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Presentation only; reads the campaign's existing secrets balance.
public sealed class TechnologySecretsCounter : MonoBehaviour
{
    TMP_Text value;
    int lastValue=int.MinValue;
    public static void Attach(Transform screen)
    {
        var frame=CampaignHUD.Panel(screen,"Available research secrets",new Vector2(.755f,.875f),new Vector2(.965f,.985f));
        ImperialTheme.Panel(frame,false);
        frame.GetComponent<Image>().raycastTarget=false;
        var title=CampaignHUD.Label(frame.transform,"AVAILABLE SECRETS",new Vector2(.05f,.58f),new Vector2(.95f,.94f),17);
        title.alignment=TextAlignmentOptions.Center;
        var count=CampaignHUD.Label(frame.transform,"",new Vector2(.05f,.06f),new Vector2(.95f,.62f),32);
        count.alignment=TextAlignmentOptions.Center;
        count.color=new Color(1f,.87f,.49f);
        count.enableAutoSizing=true;count.fontSizeMin=22;count.fontSizeMax=32;
        var counter=frame.AddComponent<TechnologySecretsCounter>();counter.value=count;counter.Refresh();
    }
    void Update()=>Refresh();
    void Refresh()
    {
        if(value==null)return;
        int current=CouncilActions.Int("iSecrets");
        if(current==lastValue)return;
        lastValue=current;value.text=current.ToString("N0");
    }
}
