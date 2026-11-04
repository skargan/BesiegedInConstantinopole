from pathlib import Path
import json

folder = Path(__file__).resolve().parent
path = folder / 'meeting-passages.json'
data = json.loads(path.read_text(encoding='utf-8'))
data.pop('cargo_delayed', None)
data.update({
    'chair_gazulus': {
        'speaker': 'Johannes Gazulus',
        'text': "This month Gazulus has the chair. He has moved the accounts aside to make room for his instructions. Cotrugli, standing beside the window, points out that the accounts were also instructions. Gazulus invites you to sit before they pursue the distinction."
    },
    'chair_cotrugli': {
        'speaker': 'Benedetto Cotrugli',
        'text': "Cotrugli has taken the chair and brought the table closer to the light. Gazulus remains within reach of the correspondence. 'A temporary arrangement,' the scholar says. 'Naturally,' replies Cotrugli. 'Almost everything profitable is.'"
    },
    'food_denial_gazulus': {
        'speaker': 'Johannes Gazulus',
        'text': "'The Republic cannot imperil its relations with the Sultan by supplying Constantinople.' Gazulus gives the sentence its full weight, then puts the sealed answer aside. Your payment is entered in a merchant's account. 'There is, however, a captain leaving for Albania. Your harbourmaster should make him welcome if his business brings him further east.'"
    },
    'food_denial_cotrugli': {
        'speaker': 'Benedetto Cotrugli',
        'text': "'Ragusa must decline,' Cotrugli says, signing the receipt for your money. He notices your expression. 'That is the Republic's answer. This is a purchase. An Albanian captain will attend to it, and I strongly advise against asking him why his cook speaks like my cousin.'"
    },
    'food_albanian_arrival': {
        'speaker': 'The harbourmaster',
        'text': "A vessel from Albania unloads five hundred measures of provisions. Its captain is unusually anxious to establish that he has never met a Ragusan councillor. The harbourmaster hears him out while the porters count the sacks. At the bottom of the manifest, in a familiar hand, someone has written: 'Please acknowledge safe receipt. Privately.'"
    },
    'war_contract_gazulus': {
        'speaker': 'Johannes Gazulus',
        'text': "Gazulus reads the terms back to you before sealing them. 'You are sending men whom your own walls may need. I shall not insult you by pretending the decision is small.' He checks the rolls against the agreement. Cotrugli has already summoned the clerk who will arrange their passage. For once, neither man has anything to add."
    },
    'war_contract_cotrugli': {
        'speaker': 'Benedetto Cotrugli',
        'text': "Cotrugli turns the agreement so you can read it in the light. 'The service, the reward and the return passage are all here. I prefer an argument before a signature.' When you have signed, he sets the troop rolls beside the contract. Gazulus asks for a second copy to be made for the captain."
    },
    'war_day90_victory': {
        'speaker': 'Ragusan dispatch',
        'text': "The dispatch carries good news and an order for payment. Your contingent has helped Ragusa prevail. The promised ten thousand gold is credited to your treasury; the soldiers are still abroad, awaiting the passage home. Gazulus's letter names the men commended by their officers. Cotrugli's addition is shorter: 'Their return has been arranged. Expect them in a fortnight.'"
    },
    'war_day90_defeat': {
        'speaker': 'Ragusan dispatch',
        'text': "The first page describes positions abandoned and orders overtaken by events. The second gives the names. Your contingent has not brought Ragusa the victory it sought. The returning men remain abroad while passage is arranged. 'A fortnight,' Cotrugli writes. Below it, Gazulus has added: 'Their officers will give you a fuller account when they reach you.'"
    },
    'war_day104_victory_return': {
        'speaker': 'The returning contingent',
        'text': "The transports enter the harbour with your men aboard. Their officers come ashore first, carrying the rolls and the Ragusan letters of commendation. The soldiers have been paid and have opinions about the wine. By evening they are back on the City's strength, telling their comrades how the fighting went."
    },
    'war_day104_defeat_return': {
        'speaker': 'The returning contingent',
        'text': "Your men come ashore without a welcome procession. Their captain presents the rolls, then asks to speak where he cannot be overheard. There will be time to examine the orders and to decide where the fault lay. For now, those who have returned are entered back on the City's strength."
    }
})
path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

characters = {
    'gazulus': {
        'name': 'Johannes Gazulus',
        'role': 'Diplomat and scholar',
        'description': 'A Dominican astronomer employed on Ragusan diplomatic business. Gazulus measures his words with the care he gives his instruments. He can make a refusal sound final, then quietly identify the small, lawful opening through which something useful might still pass.'
    },
    'cotrugli': {
        'name': 'Benedetto Cotrugli',
        'role': 'Merchant and commercial intermediary',
        'description': 'A Ragusan merchant with business across the Mediterranean and close ties to Naples. Cotrugli likes an agreement with names, dates and a captain responsible for the cargo. He enjoys mocking official caution, but relies upon it more than he admits.'
    }
}
(folder/'character-descriptions.json').write_text(json.dumps(characters, ensure_ascii=False, indent=2)+'\n',encoding='utf-8')

notes = folder / 'history-and-voices.txt'
text = notes.read_text(encoding='utf-8')
text = text.replace("The six passages are keyed openings/outcomes, not a new economic contract. Bind them only to matching existing transaction states. In particular, 'cargo agreed' is not 'cargo arrived', and the refusal text presumes the Kosača war remains ongoing.", "The passages are keyed openings/outcomes. Bind them to the actual agreement and campaign state; the speaker's presence never creates a second ruler or rector. Historical peace in 1454 is background context, not an automatic cancellation of an unfinished alternate-history campaign contract. The older gazulus_official_refusal passage is for an active local war only; do not reuse it as the paid food transaction response.\n\nThe current food design charges 2,500 gold, schedules 500 provisions after the proposed fourteen-day voyage, and does not make an acceptance roll. The two food_denial keys are successful purchase/dispatch text despite the official refusal. food_albanian_arrival is played on actual delivery, when provisions are awarded. An invented discretionary delay outcome has been removed; existing campaign storm rules, if applicable, own actual delays.\n\nThe war contract is resolved on day 90. war_day90_victory accompanies the immediate 10,000-gold reward and schedules troops to return fourteen days later; it does not restore any soldiers. war_day90_defeat likewise precedes the return passage. The two day104 keys belong to actual return and roster restoration. The surviving troop counts and all costs remain in the mechanical preview/results rather than being duplicated in this prose.\n\nThe monthly chair rotation between these contacts is a fictional presentation convention. It must not be described as a historical alternation of Ragusa's rectors.")
notes.write_text(text, encoding='utf-8')

hover = json.loads((folder/'hover-remarks.json').read_text(encoding='utf-8'))
realm = json.loads((folder/'realm-profile.json').read_text(encoding='utf-8'))
out = ['RAGUSA — FULL DIALOGUE REVIEW', 'Version 38 staging; 4 November 2026.', 'All spoken dialogue below is original game fiction.\n', 'CHARACTERS\n']
for item in characters.values():
    out += [item['name']+' — '+item['role'],item['description']+'\n']
out += ['REALM AND ITS INTERESTS\n',realm['text'],'\nHOVER REMARKS\n']
for who, lines in hover.items():
    out.append(characters[who]['name'].upper())
    out.extend(f'{i:02d}. {line}\n' for i,line in enumerate(lines,1))
out += ['\nMEETINGS AND OUTCOMES', 'Food agreement: 2,500 gold; 500 provisions after the proposed fourteen-day voyage. Official denial always accompanies dispatch, with no acceptance roll.', 'War: day 90 victory credits 10,000 gold now; troops return fourteen days later. Day 104 is the return event, not an extra award.\n']
for key,item in data.items():
    out += ['\n'+key.upper().replace('_',' '),item['speaker'],item['text']]
(folder/'full-dialogue-review.txt').write_text('\n'.join(out)+'\n',encoding='utf-8')

assert len(data)==16
assert all(len(v)==12 for v in hover.values())
assert len(set(hover['gazulus']+hover['cotrugli']))==24
assert 'cargo_delayed' not in data
print('Validated: 24 unchanged hover remarks, 16 passages, 2 character descriptions, updated review and chronology notes.')
print('New passage keys: '+', '.join(list(data)[5:]))
