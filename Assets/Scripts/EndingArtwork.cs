public static class EndingArtwork
{
    // Presentation mirrors CampaignHUD's established ending precedence; no ending is triggered here.
    public static string Current {
        get {
            if(MehmedAssassination.Victory)return null;
            if(DailyLifeEvents.I("iDeposed")>0)return "nobles";
            if(DailyLifeEvents.I("iSurrenderEnding")>0)return null;
            if(VariableSingleton.Instance.EnduringWall.CityLost)return "conquest";
            if(VariableSingleton.GetFloatVariable("fHealth")<=0)
                return DailyLifeEvents.I("iCombatAssassinationFatal")==1||DailyLifeEvents.I("iLifePoisonEnding")==1?"assassination":null;
            if(VariableSingleton.GetFloatVariable("fTroopRep")<=0)return "army";
            if(VariableSingleton.GetFloatVariable("fPeopleRep")<=0)return "people";
            if(VariableSingleton.GetFloatVariable("fNobleRep")<=0)return "nobles";
            if(VariableSingleton.GetFloatVariable("fPriestRep")<=0)return "clergy";
            return null;
        }
    }
}
