using System;
using UnityEngine;
using Yarn.Unity;

public static class CharacterPreparations
{
    public static readonly string[] Names={"Stephen of Moldavia","Giosafat Barbaro","Bertrandon de la Broquière","Giovanni Fontana","Cyriacus of Ancona","Pero Tafur"};
    public static readonly string[] Profiles={
        "A young Moldavian prince with a dispossessed family's claim and a future still to win. He watches the council as closely as the enemy camp.",
        "A Venetian merchant and traveller familiar with the Black Sea world. He knows how a rumour travels, what a caravan carries and which promises a merchant will believe.",
        "A Burgundian traveller who crossed Ottoman lands and returned with an observer's eye for roads, soldiers and the habits of their commanders.",
        "A physician and inventor whose drawings mix practical devices with startling ambitions. He wants craftsmen, materials and a chance to put his designs to work.",
        "An antiquarian who reads the City's stones as other men read letters. He sees an inheritance worth saving in its inscriptions, books and broken monuments.",
        "A Castilian knight and traveller who has seen Constantinople before. He returns with the memory of its hospitality and a nobleman's appetite for distinction."
    };
    public static readonly string[] Roles={"Prince · infantry leadership","Merchant-envoy · foreign aid","Reconnaissance adviser · wall defence","Inventor · artillery","Scholar · garrison morale","Knight · assault tactics"};
    public static readonly string[] Abilities={"Infantry lethality +4%; as commander +12%.","Aid acceptance +5 points; as commander +12 points.","Wall defence +1 point; as commander +4 points.","Cannon lethality +4%; as commander +12%.","Effective army morale in breach tests +2 points; as commander +6 points.","Enemy casualties in assaults +3%; as commander +9%."};
    static readonly int[] Channels={0,4,6,5,7,3};
    static readonly float[] Specialist={.04f,5,1,.04f,2,.03f},Command={.12f,12,4,.12f,6,.09f};
    public static bool InService=>Invited && I("iAdventurerArrived")>0;
    public static int Candidate=>I("iAdventurerChoice")-1;
    public static int CommanderId=>100+Candidate;
    public static int Loyalty=>DailyLifeEvents.I("iAdventurerLoyalty",50);
    public static bool IsCommander=>InService && I("iCommander")==CommanderId;
    public static float Bonus(int channel) => InService && Channels[Candidate]==channel ? (IsCommander?Command[Candidate]:Specialist[Candidate]) : 0;
    [YarnFunction("VisitorInService")] public static bool Available()=>InService;
    [YarnFunction("VisitorName")] public static string Name()=>Invited?Names[Candidate]:"No visiting adviser";
    [YarnFunction("VisitorAbility")] public static string Ability()=>Invited?Abilities[Candidate]:"";
    [YarnCommand("AppointVisitor")] public static void AppointFromEvent(){Appoint(out var ignored);}
    public static bool Appoint(out string report) {
        if(!InService){report="The invited adviser has not arrived.";return false;}
        if(IsCommander){report=Names[Candidate]+" already commands the defence.";return false;}
        VariableSingleton.SetInt("iCommander",CommanderId);
        report=Names[Candidate]+" takes command of the siege defence.\n"+Abilities[Candidate]+"\nThe command benefit replaces this adviser's ordinary specialist benefit.";return true;
    }
    static int I(string key)=>CouncilActions.Int(key);
    public static bool Invited=>I("iAdventurerChoice")>0 && I("iAdventurerChoice")<=Names.Length;
    public static string Status=>I("iAdventurerArrived")>0?(IsCommander?"In service · siege commander":"In service"):"Expected "+new DateTime((long)I("iAdventurerDue")*TimeSpan.TicksPerDay).ToString("d MMM yyyy");
    public static void Initialize(){VariableSingleton.SetInt("iAdventurerLoyalty",50);foreach(string key in new[]{"iTurkPolicy","iAdventurerChoice","iAdventurerDue","iAdventurerArrived"})VariableSingleton.SetInt(key,0);}
    [YarnCommand("TurkishHouseholds")]
    public static void TurkishHouseholds(int policy)
    {
        if(policy<1||policy>3||I("iTurkPolicy")!=0)return;
        VariableSingleton.SetInt("iTurkPolicy",policy);
        VariableSingleton.ChangeBool("bCharNomad3",policy==3);
        if(policy==1)return;
        VariableSingleton.ChangeInt("iInfantry",policy==2?150:300);
        VariableSingleton.ChangeInt("iTurkSpies",policy==2?1:2);
        if(policy==2)VariableSingleton.SetInt("iCourtState3",4);
        else VariableSingleton.ChangePercentFloat("fNobleRep",-.05f);
    }
    [YarnCommand("InviteAdventurer")]
    public static void Invite(int index)
    {
        if(index<0||index>=Names.Length||Invited)return;
        VariableSingleton.SetInt("iAdventurerChoice",index+1);
        VariableSingleton.SetInt("iAdventurerDue",TrainingPool.Day(VariableSingleton.GetDate().AddDays(30)));
    }
    public static string Advance()
    {
        if(!Invited||I("iAdventurerArrived")>0||TrainingPool.Day(VariableSingleton.GetDate())<I("iAdventurerDue"))return "";
        VariableSingleton.SetInt("iAdventurerArrived",1);
        return Names[I("iAdventurerChoice")-1]+" has arrived and awaits an audience.\n\n"+Profiles[I("iAdventurerChoice")-1]+"\n"+Abilities[Candidate]+"\nLoyalty: "+Loyalty+" / 100. Available for appointment in Court.\n\n";
    }
}
