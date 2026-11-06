using UnityEngine;
using Yarn.Unity;
public static class ChurchCourt
{
    public const int Gennadios=203;
    public const string ArrivalNode="cardinalReturns";
    public static bool GennadiosAvailable=>DailyLifeEvents.I("iGennadiosPresent")==1;
    public static bool ArrivalDue=>PreparationCampaign.Active && VariableSingleton.Turn==9 && DailyLifeEvents.I("iCardinalReceptionDone")==0;
    public static void Initialize(){VariableSingleton.SetInt("iGennadiosPresent",1);VariableSingleton.SetInt("iLoyalty203",50);VariableSingleton.SetInt("iCardinalReceptionDone",0);VariableSingleton.ChangeBool("bCharCardinal7",false);}
    public static bool Receive(int choice)
    {
        if(!ArrivalDue || choice<0 || choice>2)return false;
        VariableSingleton.ChangeBool("bCharCardinal7",true);VariableSingleton.SetInt("iCardinalReceptionDone",1);
        if(choice==0){PreparationCampaign.Loyalty(7,5);PreparationCampaign.Loyalty(Gennadios,-5);}
        if(choice==1){PreparationCampaign.Loyalty(7,-5);PreparationCampaign.Loyalty(Gennadios,10);}
        VariableSingleton.SetInt("iLifeLocationTurn",0);
        DecisionJournal.Record("cardinal-reception","The Cardinal Returns",choice==0?"The Emperor personally welcomed Isidore. Isidore loyalty +5; Gennadios loyalty -5.":choice==1?"Isidore waited outside the palace until he changed into Orthodox robes. Isidore loyalty -5; Gennadios loyalty +10.":"Palace officials received Isidore. Neither churchman's loyalty changed.");return true;
    }
    [YarnCommand("CardinalReception")] public static void Reception(int choice){Receive(choice);}
    public static void ResolveUnion(){VariableSingleton.SetInt("iGennadiosPresent",DailyLifeEvents.I("iReligion")==1?0:1);VariableSingleton.SetInt("iLifeLocationTurn",0);}
    public const string Biography="‘My name was Georgios before I entered the monastery. I was born here in Constantinople, though my family came from Thessaly. They had sufficient means to provide me with an education, and I devoted myself to philosophy, theology and the law. I taught, served as a judge, and worked in your brother's government. I have spent much of my life among books and men who would have benefited from reading them more carefully.’\n\n‘I accompanied Emperor John to Italy when we sought reconciliation with the Latin Church. I understand why he went. I understand the danger which drove him there. I have studied Latin and read their theologians; some possess considerable learning. My opposition does not arise from ignorance of their arguments. I came to believe that the agreement reached at Florence could not be accepted without compromising the faith entrusted to us. Mark of Ephesus strengthened that conviction. Since his death, I have continued the struggle as best I can.’\n\n‘Now I am a monk, Majesty. I write, teach, and answer those who seek my judgement. I hold no command over your soldiers, but there are people in this City who trust me to speak when they fear their own voices will not be heard. I would prefer to spend my remaining years studying in peace. That does not appear to be among the choices offered to us. You must defend the City against the Sultan. I must see that, in our desperation to preserve its walls, we do not surrender everything those walls were built to shelter.’";
}
