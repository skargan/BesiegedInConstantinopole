using UnityEngine;



// Character identity is separate from campaign state. Old bool flags remain authoritative.

public static class CourtRoster

{

    public static readonly string[] Keys={"bCharWarrior0","bCharAdmiral1","bCharAdministrator2","bCharNomad3","bCharDiplomat4","bCharArtillerist5","bCharArchitect6","bCharCardinal7","bCharEnemy8"};

    public static readonly string[] Names={"Giovanni Giustiniani Longo","Alvise Diedo","Loukas Notaras","Prince Orhan","George Sphrantzes","Orban","Johannes Grant","Cardinal Isidore","Hasan the Janissary"};

    public static readonly string[] Roles={"Genoese captain · land defence","Venetian captain · naval defence","Megas doux · senior imperial official","Ottoman claimant · allied contingent","Court official · diplomatic adviser","Gunfounder · artillery","Engineer · countermining","Papal legate · church diplomacy","Ottoman soldier · opposing camp"};

    public static readonly string[] Notes={
        "A seasoned captain whose men follow him into the hardest fighting.",
        "A naval captain who brings discipline and coordination to the harbour defence.",
        "The megas doux knows the households, storekeepers and officials who keep the City supplied.",
        "An Ottoman prince whose survival rests on holding the City against Mehmed.",
        "An experienced envoy with contacts in foreign courts and a talent for difficult negotiations.",
        "An ambitious gunfounder who demands good metal, skilled hands and enough powder to prove his work.",
        "An engineer with an eye for weak foundations and concealed approaches.",
        "The Cardinal brings connections in the West and a powerful voice among supporters of Church Union.",
        "A formidable Janissary whose courage draws other men into the assault."
    };
    public static bool InService(int index)=>index>=0 && index<8 && VariableSingleton.GetBoolVariable(Keys[index]);
    public static string CommanderName(int id) {
        if(id>=100 && id<100+CharacterPreparations.Names.Length)return CharacterPreparations.Names[id-100];
        return id>=0&&id<8?Names[id]:"The Emperor · personal direction";
    }
    public static float Bonus(int index,float specialist,float commander)
    {
        float existing=InService(index)?(VariableSingleton.GetIntVariable("iCommander")==index?commander:specialist):0;
        return existing+CharacterPreparations.Bonus(index);
    }
    public static readonly string[] Abilities={
        "Infantry lethality +5%; as commander +15%.",
        "Fleet defending the harbour: ranged lethality +5%; as commander +15%.",
        "Wall repair gold costs -5%; as commander -15%.",
        "Enemy casualties in assaults +5%; as commander +15%.",
        "Aid acceptance +10 points; as commander +20 points.",
        "Cannon lethality +5%; as commander +20%.",
        "Wall defence +2 points; as commander +6 points.",
        "Effective army morale in breach tests +3 points; as commander +10 points.",
        "Enemy champion."
    };
    public static string Status(int index)

    {

        if(index==8) return VariableSingleton.GetBoolVariable(Keys[index])?"Opposing camp":"Inactive in this campaign";

        if(VariableSingleton.GetBoolVariable(Keys[index])) return VariableSingleton.GetIntVariable("iCommander")==index?"In service · appointed commander":"In service";

        int state=VariableSingleton.GetIntVariable("iCourtState"+index);

        return state==6?"Banished":state==2?"Departed / service declined":state==3?"Dead":state==4?"Under guard / imprisoned":state==5?"Serving the Sultan":"Not in service · encounter unresolved";

    }

    public static void Record(string key,bool active)

    {

        int index=System.Array.IndexOf(Keys,key);

        if(index<0) return;

        VariableSingleton.SetInt("iCourtState"+index,active?1:2);

    }

}

