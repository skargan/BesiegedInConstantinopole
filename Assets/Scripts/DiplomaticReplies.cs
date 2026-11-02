// Original fictional correspondence for the game, not historical quotations.
// Pure text selection: does not consume gameplay RNG or change aid rules.
public static class DiplomaticReplies
{
    public static string Quote(string power,bool accepted,int religion,int variant=0)
    {
        bool alternate=(variant&1)!=0;
        bool union=religion==1||religion==2;
        if(power=="Georgia"||power=="Trebizond"||power=="Wallachia"||power=="Moldavia"||power=="Serbia")
        {
            if(union&&!accepted)return religion==1
                ?"You have bent the imperial crown before Rome and called it salvation. Let those who bought your obedience now purchase your deliverance."
                :"You profess the Union abroad while your priests deny it at home. We will not send our sons to defend an oath your own altar will not honour.";
            if(union&&accepted)return "We grieve for what you have conceded to Rome. Yet the City is more than its emperor, and we shall not abandon its people to Mehmed.";
        }
        if(power=="The Papacy")
        {
            if(accepted)return religion==1?"Your profession of unity has been heard. Let our answer be carried in ships and men, that the reconciliation of the churches may bear fruit."
                :religion==2?"We receive your promise of union, though much remains unfulfilled. Aid shall follow; may your deeds now give substance to your words."
                :"Our differences remain grievous. Nevertheless, we will not refuse those who stand between the conqueror and the churches of the East.";
            return union?"Your appeal has stirred many hearts, but devotion has not furnished enough ships. We cannot promise an expedition we have no means to send."
                :"You ask Rome to defend an altar from which her authority is rejected. Our treasury cannot supply the concord your court refuses to establish.";
        }
        switch(power)
        {
            case "Hungary": return accepted?"If Mehmed spends his strength against your walls, he has less to spend against ours. Hold the City; we shall do what lies within our power.":alternate?"Our frontier burns already. Every horse sent towards your City leaves a Hungarian village without its defender.":"You ask us to open another campaign while the wounds of the last remain unhealed. Our sympathy is sincere; our armies must remain here.";
            case "Venice": return accepted?"The straits must not become one man's tollgate. Our ships will come, and we expect Venetian lives and property to receive your protection.":alternate?"The Senate has considered your petition. Until agreement is reached, no captain may sail in the Republic's name. We regret that deliberation offers you so little comfort.":"We cannot stake our whole commerce upon a war whose cost no man can reckon. Your envoys may seek private assistance; the Republic promises none.";
            case "Genoa": return accepted?"Giustiniani has made his own wager. We shall add something of ours: the fall of your harbour would narrow every Genoese horizon.":alternate?"Our captains have obligations in more waters than yours. Courage we possess; a fleet free of commitments we do not.":"Do not mistake the devotion of one Genoese captain for a pledge by Genoa. We cannot undertake the expedition you request.";
            case "Albania": return accepted?"We have both learned what an Ottoman promise is worth. Keep Mehmed before your walls, and accept what help we can spare.":"We fight the same enemy, Emperor, but he stands at our own gates as well. Were we to strip our hills of defenders, neither of us would be saved.";
            case "Knights Hospitaller": return accepted?"Our profession is the defence of the faithful. A few brothers and ships are yours; Rhodes must still be guarded, but it shall not be our excuse for doing nothing.":"Every sail we send away leaves Rhodes more exposed. We shall pray for your walls beneath our own, but cannot leave our island undefended.";
            case "Naples": return accepted?"Let our banner be seen among the defenders. A crown that claims honour in the Mediterranean must occasionally pay for it.":"Our councillors speak warmly of your cause and coldly of its expense. The treasury has prevailed. No expedition will sail.";
            case "Holy Roman Empire": return accepted?"The princes do not speak with one voice, but enough have answered to make this pledge. May these men serve where our arguments cannot.":"Each prince agrees that Constantinople must be saved. Each also insists that another prince should bear the greater expense. We can promise you no army.";
            case "France": return accepted?"We know something of cities besieged and kingdoms brought low. What we send cannot undo your danger, but you shall not say that France answered only with prayers.":"Our own wars have emptied too many purses and households. We cannot summon another host merely by naming its cause righteous.";
            case "Athens": return accepted?"Your walls shelter more than Constantinople. If they fall, no little court in Greece will sleep easily. Take what we can spare.":"Look at our narrow borders and tell us which men we may safely remove. We fear the fate you describe; that fear keeps our soldiers here.";
            case "Georgia": return accepted?"The distance between our churches does not divide our faith. Our men shall cross the sea; may they find your walls still standing.":"We cannot defend the Queen of Cities by leaving our own mountain passes open. Pray for us, as we shall pray for you.";
            case "Trebizond": return accepted?"We know how little a Roman title weighs without men to defend it. These ships carry what our small realm can spare.":"We are a narrow shore beneath a long shadow. To aid you openly would invite the blow we have scarcely the strength to survive.";
            case "Cyprus": return accepted?"The sea joins our dangers as surely as it joins our harbours. We shall send assistance while there is still a passage to your City.":"Our island has troubles enough behind its own doors. A promise of ships would please your envoy today and shame us when none arrived.";
            case "Poland and Lithuania": return accepted?"Mehmed's ambition will not end at your walls. Better to spend men and treasure upon his resistance now than upon our own despair later.":"Our borders make demands which your messenger has not seen. We cannot leave those obligations to pursue a distant campaign.";
            case "Teutonic Order": return accepted?"Our brothers took the cross for service, not ornament. A contingent shall depart, though our own lands still require the greater part of our strength.":"A mantle marked with the cross does not make its wearer master of inexhaustible armies. Our obligations here bind the men you ask us to send.";
            case "England": return accepted?"We cannot shorten the sea between us. Nevertheless, we shall send what we have promised. God grant that your walls outlast the voyage.":"By the time an English expedition reached you, this siege might be settled one way or the other. We will not dress that distance in a false promise. God keep you.";
            case "Karaman": return accepted?"You and I do not kneel in the same faith. Yet Mehmed would have us both kneel before him. His pride could drag his people into the pit of Iblis; take this aid and keep him occupied.":alternate?"You ask a Muslim ruler to save your Christian throne? Your envoy has a bold tongue. Tell him boldness is not a currency accepted in our treasury.":"Mehmed is no friend of ours. That does not make your quarrel a war we can afford. Do not mistake our pleasure at his difficulties for a promise to rescue you.";
            case "Turkmen Confederations":
            case "Aq Qoyunlu": return accepted?"Mehmed's triumph would cast a longer shadow across Anatolia. We shall send what we can spare through Trebizond. Do not mistake these purses for a pledge of all our horsemen.":alternate?"Our riders answer many lords, and too many banners already threaten Amid. We cannot purchase your safety with our own ruin.":"We have heard your envoy. The Sultan is not the only enemy in this world, and those nearer our tents leave us nothing to promise your City.";
            case "Wallachia":return accepted?"A river is a poor defence when one watches it alone. Accept what we can spare, and make the Sultan pay dearly for every day before your walls.":"Your messenger rides away. Our villages cannot. Before you call us timid, consider who bears the answer when Mehmed learns of it.";
            case "Moldavia":return accepted?"The road is long, and our means are small. What we have promised will nevertheless set out; let neither distance nor despair waste it.":"Our neighbours count our horses as closely as we do. We cannot send away the men whose absence would invite them across the frontier.";
            case "Serbia":return accepted?"We have learned the price of surviving beneath another man's shadow. Take this assistance, and do not ask us to pretend that giving it is easy.":"You speak of duty as if we had never paid for it. Our obligations leave us no freedom to make the promise your envoy demands.";
            case "Bosnia":return accepted?"Our hills have taught us that a greater army need not have the last word. We send what our households can provide.":"Every lord approves your cause until the reckoning reaches his own purse. Our council has found agreement only in postponement.";
            case "Crimean Khanate":return accepted?"The Black Sea has room for more than one master's ships. What we send serves our interests as well as yours; remember both halves of that bargain.":"Your envoy describes the danger to your harbour. He has not yet explained why our horses and silver must cure it.";
            case "Mamluk Sultanate":return accepted?"We have heard your petition. Accept this measured assistance without imagining that it purchases our sword or directs our policy.":"Your emperor's distress does not make it our obligation. We have frontiers to guard and a treasury intended for their defence.";
            case "Morea":return accepted?"Brother, the seal upon this letter is easier to send than the men who follow it. They are coming. May our arguments survive long enough to trouble us again.":"You know the dangers here as well as any messenger can tell them. We cannot save the family in Constantinople by abandoning its last lands in the south.";
            default:return accepted?"Our own burdens are heavy, but we shall send what we have pledged. May it reach you in time.":"We have war upon our own doorstep. We can offer sympathy, Emperor, but no men to accompany it.";
        }
    }
    public static string Format(string power,bool accepted,int religion,int variant=0)
    {
        string court=power=="Turkmen Confederations"?"the Aq Qoyunlu court":power;
        return "\n\nThe reply bears the seal of "+court+":\n<i>“"+Quote(power,accepted,religion,variant)+"”</i>";
    }
    public static string FormatWar(string countryId,string power,bool accepted,int religion)
    {
        string quote;
        if(!accepted)quote="Your envoy asks for a declaration, and a declaration brings an army to our own doors. Our council will not undertake this war today.";
        else switch(countryId) {
            case "venice":quote="The Senate has committed itself. Our captains will make the Sultan's coastal traffic pay for his ambitions. Give us time to assemble them.";break;
            case "genoa":quote="This undertaking bears the republic's seal. Our captains shall act against the Sultan's maritime supply routes; the preparations begin now.";break;
            case "naples":quote="Our ports have received their orders. When the ships are ready, the Sultan's coast will learn that your appeal found more than courteous listeners.";break;
            case "karaman":quote="Let Mehmed discover how much of Anatolia he can guard while boasting before your walls. Our preparations have begun.";break;
            case "aq_qoyunlu":quote="This letter promises more than silver. Our captains are gathering their riders; the Sultan's eastern roads will not remain undisturbed.";break;
            case "albania":quote="We shall draw his men towards our hills and away from your City. A common enemy makes this a bargain neither of us can afford to neglect.";break;
            default:quote="Our council has agreed. The commanders have orders to gather their forces and trouble the roads that feed the Sultan's siege. Hold until they are ready.";break;
        }
        if((countryId=="georgia"||countryId=="trebizond"||countryId=="wallachia"||countryId=="moldavia"||countryId=="serbia")&&religion!=0)
            quote=accepted?"Our quarrel over Rome remains. It will not prevent us from striking a common enemy. "+quote:"Your concessions to Rome have won you little confidence here. "+quote;
        return "\n\nThe reply bears the seal of "+power+":\n<i>“"+quote+"”</i>";
    }
}
