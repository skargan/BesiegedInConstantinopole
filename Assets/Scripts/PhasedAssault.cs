using System;
using System.Collections.Generic;
using UnityEngine;

// One bounded assault budget. Decisions change its distribution, never create another army.
public sealed class PhasedAssault
{
    public int Attackers, EnemyLeft, Infantry, Archers, Cavalry, Guards, Health, Ammo, Cannons;
    public int InitialInfantry, InitialArchers, InitialCavalry, InitialGuards, InitialHealth;
    public int ContactBudget,PersonalBudget,MaterialsSpent,SortieSent;
    public float ArcherFactor=1,FootFactor=1,GunFactor=1,ExposureFactor=1;
    public bool ArrowScreens;public bool GunResolved,CoverResolved;public float ArcherAccrued;public int ArcherApplied,CombatSeconds;
    public int Type, Round, Breach, MainFront, SignalFront, Reserve=50, Shots, TimedMoments;
    public float Wall, ArcherBudget, GunBudget, FootBudget, CavalryBudget, GuardPower, EnemyLethality;
    public float VolleySum, Cover=1, SortieBonus, PressureRelief;
    public bool Practice;
    public enum Phase { Approach, Contact, Aftermath }
    public Phase Stage { get; private set; } = Phase.Approach;
    public bool Finished => Stage == Phase.Aftermath;
    public bool ApproachResolved => Stage != Phase.Approach;
    public bool Committed { get; private set; }
    public bool Frontline, Tower, Ram, Held=true, ReserveAssigned, SortieResolved;
    public bool EmergencyResolved { get; private set; }
    public int Seed { get; private set; }
    public AssaultResult Outcome { get; private set; }
    public event Action<PhasedAssault> Resolved;
    public bool TryCommit() { if(!Finished || Committed)return false;Committed=true;return true; }
    public int NextRandom(int upperBound) => random.Next(upperBound);
    public string Result="";
    public readonly List<string> Journal=new List<string>();
    readonly System.Random random;
    readonly HashSet<string> appliedIncidents=new HashSet<string>();
    public bool HasIncident(string id)=>appliedIncidents.Contains(id);
    public bool RecordIncident(string id)=>!Finished&&appliedIncidents.Add(id);
    public PhasedAssault(int seed){Journal.Add("Assault seed: "+seed);Seed=seed;random=new System.Random(seed);MainFront=random.Next(2);SignalFront=random.NextDouble()<.75?MainFront:1-MainFront;}
    public int Dead=>Attackers-EnemyLeft;
    public int OurDead=>InitialInfantry+InitialArchers+InitialCavalry+InitialGuards-Infantry-Archers-Cavalry-Guards;
    public float VolleyMultiplier=>1+(Shots==0?0:VolleySum/Shots);
    public static float VolleyScore(float position)=>Mathf.Clamp(.25f-Mathf.Max(0,Mathf.Abs(position-.5f)-.14f)/.72f,-.25f,.25f);
    private void FightSeconds(int seconds){int lost=0;for(int i=0;i<seconds;i++){CombatSeconds++;if(Infantry+Archers==0)continue;if((random.Next(2)==0&&Infantry>0)||Archers==0)Infantry--;else Archers--;lost++;}if(lost>0)Journal.Add("Fighting attrition: "+lost+" defenders lost over "+seconds+" simulated seconds.");}
    public void Volley(float score){
        if(Stage!=Phase.Approach||Shots>=3)return;score=Mathf.Clamp(score,-.25f,.25f);VolleySum+=score;Shots++;FightSeconds(3);
        ArcherAccrued+=ArcherBudget/3f*(ArrowScreens?.75f:1)*Mathf.Clamp((1+score)*Cover*ArcherFactor,.75f,1.25f)*(InitialArchers==0?0:(float)Archers/InitialArchers);
        int killed=Mathf.Clamp(Mathf.RoundToInt(ArcherAccrued)-ArcherApplied,0,EnemyLeft);EnemyLeft-=killed;ArcherApplied+=killed;
        Journal.Add("Volley "+Shots+": "+killed+" enemy losses; timing "+score.ToString("+0%;-0%;0%")+".");
    }
    public void Shelter(float timing) {
        if(Stage!=Phase.Approach||CoverResolved)return;CoverResolved=true;
        // Too early sacrifices firing time; late exposure causes bounded casualties.
        Cover=timing<.45f?.9f:timing>.8f?1.1f:1f;
        int lost=timing>.8f?Mathf.Min(Archers,Mathf.CeilToInt(Archers*.015f)):0;Archers-=lost;
        Journal.Add("Counter-bombardment: "+(lost>0?lost+" archers lost taking cover late.":Cover<1?"early cover; archer output −10%.":"the parapet is cleared before impact."));
    }
    public void Approach(float moat,float fire) {
        if(Stage!=Phase.Approach)return;Stage=Phase.Contact;
        float arrows=ArcherBudget*(ArrowScreens?.75f:1)*Mathf.Clamp(VolleyMultiplier*Cover*ArcherFactor,.75f,1.25f)*(InitialArchers==0?0:(float)Archers/InitialArchers);
        float guns=GunResolved?0:GunBudget*GunFactor;
        int killed=Mathf.Clamp(Mathf.RoundToInt((Shots>0?0:arrows)+guns+Attackers*(moat>0?.1f:0)+fire),0,EnemyLeft);EnemyLeft-=killed;ContactBudget=EnemyLeft;
        Journal.Add("Approach: "+(killed+ArcherApplied)+" enemy losses. Archers "+arrows.ToString("0")+" (volley ×"+VolleyMultiplier.ToString("0.00")+"), guns "+guns.ToString("0")+"; "+EnemyLeft+" reach the walls.");
    }
    public void FireGun(float quality) {
        if(Stage!=Phase.Approach||GunResolved)return;GunResolved=true;
        float multiplier=Mathf.Clamp(GunFactor*(1+quality),.75f,1.25f);int killed=Mathf.Min(EnemyLeft,Mathf.RoundToInt(GunBudget*multiplier));EnemyLeft-=killed;
        Journal.Add("Cannon shot: "+killed+" enemy losses; gun output ×"+multiplier.ToString("0.00")+". This spends the assault's existing gun allocation.");
    }
    public void FireAtScreens(){if(Stage!=Phase.Approach||GunResolved||!ArrowScreens||Cannons<=0||Ammo<=0)return;ArrowScreens=false;GunResolved=true;int killed=Mathf.Min(EnemyLeft,Mathf.RoundToInt(GunBudget*.5f));EnemyLeft-=killed;Journal.Add("Cannons shatter the arrow shields: "+killed+" enemy losses; full archer effect restored. The assault gun allocation is spent.");}
    public void Sortie(int strength,float exposure) {
        if(Stage!=Phase.Contact||Round>0||SortieResolved)return;SortieResolved=true;if(strength==0){Journal.Add("The gates remain shut. The reserve stays within the walls.");return;}
        int committed=Mathf.Min(Infantry,strength==1?100:300);SortieSent=committed;
        float chance=Mathf.Clamp01(.25f+exposure*.6f+(strength==2?.15f:0));
        int lost=Mathf.Min(Infantry,Mathf.CeilToInt(committed*(.02f+exposure*exposure*.18f)));Infantry-=lost;
        bool success=committed>0&&random.NextDouble()<chance;
        if(success){Tower=false;Ram=false;PressureRelief+=12;SortieBonus+=committed*.25f;}
        Journal.Add("Sortie: "+committed+" sent, "+lost+" lost; "+(success?"the siege engine burns. Breach pressure −12.":"the raiders withdraw without destroying the engine."));
    }
    public void Clash() {
        if(Stage!=Phase.Contact||Round>=3||EnemyLeft<=0)return;
        FightSeconds(5);
        int engaged=Mathf.Min(EnemyLeft,ContactBudget/3+(Round<ContactBudget%3?1:0));
        float exposure=Mathf.Clamp01((.25f+.75f*(1-Wall)+(Frontline?.05f:0))*ExposureFactor);
        int loss=Mathf.Min(Infantry+Archers+Cavalry,Mathf.Max(0,Mathf.RoundToInt(engaged*EnemyLethality*exposure)));
        int before=Infantry+Archers+Cavalry;
        int foot=Mathf.Min(Infantry,Mathf.CeilToInt(loss*.75f));Infantry-=foot;
        int bow=Mathf.Min(Archers,loss-foot);Archers-=bow;int horse=Mathf.Min(Cavalry,Mathf.Max(0,loss-foot-bow));Cavalry-=horse;
        Infantry-=Mathf.Min(Infantry,Mathf.Max(0,loss-foot-bow-horse));
        loss=before-Infantry-Archers-Cavalry;
        float strength=InitialInfantry==0?0:(float)Infantry/InitialInfantry;
        float damage=(FootBudget*FootFactor*strength+CavalryBudget*(InitialCavalry==0?0:(float)Cavalry/InitialCavalry)+SortieBonus)/3f*(Frontline?1.08f:1);
        int personal=Frontline?PersonalBudget/3+(Round<PersonalBudget%3?1:0):0;
        int slain=Mathf.Clamp(Mathf.RoundToInt(damage)+personal,0,EnemyLeft);EnemyLeft-=slain;Round++;
        Journal.Add("Contact "+Round+": "+slain+" enemy dead; "+loss+" defenders lost. Personal contribution "+personal+"; melee output "+damage.ToString("0")+"; "+engaged+" attackers engaged at lethality "+EnemyLethality.ToString("0.##")+", exposure "+(exposure*100).ToString("0")+"%; "+EnemyLeft+" attackers still fighting.");
    }
    public void AssignReserve(int left) {
        if(Stage!=Phase.Contact||ReserveAssigned)return;ReserveAssigned=true;Reserve=Mathf.Clamp(left,0,100);
        int match=MainFront==0?Reserve:100-Reserve;
        PressureRelief+=(match-50)*.24f;
        Journal.Add("Reserve: "+Reserve+"% to the gate, "+(100-Reserve)+"% to the curtain. Main effort: "+(MainFront==0?"gate":"curtain")+". Pressure adjustment "+((50-match)*.24f).ToString("+0;-0;0")+".");
    }
    public int GuardsNeeded=>EnemyLeft<=0?0:Mathf.Max(0,Mathf.CeilToInt(Breach-PressureRelief));
    public void Emergency(bool reckless) {
        if(Stage!=Phase.Contact||EmergencyResolved||Health<=0)return;EmergencyResolved=true;
        int damage=reckless?random.Next(15,41):random.Next(3,11);
        Health=reckless?Mathf.Max(0,Health-damage):Mathf.Max(1,Health-damage);
        if(reckless)PressureRelief+=8;
        Journal.Add(reckless?"You lead the countercharge: health −"+damage+", pressure −8. This was a potentially fatal choice.":"The guard escorts you out: health −"+damage+". You survive.");
    }
    public void CompleteDuel(bool won,int wounds) {
        if(Stage!=Phase.Contact||EmergencyResolved||Health<=0)return;EmergencyResolved=true;
        int loss=Mathf.Clamp(wounds,0,20);Health=Mathf.Max(1,Health-loss);if(won)PressureRelief+=8;
        Journal.Add("Personal duel: "+(won?"passage held":"guard intervened")+", health lost "+loss+".");
    }
    public void Resolve(bool deploy) {
        if(Finished || (EnemyLeft>0 && (Stage!=Phase.Contact || Round<3)))return;
        int needed=GuardsNeeded;if(needed>0&&deploy&&Guards<needed)return;
        Held=needed==0||deploy;
        if(needed>0&&deploy){Guards-=needed;int kills=Mathf.Min(EnemyLeft,Mathf.RoundToInt(needed*GuardPower));EnemyLeft-=kills;Journal.Add("Guard holds the breach: "+needed+" guards lost; "+kills+" attackers killed.");}
        Stage=Phase.Aftermath;Result=Health<=0?"The Emperor falls at the breach.":Held?"The assault is repulsed. The siege continues.":"This defensive line is abandoned. The siege continues if another line remains.";
        Journal.Add(Result+" Surviving attackers withdraw: "+EnemyLeft+".");
        Outcome=new AssaultResult(this);
        Resolved?.Invoke(this);
    }
}
