from pathlib import Path
root=Path(r'C:\Users\Dainius\Desktop\BesiegedInConstantinopole-codex-prototype-first-playtest\Assets\Yarn\Test')
def replace_once(text,old,new):
    assert text.count(old)==1,old[:90]
    return text.replace(old,new,1)
p=root/'Random_events_recurring.yarn';s=p.read_text(encoding='utf-8-sig')
start=s.index('title: random8a');end=s.index('===',start)
chunk=s[start:end];chunk=replace_once(chunk,'<<Change iMoney 2500>>','<<Change iMoney 1500>>')
s=s[:start]+chunk+s[end:]
s=replace_once(s,'Immediately sell the harvest, we need the money.','Sell the harvest to merchants. Gain 1,500 gold; public morale falls by 8 points.')
s=replace_once(s,'A little bit of rot is nothing. Put spoiled food into soups and feed it first, hopefully nothing will happen.','Have the cooks salvage what they can and serve it first. Risk a food-poisoning outbreak.')
s=replace_once(s,'The people who ate the slightly spoiled food seem to be fine... Indeed, when one knows how to make a good soup, nothing goes to waste!','The cooks discard the worst grain and stretch the rest through the soup kitchens. No outbreak is reported, and the threatened stores are saved.')
s=replace_once(s,"Unfortunately there's a outbreak of food poisoning - thousands of people are vomiting their guts out on the sidewalks and a few hundred even die. It seems that sometimes running a kitchen might require more skill than running an Empire... The rest of the spoiled food we burn.",'By morning, the infirmaries are crowded. A thousand civilians die; the garrison also loses men. You order the remaining spoiled grain burned.')
s=replace_once(s,"You are correct, the impoverished underclass don't care about the quality of the food and while you lose the food, at least you are compensated with money. Later you may learn about the food poisoning outbreak, but at least it doesn't affect your own servants or troops.",'The grain finds buyers among those who cannot afford better. Your treasury receives 150 gold. Then reports of illness arrive from the poorer quarters: three thousand residents die in the outbreak. The palace kitchens and barracks remain untouched.')
p.write_text(s,encoding='utf-8')
p=root/'Story_events_YARN_11.yarn';s=p.read_text(encoding='utf-8-sig')
s=replace_once(s,'    The Ottoman ships hold the sea. Our fleet is defeated, its mission abandoned. Survivors bring word of lost vessels and crews who will never return to the Golden Horn.\n    <<FleetRetreat>>','    <<jump story18b>>')
s=replace_once(s,'title: story18a','title: story18b\n---\n<b>Defeated Fleet</b>: <br> The Ottoman ships hold the sea. Our fleet is defeated, its mission abandoned. Battered survivors return to the Golden Horn, towing what damaged hulls they can save. Other crews will never return. The remaining ships take up the defence of the harbour; future orders can be given through the Fleet menu.\n<<FleetRetreat>>\n===\n\ntitle: story18a')
s=replace_once(s,'currently Constantinople already houses around 100 thousand people and all of them need to eat. Once the siege begins, our stores have enough food to hold out for about 30 days.','Constantinople already houses {GetPopulation()} people, and all of them need to eat. Once the siege begins, the stores must feed everyone sheltered within the walls.')
s=replace_once(s,'1000 people are listed as 1 unit of Population and they consume 1 unit of Provisions per day after the siege begins. For example 87 Population will consume 87 units of Provisions per day.','Every 1,000 residents consume 1 unit of Provisions per day after the siege begins. For example, 87,000 residents consume 87 units of Provisions each day.')
p.write_text(s,encoding='utf-8')
print('Updated harvest, spoiled-food prose, population explanation and distinct defeated-fleet follow-up.')
