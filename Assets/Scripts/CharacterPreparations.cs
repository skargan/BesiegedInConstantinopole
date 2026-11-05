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
    public static bool InService=>Invited && !KjoldCourtOrders.Sentenced(CommanderId) && I("iAdventurerArrived")>0;
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
    static int I(string key)=>DailyLifeEvents.I(key);
    public static bool Invited=>I("iAdventurerChoice")>0 && I("iAdventurerChoice")<=Names.Length;
    public static string Status=>I("iAdventurerArrived")>0?(IsCommander?"In service · siege commander":"In service"):"Expected "+new DateTime((long)I("iAdventurerDue")*TimeSpan.TicksPerDay).ToString("d MMM yyyy");
    public static void Initialize(){VariableSingleton.SetInt("iAdventurerLoyalty",50);foreach(string key in new[]{"iTurkPolicy","iTurkConfiscatedGold","iTurkishBattalionCreated","iTurkScreeningIntercepted","iAdventurerChoice","iAdventurerDue","iAdventurerArrived"})VariableSingleton.SetInt(key,0);}
    [YarnFunction("TurkishHouseholdsResolved")] public static bool TurksResolved()=>I("iTurkPolicy")!=0;
    [YarnCommand("TurkishHouseholds")]
    public static void TurkishHouseholds(int policy)
    {
        if(policy<1||policy>5||TurksResolved()||(policy==4&&I("iFocus")!=1)||(policy==5&&I("iFocus")!=4))return;
        VariableSingleton.SetInt("iTurkPolicy",policy);
        VariableSingleton.ChangeBool("bCharNomad3",policy!=1);
        VariableSingleton.SetInt("iCourtState3",policy==1?3:1);
        VariableSingleton.SetInt("iLoyalty3",policy==1?0:policy==2?50:90);
        if(policy==1){
            int gold=UnityEngine.Random.Range(500,3001);VariableSingleton.SetInt("iTurkConfiscatedGold",gold);
            VariableSingleton.ChangeInt("iMoney",gold);VariableSingleton.ChangePercentFloat("fPeopleRep",.05f);
        }else{
            VariableSingleton.ChangeInt("iInfantry",policy==2?150:policy==4?600:300);
            if(policy==3||policy==4){VariableSingleton.ChangeInt("iTurkSpies",policy==3?1:2);VariableSingleton.ChangePercentFloat("fNobleRep",-.05f);}
            if(policy==4){
                VariableSingleton.ChangeInt("iCavalry",50);VariableSingleton.ChangeInt("iArchers",200);
                foreach(string key in new[]{"fTroopRep","fPeopleRep","fPriestRep"})VariableSingleton.ChangePercentFloat(key,-.05f);
                VariableSingleton.SetInt("iTurkishBattalionCreated",1);
                DecisionJournal.Record("turkish_battalion_created","Turkish Battalion Created","Orhan commands a Turkish battalion raised for the defence, including additional Turkish volunteers and mercenaries offered pay and sanctuary.",true);
            }
            if(policy==5)EspionageSystem.RecordRecruitScreening();
        }
        DecisionJournal.Record("our_turks_ruling","Our Turks",TurksOutcome(),true);
    }
    [YarnFunction("TurkishHouseholdsOutcome")]
    public static string TurksOutcome(){switch(I("iTurkPolicy")){
        case 1:return "The Turkish households leave under escort, their property taken into the treasury. Confiscation yields "+I("iTurkConfiscatedGold").ToString("N0")+" gold. The petitioners welcome your ruling. Orhan loses imperial protection; word later reaches the court that Mehmed has had him executed. His service is lost permanently.";
        case 2:return "One hundred and fifty volunteers enter Roman companies. Orhan accepts a limited command under imperial officers, though he remains cautious about the court's intentions. Loyalty: 50 / 100.";
        case 3:return "You place the households under imperial protection. Three hundred men swear service under Orhan, who receives his commission with evident relief. Several nobles object to the privileges granted. Orhan's loyalty: 90 / 100.";
        case 4:return "Orhan forms the Turkish Battalion. Volunteers, mercenaries, fugitives and other opponents of Mehmed answer the promise of pay and sanctuary: 600 infantry, 200 archers and 50 cavalry enter service. Opposition spreads through the army, the streets, the clergy and the noble households. Orhan's loyalty: 90 / 100.";
        case 5:return "Three hundred of Orhan's followers enter imperial service after screening. Your agents identify a suspected Ottoman operative among the applicants and detain him before he can establish himself inside the City. The households receive protection and Orhan his command. His loyalty: 90 / 100.";
        default:return "The court awaits your ruling.";
    }}
    [YarnCommand("InviteAdventurer")]
    public static void Invite(int index)
    {
        if(index<0||index>=Names.Length||Invited)return;
        VariableSingleton.SetInt("iAdventurerChoice",index+1);
        VariableSingleton.SetInt("iAdventurerDue",TrainingPool.Day(VariableSingleton.GetDate().AddDays(30)));
    }
    public static string Advance()
    {
        if(!Invited||KjoldCourtOrders.Sentenced(CommanderId)||I("iAdventurerArrived")>0||TrainingPool.Day(VariableSingleton.GetDate())<I("iAdventurerDue"))return "";
        VariableSingleton.SetInt("iAdventurerArrived",1);
        return Names[I("iAdventurerChoice")-1]+" has arrived and awaits an audience.\n\n"+Profiles[I("iAdventurerChoice")-1]+"\n"+Abilities[Candidate]+"\nLoyalty: "+Loyalty+" / 100. Available for appointment in Court.\n\n";
    }
}
