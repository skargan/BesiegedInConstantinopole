using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

// Alternate-history detective fiction. The selected case never changes after opening.
public static class MurderMystery
{
    static int I(string key)=>DailyLifeEvents.I("iLifeMurder"+key);
    static void S(string key,int value)=>VariableSingleton.SetInt("iLifeMurder"+key,value);
    static int Day=>TrainingPool.Day(VariableSingleton.GetDate());
    static readonly int[] Court={-1,2,3,4};
    static readonly string[] Names={"Markos, the household chamberlain","Loukas Notaras","Prince Orhan","George Sphrantzes"};
    public static bool Eligible=>VariableSingleton.HasSiegeStarted()&&VariableSingleton.CompletedSiegeDays>=10&&I("Stage")==0;
    public static IEnumerable<string> OptionalNodes(){if(Eligible)yield return "murder1";}
    public static string DueNode=>I("Stage")>=2&&I("Stage")<=7&&Day>=I("Due")?"murder"+I("Stage"):null;
    static int Culprit=>Mathf.Clamp(I("Culprit")-1,0,3);
    public static int EvidenceCount {get {int n=0,b=I("Evidence");for(int i=0;i<4;i++)if((b&(1<<i))!=0)n++;return n;}}
    static void Evidence(int bit)=>S("Evidence",I("Evidence")|(1<<bit));
    [YarnFunction("MurderSuspect")] public static string Suspect(int n)=>Names[Mathf.Clamp(n,0,3)];
    [YarnFunction("MurderAvailable")] public static bool Available(int n)=>n==0||(n>0&&n<4&&(I("Suspects")&(1<<n))!=0);
    [YarnFunction("MurderCanDelegate")] public static bool CanDelegate()=>CourtRoster.InService(4);
    [YarnFunction("MurderCanSeal")] public static bool CanSeal()=>EvidenceCount>=2;
    [YarnCommand("MurderStart")] public static void Start()
    {
        if(I("Stage")!=0)return;
        var cases=new List<int>{0,0,0};int mask=1;
        if(CouncilActions.Int("iMoney")<15000)cases.Add(0);
        for(int n=1;n<4;n++)if(CourtRoster.InService(Court[n])){
            mask|=1<<n;cases.Add(n);cases.Add(n);cases.Add(n);
            if(CouncilActions.Int("iLoyalty"+Court[n])<40)cases.Add(n);
            if(n==1&&SupplyEconomy.DaysLeft<15)cases.Add(n);
            if(n==3&&CouncilActions.Int("iSpies")>=4)cases.Add(n);
        }
        S("OpeningGold",CouncilActions.Int("iMoney"));S("OpeningFoodDays",SupplyEconomy.DaysLeft);S("OpeningSpies",CouncilActions.Int("iSpies"));
        for(int n=1;n<4;n++)S("OpeningLoyalty"+n,CouncilActions.Int("iLoyalty"+Court[n]));
        S("Suspects",mask);S("Culprit",cases[Random.Range(0,cases.Count)]+1);S("Stage",1);S("Opened",Day);
    }
    static string[] Physical={
        "The white wax contains kitchen chalk. Markos's household office stretches its sealing wax this way; its damaged stamp leaves the same broken lower arc on Petros's sleeve. This establishes access, not murder.",
        "The wax bears the tiny split in Notaras's private household seal. The public seal is intact. Whoever met Petros was acting through the megas doux's family office, not the harbour administration.",
        "The wax closed a packet bearing Orhan's household countersign. A guard copies the sign before the packet is opened. His captain verifies that it was used that night, not merely copied from an old order.",
        "The wax held a narrow packet folded twice inwards, as Sphrantzes's couriers fold their concealed instructions. A dispatch clerk produces an independently dated example with the same damaged seal."};
    static string[] Money={
        "Three households hold the same berth number. Each paid Markos's collector. Petros's private book lists two refunds made from his own wages: he had begun undoing a fraud in which he had participated.",
        "The boat's counterfoil reserves places for dependants of Notaras's household. Beside the payment is an additional sum to recover Petros's register. An innocent family precaution had acquired a second, unexplained purpose.",
        "The ferryman's receipt pays for one outward passenger and one person brought back from an Ottoman-held landing. The money came from Orhan's household. Petros also sold that route to a second customer.",
        "Petros entered the same travellers as paying refugees and as unpaid diplomatic couriers. Both columns lead to Sphrantzes's office. The clerk had started demanding money to keep those two descriptions apart."};
    static string[] Witness={
        "The widow identifies Markos's collector, not a foreign soldier. She remembers him offering to pay the balance of her husband's wages before anyone had told her Petros was dead. The watch records her words separately from its own questions.",
        "A household steward remembers Notaras ordering that Petros be prevented from speaking again. He will not pretend that the words were kinder. His date agrees with the kitchen watch, whose book he had not seen.",
        "Two attendants separately remember Orhan's messenger returning with blood on a cuff and without the register he had been sent to retrieve. Neither knows the ferryman's testimony; their times nevertheless agree.",
        "A courier remembers Sphrantzes refusing a suggestion that Petros merely be detained: a prisoner could still speak. The witness admits stealing from dispatch funds, a separate offence that makes his account uncomfortable rather than worthless."};
    static string[] Order={
        "Markos sends an instruction to the collector: 'The widow must not possess what her husband kept. Do not repeat the mistake of leaving the book behind.' The collector preserves the earlier order naming Petros and directing his death.",
        "Notaras's steward is intercepted carrying a matched pair of counterfoils. The earlier one authorises Petros's death and the recovery of the register. A second hand has added the mutilation; the first order needs no such addition to condemn its author.",
        "Orhan's messenger carries a payment instruction which names Petros, the ferryman and the man hired to kill the clerk. It matches the retained half in the household accounts. The prince's private exchange has left a public trail.",
        "A courier tries to remove the original instruction from the archive. It names Petros and explicitly rejects detention in favour of killing him. The authentication agrees with both the dispatch record and the copy held by the courier."};
    static string[] Defence={
        "Markos looks at the register as though it were an account still capable of balancing. 'They paid for hope. You give it to them every morning without even promising a boat.' His insolence does not explain the order to kill.",
        "Notaras does not ask you to believe he felt nothing. 'The City had an emperor. My household had me.' He admits reserving passage. Whether he also ordered murder must be settled by the documents, not by the ugliness of that admission.",
        "Orhan asks whether imperial protection extends beyond the walls. 'You gave me safety. You did not give it to every person whose life could be used against me.' Fear explains the exchange; it does not acquit the killing.",
        "Sphrantzes folds his hands with an envoy's patience. 'You ask for intelligence, Majesty. No one asks what the intelligence has survived.' You remind him that a dead clerk is not an argument."};
    static string Audiences(){
        string text="";
        for(int n=0;n<4;n++)if(Available(n)){
            if(n>0&&!CourtRoster.InService(Court[n]))text+=Names[n]+" is no longer available for an audience. The household's surviving records must speak in his absence.\n\n";
            else text+=Defence[n]+"\n\n";
        }
        return text;
    }
    public static string Casebook(){
        string result="\n\n<b>The casebook</b>\n";
        if(I("Evidence")==0)return result+"No independently verified evidence has yet been secured. Suspicion is not proof.";
        string[][] clues={Physical,Money,Witness,Order};string[] headings={"Origin of the packet","Money and passage","Independent testimony","Authenticated instruction"};
        for(int n=0;n<4;n++)if((I("Evidence")&(1<<n))!=0)result+="\n"+headings[n]+": "+clues[n][Culprit]+"\n";
        return result;
    }
    [YarnFunction("MurderText")] public static string Text(int stage)
    {
        string text;
        switch(stage){
        case 1:text="<b>Brutal Murder — The Names Beneath the Wax</b>\nThe kitchen boy finds Petros behind the palace ovens, where yesterday's ashes are put out to cool. The petitions clerk's throat has been cut. Someone has damaged his face, then washed his hands with extraordinary care. A little clean wax remains inside one sleeve.\n\nBy noon there is already a monster abroad in the City. Men who never noticed Petros alive explain confidently why he was killed. His widow, Eirene, asks only for the book he carried: people had paid him money, she says, and somebody will have to return it.\n\nMarkos, your household chamberlain, offers to have the passage scrubbed before dinner. The ordinary confidence of the offer troubles you more than the blood.";break;
        case 2:text="<b>The Body and the Bell</b>\nThe surgeon has laid out the clerk's belongings as carefully as a merchant setting out cheap wares. The purse is present. The book is not. The wounds to the face were made after the wound that killed him.\n\n'He was not killed like this,' the surgeon says. 'He was arranged like this.'\n\nOutside, a bell rings. The widow corrects the watchman's account of the hour without looking up. Petros always came home before that bell: it was the signal for their daughter's medicine.";break;
        case 3:text="<b>A Place in the Boat</b>\nThe widow brings a scrap of paper sewn into the lining of her husband's coat. It promises passage for three people. There is no destination.\n\n'He said we must be ready,' she tells you. 'I thought that meant we were going to be saved.'\n\nAt the harbour a second woman holds a receipt with the same number. She has brought a cooking pot, two blankets and a child who keeps asking when they will leave. A collector notices your guards and turns towards the warehouses.";break;
        case 4:text="<b>Four Keys, One Door</b>\nThe room behind the kitchens is used by several households. It was chosen for that reason. A Venetian knife, a Turkish coin and a strip of clerical cloth have all been offered as proof by men who arrived with their conclusions prepared.\n\nYou have the access book brought instead. It records visits from several households. The Turkish coin was left during an earlier delivery; the cloth bound a servant's injured hand. The knife has passed through too many hands to name its owner. Such objects accuse more readily than they explain.\n\n"+Audiences();break;
        case 5:text="<b>The Man Who Confessed</b>\nA porter has confessed. He kneels so quickly that a guard has to keep him from striking his head on the floor. He describes a silver knife and a quarrel at midnight. The surgeon found the fatal wound hours earlier. No one had mentioned a silver knife until the watch captain suggested one.\n\nThe porter admits stealing bread from the kitchen. He appears to believe that, if he admits enough things, one of them will finally satisfy you.\n\nThe captain points out that the City needs an answer. He is right about the need. He has said nothing about the answer.";break;
        case 6:text="<b>A Letter Left Unsealed</b>\nYou still do not possess the missing register. You can make someone believe that you do. A single copied line, left where a household messenger will see it, may bring an instruction to retrieve the rest.\n\nThe widow asks whether this will put her daughter in danger. It is the first sensible question anyone has asked about the plan. You can place them under guard before sending the letter, or make a public accusation and hope the responsible person blunders.\n\nA third course remains: stop setting traps and judge only what you actually know.";break;
        default:text="<b>The Emperor's Account</b>\nA clerk has ruled the last page into two columns: finding and sentence. He waits with his pen raised. Outside, somebody is wheeling bread towards the barracks. The City has continued while you have been trying to understand one death.\n\nThe evidence must determine whom you accuse. Access alone is not a verdict. Two independent strands can support one; a convenient confession cannot. You may also leave the case open.\n\nAn accusation removes the named official from service if still present. Sealing a supported case retains the responsible official but sacrifices public confidence. Neither choice will make Petros come home.";break;
        }
        return text+(stage>1?Casebook():"");
    }
    [YarnCommand("MurderChoose")] public static void Choose(int stage,int choice)
    {
        if(I("Stage")!=stage||stage<1||stage>7||choice<0||choice>(stage==7?5:2))return;
        if(stage==1&&choice==1&&!CanDelegate())return;
        if(stage==7&&choice<4&&!Available(choice))return;
        if(stage==7&&choice==4&&!CanSeal())return;
        S("Choice"+stage,choice+1);S("LastStage",stage);S("LastChoice",choice);
        if(stage==1){if(choice==2){Close(5);VariableSingleton.ChangePercentFloat("fPeopleRep",-.02f);return;}S("Delegated",choice==1?1:0);}
        if(stage==2&&choice<2)Evidence(choice==0?0:2);
        if(stage==3){if(choice<2)Evidence(1);else S("Exposed",1);}
        if(stage==4&&choice==0)Evidence(2);
        if(stage==5){if(choice==2){S("Accused",5);Close(3);VariableSingleton.ChangePercentFloat("fPeopleRep",-.03f);return;}Evidence(2);}
        if(stage==6&&choice==0&&I("Exposed")==0)Evidence(3);
        if(stage==7){
            if(choice==5){Close(4);return;}
            if(choice==4){Close(2);VariableSingleton.ChangePercentFloat("fPeopleRep",-.03f);return;}
            S("Accused",choice+1);S("AccusedPresent",choice==0||CourtRoster.InService(Court[choice])?1:0);bool supported=choice==Culprit&&EvidenceCount>=2;Close(supported?1:3);
            VariableSingleton.ChangePercentFloat("fPeopleRep",supported ? .03f : -.05f);
            if(choice>0&&CourtRoster.InService(Court[choice])){
                VariableSingleton.ChangeBool(CourtRoster.Keys[Court[choice]],false);VariableSingleton.SetInt("iCourtState"+Court[choice],4);
                if(CouncilActions.Int("iCommander")==Court[choice])VariableSingleton.SetInt("iCommander",-1);
            }
            return;
        }
        S("Stage",stage+1);S("Due",Day+(stage==3||stage==5?3:2));
    }
    static void Close(int outcome){S("Outcome",outcome);S("Stage",8);S("Due",0);S("Closed",Day);}
    [YarnFunction("MurderResult")] public static string Result()
    {
        int stage=I("LastStage"),choice=I("LastChoice");
        if(I("Stage")==8)return Ending();
        switch(stage){
        case 1:return choice==0?"You order the room secured and the widow treated as a witness, not a nuisance. Markos asks who will receive tomorrow's petitions. You tell him that this, too, is a petition.":"Sphrantzes accepts the initial inquiry. You require duplicate statements lodged with an independent clerk: no household, including his, is to possess the only account. The investigation remains yours.";
        case 2:return choice<2?(choice==0?Physical[Culprit]:Witness[Culprit])+"\n\nThe surgeon records the distinction between the killing and the later mutilation. Someone wanted the wrong kind of fear.":"The watch seals the room. It is a reasonable precaution; it is not an examination. You preserve the opportunity to investigate, but obtain no verified physical evidence.";
        case 3:return choice<2?Money[Culprit]+"\n\nThe widow and child are moved to a guarded room. The collector is questioned without being told which answer would please the palace.":"The names are read aloud at the harbour. Within the hour the collector disappears and several families burn their receipts. The City now knows that something is wrong. It has become harder to establish exactly what.";
        case 4:return choice==0?Witness[Culprit]:choice==1?Audiences()+"\n\nYou have heard a motive and an explanation. Neither has yet been independently verified.":"The explanation is entered without corroboration. The clerk leaves a space beneath it. You notice that he has not written 'established'.";
        case 5:return "The porter's route cannot fit the time of death. His confession is withdrawn; the theft of bread remains a separate matter.\n\n"+Witness[Culprit];
        default:return choice==0?(I("Exposed")==0?Order[Culprit]+"\n\nThe widow remains under guard. For once, the person asked to take a risk was also given protection.":"You protect the widow and plant the copied line. No messenger comes. The earlier public disclosure gave the household time to burn its papers. You must proceed with the evidence already preserved."):choice==1?"The public accusation produces denials, petitions and a fire in a brazier that has no need of one. No authenticated instruction survives. You have made silence expensive without making it speak.":"You abandon the trap. The papers already in your keeping will have to carry the weight of the verdict. It is better to know their limits than to manufacture certainty.";
        }
    }
    public static string Epilogue(){
        if(I("Stage")==0)return "";
        string text;
        switch(I("Outcome")){
        case 1:text="You established the responsibility of "+Names[Culprit]+" for Petros's murder. When the palace asked to be judged by different laws, you refused it.";break;
        case 2:text="Petros's murder was established and concealed. The responsible household remained useful. The widow was told that usefulness was a reason.";break;
        case 3:text="You gave the City a name to blame: "+(I("Accused")==5?"the porter":Names[Mathf.Clamp(I("Accused")-1,0,3)])+". The papers you preserved did not establish that it was the right one.";break;
        case 4:text="Petros remained a name without a verdict. You preserved the evidence and refused an unsupported conviction.";break;
        case 5:text="A clerk died behind the palace kitchens. His murder was suppressed. The court learned how quickly a death could become an inconvenience.";break;
        default:text="The siege ended before your inquiry into Petros's murder reached a verdict. The unfinished casebook remained among the papers of your reign.";break;
        }
        return "\n\n<b>The names beneath the wax</b>\n"+text;
    }
    public static string Ending(){
        switch(I("Outcome")){
        case 1:return "The independent accounts converge on "+Names[Culprit]+". Your finding distinguishes the private arrangement from the order to murder Petros. The responsible household can no longer hide one behind the other.\n\n"+(I("AccusedPresent")==1?Names[Culprit]+" is placed under guard. His household no longer has authority to receive petitions or collect passage money.":"The finding is entered in the absent official's name. His surviving papers are impounded; a verdict cannot bring him back into your custody.")+" The widow asks for the register, not the sentence. Other families are still waiting for their money.\n\nA supported verdict is entered in the imperial record.";
        case 2:return "You seal the case. Enough has been established to make ignorance an impossible excuse, but the responsible household remains useful.\n\nThe widow is told that wartime requires patience. She asks whose war required her husband to be killed. Nobody writes down the question.\n\nThe proven case is concealed. No official is removed from service.";
        case 3:return "You give the City a culprit: "+(I("Accused")==5?"the porter":Names[Mathf.Clamp(I("Accused")-1,0,3)])+". The record does not establish this verdict. "+(I("Accused")==5||I("AccusedPresent")==1?"The accused is placed under guard. Any command he held is taken from him.":"The accusation is entered in his absence. Your guards have nobody to take into custody.")+"\n\nThe watch calls the case closed. The surgeon writes 'not established' in his own copy and keeps it.\n\nPublic confidence falls. An unproven conviction is recorded; the unanswered questions remain.";
        case 4:return "You refuse to turn uncertainty into a sentence. The documents are preserved and no official is removed.\n\nThe widow thanks you for not condemning another man to make her husband's death easier to explain. She does not thank you for solving it. The case remains unresolved.";
        default:return "The passage is scrubbed. The missing register is described as a matter for the household office. By evening the word 'monster' has become useful again: monsters need no patrons.\n\nThe inquiry is closed. Eirene leaves without the register.";
        }
    }
}


