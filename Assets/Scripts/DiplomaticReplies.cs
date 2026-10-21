// Original fictional correspondence for the game, not historical quotations.
// Pure text selection: does not consume gameplay RNG or change aid rules.
public static class DiplomaticReplies
{
    public static string Quote(string power,bool accepted,int religion,int variant=0)
    {
        bool alternate=(variant&1)!=0;
        bool union=religion==1||religion==2;
        if(power=="Georgia"||power=="Trebizond")
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
            default:return accepted?"Our own burdens are heavy, but we shall send what we have pledged. May it reach you in time.":"We have war upon our own doorstep. We can offer sympathy, Emperor, but no men to accompany it.";
        }
    }
    public static string Format(string power,bool accepted,int religion,int variant=0)
    {
        return "\n\nThe reply bears the seal of "+power+":\n<i>“"+Quote(power,accepted,religion,variant)+"”</i>";
    }
}
