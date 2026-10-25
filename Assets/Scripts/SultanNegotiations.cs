using System;
using Yarn.Unity;

public static class SultanNegotiations
{
    public static readonly string[] Titles={"Surrender — request terms","Talk of history","Send a gift to Mehmed","Recite poetry","Talk theology","Demand his surrender","Taunt him"};
    public static readonly string[] Effects={"Receive a sealed offer to review. Once per day; no major action.","An exchange about rulers, conquest and inheritance. One conversation per day; no major action.","Choose from eight gifts. Review gold, resources and effect before confirming. Major action. May receive a return gift.","Exchange verses about desire, victory and loss. One conversation per day; no major action.","Discuss providence, judgement and the duties of a ruler. One conversation per day; no major action.","Army morale +3 points · Sultan's fury +5 points · Major action.","Public morale +3 points · Sultan's fury +5 points · Major action."};
    public static bool IsConversation(int i)=>i==0 || i==1 || i==3 || i==4;
    public static bool IsTalk(int i)=>i==1 || i==3 || i==4;
    static int Topic(int i)=>i==1?0:i==3?1:2;
    static int Seen(int topic) {
        int mask=CouncilActions.Int("iSultanSeen"+topic);
        // Older saves presented each topic in sequence.
        int oldCount=Math.Min(Exchanges[topic].Length,CouncilActions.Int("iSultanTalk"+topic));
        return mask | ((1<<oldCount)-1);
    }
    public static string Unavailable(int i) {
        if(i<0 || i>=Titles.Length)return "Unavailable";
        int today=TrainingPool.Day(VariableSingleton.GetDate());
        if(IsTalk(i)) {
            if(!VariableSingleton.HasSiegeStarted())return "Available once the siege begins";
            if(CouncilActions.Int("iSultanConversationDay")==today || CouncilActions.Int("iSultanVisit1")==today || CouncilActions.Int("iSultanVisit3")==today || CouncilActions.Int("iSultanVisit4")==today)return "Conversation held today";
            if(Seen(Topic(i))==(1<<Exchanges[Topic(i)].Length)-1)return "All exchanges read";
        }
        if(i==0 && CouncilActions.Int("iSultanVisit0")==today)return "Terms requested today";

        return null;
    }
    public static bool Available(int i)=>Unavailable(i)==null;
    // Authored alternate-history exchanges, not historical quotations.
    static readonly string[][] Exchanges={
        new[]{
            "You ask what the conqueror owes the people he defeats.\n\nMehmed's reply arrives in a careful Greek hand. ‘A city needs bread the morning after its master changes. Judges must sit. Ships must unload. A man who can only break a gate is a poor conqueror.’\n\nBelow it, in another hand: ‘You can still spare me the necessity of breaking yours.’",
            "You remind him that Rome has survived men who were certain they would possess it.\n\n‘My father had such certainty,’ he replies. ‘Then he withdrew. I have studied his campaign more closely than his victories. Every ditch your men dig enters my calculations. You honour your ancestors by defending their work. I intend to leave work that my descendants will have to defend.’",
            "You ask whether he envies Alexander.\n\n‘His companions could tell him that he was wrong. Consider what courage that required after his first victories.’\n\nYou ask whether his own companions enjoy that liberty.\n\nThe next letter is shorter. ‘They enjoy my attention. Their survival depends on the quality of their advice. Yours will depend on it too.’"
        },
        new[]{
            "You send a verse about the garden that outlives its king.\n\nMehmed returns a pressed flower between the pages. ‘The gardener knows which branch must be cut. The branch has its own opinion.’\n\nThe envoy says the Sultan asked who tends the palace garden. He also asked whether its cisterns remain full. You cannot decide which question was the more intimate.",
            "You write of a lover kept outside a locked door.\n\n‘A familiar torment,’ Mehmed replies. ‘Though your lover appears to have brought unusually large cannon.’\n\nFor a moment, even your exhausted secretary laughs. The Sultan continues: ‘Send another verse tomorrow. Tonight the guns require my attention.’",
            "Your poem remembers the dead by the things they left unfinished: a roof, a letter, a child's lesson.\n\nMehmed's answer comes late. ‘My father left me unfinished things too. I have sometimes wondered whether the dead forgive us for finishing them differently.’\n\nAt dawn his batteries resume their work. The letter remains beside your breakfast."
        },
        new[]{
            "You ask whether victory proves God's favour.\n\n‘Then every murdered righteous man would condemn Heaven,’ Mehmed answers. ‘I do not think your priests teach that. Mine do not.’\n\nYou ask how he will answer for the people killed here.\n\n‘For my orders, I shall answer. For the gates you keep closed, you will have your own account to give.’",
            "You ask what place your churches would have beneath his rule.\n\n‘A ruler who governs only men who pray as he does soon finds himself governing ashes.’\n\nYou press him for guarantees.\n\n‘Bring me men with authority to agree terms. They may ask for seals and witnesses. Until then, you have a letter from a man whose artillery you can hear.’",
            "You send him the story of a judge who refused a king's command.\n\nMehmed asks what became of the judge. You answer that he was executed and remembered as righteous.\n\n‘Then the king lost both his judge and his reputation. A costly morning.’\n\nYour reply asks whether he would have spared him.\n\n‘I would first have listened very carefully to the judgement.’"
        }
    };
    public static bool Act(int index,out string report)
    {
        if(index==2)return SultanGifts.Send(2,out report);
        report="";if(!Available(index)) {report=Unavailable(index);return false;}
        if(IsConversation(index))
        {
            VariableSingleton.SetInt("iSultanVisit"+index,TrainingPool.Day(VariableSingleton.GetDate()));
            if(index==0) {SurrenderTerms.Request();VariableSingleton.Instance.Save();report="The envoy brings a sealed offer. Its terms remain open until the end of this day. The council awaits your decision.";return true;}
            int topic=index==1?0:index==3?1:2;
            VariableSingleton.SetInt("iSultanConversationDay",TrainingPool.Day(VariableSingleton.GetDate()));
            var unread=new System.Collections.Generic.List<int>();int seen=Seen(topic);
            for(int n=0;n<Exchanges[topic].Length;n++)if((seen&(1<<n))==0)unread.Add(n);
            if(UnityEngine.Random.Range(0,100)<5) {
                report="The messenger never reaches the Ottoman camp. At dusk a patrol finds his horse beside the road, the sealed letter still in its saddlebag. There will be no answer today.";
                return true;
            }
            int chosen=unread[UnityEngine.Random.Range(0,unread.Count)];
            VariableSingleton.SetInt("iSultanSeen"+topic,seen|(1<<chosen));
            report=Exchanges[topic][chosen];return true;
        }
        var before=VariableSingleton.Instance.CaptureState();
        if(index==2) {VariableSingleton.ChangeInt("iMoney",-1000);VariableSingleton.ChangePercentFloat("fSultanRage",-.05f);report="Mehmed examines the gifts before dismissing his attendants. ‘Someone in your treasury still has excellent taste. Tell your Emperor I shall remember the courtesy.’ The siege continues, but his reply is measured.";}
        else if(index==5) {VariableSingleton.ChangePercentFloat("fSultanRage",.05f);VariableSingleton.ChangePercentFloat("fTroopRep",.03f);report="Your herald demands that the Sultan lay down his arms. The defenders cheer from the walls. Mehmed sends the herald back unharmed. ‘He delivered his master's words bravely. Let his master show equal courage when I deliver mine.’";}
        else {VariableSingleton.ChangePercentFloat("fSultanRage",.05f);VariableSingleton.ChangePercentFloat("fPeopleRep",.03f);report="Your message asks whether his engineers require directions to the gate. The joke spreads through the City. Mehmed returns a small drawing of the walls, with the damaged sections carefully marked. ‘Please correct any errors. My engineers dislike wasting powder.’";}
        if(index==2) report+=SultanGifts.ReturnGift(UnityEngine.Random.Range(0,100));
        report+="\n\n"+EventOutcome.Describe(before,VariableSingleton.Instance.CaptureState());return true;
    }
}
