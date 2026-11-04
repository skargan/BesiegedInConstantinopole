#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

// Bounded checks exercise the same stock, dispatch and arrival methods as the campaign.
public static class ProvincialFoodChecks
{
    static int count;
    static VariableSingleton.CampaignSave baseline;
    static ProvincialWar.Province P=>ProvincialWar.Find("thrace");
    static void Check(bool ok,string name){if(!ok)throw new Exception("PROVINCIAL FOOD: "+name);count++;}
    static bool Near(double a,double b)=>Math.Abs(a-b)<.0000011;
    static void Invoke(string name,params object[] args)=>typeof(ProvincialWar).GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,args);
    static void Reset()
    {
        VariableSingleton.Instance.RestoreValidated(baseline);
        ProvincialWar.Put("ConvoyCount",0);VariableSingleton.SetInt("iWbwSupplyLoss",0);
        SharedProvisions.Set("provincial_disruption",0);SharedProvisions.Set(SharedProvisions.Camp,0);
        P.Owner=ProvincialWar.Ottoman;P.Wealth=100;P.FoodReserve=400;
        P.Set("NextShipment",0);P.Set("LastHarvestDay",0);
        ProvincialWar.Put("C1_Done",0);ProvincialWar.Put("C2_Done",0);
    }
    static int Depart(int day)
    {
        Invoke("Dispatch",P,day);return ProvincialWar.I("ConvoyCount");
    }
    static void Arrive(int day)=>Invoke("Arrive",day,new StringBuilder());
    public static int Run()
    {
        count=0;var original=VariableSingleton.Instance.CaptureState();var rules=ProvincialWar.Rules;var cfg=P.Config;
        double contribution=cfg.FortnightlyProvisions,harvest=cfg.SummerHarvestDaily,cap=cfg.MaxFoodReserve;
        int start=rules.HarvestStartMonth,end=rules.HarvestEndMonth;
        try {
            typeof(VariableSingleton).GetField("turn",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,VariableSingleton.EffectiveSiegeTurn);
            ProvincialWar.Ensure();baseline=VariableSingleton.Instance.CaptureState();
            ProvincialWarRules.Validate(rules);
            Check(Near(rules.Provinces.Sum(p=>p.InitialFoodReserve),13200),"provisional 150-day provincial reserve budget");
            Reset();P.Wealth=80;P.FoodReserve=400;P.Wealth=60;Check(Near(P.FoodReserve,300),"80 to 60 Wealth destroys exactly one quarter");
            P.Wealth=60;Check(Near(P.FoodReserve,300),"same Wealth cannot repeat destruction");
            P.Wealth=100;Check(Near(P.FoodReserve,300),"Wealth recovery creates no replacement food");
            P.Wealth=0;Check(P.FoodReserve==0,"zero Wealth destroys all undispatched food");P.Wealth=100;Check(P.FoodReserve==0,"recovery from zero does not refill reserve");

            Reset();cfg.FortnightlyProvisions=7.123456;P.Wealth=60;P.FoodReserve=10;
            int day=ProvincialWar.Day;int serial=Depart(day);double cargo=ProvincialWar.ConvoyFood(serial);
            Check(Near(cargo,4.274074)&&Near(P.FoodReserve,5.725926),"fractional departure is Wealth scaled and deducted");
            Depart(day);Check(ProvincialWar.I("ConvoyCount")==1&&Near(P.FoodReserve,5.725926),"same-day departure is idempotent");
            Check(P.Get("NextShipment")==day+rules.ShipmentInterval,"next convoy uses configured fortnight interval");
            P.Wealth=0;Check(Near(ProvincialWar.ConvoyFood(serial),cargo),"province damage cannot destroy departed cargo");
            Check(SharedProvisions.Stock(SharedProvisions.Camp)==0,"cargo does not teleport to camp");
            P.Owner=ProvincialWar.Imperial;Arrive(day+cfg.TravelDays-1);Check(SharedProvisions.Stock(SharedProvisions.Camp)==0,"arrival date respected");
            Arrive(day+cfg.TravelDays);Check(Near(SharedProvisions.Stock(SharedProvisions.Camp),cargo),"ownership change does not redirect departed cargo");
            Arrive(day+cfg.TravelDays);Check(Near(SharedProvisions.Stock(SharedProvisions.Camp),cargo)&&ProvincialWar.ConvoyFood(serial)==0,"arrival credited only once");

            Reset();cfg.FortnightlyProvisions=50;P.FoodReserve=.125001;serial=Depart(day);
            Check(Near(ProvincialWar.ConvoyFood(serial),.125001)&&P.FoodReserve==0,"shipment cannot exceed finite reserve");
            P.Set("NextShipment",day+14);Depart(day+14);Check(ProvincialWar.ConvoyFood(2)==0,"empty reserve creates no food");
            Reset();P.Owner=ProvincialWar.Imperial;serial=Depart(day);
            Check(ProvincialWar.ConvoyFood(serial)==0&&Near(P.FoodReserve,400)&&ProvincialWar.I("C1_Imperial")==1,"imperial ownership retains gold-only tribute rule");
            Reset();P.Owner=ProvincialWar.AllyOwnerOffset;Depart(day);Check(ProvincialWar.I("ConvoyCount")==0&&Near(P.FoodReserve,400),"allied ownership exports no Ottoman food");

            Reset();rules.HarvestStartMonth=6;rules.HarvestEndMonth=8;cfg.MaxFoodReserve=500;cfg.SummerHarvestDaily=10;P.Wealth=50;P.FoodReserve=498;
            int summer=TrainingPool.Day(new DateTime(1453,7,1));
            Check(Near(ProvincialWar.Harvest(P,summer),2)&&Near(P.FoodReserve,500),"summer harvest respects editable reserve maximum");
            P.FoodReserve=400;Check(ProvincialWar.Harvest(P,summer)==0&&Near(P.FoodReserve,400),"harvest cannot repeat on same day");
            Check(Near(ProvincialWar.Harvest(P,summer+1),5),"Wealth controls productive harvest capacity");
            int winter=TrainingPool.Day(new DateTime(1453,12,1));double before=P.FoodReserve;
            Check(ProvincialWar.Harvest(P,winter)==0&&Near(P.FoodReserve,before),"winter has no harvest replenishment");
            P.Set("NextShipment",winter);Depart(winter);Check(ProvincialWar.ConvoyFood(1)>0&&P.FoodReserve<before,"stored food can depart during winter");

            Reset();cfg.FortnightlyProvisions=12.345678;serial=Depart(day);double city=SharedProvisions.Stock(SharedProvisions.City);
            Check(Near(ProvincialWar.InterceptFood(serial,SharedProvisions.City),12.345678)&&Near(SharedProvisions.Stock(SharedProvisions.City)-city,12.345678),"captured cargo transfers to city using shared units");
            Check(ProvincialWar.InterceptFood(serial,SharedProvisions.City)==0,"interception cannot collect food twice");
            Arrive(day+cfg.TravelDays);Check(SharedProvisions.Stock(SharedProvisions.Camp)==0,"intercepted cargo cannot also feed camp");
            Reset();serial=Depart(day);Check(Near(ProvincialWar.InterceptFood(serial,SharedProvisions.Camp),12.345678)&&Near(SharedProvisions.Stock(SharedProvisions.Camp),12.345678),"interception supports camp recipient");
            Reset();serial=Depart(day);city=SharedProvisions.Stock(SharedProvisions.City);ProvincialWar.InterceptFood(serial,null);
            Check(ProvincialWar.ConvoyFood(serial)==0&&Near(SharedProvisions.Stock(SharedProvisions.City),city)&&SharedProvisions.Stock(SharedProvisions.Camp)==0,"destruction does not award food to either side");

            Reset();serial=Depart(day);double reserve=P.FoodReserve;cargo=ProvincialWar.ConvoyFood(serial);
            var saved=JsonUtility.FromJson<VariableSingleton.CampaignSave>(JsonUtility.ToJson(VariableSingleton.Instance.CaptureState()));
            P.FoodReserve=0;ProvincialWar.InterceptFood(serial,null);VariableSingleton.Instance.RestoreValidated(saved);
            Check(Near(P.FoodReserve,reserve)&&Near(ProvincialWar.ConvoyFood(serial),cargo),"serialized save restores separate fractional reserve and cargo");
            Arrive(day+cfg.TravelDays);var arrived=VariableSingleton.Instance.CaptureState();VariableSingleton.Instance.RestoreValidated(arrived);Arrive(day+cfg.TravelDays);
            Check(Near(SharedProvisions.Stock(SharedProvisions.Camp),cargo),"save restoration cannot repeat completed delivery");

            Reset();cfg.FortnightlyProvisions=100;SharedProvisions.Set("provincial_reference_ration",88);VariableSingleton.SetInt("iWbwSupplyLoss",1);Depart(day);Arrive(day+cfg.TravelDays);
            Check(Near(SharedProvisions.Stock(SharedProvisions.Camp),12),"legacy one-day disruption destroys 88 physical provisions");
            return count;
        } finally {
            cfg.FortnightlyProvisions=contribution;cfg.SummerHarvestDaily=harvest;cfg.MaxFoodReserve=cap;rules.HarvestStartMonth=start;rules.HarvestEndMonth=end;
            VariableSingleton.Instance.RestoreValidated(original);
        }
    }
}
#endif
