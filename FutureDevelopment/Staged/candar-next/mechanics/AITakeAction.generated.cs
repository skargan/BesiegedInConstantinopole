public partial class AI {
    public void TakeAIAction()
    {
        OttomanDirector.PrepareTurn();
        if(OttomanDirector.ExecutedThisTurn)return;
        PendingPhased=null;
        WasAssault = false;
        AwaitingBreachDecision = false;
        LastReport = "No major Ottoman operation was completed today.";
        assaultUI.SetAssaultResultButton(true);
        enemyLossDisplay.SetLossesInfo(0, 0);
        assaultSystem.ClearLossDisplay();
        if (!VariableSingleton.HasSiegeStarted()) { LastReport="The siege has not begun. No Ottoman attack takes place."; return; }
        if(!OttomanDirector.TryExecute(out var planned))return;
        string supplyReport=CouncilActions.ResolveEnemySupplies();
        if (VariableSingleton.GetIntVariable("iEnemyDelay")>0)
        {
            VariableSingleton.ChangeInt("iEnemyDelay",-1);
            LastReport="Ottoman siege operations are delayed this turn. "+VariableSingleton.GetIntVariable("iEnemyDelay")+" more days of delay remain. Food consumption and naval pressure continue."+(string.IsNullOrEmpty(supplyReport)?"":"\n"+supplyReport);
            return;
        }
        if (!string.IsNullOrEmpty(supplyReport)) { LastReport=supplyReport; return; }
        if(DailyLifeEvents.I("iLifeSurprise")==1)
        {
            VariableSingleton.SetInt("iLifeSurprise",0);
            if(VariableSingleton.GetIntVariable("iBazouks")>0){OttomanDirector.MarkAssault();AIBashiBazoukAssault();return;}
        }
        // Paid Candar postponement applies only after supply, existing delays and forced attacks.
        if(CandarDiplomacy.DelayOperation(planned,out var candarReport)){LastReport=candarReport;return;}
        string operationReport=OttomanSiegeOperations.Execute(this,planned);
        if(operationReport!=null)LastReport=operationReport;
    }


}
