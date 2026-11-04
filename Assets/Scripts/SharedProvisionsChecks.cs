#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

// Opt-in checks against the real campaign store. No independent food simulation.
public static class SharedProvisionsChecks
{
    static readonly string[] Supports={"fPeopleRep","fTroopRep","fPriestRep","fNobleRep"};
    static readonly string[] Enemy={"iBalkans","iBazouks","iJanissaries"};
    static readonly List<string> details=new List<string>();
    public static string[] Details=>details.ToArray();
    static void Check(bool ok,string label){if(!ok)throw new InvalidOperationException("PROVISIONS38: "+label);details.Add("PASS "+label);}
    static bool Near(double a,double b,double tolerance=.000002)=>Math.Abs(a-b)<=tolerance;
    static int I(string key)=>DailyLifeEvents.I(key);
    static void Set(string key,int value)=>VariableSingleton.SetInt(key,value);
    static void Move(int day)=>typeof(VariableSingleton).GetField("turn",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,VariableSingleton.EffectiveSiegeTurn+day);
    static int EnemyTotal=>Enemy.Sum(I);
    static void Fixture(int population=100000,int balkans=2000,int irregulars=5000,int janissaries=3000)
    {
        typeof(VariableSingleton).GetMethod("InitializeVariables",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(VariableSingleton.Instance,null);
        Move(30);Set("iPopulation",population);Set("iInfantry",3000);Set("iArchers",2000);Set("iCavalry",200);Set("iVarangians",150);
        Set("iBalkans",balkans);Set("iBazouks",irregulars);Set("iJanissaries",janissaries);
        foreach(string key in Supports)VariableSingleton.ChangePercentFloat(key,.9f-VariableSingleton.GetFloatVariable(key));
        SharedProvisions.Ensure();SharedProvisions.Set(SharedProvisions.City,0);SharedProvisions.Set(SharedProvisions.Camp,0);
    }
    static double[] Support()=>Supports.Select(k=>(double)VariableSingleton.GetFloatVariable(k)).ToArray();
    static void Losses(double[] before,double[] points,string label)
    {
        for(int n=0;n<Supports.Length;n++)Check(Near((before[n]-VariableSingleton.GetFloatVariable(Supports[n]))*100,points[n],.0001),label+" · "+Supports[n]);
    }
    public static int Run()
    {
        details.Clear();var original=VariableSingleton.Instance.CaptureState();var rng=UnityEngine.Random.state;string notice=VariableSingleton.ArrivalNotice;
        try{
            Fixture(100499);var early=VariableSingleton.Instance.CaptureState();
            SharedProvisions.Set("cityPenalty",2.5);SharedProvisions.Set("cargo_999",12.345);Set("iProvisionOttomanHungerDays",9);
            VariableSingleton.Instance.RestoreValidated(early);
            Check(SharedProvisions.Stock("cityPenalty")==0&&SharedProvisions.Stock("cargo_999")==0&&I("iProvisionOttomanHungerDays")==0,"loading an earlier save clears later hunger and convoy fractional state");
            Check(Near(SharedProvisions.CityDemand,100.499),"100,499 residents consume 100.499 provisions, without rounding to a thousand");
            TrainingPool.Orders.Add(new TrainingPool.Order{unit="iInfantry",quantity=1000,readyDay=TrainingPool.Day(VariableSingleton.GetDate())+5});
            Check(Near(SharedProvisions.CityDemand,100.499),"serving soldiers and queued local recruits are already included in city population");
            Set("iPopulation",1000);Check(SharedProvisions.CityDemand>=6.2&&SharedProvisions.CityDemand<10,"inconsistent small population never feeds fewer than serving and queued local personnel");

            Fixture();SharedProvisions.Set(SharedProvisions.City,123.456789);Check(Near(SharedProvisions.Stock(SharedProvisions.City),123.456789,.0000006),"stock preserves six decimal places");
            SharedProvisions.Add(SharedProvisions.City,.000001);Check(Near(SharedProvisions.Stock(SharedProvisions.City),123.456790,.0000006),"one millionth can be added");
            SharedProvisions.Take(SharedProvisions.City,.456790);Check(Near(SharedProvisions.Stock(SharedProvisions.City),123),"fractional debit leaves exact whole stock");
            SharedProvisions.Set(SharedProvisions.City,3.75);VariableSingleton.ChangeInt(SharedProvisions.City,2);Check(Near(SharedProvisions.Stock(SharedProvisions.City),5.75),"existing whole-number rewards preserve fractional stock");
            VariableSingleton.SetInt(SharedProvisions.City,9);Check(Near(SharedProvisions.Stock(SharedProvisions.City),9),"explicit integer stock assignment clears old fraction");
            SharedProvisions.Take(SharedProvisions.City,999);Check(Near(SharedProvisions.Stock(SharedProvisions.City),0),"oversized debit cannot make negative stock");

            Fixture(100499);var before=Support();SharedProvisions.Set(SharedProvisions.City,SharedProvisions.CityDemand);SharedProvisions.ConsumeCityDay();
            Check(Near(SharedProvisions.Stock(SharedProvisions.City),0),"last complete city ration consumes exact balance");Losses(before,new double[]{0,0,0,0},"last full ration is not starvation");
            string cityState=JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());SharedProvisions.ConsumeCityDay();Check(cityState==JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()),"city daily tick cannot run twice on one date");

            Fixture();for(int d=1;d<=3;d++){Move(30+d);before=Support();SharedProvisions.ConsumeCityDay();Losses(before,new double[]{d,d/2,d/2,d/2},"city shortage day "+d);}
            before=Support();Move(34);SharedProvisions.Set(SharedProvisions.City,SharedProvisions.CityDemand);SharedProvisions.ConsumeCityDay();Losses(before,new double[]{0,0,0,0},"full rations stop penalties without restoring lost support");
            Move(35);before=Support();SharedProvisions.ConsumeCityDay();Losses(before,new double[]{1,0,0,0},"new shortage starts at day one after recovery");

            Fixture();SharedProvisions.Set(SharedProvisions.City,SharedProvisions.CityDemand/2);before=Support();SharedProvisions.ConsumeCityDay();Losses(before,new double[]{.5,0,0,0},"half ration gives half the first-day penalty");

            Fixture();Check(Near(SharedProvisions.OttomanDemand,10),"10,000 camp soldiers consume 10 provisions");
            SharedProvisions.Set(SharedProvisions.Camp,SharedProvisions.OttomanDemand);Check(Near(SharedProvisions.CampDays,1),"camp endurance derives from actual troop demand");
            int men=EnemyTotal;SharedProvisions.ConsumeOttomanDay();Check(EnemyTotal==men&&Near(SharedProvisions.Stock(SharedProvisions.Camp),0),"last full Ottoman ration causes no desertion");
            string campState=JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());SharedProvisions.ConsumeOttomanDay();Check(campState==JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()),"Ottoman daily tick cannot run twice on one date");

            Fixture();int totalLoss=0;
            for(int d=1;d<=5;d++){
                Move(30+d);men=EnemyTotal;SharedProvisions.ConsumeOttomanDay();int lost=men-EnemyTotal;totalLoss+=lost;
                Check(lost==d*100,"Ottoman shortage day "+d+" loses "+(d*100)+" soldiers at full deprivation");
                int cumulative=d*(d+1)/2*100;
                Check(I("iBalkans")==2000-cumulative/5&&I("iBazouks")==5000-cumulative/2&&I("iJanissaries")==3000-cumulative*3/10,"desertion respects the three troop groups on day "+d);
            }
            Check(totalLoss==1500&&EnemyTotal==8500,"five hungry days lose exactly 1,500 soldiers");
            Move(36);men=EnemyTotal;SharedProvisions.Set(SharedProvisions.Camp,SharedProvisions.OttomanDemand);SharedProvisions.ConsumeOttomanDay();Check(EnemyTotal==men,"restored Ottoman rations do not restore deserters");
            Move(37);SharedProvisions.ConsumeOttomanDay();Check(EnemyTotal==men-100,"renewed Ottoman shortage returns to the first-day rate");

            Fixture();SharedProvisions.Set(SharedProvisions.Camp,SharedProvisions.OttomanDemand/2);SharedProvisions.ConsumeOttomanDay();Check(EnemyTotal==9950,"half Ottoman ration halves first-day desertion to 50");
            Fixture();Set("iSpies",3);Check(EspionageSystem.IntelligenceValue(3)=="Unknown","fewer than four camp agents cannot reveal enemy consumption");
            string concealed=SharedProvisions.ConsumeOttomanDay();Check(!concealed.Contains("100")&&concealed.Contains("unknown"),"low intelligence hides exact Ottoman desertion losses");
            Set("iSpies",4);Check(EspionageSystem.IntelligenceValue(3)==SharedProvisions.CampConsumptionReport,"four camp agents reveal the actual consumption and shortage report");
            Fixture(100000,1,2,297);SharedProvisions.ConsumeOttomanDay();Check(EnemyTotal==200&&I("iBalkans")==1&&I("iBazouks")==1&&I("iJanissaries")==198,"small mixed groups receive fair integer shares without wiping out the smallest group");
            Fixture(100000,1,2,7);SharedProvisions.ConsumeOttomanDay();Check(EnemyTotal==0&&Enemy.All(k=>I(k)>=0),"desertion is capped at remaining soldiers in a tiny camp");
            Fixture(100000,0,0,0);SharedProvisions.ConsumeOttomanDay();Check(EnemyTotal==0&&Near(SharedProvisions.OttomanDemand,0),"empty camp creates no demand or phantom deserters");

            Fixture();Set("iSpies",2);SharedProvisions.Set(SharedProvisions.Camp,1234.5);
            var hiddenBefore=VariableSingleton.Instance.CaptureState();SharedProvisions.Take(SharedProvisions.Camp,200);
            Check(!EventOutcome.Describe(hiddenBefore,VariableSingleton.Instance.CaptureState()).Contains("Ottoman provisions"),"event summaries do not reveal camp food below three agents");
            Set("iSpies",3);Check(!SharedProvisions.CampStockReport.Contains("days"),"tier three stock report does not indirectly reveal tier four demand");
            Set("iSpies",4);Check(SharedProvisions.CampStockReport.Contains("days"),"tier four may also show endurance at current camp strength");

            Fixture();SharedProvisions.Set(SharedProvisions.Camp,12.345678);SharedProvisions.Set(SharedProvisions.City,2.125);
            double moved=SharedProvisions.Transfer(SharedProvisions.Camp,SharedProvisions.City,1.234567);
            Check(Near(moved,1.234567)&&Near(SharedProvisions.Stock(SharedProvisions.Camp),11.111111)&&Near(SharedProvisions.Stock(SharedProvisions.City),3.359567),"cross-camp transfer conserves the exact same provisions quantity");
            moved=SharedProvisions.Transfer(SharedProvisions.Camp,SharedProvisions.City,999);
            Check(Near(moved,11.111111)&&Near(SharedProvisions.Stock(SharedProvisions.Camp),0)&&Near(SharedProvisions.Stock(SharedProvisions.City),14.470678),"transfer cannot create food when the sending stock runs out");

            Fixture(100499);SharedProvisions.Set(SharedProvisions.City,50.2495);SharedProvisions.Set(SharedProvisions.Camp,5.125678);
            SharedProvisions.ConsumeCityDay();SharedProvisions.ConsumeOttomanDay();SharedProvisions.Add(SharedProvisions.City,.345678);SharedProvisions.Add(SharedProvisions.Camp,.765432);
            double city=SharedProvisions.Stock(SharedProvisions.City),camp=SharedProvisions.Stock(SharedProvisions.Camp),carry=SharedProvisions.Stock("deserterRemainder");before=Support();men=EnemyTotal;
            Check(SaveGames.Checkpoint(),"fractional shortage checkpoint saved through real archive system");
            SharedProvisions.Set(SharedProvisions.City,999);SharedProvisions.Set(SharedProvisions.Camp,999);Set("iBalkans",0);
            Check(SaveGames.Load(VariableSingleton.SavePath,out var error),"fractional shortage archive loads: "+error);
            Check(Near(SharedProvisions.Stock(SharedProvisions.City),city)&&Near(SharedProvisions.Stock(SharedProvisions.Camp),camp)&&EnemyTotal==men,"both fractions and surviving troop mix survive save/load");
            Check(Near(SharedProvisions.Stock("deserterRemainder"),carry)&&I("iHungerDays")==1&&I("iProvisionOttomanHungerDays")==1,"fractional desertion carry and consecutive shortage days survive save/load");
            Losses(before,new double[]{0,0,0,0},"save/load preserves support");
            string restored=JsonUtility.ToJson(VariableSingleton.Instance.CaptureState());SharedProvisions.ConsumeCityDay();SharedProvisions.ConsumeOttomanDay();Check(restored==JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()),"loading and reopening cannot repeat today's hunger penalties or ration debit");
            int expected=(int)Math.Floor(carry+200*(1-camp/SharedProvisions.OttomanDemand)+.00000001);Move(31);SharedProvisions.ConsumeOttomanDay();Check(men-EnemyTotal==expected,"next day after loading uses persisted fractional carry and current survivor ration demand");
            return details.Count;
        }finally{VariableSingleton.Instance.RestoreValidated(original);UnityEngine.Random.state=rng;VariableSingleton.ArrivalNotice=notice;}
    }
}
#endif
