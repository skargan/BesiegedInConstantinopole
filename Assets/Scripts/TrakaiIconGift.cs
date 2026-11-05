using UnityEngine;

public static class TrakaiIconGift
{
    public const int Cost=1750;public const string Key="iTrakaiIconGift";
    public static bool IsCountry(int index)=>index==CountryDiplomacy.Index("lithuania");
    public static string Unavailable(int index)=>!IsCountry(index)?"This gift is intended for Lithuania":DailyLifeEvents.I(Key)>0?"Icon already sent to Trakai":CountryDiplomacy.EnvoyState(index)!=CountryDiplomacy.EnvoyAssignment.Resident?"Requires a resident envoy":CouncilActions.Int("iMoney")<Cost?"Requires 1,750 gold":null;
    public static bool Send(int index,out string report)
    {
        report=Unavailable(index);if(report!=null)return false;
        VariableSingleton.ChangeInt("iMoney",-Cost);VariableSingleton.SetInt(Key,1);float gain=CountryDiplomacy.ChangeGoodwill(index,15);
        report="At the landing below Trakai, the envoys unwrap the painted panel. Casimir steps closer to examine the gold ground and the Child resting in His Mother's arms. For a moment, the clerks leave their letters unopened.\n\nThe emperor's letter asks that the image be received at the church founded by Vytautas. It speaks of the Mother of God's protection over Constantinople, and of the prayers that might now be offered for it beside these northern lakes.\n\nCasimir entrusts the icon to his attendants. ‘Tell your emperor that his gift will be honoured here. And tell him that I have read his letter.’\n\n1,750 gold spent. Lithuanian goodwill +"+gain.ToString("0.##")+". The icon has been sent to Trakai.";
        DecisionJournal.Record("trakai-icon-gift","An icon for Trakai","Sent an icon of the Mother of God to Casimir's court for Trakai: 1,750 gold; Lithuanian goodwill +"+gain.ToString("0.##")+".",true);
        return true;
    }
}
