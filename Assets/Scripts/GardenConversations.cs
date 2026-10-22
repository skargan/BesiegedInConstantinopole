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
    public static string Personal(int person)
    {
        if(IsVisitor(person))return VisitorPersonal[person-100];
        if(person==Kjold)return "Kjold rests his hands on the pommel of his sword. ‘My father thought I would return with money. My mother thought I would return with a wife. Neither asked whether I might learn to enjoy being far away.’ He glances towards the palace. ‘Now I stand at your doors. There are worse places for a man who has delayed going home.’";
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
        if(person==Kjold)return "‘Keep enough of the Guard together to act when the wall gives way. A dozen brave detachments cannot become one reserve merely because someone sends a messenger.’\n\nCheck available Varangians before committing them to missions. The breach decision states the force required.";
        int food=SupplyEconomy.DaysLeft;
        float wall=VariableSingleton.Instance.EnduringWall.GetHealth();
        if(person==2)return food<10?"‘We have fewer than ten full days of provisions. Buy food or arrange a supply mission before making another generous promise.’\n\nTreasury and market shows the current price and daily consumption.":"‘Our stores give us room to choose. Keep watching consumption as soldiers and refugees arrive: a full granary is a quantity, not a date.’\n\nCurrent food reserve: "+food+" full days.";
        if(person==0)return wall<.5f?"‘The active wall is below half strength. Repair it if you can. Keep a reserve for the breach; do not spend the last men merely to make today's report sound brave.’\n\nReview Defence of the walls and your available Varangians.":"‘Stone buys time. Men decide what to do with it. Keep the walls repaired and check that your reserves are still available before committing them elsewhere.’";
        if(person==1)return "‘A ship cannot defend the harbour and raid a distant coast at the same time. Review our fleet mission before deciding what support the walls can expect.’\n\nOpen Fleet to inspect the current orders and surviving ships.";
        if(person==3)return "‘An army that cannot eat becomes a collection of arguments. Your agents can burn their stores, but it is their remaining provisions that determine whether they must stop and resupply.’";
        if(person==4)return "‘An accepted appeal is not an arrival. Count the journeys still under way, and leave room in your plans for a late ship.’\n\nDiplomacy lists committed expeditions and expected travel time.";
        if(person==5)return "‘Count ammunition before ordering another demonstration. A gun without powder is an expensive reassurance.’\n\nCheck ammunition and artillery actions at Defence of the walls.";
        if(person==6)return "‘Watch the moat as well as the wall. When the ditch is filled, the enemy's work becomes easier. Their daily report will tell you when the labourers have made progress.’";
        return "‘A promise about faith is heard by people who will remember it after this audience ends. Look at the factions before assuming that a sermon has satisfied them all.’\n\nSociety and politics shows their influence and support.";
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
    // React to the report shown above, not to an unrelated strategic concern.
    public static string ReportAssessment(int person,string report)
    {
        string r=(report??"").ToLowerInvariant();
        if(string.IsNullOrWhiteSpace(r))return "‘I have no fresh report to judge, Majesty. Let us hear what happened before deciding what it means.’";
        if(r.Contains("emperor falls"))return "The report breaks off at the breach. There is no counsel here that can undo what happened.";
        if(r.Contains("abandoned")||r.Contains("fall back"))return person==3?
            "‘They have taken ground from us. They will want to turn the withdrawal into a rout. Find out who reached the next line before asking those men to hold it.’":
            "‘We have lost a defensive line. We must account for the men who withdrew and inspect the next wall. Calling it a withdrawal will not make the losses smaller.’";
        if(r.Contains("repulsed")||r.Contains("repelled")||r.Contains("holds the breach"))
        {
            if(person==3)return "‘They came expecting to break us, and they have been driven back. My kinsman will not welcome that news. But a failed attack tells us nothing certain about his next move. Count our losses and see what remains of the wall.’";
            if(person==0||person==Kjold||person==100)return "‘The men held. Give the wounded help and replace the exhausted watches. We must inspect the damage before deciding how many reserves we can spare.’";
            if(person==2)return "‘We still hold the line. Now we must reckon the cost: men to replace, wounds to tend, stone to repair. Victory is easier to announce than to pay for.’";
            if(person==5||person==6||person==103)return "‘The assault has failed, but the damage remains. Inspect the wall and the approaches while the men can still tell us where the enemy nearly got through.’";
            if(person==7)return "‘We have been spared another defeat. Let us give thanks, and then see to the wounded. They should not have to wait until the celebrations are over.’";
            return "‘The attack was beaten back. That is cause for relief, but we must learn what it cost us before promising that we can endure another.’";
        }
        if(r.Contains("assemble")||r.Contains("launch an assault"))return "‘They are coming against the wall. See that the men know their posts and that the reserve can reach them. We can discuss their intentions after we have survived them.’";
        if(r.Contains("delayed"))return "‘Their land operations have been delayed. Use the time for repairs and relief of the watches. Our own stores will still diminish, and the sea has not become safe.’";
        if(r.Contains("galata"))return "‘The report concerns Galata. We must establish what remains in our hands there before promising troops from the City's walls. A second threatened position can divide us very quickly.’";
        if(r.Contains("provisions")||r.Contains("resupply")||r.Contains("stores"))return person==3?
            "‘An army that cannot eat becomes a collection of arguments. This report concerns their supplies; watch whether they resume their attacks. We should not mistake an interruption for their departure.’":
            "‘Their supply situation has interrupted the usual fighting. Read what the report actually confirms before committing men on the assumption that the enemy is helpless.’";
        if(r.Contains("bombardment"))return "‘Their guns have struck the wall. Inspect the damage before allocating repairs; we must know what can still withstand another bombardment.’";
        if(r.Contains("cannon"))return "‘Another gun has reached their lines. Its arrival is not yet a report of damage, but we should check our ammunition and the condition of the wall it may face.’";
        if(r.Contains("moat"))return "‘They are filling the ditch. That will ease their approach to the wall. Watch the remaining protection; this work matters even on a day when no infantry attack comes.’";
        if(r.Contains("ramp")||r.Contains("tower")||r.Contains("ram"))return "‘They have advanced their siege works. Our watches must attend to the approaches as well as the battlements. Construction is a warning; it does not tell us when the assault will begin.’";
        return "‘That is what the report establishes. It gives us no certain account of their next move. Keep the watches alert; I would rather admit what we do not know than invent an intention for the Sultan.’";
    }
    public static string Siege(int person)
    {
        if(IsVisitor(person)&&!VariableSingleton.HasSiegeStarted())return "The siege has not begun. "+VisitorAdvice[person-100];
        if(!VariableSingleton.HasSiegeStarted())return "The siege has not begun. "+(person==0?"‘Use these months to decide what we shall be able to do when decisions become expensive.’":person==2?"‘Every preparation is a choice against another preparation. Let us at least make those choices deliberately.’":"‘We still have time to prepare. We do not have time to do everything.’");
        string report=AI.Instance==null?"":AI.Instance.LastReport;
        return "<b>Latest report available to the council</b>\n"+(string.IsNullOrEmpty(report)?"No Ottoman operation has yet been reported.":report)+"\n\n<b>His assessment</b>\n"+ReportAssessment(person,report);
    }
}
