using System;
using System.Linq;

// iEngineers is the available roster. Timed commitments live in saved orders;
// operational crews never return to the specialist or recruitment pool.
public static class SpecialistPersonnel
{
    public const int EngineerPrice=250, EngineerTrainingDays=10, AgentPrice=1000, EnvoyPrice=750, EnvoyTrainingDays=10;
    public const int CannonMaterials=10, ShipMaterials=30, CannonBuilders=2, ShipBuilders=5;
    public const int CannonCrew=5, ShipCrew=50, CannonOrdinaryCrew=4, ShipOrdinaryCrew=49;
    public const int ShipRepairPrice=250, ShipRepairMaterials=5, ShipRepairManpower=10, ShipRepairEngineers=2, ShipRepairDays=5;
    const string DrawnKey="iSpecialistManpowerDrawn", VersionKey="iSpecialistPersonnelVersion";
    static int I(string key)=>Math.Max(0,DailyLifeEvents.I(key));
    static int Bound(long n)=>(int)Math.Min(int.MaxValue,Math.Max(0L,n));
    static void Put(string key,int n)=>VariableSingleton.SetInt(key,Math.Max(0,n));
    static int Sum(Func<TrainingPool.Order,int> value)=>Bound(TrainingPool.Orders.Where(o=>o!=null).Sum(o=>(long)Math.Max(0,value(o))));
    public static int AvailableEngineers=>I("iEngineers");
    public static int Available=>AvailableEngineers;
    public static int ConstructionEngineers=>Sum(o=>o.kind==TrainingPool.Construction?o.engineersReserved:0);
    public static int RepairingEngineers=>Sum(o=>o.kind==TrainingPool.ShipRepair?o.engineersReserved:0);
    public static int Instructors=>Bound((long)Sum(o=>o.kind==TrainingPool.Instruction?o.engineersReserved:0)+(I("iLifeMentorDue")>0?1:0));
    public static int RecuperatingEngineers=>Sum(o=>o.kind==TrainingPool.Recuperation?o.quantity:0);
    public static int WoundedEngineers=>MedicalRecovery.CountFor("iEngineers");
    public static int OperationalEngineers=>Bound((long)I("iCannons")+I("iShips")+I("iDamagedShips"));
    public static int OperationalCrew=>Bound((long)CannonCrew*I("iCannons")+(long)ShipCrew*I("iShips"));
    public static int TotalEngineers=>Bound((long)AvailableEngineers+ConstructionEngineers+RepairingEngineers+Instructors+RecuperatingEngineers+WoundedEngineers+OperationalEngineers);
    public static int LocalManpowerCommitted=>I(DrawnKey);
    public static int RepairingShips=>Sum(o=>o.kind==TrainingPool.ShipRepair?o.quantity:0);
    public static int AvailableDamagedShips=>Math.Max(0,I("iDamagedShips")-RepairingShips);
    public static bool EmergencyTrainingActive=>TrainingPool.Orders.Any(o=>o!=null&&o.unit=="iEngineers"&&o.instructorFree);
    public static int OrdinaryManpowerPerUnit(string unit)=>unit=="iShips"?ShipOrdinaryCrew:unit=="iCannons"?CannonOrdinaryCrew:unit=="iEngineers"||unit=="iEnvoys"?1:0;
    public static int MaterialsPerUnit(string unit)=>unit=="iShips"?ShipMaterials:unit=="iCannons"?CannonMaterials:0;
    public static int EngineersPerUnit(string unit)=>unit=="iShips"?ShipBuilders:unit=="iCannons"?CannonBuilders:unit=="iEngineers"?1:0;

    public static void EnsureInitialized()
    {
        if(I(VersionKey)>0)return;
        // Existing units and unfinished legacy orders are grandfathered with their
        // original crews. Never debit resources retrospectively on load.
        Put(DrawnKey,0);Put(VersionKey,1);
        foreach(var order in TrainingPool.Orders)if(order!=null&&order.accountingVersion==0){order.engineersReserved=0;order.manpowerReserved=0;order.instructorFree=false;}
    }
    public static void ResetForRestore()
    {
        foreach(string key in new[]{VersionKey,DrawnKey,"iSpecialistMentorManpower","iSpecialistLostEngineers","iSpecialistLostCrew"})Put(key,0);
    }
    public static void CommitLocalManpower(int count)
    {
        EnsureInitialized();if(count>0)Put(DrawnKey,Bound((long)I(DrawnKey)+count));
    }
    public static bool CanStart(string unit,int quantity,out string reason)
    {
        reason="";if(quantity<=0){reason="Choose at least one recruit or unit.";return false;}
        int perMan=OrdinaryManpowerPerUnit(unit),perMaterial=MaterialsPerUnit(unit),perEngineer=EngineersPerUnit(unit);
        if((long)quantity*perMan>RecruitmentLogistics.AvailableManpower){reason="Not enough uncommitted recruitment manpower.";return false;}
        if((long)quantity*perMaterial>I("iMaterials")){reason="Not enough materials.";return false;}
        if(unit=="iEngineers"&&Available==0){
            if(quantity!=1||EmergencyTrainingActive){reason=EmergencyTrainingActive?"One instructor-free trainee is already learning.":"With no available instructor, only one engineer may train.";return false;}
        }else if((long)quantity*perEngineer>Available){reason="Not enough available engineers.";return false;}
        return true;
    }
    public static int MaximumOrder(string unit,int maximum)
    {
        maximum=Math.Max(0,maximum);int manpower=OrdinaryManpowerPerUnit(unit),materials=MaterialsPerUnit(unit),engineers=EngineersPerUnit(unit);
        if(manpower>0)maximum=Math.Min(maximum,RecruitmentLogistics.AvailableManpower/manpower);
        if(materials>0)maximum=Math.Min(maximum,I("iMaterials")/materials);
        if(unit=="iEngineers"&&Available==0)maximum=Math.Min(maximum,EmergencyTrainingActive?0:1);
        else if(engineers>0)maximum=Math.Min(maximum,Available/engineers);
        return maximum;
    }
    // Called only after TrainingPool has validated money, manpower and resources.
    public static void ReserveForOrder(TrainingPool.Order order)
    {
        EnsureInitialized();order.accountingVersion=1;
        order.manpowerReserved=OrdinaryManpowerPerUnit(order.unit)*order.quantity;
        order.instructorFree=order.unit=="iEngineers"&&Available==0;
        order.engineersReserved=order.instructorFree?0:EngineersPerUnit(order.unit)*order.quantity;
        if(order.engineersReserved>0)VariableSingleton.ChangeInt("iEngineers",-order.engineersReserved);
        int materials=MaterialsPerUnit(order.unit)*order.quantity;if(materials>0)VariableSingleton.ChangeInt("iMaterials",-materials);
        CommitLocalManpower(order.manpowerReserved);
    }

    public sealed class WallRepairReservation
    {
        internal int count;internal bool settled;
        public int Count=>count;
    }
    public static bool TryReserveWallRepair(int count,out WallRepairReservation reservation)
    {
        reservation=null;if(count<=0||count>Available)return false;
        EnsureInitialized();VariableSingleton.ChangeInt("iEngineers",-count);
        reservation=new WallRepairReservation{count=count};return true;
    }
    public static void CompleteWallRepair(WallRepairReservation reservation,int killed,int wounded,DateTime returnDate)
    {
        if(reservation==null||reservation.settled)throw new InvalidOperationException("Repair assignment has already been settled.");
        if(killed<0||wounded<0||killed+wounded>1||killed+wounded>reservation.count)throw new ArgumentOutOfRangeException("casualties");
        if(returnDate.Date<VariableSingleton.GetDate().Date.AddDays(2))throw new ArgumentOutOfRangeException("returnDate");
        int survivors=reservation.count-killed-wounded;
        if(survivors>0)TrainingPool.Orders.Add(new TrainingPool.Order{unit="iEngineers",quantity=survivors,readyDay=TrainingPool.Day(returnDate),kind=TrainingPool.Recuperation,accountingVersion=1});
        if(killed>0)Put("iSpecialistLostEngineers",Bound((long)I("iSpecialistLostEngineers")+killed));
        if(wounded>0)MedicalRecovery.AdmitWoundedEngineers(wounded);
        reservation.settled=true;
    }
    public static string Advance(DateTime date)
    {
        EnsureInitialized();int returned=0;
        foreach(var order in TrainingPool.Orders.Where(o=>o!=null&&o.kind==TrainingPool.Recuperation&&o.readyDay<=TrainingPool.Day(date)).ToArray()){
            returned+=order.quantity;VariableSingleton.ChangeInt("iEngineers",order.quantity);TrainingPool.Orders.Remove(order);
        }
        return returned>0?returned.ToString("N0")+" engineers have rested after wall repairs and return to duty.\n\n":"";
    }
    public static bool BeginMentoring()
    {
        if(I("iLifeMentorDue")>0||Available<1||I("iInfantry")<1)return false;
        EnsureInitialized();VariableSingleton.ChangeInt("iEngineers",-1);
        // This is a transfer, not a combat loss. The pupil remains committed.
        Put("iInfantry",I("iInfantry")-1);CommitLocalManpower(1);Put("iSpecialistMentorManpower",1);return true;
    }
    public static void CompleteMentoring()
    {
        if(I("iLifeMentorDue")==0)return;
        Put("iLifeMentorDue",0);VariableSingleton.ChangeInt("iEngineers",1);
        if(I("iLifeMentorSuccess")==1)VariableSingleton.ChangeInt("iEngineers",1);
        else{
            VariableSingleton.ChangeInt("iInfantry",1);
            // Only a failed transfer releases its reservation. Dead crews never do.
            if(I("iSpecialistMentorManpower")==1)Put(DrawnKey,I(DrawnKey)-1);
        }
        Put("iSpecialistMentorManpower",0);
    }
    public static void RecordSunkShips(int ships)
    {
        if(ships<=0)return;Put("iSpecialistLostCrew",Bound((long)I("iSpecialistLostCrew")+(long)ships*ShipCrew));
        Put("iSpecialistLostEngineers",Bound((long)I("iSpecialistLostEngineers")+ships));
        // Deliberately no refund to iEngineers or LocalManpowerCommitted.
    }
}
