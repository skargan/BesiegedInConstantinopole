// Presentation only: reads persisted battle decisions, never rolls or changes them.
public static class ProvincialBattleArt
{
    public static string Key(ProvincialWar.Province p)
    {
        int stage=p.Get("BattleStage");
        if(stage==1)return p.Get("BTerrain")==1?"02-encounter-woodland":p.Get("BTerrain")==2?"03-encounter-mountain":"01-encounter-plains";
        if(stage==2)return "04-manoeuvres";
        if(stage==3||stage==4)return "05-clash";
        if(stage==6&&p.Get("BRenewed")==0)
        {
            if(p.Get("BResult")>0&&p.Get("BAfterChoice")==0&&p.Get("BCounter")==0)return "06-pursuit";
            if(p.Get("BResult")<0&&p.Get("BRouted")==0&&(p.Get("BAfterChoice")==0||p.Get("BAfterChoice")==1))return "07-fighting-withdrawal";
        }
        return "08-aftermath";
    }
}
