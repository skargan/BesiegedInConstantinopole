using System;
using System.Text;

public static class CampaignPersonnelCalendar
{
    // Preparation turns can cover a month. Resolve each intervening day using the
    // people actually present that day, before either medical or research work.
    public static string Advance(DateTime previous,DateTime current)
    {
        if(current.Date<=previous.Date)return "";
        var reports=new StringBuilder();int discoveries=0;bool researching=false,medicalActivity=false;
        long medicalReturns=0,medicalProvisions=0;
        for(var date=previous.Date.AddDays(1);date<=current.Date;date=date.AddDays(1)) {
            reports.Append(TrainingPool.CompleteDueOrders(date));
            reports.Append(SpecialistPersonnel.Advance(date));
            MonasticResearch.AllocationsForDay(date);
            string medical=MedicalRecovery.Advance(date);
            // Patients may finish well before a preparation month ends. Aggregate
            // every treated day instead of silently losing those earlier returns.
            if(!string.IsNullOrEmpty(medical)){
                medicalActivity=true;medicalReturns+=DailyLifeEvents.I("iMedicalLastReturned");medicalProvisions+=DailyLifeEvents.I("iMedicalLastCost");
            }
            researching|=MonasticResearch.ResearchMonks>0;
            discoveries+=MonasticResearch.Advance(date);
            reports.Append(CountryDiplomacy.Advance(date));
        }
        if(medicalActivity)reports.Append("Infirmary: ").Append(medicalReturns.ToString("N0")).Append(" recovered patients return to duty; ")
            .Append(medicalProvisions.ToString("N0")).Append(" provisions used; ").Append(MedicalRecovery.Total.ToString("N0")).Append(" remain wounded.")
            .Append(MedicalRecovery.Total>0&&DailyLifeEvents.I("iMedicalLastWaiting")>0?" Some treatment is paused: insufficient infirmary monks or provisions.":"").Append("\n\n");
        reports.Append(MonasticResearch.DiscoveryReport(discoveries,researching));
        return reports.ToString();
    }
}
