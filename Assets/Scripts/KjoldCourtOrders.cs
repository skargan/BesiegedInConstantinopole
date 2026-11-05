using System.Collections.Generic;

// Court sentences use existing service/fate states. No separate plot mechanic.
public static class KjoldCourtOrders
{
    public enum Sentence { None,Exile,Imprison,Execute }
    public const int Storyteller=201;
    public static Sentence For(int person)=>(Sentence)DailyLifeEvents.I("iCourtSentence"+person);
    public static bool Sentenced(int person)=>For(person)!=Sentence.None;
    public static void ResetForRestore(){
        VariableSingleton.SetInt("iKjoldSecurityHeard",0);
        for(int i=0;i<8;i++)VariableSingleton.SetInt("iCourtSentence"+i,0);
        for(int i=0;i<CharacterPreparations.Names.Length;i++)VariableSingleton.SetInt("iCourtSentence"+(100+i),0);
        VariableSingleton.SetInt("iCourtSentence"+Storyteller,0);
    }
    public static bool IsServing(int person){
        if(Sentenced(person))return false;
        if(person>=0&&person<8)return CourtRoster.InService(person);
        if(GardenConversations.IsVisitor(person))return CharacterPreparations.InService&&CharacterPreparations.CommanderId==person;
        return person==Storyteller&&SultanGifts.Known&&(DailyLifeEvents.I("iShahStatus")==4||DailyLifeEvents.I("iShahStatus")==7);
    }
    public static List<int> Targets(){
        var result=new List<int>();for(int i=0;i<8;i++)if(IsServing(i))result.Add(i);
        if(CharacterPreparations.InService)result.Add(CharacterPreparations.CommanderId);
        if(IsServing(Storyteller))result.Add(Storyteller);
        return result;
    }
    public static string Name(int person)=>person==Storyteller?"Shaherezade":GardenConversations.Name(person);
    public static string Stamp(int person)=>For(person)==Sentence.Exile?"EXILED":For(person)==Sentence.Imprison?"IMPRISONED":For(person)==Sentence.Execute?"EXECUTED":"";
    public static bool Available=>KjoldConversations.SecurityHeard&&GardenConversations.Available(GardenConversations.Kjold);
    public static string Review(int person,Sentence sentence){
        string action=sentence==Sentence.Exile?"exile":sentence==Sentence.Imprison?"imprison":"execute";
        return "Order Kjold to "+action+" "+Name(person)+".\n\nTheir imperial service will end, including any active specialist or commander benefit. They will no longer appear for conversations.\n\n"+
            (sentence==Sentence.Imprison?"They will remain in custody. Release is not available through this order.":sentence==Sentence.Exile?"They will be escorted from the City.":"The death sentence cannot be withdrawn after it is carried out.");
    }
    public static bool CarryOut(int person,Sentence sentence,out string report){
        report="This order is no longer available.";
        if(!Available||sentence<Sentence.Exile||sentence>Sentence.Execute||!IsServing(person))return false;
        bool commanded=DailyLifeEvents.I("iCommander")==person;
        VariableSingleton.SetInt("iCourtSentence"+person,(int)sentence);
        if(person<8){
            VariableSingleton.ChangeBool(CourtRoster.Keys[person],false);
            VariableSingleton.SetInt("iCourtState"+person,sentence==Sentence.Exile?6:sentence==Sentence.Imprison?4:3);
        } else if(GardenConversations.IsVisitor(person)){
            VariableSingleton.SetInt("iAdventurerArrived",0);
        } else VariableSingleton.SetInt("iShahStatus",sentence==Sentence.Imprison?6:5);
        if(commanded)VariableSingleton.SetInt("iCommander",-1);
        string name=Name(person);
        report=sentence==Sentence.Exile?"“"+name+" has been escorted from the City, my emperor. Their service is ended.”":
            sentence==Sentence.Imprison?"“"+name+" is under guard. They will receive no visitors without your permission.”":
            "“Your sentence has been carried out, my emperor.”";
        report+="\n\n"+name+" — "+Stamp(person)+".";
        if(commanded)report+="\nCommand of the defence returns to your personal direction.";
        DecisionJournal.Record("court-sentence-"+person,name+" — "+Stamp(person),report,true);
        return true;
    }
}
