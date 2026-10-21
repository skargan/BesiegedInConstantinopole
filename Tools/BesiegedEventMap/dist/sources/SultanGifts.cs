using UnityEngine;
using Yarn.Unity;

public static class SultanGifts
{
    static int Get(string suffix)=>CouncilActions.Int("iShah"+suffix);
    static void Set(string suffix,int value)=>VariableSingleton.SetInt("iShah"+suffix,value);
    static int Today=>TrainingPool.Day(VariableSingleton.GetDate());
    public static bool Known=>Get("Stage")>0;
    public static string PendingNode=>Get("Stage")>=1 && Get("Stage")<=3 && Get("Due")<=Today ? "shah"+Get("Stage") : null;
    // One roll per paid exchange. Identity is drawn once and saved with all other campaign state.
    public static string ReturnGift(int roll)
    {
        if(roll<0 || roll>=35) return "";
        if(roll<10 && !Known) {
            Set("Stage",1);Set("Due",Today);Set("Identity",Random.Range(0,2));Set("Status",1);
            return "\n\nThe returning envoy brings a young woman named Shaherezade, a slave from the Sultan's household. She carries a book bound in green leather. Mehmed's letter describes her as an accomplished storyteller: ‘You have sent me pleasant things. Here is someone who may make your evenings bearable.’ She waits with the chamberlain for your decision.";
        }
        if(roll<18) {VariableSingleton.ChangeInt("iProvisions",40);return "\n\nMehmed returns baskets of figs, rice and preserved fruit. ‘Your cooks may appreciate a change.’ Provisions +40.";}
        if(roll<27) {VariableSingleton.ChangeInt("iSecrets",1);return "\n\nA manuscript arrives with the Sultan's compliments. Between its pages your secretary finds a discarded quartermaster's account. Whether it was left there deliberately is unclear. Secrets +1.";}
        VariableSingleton.ChangeInt("iMoney",300);return "\n\nThe envoy delivers a silver cup. ‘For the wine you will drink when this is over,’ reads the note. Your treasurer values it at 300 gold.";
    }
    public static string Status {
        get {string[] names={"Unknown","Awaiting an audience","Under palace guard","Guest in the palace","Court storyteller","Departed","Prisoner","Working with your spies"};return names[Mathf.Clamp(Get("Status"),0,7)];}
    }
    public static string Note=>Get("Stage")==4 ? (Get("Status")==7?"She has disclosed her Ottoman contact. Your agents have taken over the correspondence.":Get("Status")==4?"Cleared of suspicion. She reads and tells stories to the palace household.":"Her audience and its consequences are recorded in the palace ledger.") : "Sent from Mehmed's household with a book of tales. The chamberlain is responsible for her quarters.";
    static void Next(int stage) {Set("Stage",stage);Set("Due",Today+2);}
    [YarnCommand("ShahChoose")]
    public static void Choose(string choice)
    {
        int stage=Get("Stage");
        if(choice=="return" && stage>=1 && stage<=3) {Set("Status",5);Set("Stage",4);return;}
        if(stage==1) {
            if(choice=="guard") {Set("Status",2);Next(2);}
            if(choice=="free") {Set("Status",3);Set("Mercy",1);Next(2);}
        } else if(stage==2) {
            if(choice=="investigate" && CouncilActions.Int("iSecrets")>=1) {VariableSingleton.ChangeInt("iSecrets",-1);Set("Evidence",1);Next(3);}
            if(choice=="trust") {Set("Access",1);Set("Status",3);Next(3);}
            if(choice=="watch") {Set("Access",0);Set("Status",2);Next(3);}
        } else if(stage==3) {
            if(choice=="recruit" && Get("Identity")==1 && (Get("Evidence")==1 || Get("Mercy")==1)) {Set("Status",7);VariableSingleton.ChangeInt("iSecrets",2);Set("Stage",4);}
            if(choice=="prison") {Set("Status",6);Set("Stage",4);}
            if(choice=="storyteller" && Get("Identity")==0) {Set("Status",4);VariableSingleton.ChangeInt("iSecrets",1);Set("Stage",4);}
        }
    }
    [YarnCommand("ShahAttempt")]
    public static void Attempt() {
        if(Get("Stage")!=3 || Get("Attempted")==1) return;
        Set("Attempted",1);
        if(Get("Identity")==1 && Get("Access")==1 && Get("Evidence")==0) VariableSingleton.ChangePercentFloat("fHealth",-.35f);
    }
}
