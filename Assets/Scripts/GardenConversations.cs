using System;

// Conversation is inspection: no repeatable resource or loyalty rewards.
public static class GardenConversations
{
    public const int Kjold=200;
    public static string Name(int person)=>person==Kjold?"Kjold":CourtRoster.Names[person];
    public static string Role(int person)=>person==Kjold?"Varangian officer · imperial guard":CourtRoster.Roles[person];
    public static bool Available(int person)
    {
        if(person==Kjold){if(DailyLifeEvents.I("iKjoldState",1)!=1)return false;}
        else if(!CourtRoster.InService(person))return false;
        int today=TrainingPool.Day(VariableSingleton.GetDate());
        return DailyLifeEvents.I("iHardOfficerReturn"+person)<=today;
    }
    public static string Personal(int person)
    {
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
    public static string Siege(int person)
    {
        if(!VariableSingleton.HasSiegeStarted())return "The siege has not begun. "+(person==0?"‘Use these months to decide what we shall be able to do when decisions become expensive.’":person==2?"‘Every preparation is a choice against another preparation. Let us at least make those choices deliberately.’":"‘We still have time to prepare. We do not have time to do everything.’");
        string report=AI.Instance==null?"":AI.Instance.LastReport;
        return "<b>Latest report available to the council</b>\n"+(string.IsNullOrEmpty(report)?"No Ottoman operation has yet been reported.":report)+"\n\n<b>His assessment</b>\n"+Advice(person);
    }
}
