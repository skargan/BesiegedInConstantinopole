using System;
using System.Linq;
using System.Text;

// A negotiated intervention disrupts a finite number of real provincial parcels.
// It creates neither a second army simulation nor additional supply deliveries.
// iWbw fields use the existing save/reset boundary; country indices are append-only.
public static class WarInterventions
{
    const string Prefix = "iWbwIntervention";
    static int I(string key) => Math.Max(0, DailyLifeEvents.I(Prefix + key));
    static void Put(string key, int value) => VariableSingleton.SetInt(Prefix + key, Math.Max(0, value));
    static string Parcel(int province, int wave) => province + "_" + wave;
    static bool CountryExists(int country) => country >= 0 && country < CountryDiplomacy.Definitions.Length;
    public static int UsedShipments(int country) => CountryExists(country) ? I("Used_" + country) : 0;
    public static int RemainingShipments(int country) => CountryExists(country) ? Math.Max(0, CountryDiplomacy.Definitions[country].ShipmentBudget - UsedShipments(country)) : 0;
    public static int CountryForShipment(int province, int wave) => I("Country_" + Parcel(province, wave)) - 1;
    public static int DelayForShipment(int province, int wave) => I("Days_" + Parcel(province, wave));
    public static string Summary(int country)
    {
        if (!CountryExists(country)) return "";
        var d = CountryDiplomacy.Definitions[country];
        return d.ShipmentBudget <= 0 ? "No convoy disruption agreed."
            : UsedShipments(country) + " of " + d.ShipmentBudget + " Ottoman deliveries delayed; " + RemainingShipments(country) + " remaining. Each eligible delivery can be delayed by " + d.DelayDays + " days.";
    }

    public static bool TryDelayShipment(int province, int wave, int siegeDay)
    {
        if (!VariableSingleton.HasSiegeStarted() || province < 0 || province >= WarTheatre.ProvinceIds.Length || wave < 0 || wave >= 5) return false;
        string parcel = Parcel(province, wave), id = WarTheatre.ProvinceIds[province];
        int due = ProvinceCampaign.Get("Due" + parcel);
        if (ProvinceCampaign.Get("Done" + parcel) > 0 || due > siegeDay || CountryForShipment(province, wave) >= 0) return false;
        // Rebellions already stop the shipment. Do not claim it again or spend an ally's budget.
        if (ProvinceCampaign.Rebellion(id) || ProvinceCampaign.Free(id)) return false;
        int calendarDue = TrainingPool.Day(VariableSingleton.GetDate()) + due - siegeDay;
        foreach (int country in CountryDiplomacy.ActiveInterventions.OrderBy(CountryDiplomacy.MobilisationDeadline).ThenBy(i => i))
        {
            var d = CountryDiplomacy.Definitions[country];
            if (d.DelayDays <= 0 || RemainingShipments(country) <= 0 || d.TargetProvinces == null || !d.TargetProvinces.Contains(id)) continue;
            // A court mobilising after the original due date cannot reach back in time.
            // Likewise a delivery processed after its entire disruption window is over is not delayed anew.
            if (CountryDiplomacy.MobilisationDeadline(country) > calendarDue || due + d.DelayDays <= siegeDay) continue;
            ProvinceCampaign.Set("Due" + parcel, due + d.DelayDays);
            Put("Used_" + country, UsedShipments(country) + 1);
            Put("Country_" + parcel, country + 1);
            Put("Days_" + parcel, d.DelayDays);
            Put("Due_" + parcel, calendarDue + d.DelayDays);
            Put("Report_" + parcel, 1);
            return true;
        }
        return false;
    }

    // Separate notification from SupplyDay's return value: a report alone must not
    // accidentally turn an otherwise supplied Ottoman day into a foraging stoppage.
    public static string TakeReport()
    {
        var text = new StringBuilder();
        for (int province = 0; province < WarTheatre.ProvinceIds.Length; province++)
            for (int wave = 0; wave < 5; wave++)
            {
                string parcel = Parcel(province, wave); if (I("Report_" + parcel) == 0) continue;
                Put("Report_" + parcel, 0);
                int country = CountryForShipment(province, wave); if (!CountryExists(country)) continue;
                int due = I("Due_" + parcel); var arrival = new DateTime((long)due * TimeSpan.TicksPerDay);
                text.Append("\n").Append(CountryDiplomacy.Definitions[country].Name).Append(" delays an Ottoman delivery from ")
                    .Append(ProvinceCampaign.Name(WarTheatre.ProvinceIds[province])).Append(" by ").Append(DelayForShipment(province, wave))
                    .Append(" days. Its food, troops and military stores are now due on ").Append(arrival.ToString("d MMM yyyy"))
                    .Append(". ").Append(RemainingShipments(country)).Append(" further deliveries can be disrupted by this intervention.\n");
            }
        return text.ToString();
    }
}
