// Narrative presentation only. Indices follow the existing append-only contract roster.
// None of the named captains or companies is presented as a documented formation.
public static class MercenaryCompanyNotes
{
    static readonly string[] Descriptions={
        "Yusuf offers the mounted retainers of a displaced household, with bowmen to support them. Their equipment is costly, and their service is offered without the backing of a sultan.",
        "Pere's soldiers know a deck as well as a street. They bring their own fighting ships, with crossbowmen and boarding parties ready to serve wherever the harbour needs them.",
        "Matteo has gathered Sicilian mariners accustomed to carrying weapons as well as cargo. His brotherhood offers ships and a substantial party of foot soldiers.",
        "Hugues brings armoured horsemen and the men who march with them. The company must find passage from Venice before its white crosses can be seen beneath your walls.",
        "Temir offers horsemen and bowmen from the northern shores of the Black Sea. His captaincy has a reputation for changing its commitments; the broker advises reading the agreement closely.",
        "Ali's following has travelled with its horses and bows through the Anatolian marches. He now seeks employment for men whose former patron can no longer maintain them.",
        "Ibrahim offers three fighting ships and the men to board an enemy vessel. His company comes seeking an employer, not as an embassy from the ruler of Tunis.",
        "Nikola's riders travel light, with a small body of foot soldiers beside them. They seek passage from Venice and employment wherever their horses can still be put to use.",
        "Manuel's company offers bowmen with a small infantry escort. They have assembled in Venice for the passage east, far from the island that gave them their name.",
        "Luca offers crossbowmen and the foot soldiers who guard their firing line. Their contract promises a body of fighting men; it carries no commission from the Bank of Saint George.",
        "Ulrich has brought a company of infantry down from the Alpine country. Their long weapons and close ranks give them a recognisable appearance among the broker's offers.",
        "Rinaldo maintains an expensive mounted retinue, accompanied by a smaller force on foot. His men expect their horses and fine equipment to be maintained as carefully as their pay.",
        "Marco offers infantry and missile troops gathered under a veteran captain. Their battered equipment gives little impression of a parade, but their numbers may be welcome on a long defensive line.",
        "Bartolomeo offers a mixed following at a more negotiable price. His broker has considerably more to say about the company's equipment than about its previous contracts.",
        "Giorgi brings a noble following of riders, infantry and bowmen. They will gather at Caffa before crossing the Black Sea; their name refers to Caucasian Iberia, not Spain.",
        "Jan offers foot soldiers and crossbowmen from the Bohemian lands. They carry the appearance of a country shaped by prolonged religious and military conflict.",
        "Maro commands mariners and soldiers from the Adriatic shores. Two fighting ships belong to the company, allowing it to offer more than a place aboard hired transports.",
        "Andronikos seeks refuge as well as employment. His soldiers bring their households from the Morea, asking for protection and a place in the city alongside their service in its defence.",
        "Brother Antoine offers a small Hospitaller contingent of sergeants and sailors. Its service requires the Order's goodwill, and the same promised detachment cannot be obtained twice.",
        "Master Paolo travels with engineers, two cannons and an armed escort. His establishment offers skilled hands and artillery together, rather than a company formed chiefly for infantry fighting.",
        "Aslan offers a mixed household following from the Circassian coast. Riders, foot soldiers and bowmen will travel together once passage has been arranged.",
        "The captain of Mangup offers a mounted retinue through Theodoro's court. Their agreement belongs to the relations you have established there, rather than the brokers' ordinary monthly offers."
    };
    public static string Description(int id)=>id>=0&&id<Descriptions.Length?Descriptions[id]:"A company seeking service through the mercenary brokers.";
}
