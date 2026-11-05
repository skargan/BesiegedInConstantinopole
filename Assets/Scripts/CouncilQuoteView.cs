using TMPro;
using UnityEngine;

// The council object survives turns. Refresh only when campaign or turn changes, including reloads.
public class CouncilQuoteView : MonoBehaviour
{
    private TMP_Text caption;
    private string shownCampaign;
    private int shownTurn=-1;
    public string QuoteId {get;private set;}
    public static void Attach(TMP_Text text)
    {
        text.gameObject.name="Council quotation";
        text.alignment=TextAlignmentOptions.Center;
        text.enableAutoSizing=true;text.fontSizeMin=18;text.fontSizeMax=22;
        text.overflowMode=TextOverflowModes.Overflow;
        text.lineSpacing=5;text.paragraphSpacing=7;
        var view=text.gameObject.AddComponent<CouncilQuoteView>();
        view.caption=text;view.Refresh();
    }
    private void OnEnable(){Refresh();}
    private void Update(){Refresh();}
    public void Refresh()
    {
        if(!VariableSingleton.IsInitialized || string.IsNullOrEmpty(SaveGames.CampaignId))return;
        if(caption==null)caption=GetComponent<TMP_Text>();
        int turn=VariableSingleton.Turn;string identity=SaveGames.CampaignId;
        if(shownCampaign==identity && shownTurn==turn)return;
        var quote=CouncilQuotations.ForTurn(identity,turn);
        caption.text=quote.DisplayText;QuoteId=quote.id;
        shownCampaign=identity;shownTurn=turn;
    }
}
