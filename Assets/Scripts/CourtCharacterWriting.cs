// Dramatic invention for the game, never presented as historical quotation.
public static class CourtCharacterWriting
{
    static int Index(int p){if(p==200)return 14;return p>=100?p-92:p;}
    static readonly string[] ArrivalWords={
        "‘I came with men who had agreed to follow me, not with a guarantee from Genoa. You may find the distinction troublesome. I find it useful whenever somebody explains what my city has supposedly promised.’",
        "‘Venetian business brought ships here long before this siege. Now the men at the oars have become a garrison. A man may choose his berth and discover that history has chosen his shore.’",
        "‘I did not arrive at the City as you mean it. I grew into offices, marriages, obligations. One morning I was an old man explaining why none of them could be moved without disturbing the others.’",
        "‘I was useful here because I was dangerous elsewhere. That is a kind of welcome, though the guest learns to count the doors. I stayed because leaving did not offer the freedom that people imagined.’",
        "‘I followed imperial business from one court to another until I understood how often the journey itself was the answer. Here, at least, I can place a refusal in the hands of the man who must live with it.’",
        "‘A founder goes where somebody will buy metal and feed workmen. Then the people who have hired him begin asking whom he loves. I have never made a furnace burn more steadily by answering that question.’",
        "‘There was work to be done under these walls. People find that answer disappointing. They expect a vow. Give an engineer a crack that must be understood, and he may remain longer than a man who has promised never to leave.’",
        "‘I came bearing an agreement men had already begun to interpret against one another. You may ask whether I expected gratitude. I expected a difficult reception. Expecting it did not make me immune to disappointment.’",
        "‘A prince without secure land is a young man who must persuade other households that his name is worth their supper. Here I can learn to command something more substantial than a claim.’",
        "‘Trade gave me reasons to travel, and travel gave me reasons to distrust simple accounts of people I had not met. I know what a harbour promises when viewed from the sea. The quay often tells another story.’",
        "‘I crossed lands where the clothes on my back sometimes protected me better than any title I possessed. When I returned, people preferred the distances and marvels. They were less interested in how often I had depended on a stranger’s patience.’",
        "‘I wanted workmen to test the drawings. Drawings are agreeable companions: they do not blister their hands or ask which part will strike them if the rope breaks. A workshop improves a philosopher’s manners.’",
        "‘I followed names cut into stone. Eventually I noticed that living people were being asked to carry the weight of the same names. I have tried to keep both discoveries in my notebooks.’",
        "‘I knew the City as a guest. Returning is an awkward privilege: you discover how little hospitality entitles you to understand. I came because I had written that I loved the place. It seemed time to discover what I meant.’",
        "‘I took service for wages and a journey worth speaking of. The journey has become longer than the road here. When someone at home asks what held me, I hope I shall have a better answer than pride.’"
    };
    static readonly string[] CityWords={
        "‘The City is where my men must make their courage useful. Do not ask them to love every stone. Give them someone alive to defend behind it.’",
        "‘From the sea, the City seems certain of itself. In its harbour every man is bargaining against the possibility that the next voyage will be the last. I recognise both descriptions.’",
        "‘It is a thousand arrangements that no proclamation can replace. The man who delivers oil to a hospital knows a part of the City an emperor may never see.’",
        "‘Here I can be a prince without being required to disappear for another prince’s convenience. That is a modest freedom. I would like it to become a less exceptional one.’",
        "‘The City has taught me the difference between keeping a record and keeping a promise. We are good at the first when the second becomes impossible.’",
        "‘There are craftsmen here who can hear a flaw before a nobleman can see a triumph. I would save their workshops before I saved a statue of a man who never made anything.’",
        "‘A wall is maintained by people who will receive no inscription upon it. Constantinople gives such people magnificent things to work on and very little time to explain what those things need.’",
        "‘I see people who fear that rescue will arrive wearing the wrong vestments. I do not mock the fear. I wish we had not taught them so many reasons for it.’",
        "‘It is a place old enough to remember men who thought their defeat permanent. I find that comforting until I remember that the dead did not live to enjoy the comparison.’",
        "‘It is a market in which a name can buy you passage or get you watched. I admire its memory. I do not always admire the prices it puts upon remembering.’",
        "‘I have seen powerful cities with less reason to boast. What troubles me here is the expectation that its past must somehow pay for its present.’",
        "‘Here a device may be judged as an insult to an older device before anyone asks whether it works. Yet I have never lacked for an ingenious man willing to argue with me.’",
        "‘Every generation has broken something the next wished it had kept. The living are not obliged to become curators of their own hunger. I still wish to leave them more than a list of excuses.’",
        "‘I remember a welcome. That is a selfish reason to love a place, perhaps, but it obliges me to notice when the people who gave it are in need.’",
        "‘It is where men I knew are buried, and men I know must stand watch tonight. The marble is handsome. Those are the parts that have a claim upon me.’"
    };
    static readonly string[] BondWords={
        "‘Kjold will tell a man where he must stand. Notaras will tell me why his pay has not arrived. I need both answers. I find the first easier to like.’",
        "‘Barbaro knows when a promise requires a boat. Giovanni knows when the boat is needed somewhere else. I would prefer them to disagree in the same room.’",
        "‘Sphrantzes understands an obligation that will outlive its wording. Giovanni sometimes assumes that being clear is enough. I envy him that assumption on the days it is true.’",
        "‘Kjold watches what my men do before deciding what they are. Notaras must consider what they might become. I can work with both judgements. I sleep more easily beside the first.’",
        "‘Notaras knows how fear enters an account without falsifying a single number. Orhan knows how fear enters a title. Their honesty is seldom comfortable for the listener.’",
        "‘Fontana asks whether a thing can work. Grant asks how it will fail. I would rather have both at a trial than either at an inquiry held after it.’",
        "‘Orban is good with metal and impatient with objections. Cyriacus will defend a stone as if it were his child. They remind me to ask what a man thinks he is saving.’",
        "‘Sphrantzes can distinguish discretion from a convenient silence. Cyriacus asks what will remain after us. I sometimes wish he would ask more often who remains now.’",
        "‘Kjold corrects me without remembering to be impressed by my birth. Giovanni is kinder about it. You may guess which kindness teaches me more.’",
        "‘Diedo asks how a ship will return. Notaras asks what it will owe. Neither question is an insult to the man proposing the voyage.’",
        "‘Orhan knows what a traveller can learn by listening badly. Tafur remembers what he was offered. I trust both more when they tell me which part they could have misunderstood.’",
        "‘Grant writes down the objection. Orban would rather build the answer. An emperor with patience might obtain a useful machine from the quarrel.’",
        "‘Isidore believes a divided inheritance can still be shared. Orban wants bronze. We ought to hear both before we call the melting of a statue a simple matter.’",
        "‘Giovanni takes responsibility in person. Barbaro asks whether the responsibility has a price written beside it. They are both useful company when a gentleman offers his word.’",
        "‘Giovanni knows that a reserve is made of men who have not yet been spent. Orhan knows what it costs to be considered useful chiefly because someone else fears you. I listen to both.’"
    };
    public static string Arrival(int p){int i=Index(p);return i>=0&&i<ArrivalWords.Length?ArrivalWords[i]:GardenConversations.Personal(p);}
    public static string City(int p){int i=Index(p);return i>=0&&i<CityWords.Length?CityWords[i]:GardenConversations.Personal(p);}
    public static string Bonds(int p){int i=Index(p);return i>=0&&i<BondWords.Length?BondWords[i]:"There is no private judgement recorded.";}
}
