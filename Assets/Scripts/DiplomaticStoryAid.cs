using System;
using Yarn.Unity;

// Named state expeditions in the preparation story use the same one-package
// entitlement as the ordinary country panel. Broader crusading volunteers remain
// separate recruitment outcomes and do not claim to be an individual court's aid.
public static class DiplomaticStoryAid
{
    static string TypeKey(int i)=>"iDiplomacyStoryAidType_"+i;
    static string UsedKey(int i)=>"iDiplomacyStoryAidUsed_"+i;
    [YarnFunction("CanDiplomaticStoryAid")]
    public static bool CanReceive(string countryId)
    {
        CountryDiplomacy.Ensure();int i=CountryDiplomacy.Index(countryId);
        return i>=0&&DailyLifeEvents.I("iAidState"+i)==0&&DailyLifeEvents.I(UsedKey(i))==0;
    }
    [YarnCommand("DiplomaticStoryAid")]
    public static void Grant(string countryId,string package)
    {
        if(!TryGrant(countryId,package,out string report))throw new InvalidOperationException(report);
    }
    public static bool TryGrant(string countryId,string package,out string report)
    {
        int i=CountryDiplomacy.Index(countryId);report="This court's initial aid appeal has already been used. Its existing commitment is unchanged.";
        if(!CanReceive(countryId))return false;
        int type=countryId=="papacy"?(package=="major"?2:package=="minor"?1:0):countryId=="hospitallers"&&package=="fleet"?3:0;
        if(type==0){report="Unknown story aid package.";return false;}
        // Commit identity before delivery. Reopening the node cannot issue this package again.
        VariableSingleton.SetInt(UsedKey(i),1);VariableSingleton.SetInt(TypeKey(i),type);
        VariableSingleton.SetInt("iAidState"+i,2);VariableSingleton.SetInt("iAidDays"+i,0);
        VariableSingleton.SetInt("iAidWarships"+i,type==3?5:0);CountryDiplomacy.ConsumeAidPermission(i);
        if(type==3) {
            VariableSingleton.ChangeInt("iInfantry",200);VariableSingleton.ChangeInt("iArchers",200);VariableSingleton.ChangeInt("iCavalry",100);
            VariableSingleton.ChangeInt("iShips",5); // Explicitly donated warships, including their crews.
        } else {
            VariableSingleton.ChangeInt("iArchers",type==2?600:200);VariableSingleton.ChangeInt("iMoney",10000);VariableSingleton.ChangeInt("iProvisions",type==2?3000:1000);
        }
        report=CountryDiplomacy.Definitions[i].Name+" delivers its preparation aid: "+DeliveredCargo(i)+". A resident envoy may later renew this court's appeal.";return true;
    }
    public static string DeliveredCargo(int i)
    {
        if(DailyLifeEvents.I("iAidState"+i)!=2)return null;
        switch(DailyLifeEvents.I(TypeKey(i))) {
            case 1:return "200 archers, 10,000 gold, 1,000 provisions";
            case 2:return "600 archers, 10,000 gold, 3,000 provisions";
            case 3:return "200 infantry, 200 archers, 100 cavalry, 5 warships with crews";
            default:return null;
        }
    }
    public static void ClearLastPackage(int i)=>VariableSingleton.SetInt(TypeKey(i),0);
}
