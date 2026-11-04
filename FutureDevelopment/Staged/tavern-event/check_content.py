from pathlib import Path
import json, re, hashlib
from PIL import Image
p=Path(__file__).resolve().parent
t=(p/'Evening_in_a_Tavern.yarn').read_text(encoding='utf-8-sig')
nodes=re.findall(r'^title: (\w+)$',t,re.M)
assert nodes==['lifeTavern','lifeTavernPledge']
assert len(re.findall(r'^-> ',t,re.M))==4
assert t.count('dice(100)')==3 and t.count('$chance <= 50')==3
assert t.count('<<if ') == t.count('<<endif>>') == 3
assert len(re.findall(r'^===$',t,re.M))==2
assert '<<LifeSeen lifeTavern>>' in t
assert '<<Change iInfantry' not in t and '<<Change iArchers' not in t
assert t.count('<<LifeTavernPledgeComplete>>')==1
assert all(not re.search(r'50|chance|%',x) for x in t.splitlines() if x.startswith('->'))
entries=json.loads((p/'EventArtwork.additions.json').read_text())
assert {e['node'] for e in entries}==set(nodes)
assert all(e['path']=='EventArt/Covers/lifeTavern' and e['fit']=='contain' for e in entries)
im=Image.open(p/'lifeTavern.png');im.verify()
baseline=json.loads((p/'baseline.json').read_text())
for relative,digest in baseline['files'].items():
    assert hashlib.sha256((Path(baseline['checkout'])/relative).read_bytes()).hexdigest()==digest,relative+' baseline drift'
print('TAVERN_CONTENT_CHECKS: node structure, four choices, three hidden 50/50 branches, cooldown, guarded payout, artwork and source baselines passed. Not a Yarn compiler or visual playtest.')
