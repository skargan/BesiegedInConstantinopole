using System;
using System.Collections.Generic;
using System.Linq;

public static class TrainingPool
{
    [Serializable] public class Order
    {
        public string unit;
        public int quantity;
        public int readyDay;
        public Order Copy() => new Order {unit=unit,quantity=quantity,readyDay=readyDay};
    }
    public static readonly string[] Units={"iInfantry","iArchers","iCavalry","iVarangians","iShips","iCannons"};
    public static readonly string[] Names={"Infantry","Archers","Cavalry","Varangians","Warships","Cannons"};
    public static readonly int[] Days={5,10,15,20,30,10};
    public static int Duration(string unit) {int i=Array.IndexOf(Units,unit);return i<0?0:Days[i];}
    public static string Name(string unit) {int i=Array.IndexOf(Units,unit);return i<0?unit:Names[i];}
    public static int Day(DateTime date) => (int)(date.Ticks/TimeSpan.TicksPerDay);
    public static DateTime ReadyDate(Order order) => new DateTime((long)order.readyDay*TimeSpan.TicksPerDay);
    public static int Remaining(Order order) => Math.Max(0,order.readyDay-Day(VariableSingleton.GetDate()));
    public static List<Order> Orders => VariableSingleton.Instance.TrainingOrders;
    public static bool Place(string unit,int quantity,int unitCost)
    {
        long cost=(long)quantity*unitCost;
        if(Duration(unit)==0 || quantity<=0 || unitCost<=0 || cost>VariableSingleton.GetIntVariable("iMoney")) return false;
        VariableSingleton.ChangeInt("iMoney",-(int)cost);
        Orders.Add(new Order {unit=unit,quantity=quantity,readyDay=Day(VariableSingleton.GetDate())+Duration(unit)});
        return true;
    }
    public static string CompleteDueOrders(DateTime date)
    {
        var report=new List<string>();
        foreach(var order in Orders.Where(o=>o.readyDay<=Day(date)).OrderBy(o=>o.readyDay).ToArray())
        {
            VariableSingleton.ChangeInt(order.unit,order.quantity);
            report.Add(Name(order.unit)+": +"+order.quantity.ToString("N0")+" ready for service. Total now "+VariableSingleton.GetIntVariable(order.unit).ToString("N0")+".");
            Orders.Remove(order);
        }
        return report.Count==0 ? "" : "\n\nTraining and construction completed\n"+string.Join("\n",report);
    }
}
