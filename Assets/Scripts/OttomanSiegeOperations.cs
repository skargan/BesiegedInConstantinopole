using UnityEngine;
// Executes the stored tactical plan through existing campaign authorities.
public static class OttomanSiegeOperations
{
    static int I(string key)=>DailyLifeEvents.I(key);
    public static string Execute(AI ai,OttomanDirector.Operation op)
    {
        switch(op){
            case OttomanDirector.Operation.Resupply:return "The siege command waits for its next supply convoy.";
            case OttomanDirector.Operation.AwaitOrders:return "Delayed orders hold the siege works.";
            case OttomanDirector.Operation.Infiltrate:
                if(UnityEngine.Random.Range(0,100)<45)VariableSingleton.ChangeInt("iTurkSpies",1);
                return "Night patrols report suspicious movement near the gates. The Ottoman camp is trying to establish new contacts inside the City; its success remains uncertain.";
            case OttomanDirector.Operation.GalataAttack:return PreparationCampaign.AttackGalata();
            case OttomanDirector.Operation.FillMoat:ai.AIFillMoat();break;
            case OttomanDirector.Operation.Bombard:OttomanDirector.UseAmmo();ai.AIBombard();break;
            case OttomanDirector.Operation.Ramp:ai.AIBuildRamp();break;
            case OttomanDirector.Operation.Tower:ai.AIBuildTower();break;
            case OttomanDirector.Operation.Ram:ai.AIBuildRam();break;
            case OttomanDirector.Operation.BalkanAssault:OttomanDirector.MarkAssault();ai.AIBalkanSubjectAssault();break;
            case OttomanDirector.Operation.IrregularAssault:OttomanDirector.MarkAssault();ai.AIBashiBazoukAssault();break;
            case OttomanDirector.Operation.JanissaryAssault:OttomanDirector.MarkAssault();ai.AIJanissaryAssault();break;
            case OttomanDirector.Operation.Reinforce:
                ai.AIMoarCannons();OttomanDirector.AddAmmo(8);VariableSingleton.ChangeInt("iBazouks",150);if(VariableSingleton.CompletedSiegeDays>=21&&I("iOttGrandCannon")==0){VariableSingleton.SetInt("iOttGrandCannon",1);return "A heavy bombard convoy reaches the siege from the imperial foundries. The great cannon adds three percentage points to subsequent wall bombardments. Ammunition +8;150 irregulars arrive.";}return "Imperial convoy: one cannon,8 ammunition and150 irregulars reach the camp.";
            case OttomanDirector.Operation.ArrowScreens:VariableSingleton.SetInt("iOttArrowScreens",1);return "Ottoman carpenters assemble mantlets: movable timber arrow shields. Protected ranks suffer25% less arrow damage until your artillery breaks the screens.";
            case OttomanDirector.Operation.OverlandShips:
                VariableSingleton.ChangeInt("iOttHornWorks",UnityEngine.Random.Range(20,36));if(I("iOttHornWorks")>=100){DefencePosture.SetHornAccess();VariableSingleton.SetInt("iOttHornShips",Mathf.Min(I("iTurkShips"),10));return "Overland slipways are complete. Ottoman vessels enter the Golden Horn behind the chain; review the sea-wall garrison.";}return "Ottoman crews extend the overland slipways. Progress "+I("iOttHornWorks")+"%.";
            case OttomanDirector.Operation.ChainBattery:
                OttomanDirector.UseAmmo();VariableSingleton.ChangeInt("iOttChainDamage",20+(I("iOttGrandCannon")>0?10:0));if(I("iOttChainDamage")>=100){VariableSingleton.ChangeBool("bGoldenHornChain",false);DefencePosture.SetHornAccess();return "Repeated bombardment disables the chain anchorage. The Golden Horn is open.";}return "The Ottoman batteries fire on the chain anchorage. Damage "+I("iOttChainDamage")+"%.";
            case OttomanDirector.Operation.NavalAssault:VariableSingleton.SetInt("iSeaRaidDue",VariableSingleton.CompletedSiegeDays+1);return "COASTAL WARNING: Ottoman landing boats gather for tomorrow. Review the sea-wall garrison; the warning resolves on the next council day.";
            case OttomanDirector.Operation.SabotageFood:
                if(UnityEngine.Random.Range(0,100)<Mathf.Clamp(20+I("iTurkSpies")*5,20,75)){int lost=Mathf.Min(I("iProvisions"),Mathf.Max(1,SupplyEconomy.DailyNeed/2));VariableSingleton.ChangeInt("iProvisions",-lost);return "Fire spreads through a provision store. Enemy infiltrators destroy "+lost+" provisions.";}return "The city watch interrupts an attempt to burn a provision store.";
            case OttomanDirector.Operation.Carcasses:OttomanDirector.UseAmmo();VariableSingleton.ChangePercentFloat("fPeopleRep",-.01f);return "The besiegers hurl carcasses into the City. Burial parties clear them; public morale falls one point. The effect is terror and sanitation pressure, not instant infection.";
            case OttomanDirector.Operation.Advance:VariableSingleton.Instance.EnduringWall.Abandon();VariableSingleton.ChangeFloat("fSiegeRamp",-VariableSingleton.GetFloatVariable("fSiegeRamp"));return "Ottoman troops occupy the abandoned defensive line. The defence falls back.";
            default:OttomanDirector.Observe(true);return "Ottoman scouts and informants survey the defences. Their estimate improves; no assault is launched today.";
        }
        return null;
    }
}
