using TMPro;
using UnityEngine;
public partial class CouncilUI
{
    void PrivateReflection()
    {
        Open("The Emperor's Reflection","Constantine XI Palaiologos");
        ControlSingleton.Instance.ReturnAction=PrivateChamber;BodyHeight(350);
        var title=CampaignHUD.Label(body,"HEALTH · "+(VariableSingleton.GetFloatVariable("fHealth")*100).ToString("0.#")+"%",new Vector2(.08f,.69f),new Vector2(.92f,.93f),38);title.alignment=TextAlignmentOptions.Center;
        var text=CampaignHUD.Explanation(body,"For a moment there is nobody waiting for an answer. The face in the glass is your own: a man whose sleep, wounds and years do not become less real because he wears a crown.",new Vector2(.09f,.29f),new Vector2(.91f,.66f),25);text.enableAutoSizing=true;text.fontSizeMin=18;text.fontSizeMax=25;
        CampaignHUD.MakeButton(body,"Return to the chamber",new Vector2(.25f,.06f),new Vector2(.75f,.23f),PrivateChamber);
    }
}
