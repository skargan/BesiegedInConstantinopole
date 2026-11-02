using System;
using System.Collections.Generic;
using System.Linq;

public static class TrainingPool
{
    public const string Training="training", Construction="construction", Instruction="instruction", ShipRepair="shipRepair", Recuperation="recuperation";
    [Serializable] public class Order
    {
        public string unit;
        public int quantity;
        public int readyDay;
        public string kind;
        public int accountingVersion;
        public int engineersReserved;
        public int manpowerReserved;
        public bool instructorFree;
        public Order Copy() => new Order {unit=unit,quantity=quantity,readyDay=readyDay,kind=kind,accountingVersion=accountingVersion,engineersReserved=engineersReserved,manpowerReserved=manpowerReserved,instructorFree=instructorFree};
    }
    public static readonly string[] Units={"iInfantry","iArchers","iCavalry","iVarangians","iShips","iCannons","iHealers","iEngineers","iEnvoys"};
    public static readonly string[] Names={"Infantry","Archers","Cavalry","Varangians","Warships","Cannons","Monks","Engineers","Envoys"};
    public static readonly int[] Days={5,10,15,20,30,10,MedicalRecovery.RecruitmentDays,SpecialistPersonnel.EngineerTrainingDays,SpecialistPersonnel.EnvoyTrainingDays};
    public static int Duration(string unit) {int i=Array.IndexOf(Units,unit);return i<0?0:Days[i];}
    public static string Name(string unit) {int i=Array.IndexOf(Units,unit);return i<0?unit:Names[i];}
    public static string OrderName(Order order)=>order.kind==ShipRepair?"Warships · Repairing":order.kind==Recuperation?"Engineers · Recuperating":Name(order.unit)+(order.instructorFree?" · No instructor":"");
    public static int Day(DateTime date) => (int)(date.Ticks/TimeSpan.TicksPerDay);
    public static DateTime ReadyDate(Order order) => new DateTime((long)order.readyDay*TimeSpan.TicksPerDay);
    public static int Remaining(Order order) => Math.Max(0,order.readyDay-Day(VariableSingleton.GetDate()));
    public static List<Order> Orders => VariableSingleton.Instance.TrainingOrders;
    public static bool IsValidOrder(Order order)
    {
        if(order==null||!Units.Contains(order.unit)||order.quantity<0||order.readyDay<0||order.readyDay>3652058||order.accountingVersion<0||order.accountingVersion>1||order.engineersReserved<0||order.manpowerReserved<0)return false;
        if(order.accountingVersion==0)return string.IsNullOrEmpty(order.kind)&&order.engineersReserved==0&&order.manpowerReserved==0&&!order.instructorFree;
        if(order.quantity==0)return false;
        long quantity=order.quantity;
        if(order.kind==Recuperation)return order.unit=="iEngineers"&&order.engineersReserved==0&&order.manpowerReserved==0&&!order.instructorFree;
        if(order.kind==ShipRepair)return order.unit=="iShips"&&order.engineersReserved==quantity*SpecialistPersonnel.ShipRepairEngineers&&order.manpowerReserved==quantity*SpecialistPersonnel.ShipRepairManpower&&!order.instructorFree;
        if(order.kind==Construction)return (order.unit=="iShips"||order.unit=="iCannons")&&order.engineersReserved==quantity*SpecialistPersonnel.EngineersPerUnit(order.unit)&&order.manpowerReserved==quantity*SpecialistPersonnel.OrdinaryManpowerPerUnit(order.unit)&&!order.instructorFree;
        if(order.kind==Instruction)return order.unit=="iEngineers"&&order.manpowerReserved==quantity&&(order.instructorFree?quantity==1&&order.engineersReserved==0:order.engineersReserved==quantity);
        return order.kind==Training&&order.unit!="iShips"&&order.unit!="iCannons"&&order.unit!="iEngineers"&&!order.instructorFree&&order.engineersReserved==0&&order.manpowerReserved==quantity*SpecialistPersonnel.OrdinaryManpowerPerUnit(order.unit);
    }
    public static int UnitPrice(string unit,int offered)=>unit=="iEngineers"?SpecialistPersonnel.EngineerPrice:unit=="iEnvoys"?SpecialistPersonnel.EnvoyPrice:unit=="iHealers"?MedicalRecovery.MonkCost:offered;
    public static int MaximumOrder(string unit,int unitCost)
    {
        unitCost=UnitPrice(unit,unitCost);if(unitCost<=0||Duration(unit)==0)return 0;
        int affordable=Math.Max(0,DailyLifeEvents.I("iMoney"))/unitCost;
        return SpecialistPersonnel.MaximumOrder(unit,RecruitmentLogistics.MaximumOrder(unit,affordable));
    }
    public static bool CanPlace(string unit,int quantity,int unitCost,out string reason)
    {
        reason="";unitCost=UnitPrice(unit,unitCost);long cost=(long)quantity*unitCost;
        if(quantity<=0||Duration(unit)==0||unitCost<=0){reason="Choose a valid recruit or construction order.";return false;}
        if(cost>DailyLifeEvents.I("iMoney")){reason="Not enough gold.";return false;}
        if(RecruitmentLogistics.IsRegular(unit)&&!RecruitmentLogistics.CanRecruit(unit,quantity)){reason="Not enough uncommitted recruitment manpower.";return false;}
        return SpecialistPersonnel.CanStart(unit,quantity,out reason);
    }
    public static bool Place(string unit,int quantity,int unitCost)
    {
        string reason;if(!CanPlace(unit,quantity,unitCost,out reason))return false;
        unitCost=UnitPrice(unit,unitCost);
        var order=new Order {unit=unit,quantity=quantity,readyDay=Day(VariableSingleton.GetDate())+Duration(unit),kind=unit=="iShips"||unit=="iCannons"?Construction:unit=="iEngineers"?Instruction:Training};
        SpecialistPersonnel.ReserveForOrder(order);
        VariableSingleton.ChangeInt("iMoney",-(quantity*unitCost));Orders.Add(order);
        return true;
    }
    public static int MaximumShipRepairs=>Math.Max(0,new[]{SpecialistPersonnel.AvailableDamagedShips,DailyLifeEvents.I("iMoney")/SpecialistPersonnel.ShipRepairPrice,DailyLifeEvents.I("iMaterials")/SpecialistPersonnel.ShipRepairMaterials,RecruitmentLogistics.AvailableManpower/SpecialistPersonnel.ShipRepairManpower,SpecialistPersonnel.Available/SpecialistPersonnel.ShipRepairEngineers}.Min());
    public static bool PlaceShipRepairs(int quantity,out string report)
    {
        if(quantity<=0||quantity>MaximumShipRepairs){report="Each damaged ship needs 250 gold, 5 materials, 10 replacement crew and 2 available engineers. Already assigned hulls cannot be repaired twice.";return false;}
        SpecialistPersonnel.EnsureInitialized();
        VariableSingleton.ChangeInt("iMoney",-quantity*SpecialistPersonnel.ShipRepairPrice);
        VariableSingleton.ChangeInt("iMaterials",-quantity*SpecialistPersonnel.ShipRepairMaterials);
        VariableSingleton.ChangeInt("iEngineers",-quantity*SpecialistPersonnel.ShipRepairEngineers);
        SpecialistPersonnel.CommitLocalManpower(quantity*SpecialistPersonnel.ShipRepairManpower);
        var order=new Order{unit="iShips",quantity=quantity,readyDay=Day(VariableSingleton.GetDate())+SpecialistPersonnel.ShipRepairDays,kind=ShipRepair,accountingVersion=1,engineersReserved=quantity*SpecialistPersonnel.ShipRepairEngineers,manpowerReserved=quantity*SpecialistPersonnel.ShipRepairManpower};
        Orders.Add(order);report=quantity.ToString("N0")+" damaged ships assigned for repair. Ready "+ReadyDate(order).ToString("d MMM yyyy")+". Both engineers per ship return on completion; surviving crews stay aboard.";return true;
    }
    public static string CompleteDueOrders(DateTime date)
    {
        var report=new List<string>();
        SpecialistPersonnel.EnsureInitialized();
        foreach(var order in Orders.Where(o=>o!=null&&o.kind!=Recuperation&&o.readyDay<=Day(date)).OrderBy(o=>o.readyDay).ToArray())
        {
            int completed=order.quantity,returned=order.engineersReserved;
            if(order.kind==ShipRepair){
                completed=Math.Min(completed,Math.Max(0,DailyLifeEvents.I("iDamagedShips")));
                VariableSingleton.ChangeInt("iDamagedShips",-completed);
            }else if(order.kind==Construction&&order.accountingVersion>0)returned=Math.Max(0,returned-order.quantity);
            if(completed>0)VariableSingleton.SetInt(order.unit,DailyLifeEvents.I(order.unit)+completed);
            if(returned>0)VariableSingleton.ChangeInt("iEngineers",returned);
            report.Add(OrderName(order)+": +"+completed.ToString("N0")+" ready for service. Total now "+DailyLifeEvents.I(order.unit).ToString("N0")+"."+(returned>0?" "+returned.ToString("N0")+" engineers return to available duty.":""));
            Orders.Remove(order);
        }
        return report.Count==0 ? "" : "\n\nTraining and construction completed\n"+string.Join("\n",report);
    }
}
