using System;
using System.Linq;
using UnityEngine;

// One saved encounter per province. All chance seeds/enemy decisions are fixed before player choices.
public static class ProvincialBattle
{
    static ProvincialWarRules R=>ProvincialWar.Rules;
    public static void Begin(ProvincialWar.Province p,int lead,bool enemyInitiated)
    {
        if(p.Pending||!p.Hostile||p.Garrison<=0)return;
        p.Set("BattleStage",1);p.Set("BLead",lead);p.Set("BEnemyInitiated",enemyInitiated?1:0);p.Set("BRenewed",0);p.Set("BTotalOurLoss",0);p.Set("BTotalEnemyLoss",0);
        p.Set("BCounter",0);p.Set("BDecisive",0);p.Set("BCovered",0);
        for(int phase=0;phase<5;phase++)p.Set("BSeed"+phase,ProvincialWar.Roll(1,int.MaxValue-1));
        var rng=new System.Random(p.Get("BSeed0"));int roll=rng.Next(100),sum=0,terrain=0;
        for(int i=0;i<3;i++){sum+=p.Config.Terrain[i];if(roll<sum){terrain=i;break;}}
        p.Set("BTerrain",terrain);p.Set("BEscape",Escape(rng,terrain));p.Set("BEnemyEscape",Escape(rng,terrain));
        p.Set("BEnemyApproach",rng.Next(2)); // One shared garrison has no invented support contingent.
        p.Set("BEnemyTactic",rng.Next(3));p.Set("BEnemyRenewTactic",rng.Next(3));p.Set("BBeforeAuthority",p.Authority);p.Set("BBeforeControl",p.TotalControl);
        foreach(var a in p.Armies){p.Set("BPart"+a.Slot,0);p.Set("BStart"+a.Slot,0);}
    }
    static int Escape(System.Random r,int terrain){int n=r.Next(100);return n<R.EscapeWeights[terrain*3]?0:n<R.EscapeWeights[terrain*3]+R.EscapeWeights[terrain*3+1]?1:2;}
    static bool Chance(System.Random r,int chance)=>r.Next(100)<chance;
    static int Percent(System.Random r,int min,int max)=>r.Next(min,max+1);
    public static string Terrain(ProvincialWar.Province p)=>new[]{"Plains","Forest","Mountains"}[p.Get("BTerrain")];
    public static string EscapeName(int value)=>new[]{"Favourable","Ordinary","Trapped"}[Mathf.Clamp(value,0,2)];
    public static string[] Approaches={"Caution","Speed","Coordination"};
    public static string[] Tactics={"Press the Attack","Envelop","Hold Formation"};
    public static int OurTroops(ProvincialWar.Province p)=>p.Armies.Sum(a=>p.Get("BPart"+a.Slot));
    public static bool CanCoordinate(ProvincialWar.Province p)=>p.Present.Any(a=>a.Slot!=p.Get("BLead")&&!a.Protected&&!a.Paused);
    public static bool Maneuver(ProvincialWar.Province p,int choice)
    {
        if(p.Get("BattleStage")!=1||choice<0||choice>2||(choice==2&&!CanCoordinate(p)))return false;
        var rng=new System.Random(p.Get("BSeed1"));int terrain=p.Get("BTerrain"),enemy=p.Get("BEnemyApproach");
        int probability=choice==0?(terrain==0?R.CautionPlains:R.CautionOther):choice==1?(terrain==0?R.SpeedPlains:R.SpeedOther):R.CoordinationChance;
        int enemyProbability=enemy==0?(terrain==0?R.CautionPlains:R.CautionOther):(terrain==0?R.SpeedPlains:R.SpeedOther);
        bool ok=Chance(rng,probability),enemyOk=Chance(rng,enemyProbability);p.Set("BApproach",choice);p.Set("BManeuverOK",ok?1:0);p.Set("BEnemyManeuverOK",enemyOk?1:0);
        p.Set("BCaution",ok&&choice==0?1:0);p.Set("BEnemyCaution",enemyOk&&enemy==0?1:0);
        p.Set("BEscape",Mathf.Clamp(p.Get("BEscape")-(ok&&choice==0?1:0)+(enemyOk&&enemy==1?1:0),0,2));
        p.Set("BEnemyEscape",Mathf.Clamp(p.Get("BEnemyEscape")-(enemyOk&&enemy==0?1:0)+(ok&&choice==1?1:0),0,2));
        foreach(var a in p.Present)if(a.Slot==p.Get("BLead")||(choice==2&&ok&&!a.Protected&&!a.Paused)){p.Set("BPart"+a.Slot,a.Troops);p.Set("BStart"+a.Slot,a.Troops);}
        p.Set("BEnemyPart",p.Garrison);int ours=OurTroops(p),theirs=p.Garrison;
        int ourLoss=Mathf.Min(ours*Percent(rng,R.ManeuverLossMin,R.ManeuverLossMax)/100,theirs*R.ManeuverCap/100);
        int theirLoss=Mathf.Min(theirs*Percent(rng,R.ManeuverLossMin,R.ManeuverLossMax)/100,ours*R.ManeuverCap/100);
        Remove(p,ourLoss,theirLoss);p.Set("BPhaseOurLoss",ourLoss);p.Set("BPhaseEnemyLoss",theirLoss);
        if(enemyOk&&enemy==1)Restrict(p,R.SpeedParticipation);
        if(ok&&choice==1)p.Set("BEnemyPart",p.Get("BEnemyPart")*R.SpeedParticipation/100);
        foreach(var a in p.Armies)p.Set("BStart"+a.Slot,p.Get("BPart"+a.Slot));
        p.Set("BattleStage",2);return true;
    }
    static void Restrict(ProvincialWar.Province p,int percent)
    {
        var armies=p.Armies.ToArray();int[] before=armies.Select(a=>p.Get("BPart"+a.Slot)).ToArray();int[] after=Allocate(before,OurTroops(p)*percent/100);
        for(int i=0;i<armies.Length;i++)p.Set("BPart"+armies[i].Slot,after[i]);
    }
    // Largest remainders preserve the exact whole-number casualty/territory total.
    public static int[] Allocate(int[] weights,int total)
    {
        int sum=weights.Sum();var result=new int[weights.Length];if(sum<=0||total<=0)return result;
        var remainders=new long[weights.Length];int assigned=0;
        for(int i=0;i<weights.Length;i++){long n=(long)weights[i]*total;result[i]=(int)(n/sum);remainders[i]=n%sum;assigned+=result[i];}
        foreach(int i in Enumerable.Range(0,weights.Length).OrderByDescending(i=>remainders[i]).ThenBy(i=>i)){if(assigned>=total)break;if(weights[i]>0){result[i]++;assigned++;}}
        return result;
    }
    static void Remove(ProvincialWar.Province p,int ours,int enemy)
    {
        ours=Mathf.Clamp(ours,0,OurTroops(p));enemy=Mathf.Clamp(enemy,0,p.Get("BEnemyPart"));
        var armies=p.Armies.ToArray();int[] shares=Allocate(armies.Select(a=>p.Get("BPart"+a.Slot)).ToArray(),ours);
        for(int i=0;i<armies.Length;i++){armies[i].Troops-=shares[i];p.Set("BPart"+armies[i].Slot,p.Get("BPart"+armies[i].Slot)-shares[i]);}
        p.Garrison-=enemy;p.Set("BEnemyPart",p.Get("BEnemyPart")-enemy);p.Set("BTotalOurLoss",p.Get("BTotalOurLoss")+ours);p.Set("BTotalEnemyLoss",p.Get("BTotalEnemyLoss")+enemy);
    }
    public static int Matchup(int ours,int enemy)=>ours==enemy?0:(ours+1)%3==enemy?1:-1;
    public static double WinProbability(double a,double b)=>a<=0?0:b<=0?1:(1-R.InconclusiveChance/100d)*a*a/(a*a+b*b);
    public static bool Clash(ProvincialWar.Province p,int choice)
    {
        if(p.Get("BattleStage")!=3||choice<0||choice>2)return false;
        bool renewed=p.Get("BRenewed")>0;var rng=new System.Random(p.Get(renewed?"BSeed4":"BSeed2"));int enemy=p.Get(renewed?"BEnemyRenewTactic":"BEnemyTactic");
        p.Set("BTactic",choice);p.Set("BShownEnemyTactic",enemy);int matchup=Matchup(choice,enemy),ours=OurTroops(p),theirs=p.Get("BEnemyPart");
        bool local=p.Get("BTerrain")>0&&ours>0&&p.Get("BPart0")*100L>=ours*(long)R.LocalMinimum;
        double a=ours*(p.Get("BCaution")>0?1+R.CautionBonus/100d:1)*(matchup>0?1+R.TacticBonus/100d:1)*(local?1+R.LocalBonus/100d:1);
        double b=theirs*(p.Get("BEnemyCaution")>0?1+R.CautionBonus/100d:1)*(matchup<0?1+R.TacticBonus/100d:1);
        p.Set("BEffective",(int)Math.Round(a));p.Set("BEnemyEffective",(int)Math.Round(b));p.Set("BLocalBonus",local?1:0);
        double draw=R.InconclusiveChance/100d,n=rng.NextDouble();int result=a<=0?(b<=0?0:-1):b<=0?1:n<WinProbability(a,b)?1:n<WinProbability(a,b)+draw?0:-1;
        int ourRate=Percent(rng,result>0?R.WinnerLossMin:result<0?R.LoserLossMin:R.DrawLossMin,result>0?R.WinnerLossMax:result<0?R.LoserLossMax:R.DrawLossMax);
        int theirRate=Percent(rng,result<0?R.WinnerLossMin:result>0?R.LoserLossMin:R.DrawLossMin,result<0?R.WinnerLossMax:result>0?R.LoserLossMax:R.DrawLossMax);
        int ourLoss=Mathf.Min((int)Math.Round(ours*ourRate/100d),theirs*R.ClashCap/100),enemyLoss=Mathf.Min((int)Math.Round(theirs*theirRate/100d),ours*R.ClashCap/100);
        Remove(p,ourLoss,enemyLoss);p.Set("BPhaseOurLoss",ourLoss);p.Set("BPhaseEnemyLoss",enemyLoss);p.Set("BResult",result);
        double ratio=result==1?a/Math.Max(1,b):b/Math.Max(1,a);int rout=ratio>=2?R.RoutHigh:ratio>=1.5?R.RoutMedium:R.RoutLow;
        p.Set("BRouted",result!=0&&Chance(rng,rout)?1:0);
        // Save the enemy's aftermath decision now, before presenting the player's next choice.
        p.Set("BEnemyAfter",rng.Next(result<0?2:3));p.Set("BEnemyRenews",Chance(rng,R.EnemyRenewChance)?1:0);
        p.Set("BattleStage",4);return true;
    }
    public static void Continue(ProvincialWar.Province p)
    {
        if(p.Get("BattleStage")==2)p.Set("BattleStage",3);
        else if(p.Get("BattleStage")==4){if(p.Get("BRenewed")>0)AutomaticAftermath(p);else p.Set("BattleStage",5);}
        else if(p.Get("BattleStage")==6){p.Set("BattleStage",0);ProvincialWar.Collapse(p);}
    }
    static void AutomaticAftermath(ProvincialWar.Province p)
    {
        var rng=new System.Random(p.Get("BSeed3"));int result=p.Get("BResult"),ours=OurTroops(p),enemy=p.Get("BEnemyPart");
        int a=ours*Percent(rng,result<0?R.WithdrawalMin[0]:R.ConsolidateLossMin,result<0?R.WithdrawalMax[0]:R.ConsolidateLossMax)/100,b=enemy*Percent(rng,result>0?R.WithdrawalMin[0]:R.ConsolidateLossMin,result>0?R.WithdrawalMax[0]:R.ConsolidateLossMax)/100;
        Remove(p,Mathf.Min(a,enemy),Mathf.Min(b,ours));p.Set("BAfterOurLoss",Mathf.Min(a,enemy));p.Set("BAfterEnemyLoss",Mathf.Min(b,ours));p.Set("BAfterChoice",3);p.Set("BDecisive",0);Finish(p);
    }
    public static bool Aftermath(ProvincialWar.Province p,int choice)
    {
        if(p.Get("BattleStage")!=5)return false;int result=p.Get("BResult"),enemyChoice=p.Get("BEnemyAfter");
        if(choice<0||choice>(result<0?2:1))return false;
        p.Set("BAfterChoice",choice);
        bool renew=result<0?choice==2:result>0?enemyChoice==2:choice==1||p.Get("BEnemyRenews")>0;
        if(renew&&OurTroops(p)>0&&p.Get("BEnemyPart")>0){p.Set("BRenewed",1);p.Set("BattleStage",3);return true;}
        if(result==0){AutomaticAftermath(p);return true;}
        var rng=new System.Random(p.Get("BSeed3"));bool ourWin=result>0;
        bool pursue=ourWin?choice==0:enemyChoice==0,cover=ourWin?enemyChoice==1:choice==1;
        int winners=ourWin?OurTroops(p):p.Get("BEnemyPart"),losers=ourWin?p.Get("BEnemyPart"):OurTroops(p);
        bool routed=p.Get("BRouted")>0;int escape=p.Get(ourWin?"BEnemyEscape":"BEscape");
        int category=!pursue?0:!routed?1:escape==2?3:2;
        if(pursue&&escape==0)category=Mathf.Max(0,category-1);
        int sacrifice=cover?losers*R.CoverCost/100:0;
        bool covered=cover&&Chance(rng,R.CoverChance);if(covered)category=Mathf.Max(0,category-1);
        bool counter=pursue&&!routed&&cover&&Chance(rng,R.CounterChance);
        int loserLoss=sacrifice+(losers-sacrifice)*Percent(rng,R.WithdrawalMin[counter?0:category],R.WithdrawalMax[counter?0:category])/100;
        int winnerLoss=winners*Percent(rng,counter?R.CounterLossMin:pursue?R.PursuitLossMin:R.ConsolidateLossMin,counter?R.CounterLossMax:pursue?R.PursuitLossMax:R.ConsolidateLossMax)/100;
        loserLoss=Mathf.Min(loserLoss,winners);winnerLoss=Mathf.Min(winnerLoss,losers);
        int ourLoss=ourWin?winnerLoss:loserLoss,enemyLoss=ourWin?loserLoss:winnerLoss;
        Remove(p,ourLoss,enemyLoss);p.Set("BAfterOurLoss",ourLoss);p.Set("BAfterEnemyLoss",enemyLoss);p.Set("BCounter",counter?1:0);p.Set("BCovered",covered?1:0);p.Set("BDecisive",routed&&pursue&&!counter?1:0);Finish(p);return true;
    }
    static void Finish(ProvincialWar.Province p)
    {
        int result=p.Get("BResult");bool decisive=p.Get("BDecisive")>0;
        int territory=(decisive?R.DecisiveTerritory:R.VictoryTerritory)*100,authority=decisive?R.DecisiveAuthority:R.VictoryAuthority;
        if(result>0)
        {
            var armies=p.Armies.ToArray();int[] weights=armies.Select(a=>p.Get("BStart"+a.Slot)).ToArray();int[] shares=Allocate(weights,Mathf.Min(territory,10000-p.TotalControl));
            for(int i=0;i<armies.Length;i++)ProvincialWar.Gain(armies[i],shares[i]);p.Authority-=authority;ProvincialWar.Disrupt(p);
        }
        else if(result<0){ProvincialWar.Reclaim(p,territory);p.Authority+=authority;}
        p.Set("BAfterAuthority",p.Authority);p.Set("BAfterControl",p.TotalControl);p.Set("BattleAfter",ProvincialWar.I("LastDay")+R.BattleCooldown);p.Set("BattleStage",6);
        foreach(var a in p.Present)if(a.Troops<R.MinimumArmy&&a.Offensive)a.Stance=ProvincialWar.Order.Gather;
    }
    public static string Report(ProvincialWar.Province p)
    {
        int stage=p.Get("BattleStage");string terrain=Terrain(p),escape=EscapeName(p.Get("BEscape"));
        string header=terrain+" · Withdrawal: "+escape+"\n"+(p.Get("BEscape")==2?"The opposing force threatens your route of retreat.":"Your withdrawal routes remain open.");
        string troops="\nOur participating survivors: "+OurTroops(p).ToString("N0")+" · Ottoman participants: "+(p.Known?p.Get("BEnemyPart").ToString("N0"):"not precisely known");
        string advantages="\n"+string.Join(" · ",new[]{p.Get("BCaution")>0?"Our Caution: +15% effectiveness":null,p.Get("BEnemyCaution")>0?"Enemy Caution: +15% effectiveness":null,p.Get("BApproach")==1&&p.Get("BManeuverOK")>0?"Enemy participation reduced to 75%":null,p.Get("BEnemyApproach")==1&&p.Get("BEnemyManeuverOK")>0?"Our participation reduced to 75%":null}.Where(x=>x!=null));
        string participants="\nParticipating: "+string.Join(", ",p.Armies.Where(a=>p.Get("BStart"+a.Slot)>0).Select(a=>a.Name));
        if(stage==1)return header+"\n"+(p.Get("BEnemyInitiated")>0?"The Ottoman garrison has sought this encounter.":"Our army has sought battle.")+"\nChoose the approach before the armies engage.";
        if(stage==2)return header+"\nOur maneuver: "+Approaches[p.Get("BApproach")]+(p.Get("BManeuverOK")>0?" — successful":" — unsuccessful")+"\nEnemy maneuver: "+Approaches[p.Get("BEnemyApproach")]+(p.Get("BEnemyManeuverOK")>0?" — successful":" — unsuccessful")+advantages+participants+"\nManeuver losses: "+p.Get("BPhaseOurLoss")+" ours · "+(p.Known?p.Get("BPhaseEnemyLoss").ToString()+" enemy":"enemy losses uncertain")+troops+"\nExcluded troops remain alive.";
        string verdict=p.Get("BResult")>0?"Our victory":p.Get("BResult")<0?"Ottoman victory":"Inconclusive";
        if(stage==3)return header+troops+"\n"+(p.Get("BRenewed")>0?"Renewed clash — there can be no further renewal.":"Press beats Envelop; Envelop beats Hold; Hold beats Press.");
        if(stage==4||stage==5)return header+"\nOur tactic: "+Tactics[p.Get("BTactic")]+" · Enemy: "+Tactics[p.Get("BShownEnemyTactic")]+"\n"+verdict+(p.Get("BResult")==0?"":p.Get("BRouted")>0?" — the defeated side is routed":" — the defeated side is forced back in order")+"\nClash losses: "+p.Get("BPhaseOurLoss")+" ours · "+(p.Known?p.Get("BPhaseEnemyLoss").ToString()+" enemy":"enemy losses uncertain")+troops;
        string after=p.Get("BRenewed")>0?"Automatic withdrawal after the renewed clash":p.Get("BResult")==0?"Both sides disengage":"Our aftermath: "+AfterName(p.Get("BResult")>0,p.Get("BAfterChoice"))+" · Enemy: "+AfterName(p.Get("BResult")<0,p.Get("BEnemyAfter"));
        return verdict+(p.Get("BDecisive")>0?" — decisive":"")+"\n"+after+"\nAftermath losses: "+p.Get("BAfterOurLoss")+" ours · "+(p.Known?p.Get("BAfterEnemyLoss").ToString()+" enemy":"enemy losses uncertain")+"\nTotal losses: "+p.Get("BTotalOurLoss")+" ours · "+(p.Known?p.Get("BTotalEnemyLoss").ToString():"enemy losses uncertain")+troops+"\nAuthority: "+p.Get("BBeforeAuthority")+" → "+p.Get("BAfterAuthority")+"\nNon-Ottoman territory: "+(p.Get("BBeforeControl")/100f).ToString("0.##")+"% → "+(p.Get("BAfterControl")/100f).ToString("0.##")+"%\n"+(p.Get("BCounter")>0?"A covering counterattack stopped the pursuit.":p.Get("BRenewed")>0?"After renewed fighting, the forces disengage.":"The survivors regroup.")+" No further pitched battle here for "+R.BattleCooldown+" days.";
    }
    static string AfterName(bool winner,int choice)=>winner?new[]{"Pursue","Consolidate"}[Mathf.Clamp(choice,0,1)]:new[]{"Withdraw","Cover the Retreat","Fight On"}[Mathf.Clamp(choice,0,2)];
}
