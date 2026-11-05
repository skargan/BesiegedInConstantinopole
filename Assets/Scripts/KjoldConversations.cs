using System;
using UnityEngine;

public static class KjoldConversations
{
    [Serializable] public sealed class Dialogue {
        public string biography,introduction,securityOpening,spiesUnknown,spiesKnown,securityAdvice,guardHistory;
    }
    static Dialogue data;
    public static Dialogue Text {
        get {
            if(data==null){var asset=Resources.Load<TextAsset>("Content/KjoldDialogue");if(asset==null)throw new InvalidOperationException("Kjold dialogue is missing.");data=JsonUtility.FromJson<Dialogue>(asset.text);}
            return data;
        }
    }
    public static bool SecurityHeard=>DailyLifeEvents.I("iKjoldSecurityHeard")>0;
    public static string SecurityAssessment()=>Text.securityOpening+"\n\n"+
        (CouncilActions.Int("iSpies")>=8?string.Format(Text.spiesKnown,Mathf.Max(0,CouncilActions.Int("iTurkSpies"))):Text.spiesUnknown)+"\n\n"+Text.securityAdvice;
    public static void HearSecurity(){VariableSingleton.SetInt("iKjoldSecurityHeard",1);}
}
