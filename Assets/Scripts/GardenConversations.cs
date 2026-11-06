using System;

// Conversation is inspection: no repeatable resource or loyalty rewards.
public static class GardenConversations
{
    public const int Kjold=200;
    public static bool IsVisitor(int person)=>person>=100&&person<100+CharacterPreparations.Names.Length;
    public static string Name(int person)=>person==Kjold?"Kjold":IsVisitor(person)?CharacterPreparations.Names[person-100]:person>=0&&person<8?CourtRoster.Names[person]:"Unknown visitor";
    public static string Role(int person)=>person==Kjold?"Varangian officer · imperial guard":IsVisitor(person)?CharacterPreparations.Roles[person-100]:person>=0&&person<8?CourtRoster.Roles[person]:"";
    public static bool Available(int person)
    {
        if(person==Kjold){if(DailyLifeEvents.I("iKjoldState",1)!=1)return false;}
        else if(IsVisitor(person)){if(!CharacterPreparations.InService||CharacterPreparations.CommanderId!=person)return false;}
        else if(!CourtRoster.InService(person))return false;
        int today=TrainingPool.Day(VariableSingleton.GetDate());
        return DailyLifeEvents.I("iHardOfficerReturn"+person)<=today;
    }
    public static string Introduction(int person)
    {
        if(IsVisitor(person))return CharacterPreparations.Profiles[person-100];
        if(person==Kjold)return KjoldConversations.Text.introduction;
        string[] introductions={
            "‘Genoese, as you know. A captain when somebody can afford a company, and a creditor when he discovers he cannot.’ Giustiniani smiles briefly.\n\n‘I brought my men here because I thought the City could be defended. I still do. You have walls other captains would pray for. What troubles me is how much ground there is between one man and the next. Show me that, Majesty, and I learn more than I do from the height of the towers.’",
            "‘I am Alvise Diedo, and I have spent enough years aboard ship to find a bed ashore suspiciously still. Venice teaches a captain to read a cargo account as carefully as a chart. An error in either can ruin him.’\n\n‘When I first looked at Constantinople, I looked at the harbour. I expect you find that unromantic. But I thought of the ships it could shelter, and the people they could feed. I have grown rather attached to the rest of it since.’",
            "‘About myself? After all these years, Majesty?’ Notaras allows himself a smile. ‘I suspect you know more of my faults than my household would willingly admit.’\n\n‘My office is megas doux. In practice, I spend a great deal of time persuading men to do something they have already agreed to do. I know the merchants, the officials, their cousins and their excuses. This is my home. I should like my children to inherit something of it besides our grievances.’",
            "‘I am Orhan, of the Ottoman house. Here I am your guest. In my cousin's camp, my name is a difficulty to be dealt with.’\n\n‘Constantinople has sheltered me, although some of its people would prefer a less troublesome guest. I cannot blame them. They see an Ottoman prince and wonder whom he serves. My men and I will have to answer that on the walls. I would rather be judged there than by another rumour about my family.’",
            "‘I was born here in Constantinople, Majesty, and entered your father's service while I was still a young man. Afterwards I served your brother, and then you. You know much of the story already—you were present for the more troublesome parts. I have governed Patras, Selybria and Mystras, carried letters, negotiated agreements and spent a considerable portion of my life travelling somewhere I would rather not have gone. Usually because you required something which could not be obtained by sending a less persistent man.’\n\n‘I have dealt with the Turks, visited foreign courts, and helped arrange your marriage. More recently, I have travelled to Georgia and Trebizond in search of another suitable match. Such journeys require patience. A ruler may speak warmly of friendship for an entire evening without committing himself to a single useful thing. One must listen carefully, remember what was actually promised, and return the following morning with the question he hoped you had forgotten.’\n\n‘I have a wife and children, and would be pleased to spend more time with them. But my household's future is bound to yours, as it has been for many years. My work is to learn who might help us, what they want, and whether an agreement can be made before circumstances change again. I shall tell you when I believe there is an opportunity. I shall also tell you when I think you are being deceived. You have enough people willing to give you the more agreeable answer.’",
            "‘Orban. I cast guns. I also argue about metal, wages and why a furnace cannot be hurried by shouting at it.’\n\n‘I came looking for work worth doing and a patron who would pay for it. Constantinople has given me plenty of the first. Your City knows what bronze can become; one sees it even in the old broken things lying about. I should like to add something of my own. Preferably something that does not break.’",
            "‘Johannes Grant. An engineer, though I seem to spend as much time listening as building. Put men underground and the sound of their work travels farther than they imagine.’\n\n‘There is a great deal beneath this City: old masonry, drains, foundations laid by men who never expected us to trouble them. I like to see a place before someone hands me a drawing of it. The drawing generally leaves out the thing that will cost us a fortnight.’",
            "‘Isidore. A cardinal now; before that, metropolitan of Kyiv. I have crossed a good deal of Christendom trying to persuade Christians that they have something in common.’\n\n‘Here, one can hear the same prayer sung with a devotion that moves me deeply, then leave the church and be called a traitor. I know what the quarrel means to them. That does not make the word easier to hear. I came to help preserve this City, including those who would prefer that I had stayed away.’"
        };
        return person>=0&&person<introductions.Length?introductions[person]:Personal(person);
    }
    public static string Personal(int person)
    {
        if(IsVisitor(person))return VisitorPersonal[person-100];
        if(person==Kjold)return KjoldConversations.Text.biography;
        string[] words={
            "Giustiniani turns a fallen leaf between his fingers. ‘Men call me a soldier of fortune as though fortune ever paid a company on time. My men came because I came. If I ask them to remain, I must be somewhere they can see me.’ He drops the leaf. ‘You understand that part of command, I think.’",
            "Diedo listens to the water in the garden basin. ‘On a ship you learn quickly which quarrels matter. A man may hate his neighbour and still have to take the same rope. I have sometimes thought cities would govern themselves better if they occasionally began to sink.’",
            "Notaras notices a gardener tying up a branch and waits until the man has finished. ‘My work is much like his, Majesty. Everyone sees the flowers. Nobody remembers who found the string.’ He smiles. ‘Until the branch breaks. Then they remember the name very precisely.’",
            "Orhan stands where he can see both entrances. ‘Your men call me a Turk when they distrust me, and a prince when they require something. Both are true. Neither tells them where I shall stand when my kinsman's men reach the gate.’",
            "Sphrantzes has brought no papers. ‘I wanted one conversation in which nobody could ask me to produce a copy.’ He watches the path. ‘At a foreign court, the smallest courtesy can be a promise or a refusal. The difficult part is learning which before the ship sails.’",
            "Orban studies the bronze fittings of the fountain. ‘People believe a great gun begins with a great idea. It begins with men who can make the same mistake less often than yesterday.’ He runs his thumb along a joint. ‘And metal that has not been bought from a liar.’",
            "Grant kneels to examine a line of ants beneath the coping. ‘They have found a way under it.’ He looks up at your expression. ‘No, Majesty, that was not advice to the enemy. But the earth does not take sides. We must remember that even when we cannot see what is moving through it.’",
            "Isidore pauses beside a rose stripped by the wind. ‘I have heard men describe unity as though it meant that nobody would ever disagree again. Such a Church would need neither patience nor charity. I have yet to visit it.’"
        };
        return person>=0&&person<words.Length?words[person]:"There is no conversation available.";
    }
    public static string Advice(int person)
    {
        if(IsVisitor(person))return VisitorAdvice[person-100];
        if(person==Kjold)return "‘Leave enough of the Guard together to answer a breach. I know every captain has an excellent reason to borrow a few of us. When they have all had their few, who comes when you call?’\n\nCheck available Varangians before committing them to missions. A breach will show how many are needed.";
        int food=SupplyEconomy.DaysLeft;
        float wall=VariableSingleton.Instance.EnduringWall.GetHealth();
        if(person==2)return food<10?"‘Less than ten full days of food. That is what I would attend to first. The merchants will ask too much; they generally do. We can quarrel over the price while there is still something to buy.’\n\nTreasury and market shows prices and daily consumption. Supply missions may bring food later.":"‘At present we have "+food+" full days of food. Keep that figure beside the invitations you send out. I welcome another company as much as any captain, but someone must arrange its supper.’\n\nReview food consumption when troops or refugees arrive.";
        if(person==0)return wall<.5f?"‘This line is below half its former strength. Put the repair gangs to work, and keep men where they can reach a breach. If the stone gives way, we shall need them before we need a discussion about whose fault it was.’\n\nReview Defence of the walls and the available Varangians.":"‘Walk the line with the men who actually hold it. Look at the stairs, the approaches and where a reserve would have to pass. Keep up the repairs while we can still do them without fighting over the same stones.’\n\nReview the wall's condition before assigning men elsewhere.";
        if(person==1)return "‘Settle what you want from the fleet before sending it out. If we are raiding elsewhere, we cannot also stand in the harbour and support the walls. The captains need one set of orders they can carry out.’\n\nFleet shows the current mission and surviving ships.";
        if(person==3)return "‘Find out how the army is being fed. Soldiers will endure a great deal for a commander who keeps his promises about bread. Let him begin breaking those promises. Your agents can help him into that difficulty.’\n\nDestroying stores can force a supply interruption; the result depends on the enemy's remaining provisions.";
        if(person==4)return "‘Review the help already promised before making plans around it. A favourable letter has cheered many councils. It has never, to my knowledge, stood a watch. We need to know who has actually set out, and when they may reach us.’\n\nDiplomacy lists committed expeditions and their expected arrival.";
        if(person==5)return "‘Have somebody count the powder and shot before you commission another gun. I am quite willing to build it. I am less willing to stand beside it explaining why it cannot fire.’\n\nCheck ammunition and artillery orders at Defence of the walls.";
        if(person==6)return "‘The moat, Majesty. Men notice a cracked tower; they pay less attention to carts tipping earth into a ditch. Read the reports of that work. Once the approach is filled, we shall have a much harder time keeping them away from the masonry.’\n\nReview moat condition and the latest Ottoman operation.";
        return "‘Hear what people are actually saying about the Church settlement. A man may leave an audience quietly and go home angrier than he came. I would rather hear his objection here than discover it in a sermon preached to a frightened crowd.’\n\nSociety and politics shows the factions' influence and support.";
    }
    private static readonly string[] VisitorPersonal={
        "Stephen stops to watch a young guard struggling with a buckle. ‘When a household loses its land, everyone suddenly remembers that its sons are young. Before that, we were old enough to inherit.’ He turns back to you. ‘I have come to learn what a claim is worth when a man must defend it himself. I do not expect the lesson to be gentle.’",
        "Barbaro watches a servant carrying a basket through the doorway. ‘People think a journey is the line between two cities. It is the man who lends you a horse, the official who opens the gate, the interpreter who decides which part of your complaint to repeat. I have spent much of my life learning to remember their names.’",
        "Bertrandon smiles when you ask about his travels. ‘I was once able to pass through places where a lord's banner would have brought a company of soldiers down upon me. There is something to be said for arriving without importance.’ He glances towards your attendants. ‘An advantage I fear Your Majesty will seldom enjoy.’",
        "Fontana produces a folded sheet, then thinks better of opening it. ‘No drawings today. I have discovered that once a man sees a machine, he stops hearing the person who brought it.’ He slips the paper away. ‘I studied how bodies fail before I began asking similar questions of wood and metal. Neither benefits from an impressive explanation after the fact.’",
        "Cyriacus points towards a fragment built into a newer wall. ‘Someone once commissioned those letters because he wished a deed remembered. Later someone else needed a stone of precisely that size. Both men had good reasons.’ He bends to trace a damaged word. ‘I try to arrive before the second reason erases the first.’",
        "Tafur looks out across the roofs. ‘Returning to a city is stranger than first arriving. You remember a face, a doorway, a meal—and discover that the people who welcomed you have continued their lives without preserving the scene for your return.’ He smiles ruefully. ‘Travellers can become very possessive of other men's homes.’"
    };
    private static readonly string[] VisitorAdvice={
        "‘Your infantry must know which officers will remain with them. Drills are useful, but the men should recognise the voices giving the orders when they cannot see the banners.’ Review infantry training and the appointed siege commander; new recruits are unavailable until their training finishes.",
        "‘An expedition promised in a distant court still needs ships, provisions and a reason to sail. Attend to the appeals we can actually support, not simply the longest list of names.’ Diplomacy distinguishes an accepted appeal from reinforcements already arrived.",
        "‘Look for what the enemy must do before he can do what he wishes. A ramp needs labour, an assault needs an approach. Their preparations may tell us more than a shouted threat.’ Check the moat, siege ramp and latest reported Ottoman operation.",
        "‘Before ordering more guns, decide whether we can feed those we already possess. A workshop can finish a weapon more easily than it can conjure powder.’ Review ammunition, artillery technologies and the shipyard or recruitment queue before placing another order.",
        "‘Men defend a place more stubbornly when they can name what they hope to find there tomorrow. Let them hear something besides the count of stones and sacks.’ Review army morale and civic support; promises made to one faction may trouble another.",
        "‘A sortie is not improved merely by calling it bold. Know how the men will return, and what the wall must do while they are outside it.’ Inspect troop availability and the enemy's reported activity before committing a force to an exceptional action."
    };
    public static string Siege(int person)
    {
        if(IsVisitor(person)&&!VariableSingleton.HasSiegeStarted())return "The siege has not begun. "+VisitorAdvice[person-100];
        if(!VariableSingleton.HasSiegeStarted())return PreparationAssessment(person);
        string report=AI.Instance==null?"":AI.Instance.LastReport;
        string reaction=ReportedAssessment(person,report);
        return "<b>The council's latest report</b>\n"+(string.IsNullOrEmpty(report)?"No Ottoman operation has yet been reported.":report)+
            "\n\n"+(reaction==null?"<b>On the wider defence</b>\n"+Advice(person):reaction);
    }
    private static string PreparationAssessment(int person)
    {
        string[] words={
            "‘We are not under siege yet. I am glad of every day in which we can inspect the walls without somebody shooting at us. Decide which work must be finished first. If every captain's request is urgent, the workmen will do whichever one is nearest.’",
            "‘For now we can make arrangements with captains who still have a choice about coming here. Use that. Once the enemy is at the harbour mouth, assurances of a safe and profitable voyage will be rather less convincing.’",
            "‘The City is still preparing. Each month someone brings me a proposal we cannot possibly neglect. I lay it beside the other proposals we cannot possibly neglect. We shall have to disappoint some of these gentlemen before the Turks do it for us.’",
            "‘We still have time to learn something of the army coming against us. I would use it. Once the roads are watched, a man who might have carried a useful message will have more pressing concerns about his own neck.’",
            "‘While the roads are open, send the messages that require an answer. It is tempting to wait until we know exactly what we need. By then the people who could have helped may be a month's journey away.’",
            "‘I would much rather discover a fault in a casting now than during the first assault. Give the workmen time to finish, and give us powder to try what they have made. The Sultan will not postpone his visit because we need another test.’",
            "‘I should like to examine the works while the ground is quiet. An old drain can be repaired. An old drain discovered by an enemy miner presents a different problem. There is still time to learn which is which.’",
            "‘We are already asking people to bear losses for a danger they cannot yet see from their windows. Explain what the sacrifices are for. They will remember who asked, and whether the same burdens fell upon his own household.’",
            "‘My men can stand a watch. I want them to know the route to the next post in the dark, with smoke in their eyes and someone shouting the wrong directions. There is time to practise that now. We should use it.’"
        };
        int index=person==Kjold?8:person;
        return index>=0&&index<words.Length?words[index]:"‘We still have time to prepare. We do not have time to do everything.’";
    }
    private static string ReportedAssessment(int person,string report)
    {
        int index=person==Kjold?8:person;
        if(index<0||index>8||string.IsNullOrEmpty(report))return null;
        string text=report.ToLowerInvariant();
        string[] words;
        if(text.Contains("defensive line is abandoned"))words=new[]{
            "‘Then we must make the next line ready to receive them. Find the officers, gather their men and give each company a place. I can do nothing with a reassuring total if half the men in it cannot find their captain.’",
            "‘Losing a land wall does not change my orders at sea. But it will change what people ask of the ships. Tell the captains what you intend before frightened men begin asking them separately.’",
            "‘We must find out what was left behind and where the men have gone. Then let the households hear an honest account. If the palace says nothing, by evening every street will have its own version of the retreat.’",
            "‘Mehmed's men will take heart from this. Do not give them a second victory by letting our companies blame one another. Put them in position and let their officers see that there is still a defence to command.’",
            "‘I must tell our friends what has happened. I shall not write that all is well; the next traveller would make a liar of me. But they must hear that the defence continues, and where their help is now needed.’",
            "‘Tell me which guns we still have and where you want them. There will be plenty of time later to argue about what ought to have been moved. At present I need men who can move what remains.’",
            "‘The next wall needs examining at once. We cannot assume that a line which has seen less fighting is sound in every place. I shall want the men who know its repairs, including those who would rather not discuss them.’",
            "‘The people will be frightened. They have reason to be. Let the churches receive those who need help, and let them hear where the defence still stands. A rumour of the whole City lost will run faster than any messenger we send after it.’",
            "‘Get the scattered men back to their officers. They have just come away from a lost wall; do not make them spend the night wondering whether they have been abandoned as well. The Guard must know where you will be.’"
        };
        else if(text.Contains("assault is repulsed"))words=new[]{
            "‘We held. Let the men hear that plainly; they have earned it. Then relieve the worst of the exhausted companies and look at the damage. I would prefer the enemy to find us ready when he tries again.’",
            "‘Good. A captain at sea may not know how close that was. Send word to the harbour that the line held, and keep the fleet's orders clear. There is no reason for a victory on the walls to produce confusion on the water.’",
            "‘I am relieved, Majesty. So will every household with a man upon that wall be—once somebody tells them. Have the officers account for their companies. The families will come looking for news whether we have prepared it or not.’",
            "‘My cousin has been refused something he wanted. He will dislike that. I would not assume it has discouraged him, but the men who climbed those ladders will remember the reception. Let ours remember that they gave it.’",
            "‘That is news I can put in a letter without dressing it up. The City held an assault. It may persuade a hesitant friend that help can still arrive in time to be useful.’",
            "‘The wall held. Now let the gun crews inspect their pieces before somebody orders a celebration with powder. I should like the cannon that served us today to be fit for tomorrow.’",
            "‘We kept the position. That gives us a chance to inspect it properly. Ask where the men heard cracking, where the stair shook, where the masonry moved. Those details tend to disappear from a victorious report.’",
            "‘Then we have something for which to give thanks. And men who need care, and families waiting to learn whether that thanks includes the return of someone they love. Let us attend to them before making speeches.’",
            "‘They are off the wall, then. Good.’ Kjold lets out a breath. ‘Now count the men. Those who can still stand will say they need no rest, especially if an officer is listening. I would find someone else to take their next watch.’"
        };
        else if(text.Contains("delayed this turn"))words=new[]{
            "‘We have bought a little time. Put it into the wall and the reserves. The enemy will eventually finish whatever is keeping him; I should like him to discover that we have finished something too.’",
            "‘Their land operations are delayed. The sea remains another matter. Keep the coastal watch and the fleet's orders in place; I do not want a boat getting through because a courier told everyone the Turks were waiting.’",
            "‘A useful respite. It does not stop the City eating. I would use it to look at the stores and the repairs we have been postponing, rather than explain next week how pleasantly quiet today was.’",
            "‘Use the delay, but do not build a plan upon its lasting longer than reported. An Ottoman officer who has displeased his master can become remarkably industrious.’",
            "‘Then perhaps a messenger has time to gain on the enemy's preparations. I would keep our requests moving. It is much easier to make good use of an extra day before it has passed.’",
            "‘Good. The crews can inspect the vents and carriages without being interrupted. There is always some small defect nobody mentions while the gun is firing. This is when I want to hear about it.’",
            "‘Give that time to the repair gangs. We know they have a list. Let us see how much shorter we can make it before the enemy begins again.’",
            "‘Let those who have been keeping watch sleep. Let those who have put off visiting an injured friend go and see him. A quiet day is a kindness if we allow people to use it.’",
            "‘Then we can change the watches properly. I shall still keep a reserve together. A promise that the enemy will be late is not a reason to be late ourselves.’"
        };
        else if(text.Contains("resupply")||text.Contains("supply convoy"))words=new[]{
            "‘They need to attend to their supplies. Good. We should attend to ours, and to the wall. I would rather be ready for their return than discover that we spent the interruption congratulating ourselves.’",
            "‘Their army is waiting for supplies; that does not tell us what their ships are doing. Keep the harbour reports separate. If we send the fleet out, I want a reason better than a quiet morning on the landward side.’",
            "‘So the Sultan has a provisioning problem too. I wish him a thoroughly instructive experience. Meanwhile, let us make certain that our own storekeepers have something better to report.’",
            "‘This is the difficulty I meant. The army needs to eat before it can do what Mehmed wants. Our agents may be able to make the next delivery troublesome as well, but first establish what is actually coming.’",
            "‘An interruption gives us time, not a settlement. I shall keep pressing the promised assistance. It would be unfortunate if our friends relaxed their efforts just as the Sultan's suppliers renewed theirs.’",
            "‘If their guns are quiet, let us use the workshops. A supply interruption is a very convenient time to discover that one of our own carriages needs a new axle.’",
            "‘Their stores require attention. I should like to know whether their labourers have stopped too; one does not follow from the other. Keep watching the approaches while our own men work.’",
            "‘There are hungry men in that camp as well. Their suffering may give us a respite, but it does not absolve us of caring for our own. Use the quieter hours while we have them.’",
            "‘They cannot fight well hungry. Nor can we. Check that the men returning from watch receive what they are owed before somebody decides the quieter day is an opportunity to save a ration.’"
        };
        else return null;
        return words[index];
    }
}
