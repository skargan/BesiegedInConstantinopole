using System;
using System.Collections.Generic;
using UnityEngine;

// One bounded assault budget. Decisions change its distribution, never create another army.
public sealed class PhasedAssault
{
    public int Attackers, EnemyLeft, Infantry, Archers, Cavalry, Guards, Health, Ammo, Cannons;
    public int InitialInfantry, InitialArchers, InitialCavalry, InitialGuards, InitialHealth;
    public int ContactBudget,PersonalBudget;
    public int Type, Round, Breach, MainFront, SignalFront, Reserve=50, Shots, TimedMoments;
    public float Wall, ArcherBudget, GunBudget, FootBudget, CavalryBudget, GuardPower, EnemyLethality;
    public float VolleySum, Cover=1, SortieBonus, PressureRelief;
    public bool Frontline, Tower, Ram, Finished, Held=true, Committed, ApproachResolved, ReserveAssigned, SortieResolved;
    public string Result="";
    public readonly List<string> Journal=new List<string>();
    readonly System.Random random;
    public PhasedAssault(int seed){random=new System.Random(seed);MainFront=random.Next(2);SignalFront=random.NextDouble()<.75?MainFront:1-MainFront;}
    public int Dead=>Attackers-EnemyLeft;
    public int OurDead=>InitialInfantry+InitialArchers+InitialCavalry+InitialGuards-Infantry-Archers-Cavalry-Guards;
    public float VolleyMultiplier=>1+(Shots==0?0:VolleySum/Shots);
    public static float VolleyScore(float position)=>Mathf.Clamp(.25f-Mathf.Abs(position-.5f),-.25f,.25f);
    public void Volley(float score){if(Shots>=3||ApproachResolved)return;VolleySum+=Mathf.Clamp(score,-.25f,.25f);Shots++;}
    public void Shelter(float timing) {
        // Too early sacrifices firing time; late exposure causes bounded casualties.
        Cover=timing<.45f?.9f:timing>.8f?1.1f:1f;
        int lost=timing>.8f?Mathf.Min(Archers,Mathf.CeilToInt(Archers*.015f)):0;Archers-=lost;
        Journal.Add("Counter-bombardment: "+(lost>0?lost+" archers lost taking cover late.":Cover<1?"early cover; archer output −10%.":"the parapet is cleared before impact."));
    }
    public void Approach(float moat,float fire) {
        if(ApproachResolved)return;ApproachResolved=true;
        float arrows=ArcherBudget*VolleyMultiplier*Cover*(InitialArchers==0?0:(float)Archers/InitialArchers);
        int killed=Mathf.Clamp(Mathf.RoundToInt(arrows+GunBudget+Attackers*(moat>0?.1f:0)+fire),0,EnemyLeft);EnemyLeft-=killed;ContactBudget=EnemyLeft;
        Journal.Add("Approach: "+killed+" enemy losses. Archers "+arrows.ToString("0")+" (volley ×"+VolleyMultiplier.ToString("0.00")+"), guns "+GunBudget.ToString("0")+"; "+EnemyLeft+" reach the walls.");
    }
    public void Sortie(int strength,float exposure) {
        if(SortieResolved)return;SortieResolved=true;if(strength==0){Journal.Add("The gates remain shut. The reserve stays within the walls.");return;}
        int committed=Mathf.Min(Infantry,strength==1?100:300);
        float chance=Mathf.Clamp01(.25f+exposure*.6f+(strength==2?.15f:0));
        int lost=Mathf.Min(Infantry,Mathf.CeilToInt(committed*(.02f+exposure*exposure*.18f)));Infantry-=lost;
        bool success=committed>0&&random.NextDouble()<chance;
        if(success){Tower=false;Ram=false;PressureRelief+=12;SortieBonus+=committed*.25f;}
        Journal.Add("Sortie: "+committed+" sent, "+lost+" lost; "+(success?"the siege engine burns. Breach pressure −12.":"the raiders withdraw without destroying the engine."));
    }
    public void Clash() {
        if(Round>=3||EnemyLeft<=0||Finished)return;
        int engaged=Mathf.Min(EnemyLeft,ContactBudget/3+(Round<ContactBudget%3?1:0));
        float exposure=Mathf.Clamp01(1-Wall+(Frontline?.05f:0));
        int loss=Mathf.Min(Infantry+Archers+Cavalry,Mathf.Max(0,Mathf.RoundToInt(engaged*EnemyLethality*exposure)));
        int before=Infantry+Archers+Cavalry;
        int foot=Mathf.Min(Infantry,Mathf.CeilToInt(loss*.75f));Infantry-=foot;
        int bow=Mathf.Min(Archers,loss-foot);Archers-=bow;int horse=Mathf.Min(Cavalry,Mathf.Max(0,loss-foot-bow));Cavalry-=horse;
        Infantry-=Mathf.Min(Infantry,Mathf.Max(0,loss-foot-bow-horse));
        loss=before-Infantry-Archers-Cavalry;
        float strength=InitialInfantry==0?0:(float)Infantry/InitialInfantry;
        float damage=(FootBudget*strength+CavalryBudget*(InitialCavalry==0?0:(float)Cavalry/InitialCavalry)+SortieBonus)/3f*(Frontline?1.08f:1);
        int personal=Frontline?PersonalBudget/3+(Round<PersonalBudget%3?1:0):0;
        int slain=Mathf.Clamp(Mathf.RoundToInt(damage)+personal,0,EnemyLeft);EnemyLeft-=slain;Round++;
        Journal.Add("Contact "+Round+": "+slain+" enemy dead; "+loss+" defenders lost. Personal contribution "+personal+"; melee output "+damage.ToString("0")+"; "+engaged+" attackers engaged at lethality "+EnemyLethality.ToString("0.##")+", exposure "+(exposure*100).ToString("0")+"%; "+EnemyLeft+" attackers still fighting.");
    }
    public void AssignReserve(int left) {
        if(ReserveAssigned)return;ReserveAssigned=true;Reserve=Mathf.Clamp(left,0,100);
        int match=MainFront==0?Reserve:100-Reserve;
        PressureRelief+=(match-50)*.24f;
        Journal.Add("Reserve: "+Reserve+"% to the gate, "+(100-Reserve)+"% to the curtain. Main effort: "+(MainFront==0?"gate":"curtain")+". Pressure adjustment "+((50-match)*.24f).ToString("+0;-0;0")+".");
    }
    public int GuardsNeeded=>EnemyLeft<=0?0:Mathf.Max(0,Mathf.CeilToInt(Breach-PressureRelief));
    public void Emergency(bool reckless) {
        if(Health<=0)return;
        int damage=reckless?random.Next(15,41):random.Next(3,11);
        Health=reckless?Mathf.Max(0,Health-damage):Mathf.Max(1,Health-damage);
        if(reckless)PressureRelief+=8;
        Journal.Add(reckless?"You lead the countercharge: health −"+damage+", pressure −8. This was a potentially fatal choice.":"The guard escorts you out: health −"+damage+". You survive.");
    }
    public void Resolve(bool deploy) {
        if(Finished)return;
        int needed=GuardsNeeded;if(needed>0&&deploy&&Guards<needed)return;
        Held=needed==0||deploy;
        if(needed>0&&deploy){Guards-=needed;int kills=Mathf.Min(EnemyLeft,Mathf.RoundToInt(needed*GuardPower));EnemyLeft-=kills;Journal.Add("Guard holds the breach: "+needed+" guards lost; "+kills+" attackers killed.");}
        Finished=true;Result=Health<=0?"The Emperor falls at the breach.":Held?"The assault is repulsed. The siege continues.":"This defensive line is abandoned. The siege continues if another line remains.";
        Journal.Add(Result+" Surviving attackers withdraw: "+EnemyLeft+".");
    }
}
