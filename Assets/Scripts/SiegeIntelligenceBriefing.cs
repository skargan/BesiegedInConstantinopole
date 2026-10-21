public static class SiegeIntelligenceBriefing
{
    public static string Text()
    {
        if(!VariableSingleton.HasSiegeStarted())return "The siege has not begun. There is no daily Ottoman operation to report yet.\n\nAgents improve access to enemy information; the intelligence cards show what your present network can establish.";
        string report=AI.Instance==null?"":AI.Instance.LastReport;
        string actual=string.IsNullOrEmpty(report)?"No current operation report is available. Advance the turn to receive the next report.":report;
        string advice="A quiet report does not establish that reinforcements arrived or food was gathered. Those actions must be reported separately. Watch the next operation before committing your reserves.";
        if(report.Contains("provisions"))advice="Resupply is a real interruption to Ottoman attacks. Their stores recover; destroying food again can force another supply day. Exact stock information depends on your agents.";
        else if(report.Contains("delayed"))advice="Land operations are blocked for the stated delay. Naval pressure and your own food consumption continue; use the interval for repairs or supply.";
        else if(report.Contains("bombardment")||report.Contains("cannon"))advice="Enemy guns threaten the active wall. Compare its remaining integrity with the artillery pressure, then consider repairs or counter-battery fire at Defence of the walls.";
        else if(report.Contains("moat"))advice="The ditch is losing protection. This prepares the approach to the walls; it is not an infantry assault by itself.";
        else if(report.Contains("ramp")||report.Contains("tower")||report.Contains("ram"))advice="Enemy siege works have progressed. Check the overview's siege-work indicators and prepare your defenders; construction alone does not mean an assault has already occurred.";
        else if(report.Contains("assault")||report.Contains("breach"))advice="Read the battle's recorded casualties and wall outcome before replacing losses. A repelled attack does not repair the damaged wall.";
        return "<b>REPORTED ACTION</b>\n"+actual+"\n\n<b>COUNCIL ASSESSMENT</b>\n"+advice;
    }
}
