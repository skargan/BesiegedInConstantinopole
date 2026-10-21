using System.Collections.Generic;

// Original fictional conversations, not attributed historical quotations.
// Replies express the Emperor's position; this first pass grants no repeatable stat rewards.
public static class CharacterQuestions
{
    public sealed class Exchange
    {
        public readonly string Question;
        public readonly string[] Replies, Responses;
        public Exchange(string question,string[] replies,string[] responses){Question=question;Replies=replies;Responses=responses;}
    }
    public static readonly int[] People={0,1,2,3,4,5,6,7,100,101,102,103,104,105,200};
    public static Exchange For(int person)=>All.TryGetValue(person,out var exchange)?exchange:null;
    static readonly Dictionary<int,Exchange> All=new Dictionary<int,Exchange>
    {
        [0]=new Exchange(
            "Giustiniani looks back towards the palace before answering. ‘When a wall breaks, men look for someone who seems to know what happens next. I can stand where my company sees me. But if the fighting spreads, Majesty, will you stay where the City can see you, or go where you can still command it? I ask because I may have to tell your men why you are no longer beside them.’",
            new[]{"I will remain with the men at the breach.","I must go where I can still direct the defence.","We should agree on that signal before the wall breaks."},
            new[]{"‘Then let us choose a place with a route out. Courage should not prevent the next order from reaching us.’ His approval comes with the practical severity of a man already measuring the ground.","‘Then give me your destination, and someone I can trust to find you. A necessary withdrawal and a flight can look exactly alike from a crowded stair.’","‘Yes.’ He nods at once. ‘A signal, a meeting place, and a second messenger. We can ask men to be brave. We should not also ask them to guess.’"}),
        [1]=new Exchange(
            "Diedo turns a ring upon his finger. ‘A Venetian ship enters your harbour and half the City counts its guns; the other half counts what Venice may ask in return. If my men die here, do they become defenders of Constantinople? Or will someone still explain that they must have come for profit?’",
            new[]{"Their deeds here will determine how I honour them.","The City's old wounds cannot disappear on my command.","What recognition would matter to your men now?"},
            new[]{"‘Then write their names while they can still hear them read. The dead are very easily praised; the living have to decide whether we mean it.’","‘I know. I ask only that an old debt not be collected from the man carrying your wounded to his boat.’","‘Their pay, their rites, and an honest account of where they fought. Men are less complicated than the republics that employ them.’"}),
        [2]=new Exchange(
            "Notaras remains silent long enough for you to hear the fountain. ‘I can list what the City needs without looking at a page. Yet when I think of what I must save, I see my household before I see the treasury. Does that make me unfit to serve you—or merely less accomplished at speaking of duty than the men who have nothing to lose?’",
            new[]{"A man who loves his household knows what he defends.","Your office requires you to put other households beside your own.","Tell me plainly what provision you wish to make for them."},
            new[]{"‘That is what I tell myself. I am less certain when the same cart could carry my books or another family's grain.’ He meets your eyes. ‘I would rather you knew the difficulty.’","‘Beside it. Yes.’ He repeats the words more quietly. ‘I can promise that. I will not insult you by promising that I feel the same love for every door in the City.’","‘A lawful arrangement, openly made, before fear turns it into a secret. I have spent too long among officials to pretend that secrecy improves their judgement.’"}),
        [3]=new Exchange(
            "Orhan waits until a servant has passed. ‘If I die upon your walls, men will find it easy to say where my loyalty lay. If we survive, the question becomes troublesome again. What place would you give me in a victory, Majesty: a guest, an ally—or a claimant kept near enough to be useful?’",
            new[]{"You would remain an ally who chose his side.","Your claim has value, and I will not pretend otherwise.","What place would you choose for yourself?"},
            new[]{"‘Then allow me to disagree with you when the danger is past. An ally must be permitted a purpose beyond gratitude.’","‘Good. A useful man can bargain. A cherished prisoner must first persuade his keeper that there is a lock.’ There is no anger in his voice, which makes the answer harder to dismiss.","‘A household whose safety does not depend on a cousin's mood. You may find that a disappointingly small ambition for a prince. I have had time to revise it.’"}),
        [4]=new Exchange(
            "Sphrantzes folds his hands behind his back. ‘There are mornings when I know a report will make you less able to face the next one. I have sometimes put the gentler paper on top. Tell me what you expect of an old servant: the truth in the order it arrives, or in the order a man can bear it?’",
            new[]{"Bring me the most urgent truth first, however unwelcome.","Tell me what is certain, then what you fear.","If I begin refusing bad news, remind me of this conversation."},
            new[]{"‘Urgent, then. I shall try not to confuse it with merely alarming. The loudest messenger is seldom the best informed.’","‘That distinction would save us many quarrels. I can tell you when a ship was due. I cannot tell you that its lateness means it has sunk.’","‘I shall remind you once politely.’ His expression softens. ‘After that, we must rely on the privileges of a long acquaintance.’"}),
        [5]=new Exchange(
            "Orban studies his blackened thumbnail. ‘When a gun bursts, everyone asks who cast it. When it strikes a crowded street, they ask who ordered it fired. At what point does a maker cease to be responsible for the thing he has made? I have heard convenient answers from both sides of that question.’",
            new[]{"You answer for the weapon's soundness; I answer for its use.","Neither of us can hand the whole responsibility to the other.","Have you seen something in our preparations that troubles you?"},
            new[]{"‘Then you must let me say when it is unsound, even if the enemy is watching. A founder cannot be responsible for advice he is forbidden to give.’","‘That is the least comfortable answer.’ He rubs the nail against his sleeve. ‘It is probably the one I hoped you would not choose.’","‘Men are beginning to speak of a cannon as though its size were a promise. Metal makes no promises. Give me time to test what we intend to trust.’"}),
        [6]=new Exchange(
            "Grant has brought a small lump of clay from beneath the works. ‘If I am successful, nothing happens. No mine opens, no tower falls, nobody knows which passage we filled. How am I to persuade your council to keep men digging when the only proof I can offer is the absence of a disaster?’",
            new[]{"Show us the work and the danger it prevents.","You will have my support even when your work is unseen.","Let another experienced man inspect your findings with you."},
            new[]{"‘A section drawing, perhaps. And the spoil from their tunnel. Men believe a sack of earth more readily than a warning about what is under it.’","‘Then put that support in an order the quartermaster can read. Your confidence is valuable, Majesty, but he cannot issue timber against it.’","‘Gladly. Let him find my error while we can still reach it with a shovel. I have no wish to be proved right beneath a collapsed wall.’"}),
        [7]=new Exchange(
            "Isidore lowers his voice. ‘A frightened man may pronounce the words we ask of him and keep his conviction untouched. If union saves a city but leaves its people believing they were forced to betray their fathers, what have we reconciled? I ask you as the man who must hear them after the ceremony is over.’",
            new[]{"Agreement must leave room for conviction to follow.","We may not have time to wait for every conscience.","Help me distinguish opposition from a fear we can answer."},
            new[]{"‘Then we must endure some voices we would prefer to silence. Patience is praised most warmly by men who have not yet been asked to practise it.’","‘No. But haste does not turn compulsion into belief. Let us be honest about what an emergency can accomplish—and what it cannot.’","‘Ask them what they think will be taken from them. Some will speak of rites, some of authority, some of their dead. Those are not all the same dispute.’"}),
        [100]=new Exchange(
            "Stephen watches the guards change places. ‘A man who has lost his inheritance is encouraged to speak of justice. Once he has won it back, the same men advise him to be practical. If I ever recover what I claim, Majesty, how shall I know whether I am restoring my country or merely satisfying myself?’",
            new[]{"Judge by what your people gain when you return.","You will never be free of personal ambition. Govern it.","Ask who will be able to contradict you after your victory."},
            new[]{"‘Then the reckoning begins after the banners are raised.’ He seems to dislike the answer enough to remember it.","‘Govern it.’ He gives a small, reluctant smile. ‘I had hoped you might tell me that a rightful claim made the question simpler.’","‘Those who help me win will expect rewards. Those who opposed me will fear punishment. I see why a prince may find himself surrounded by agreement.’"}),
        [101]=new Exchange(
            "Barbaro draws a route with his finger upon the stone. ‘Trade depends on men believing a promise will outlive the advantage of breaking it. When a city is desperate, everyone recommends an exception. Which promise would you keep even if breaking it bought another month?’",
            new[]{"Safe conduct. Whoever comes under my protection must be able to leave.","The provision of food before private privilege.","I would need to know who pays for that extra month."},
            new[]{"‘A costly answer. Also one a messenger can carry without needing to explain your mood. That has value beyond the next ship.’","‘Then make that order clear before the warehouses are opened. A merchant can price a rule. He cannot price a crowd discovering it on the quay.’","‘Precisely. The month is always described as a gift to the City. The bill usually carries a smaller name.’"}),
        [102]=new Exchange(
            "Bertrandon touches the edge of his cloak. ‘I have crossed lands whose people I was expected to describe as enemies. A traveller notices a bridge, a meal, a man who gives good directions. A prince asks how many soldiers use the road. Can an account be useful to a ruler without becoming a preparation for conquest?’",
            new[]{"Knowledge should let us negotiate as well as fight.","A ruler must understand roads before an enemy uses them.","Keep the people in your account, even when we ask only for armies."},
            new[]{"‘Then ask your envoys more than whether a lord can send troops. The answer to another question may be the reason he sends them.’","‘Certainly. I ask only that a road not cease to be someone's way home because it appears on our military map.’","‘I shall. Though you may find my reports longer than your captains would prefer.’ His smile suggests this is not a new complaint."}),
        [103]=new Exchange(
            "Fontana keeps one hand over a folded drawing. ‘I can show you a machine that might work, or spend another month explaining why it might not. The first wins patrons; the second prevents funerals. How much uncertainty will you permit me to admit before you decide I am a fraud?’",
            new[]{"Give me a small test before you ask for a large commitment.","Name the failure you fear most and what it would cost us.","I will accept uncertainty. I will not accept concealed doubt."},
            new[]{"‘Then I need craftsmen and a place where a failure will injure nobody. An unglorious beginning. I would welcome it.’","‘That question should be written above every workshop door. Preferably by someone other than the man selling the invention.’","‘There will be disappointments.’ He uncovers the drawing. ‘I can promise to bring them to you while they are still cheaper than the alternative.’"}),
        [104]=new Exchange(
            "Cyriacus runs his fingers over a worn inscription. ‘Suppose a cart could carry either these stones or enough flour for several households. You would choose the flour; so should I. Yet if we make that choice often enough, what remains to tell those households who they are? Where would you have me draw the line?’",
            new[]{"Copy what you can; save the living who may read it.","Some things deserve a place even in a crowded cart.","Help me choose. Do not ask me to pretend nothing must be lost."},
            new[]{"‘Then give me paper before you give me wagons. A copy is a poor substitute for a stone, until the stone is gone.’","‘Then we must name those things now. In the final hour, the largest household will declare its silver the whole inheritance of Rome.’","‘That is a commission I can understand.’ He looks once more at the letters. ‘I shall hate parts of the list. I suspect that will mean we have taken it seriously.’"}),
        [105]=new Exchange(
            "Tafur looks towards the roofs beyond the wall. ‘When I first travelled, I imagined returning with stories that would make men listen. Now I wonder how often the wish for a memorable deed has led me past a useful one. Would you trust a man who came partly because he hoped to be remembered?’",
            new[]{"Yes, if he will also perform the duties nobody celebrates.","I would ask whose account he hopes will survive.","Ambition brought you here. Let your conduct answer the rest."},
            new[]{"‘Then assign me one. I should prefer to discover the answer before a poet does it for me.’","‘A harsher question than whether the tale is true.’ He pauses. ‘Perhaps the men who must stand beside me ought to have the first telling.’","‘Fairly said. I have offered you a confession when you require a captain. You may remind me of that if I become too interested in how the battle will sound.’"}),
        [200]=new Exchange(
            "Kjold leans against the cold stone. ‘There are men in the Guard who speak of the north as though we all remember the same country. I remember my mother's courtyard more clearly than any shore they name. When you call us your Varangians, Majesty, is it the men before you that you trust, or the men the name promises?’",
            new[]{"The men before me must earn that trust; the name cannot do it for them.","An inherited name can give men something worth becoming.","Tell me what you would want the Guard to stand for."},
            new[]{"‘Good. Then praise us for what we do, and tell us when we fail. We have enough songs to conceal a bad watch without imperial assistance.’","‘It can. Provided we are allowed to become it in our own voices. A man should not need a dead grandfather to prove he belongs at his post.’","‘That the frightened man behind us knows we will stay. Whether he can pronounce our names seems less important.’"})
    };
}
