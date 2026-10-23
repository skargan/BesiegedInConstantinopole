"""Deterministic, read-only import of Besieged reference extracts and a fork snapshot.
No third-party dependencies. Run from any directory. Never writes into input sources.
"""
from pathlib import Path
import re, json, hashlib, shutil, subprocess, struct, argparse

BASE = Path(__file__).resolve().parents[1]
WORK = BASE.parent
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--repo',type=Path,default=Path.cwd() if (Path.cwd()/'Assets').exists() else WORK/'game-repo')
parser.add_argument('--extracts',type=Path,default=BASE/'reference-extracts' if (BASE/'reference-extracts').exists() else WORK/'output')
parser.add_argument('--output',type=Path,default=BASE/'dist')
args=parser.parse_args()
OUT = args.output.resolve()
REPO = args.repo.resolve()
EXTRACTS = args.extracts.resolve()
events, edges, sources = [], [], []

def clean(s):
    return re.sub(r'\s+', ' ', re.sub(r'<<.*?>>|<[^>]+>|#line:\w+', '', s)).strip(' :\\')

def add_source(path, name, kind, locator):
    raw = path.read_text(encoding='utf-8-sig')
    sid = 'src-' + hashlib.sha256(name.encode()).hexdigest()[:12]
    sources.append(dict(id=sid, name=name, kind=kind, locator=locator,
                        sha256=hashlib.sha256(path.read_bytes()).hexdigest(), text=raw))
    (OUT/'sources').mkdir(parents=True, exist_ok=True)
    shutil.copy2(path, OUT/'sources'/path.name)
    return sid, raw

def effect(command, line, guards):
    c = command.strip()
    m = re.match(r'(set|declare)\s+\$(\w+)\s*(?:=|to)\s*(.*)', c, re.I)
    if m:
        return dict(command=c, variable=m[2], operation=m[1].lower(), value=m[3], line=line, guards=list(guards))
    m = re.match(r'(ChangeFraction|Change|SetInt|RndChange)\s+(\w+)(?:\s+(.*))?$', c, re.I)
    if m:
        return dict(command=c, variable=m[2], operation=m[1], value=m[3] or '(argument missing)', line=line, guards=list(guards))
    return dict(command=c, variable='', operation='command', value='', line=line, guards=list(guards))

def parse_nodes(lines, source_id, collection, category, catalogue=None):
    catalogue = catalogue or {}
    starts = [i for i, (_, t) in enumerate(lines) if re.match(r'\s*title:\s*\S+', t)]
    seen = {}
    for k, start in enumerate(starts):
        chunk = lines[start:starts[k+1] if k+1<len(starts) else len(lines)]
        # A Yarn node ends at ===; do not absorb trailing design notes into its body.
        end = next((i for i, (_, t) in enumerate(chunk[1:],1) if t.strip() == '==='), len(chunk))
        chunk = chunk[:end]
        code = re.search(r'title:\s*(\S+)', chunk[0][1])[1]
        occurrence = seen.get(code, 0); seen[code] = occurrence+1
        bold = next((re.search(r'</?b>\s*(.*?)\s*</b>', t, re.I) for _, t in chunk if re.search(r'</?b>\s*(.*?)\s*</b>', t, re.I)), None)
        title = clean(bold[1]) if bold else catalogue.get(code,{}).get('title',code)
        eid = f'{collection}:{code}' + (':variant-'+hashlib.sha256(title.encode()).hexdigest()[:10] if occurrence else '')
        node = dict(id=eid, code=code, title=title, collection=collection, category=category,
                    origin='imported', status=catalogue.get(code,{}).get('status','Imported'),
                    sourceId=source_id, sourceLine=chunk[0][0], raw='\n'.join(f'{n}: {t}' for n,t in chunk),
                    description='', choices=[], effects=[], conditions=[], reads=[], writes=[], notes='',
                    entry=collection=='design' and code=='story0', external=False,
                    review=[], sourceDuplicate=bool(occurrence))
        choices = node['choices']; current = None; guards = []; base_guards = []
        arrow_lines = [i for i,(_,t) in enumerate(chunk) if t.lstrip().startswith('->')]
        # Unmarked prose options are segmented conservatively and explicitly flagged.
        prose_verbs = r'(?:Send|Order|Ask|Consult|No way|Mere rumours|Get Sprantzes|Find officers|This might|Create a trap|Use this opportunity|Buy food|Compensate|Punish surviving|Offer tavern|Ignore the matter|I choose|Withdraw|Leave Garrison|Grant the Castles|Extend them|Do nothing|Launch a Sudden|Bribe someone|Summon all|We should not|Search|Turn|Allow|Accept|Reject)\b'
        for i,(ln,t) in enumerate(chunk[1:],1):
            st=t.strip()
            is_arrow = st.startswith('->')
            is_prose = (not arrow_lines or i<arrow_lines[0]) and bool(re.match(r'^(?:[1-5]\.\s+)?'+prose_verbs,st,re.I)) and len(st)<330
            # random23 uses three unmarked, full-sentence choice labels.
            if code=='random23' and collection=='design' and ln in (219,222,225): is_prose=True
            if is_arrow or is_prose:
                choice_label=clean(st[2:] if is_arrow else st)
                choice_key=hashlib.sha256(choice_label.encode()).hexdigest()[:12]
                current = dict(id=f'{eid}:choice-{choice_key}', label=choice_label,
                               line=ln, extraction='explicit' if is_arrow else 'prose-review',
                               conditions=[], effects=[], outcomes=[], text='')
                choices.append(current); guards=[]; base_guards=[]
                for g in re.findall(r'<<if\s+(.*?)>>',st): current['conditions'].append(g)
                for g in re.findall(r'\((IF\s+[^)]+)\)',st,re.I): current['conditions'].append(g)
                # Inline <<if>> is a choice gate, not an open block.
                continue
            cmds = re.findall(r'<<(.*?)>>',st)
            for c in cmds:
                if c.startswith('if '):
                    guards.append(c[3:]); base_guards.append(c[3:])
                    if not current: node['conditions'].append(c[3:])
                elif c.startswith('elseif '):
                    if guards: guards[-1] = 'else if '+c[7:]+' (after previous branches)'
                    else: guards.append(c[7:])
                elif c=='else':
                    if guards: guards[-1]='else of '+base_guards[-1]
                elif c=='endif':
                    if guards: guards.pop()
                    if base_guards: base_guards.pop()
                elif c.startswith('jump '):
                    target = c[5:].strip()
                    # Document terminal jumps are often misindented. Treat the last jump
                    # outside a conditional as the shared continuation, retaining raw evidence.
                    tail = collection=='design' and category=='story' and i>=len(chunk)-4 and not guards
                    ch = None if tail else current
                    edges.append(dict(id=f'jump:{eid}:{ln}', source=eid, target=f'{collection}:{target}',
                                      kind='jump', label='Continue' if ch is None else ch['label'][:85],
                                      choiceId=ch['id'] if ch else '', conditions=list(guards)+(ch['conditions'] if ch else []),
                                      sourceId=source_id, sourceLine=ln, origin='imported',
                                      note='Shared continuation inferred from terminal position; check source indentation.' if tail else 'Explicit jump target; branch context extracted.'))
                elif not c.startswith(('stop','return')):
                    e=effect(c,ln,guards)
                    (current['effects'] if current else node['effects']).append(e)
            txt=clean(st)
            if txt and not st.startswith(('title:','---','===','///','//','###')):
                if current:
                    current['text'] += ('\n' if current['text'] else '')+txt
                    # Split prose probability outcomes even when a Word paragraph contains several.
                    markers=list(re.finditer(r'(?:\d\s*\.\s*\d\s*)?\((\d+(?:\.\d+)?)%\)|\[(\d+(?:\.\d+)?)%\]',txt))
                    for j,m in enumerate(markers):
                        current['outcomes'].append(dict(probability=float(m[1] or m[2]), text=txt[m.end():markers[j+1].start() if j+1<len(markers) else len(txt)].strip(), line=ln))
                else:
                    if re.search(r'</?b>',st,re.I):
                        txt=clean(re.sub(r'</?b>\s*.*?\s*</b>\s*:?\s*(?:<br>\s*)?', '', st, count=1, flags=re.I))
                    if txt: node['description']+=('\n' if node['description'] else '')+txt
        all_effects=node['effects']+[e for c in choices for e in c['effects']]
        node['writes']=sorted(set(e['variable'] for e in all_effects if e['variable']))
        conds=node['conditions']+[g for c in choices for g in c['conditions']]+[g for e in all_effects for g in e['guards']]
        node['reads']=sorted(set(re.findall(r'\$(\w+)|Get(?:Int|Float|Bool)\("?(\w+)"?\)', ' '.join(conds))))
        node['reads']=sorted(set(a or b for a,b in node['reads']))
        if any(c['extraction']=='prose-review' for c in choices): node['review'].append('Prose choice boundaries need review')
        if re.search(r'PROBLEM|TODO|NOT IMPLEMENTED',node['raw'],re.I): node['review'].append('Source contains unfinished implementation notes')
        if len(node['description'])<40 and not choices: node['review'].append('No complete event body')
        if occurrence: node['review'].append('Duplicate source code; retained as a separate variant')
        node['external']=collection=='design' and category in ('random','unique','enemy')
        events.append(node)
    # Keep index-only records: absence of a body is real source information, not a demo.
    present={n['code'] for n in events if n['sourceId']==source_id}
    for code,meta in catalogue.items():
        if code in present: continue
        events.append(dict(id=f'{collection}:{code}', code=code,title=meta['title'] or f'Untitled source slot {code}',
                           collection=collection,category=category,origin='imported',status=meta['status'] or 'Index only',
                           sourceId=source_id,sourceLine=meta['line'],raw=meta['raw'],description='',choices=[],effects=[],
                           conditions=[],reads=[],writes=[],notes='',entry=False,external=category in ('random','unique','enemy'),
                           review=['Listed in source index; no event body supplied'],sourceDuplicate=False))

for stem,cat in [('events - story','story'),('events - random','random'),('events - random unique','unique'),('events - enemy','enemy')]:
    path=EXTRACTS/f'{stem}.txt'
    sid,raw=add_source(path,stem+'.docx','cached-extract',f'Cached paragraph-numbered extract: output/{stem}.txt; original DOCX unavailable in sources/')
    lines=[(int(m[1]),m[2]) for l in raw.splitlines() if (m:=re.match(r'^(\d+): (.*)',l))]
    first=next((i for i,(_,t) in enumerate(lines) if t.startswith('title:')),len(lines))
    catalog={}
    for i,(ln,t) in enumerate(lines[:first]):
        if re.fullmatch(r'(story|random|enemy)\d+[a-z]?',t.strip()):
            title=lines[i-1][1].strip() if i else ''
            if title.isdigit(): title=''
            following=lines[i+1][1] if i+1<first else ''
            catalog[t.strip()] = dict(title=title,status=following if following in ('Coded','Written','Part-Written') else 'Index only',line=ln,raw='\n'.join(f'{n}: {v}' for n,v in lines[max(0,i-1):i+3]))
    parse_nodes(lines,sid,'design',cat,catalog)

sid,raw=add_source(EXTRACTS/'Events.txt','Events.xlsx','cached-extract','Cached cell-addressed extract; original workbook unavailable in sources/')
for l in raw.splitlines()[3:]:
    cells=dict(re.findall(r'([A-Z]+)\d+=(.*?)(?= \| [A-Z]+\d+=|$)',l))
    if not cells or ('C' not in cells and 'D' not in cells): continue
    row=re.search(r'[A-Z]+(\d+)=',l)[1]
    code=cells.get('D','row'+row)
    events.append(dict(id='catalogue:'+code,code=code,title=cells.get('C',f'Untitled source slot {code}'),
                       collection='catalogue',category='catalogue',origin='imported',status=cells.get('H','Unspecified'),
                       sourceId=sid,sourceLine=int(row),raw=l,description=cells.get('J',''),
                       choices=[],effects=[],conditions=[cells['F']] if cells.get('F') and cells['F']!='NA' else [],
                       reads=[],writes=[],notes='',entry=False,external=True,review=['Legacy catalogue; ID mapping to narrative documents is unverified'],
                       timing=cells.get('E',''),firing=cells.get('G',''),stage=cells.get('I','')))
add_source(EXTRACTS/'Core mechanics 1.4.txt','Core mechanics 1.4.docx','cached-extract','Cached paragraph-numbered extract; design reference, not executable rules')

# Import only the Yarn project selected by Continue Game, not obsolete duplicate Yarn files.
runtime=REPO/'Assets/Yarn/Test/Story_events_YARN_11.yarn'
if runtime.exists():
    git=shutil.which('git') or 'C:/Users/Dainius/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/git/cmd/git.exe'
    try: commit=subprocess.check_output([git,'-C',str(REPO),'rev-parse','HEAD'],text=True,stderr=subprocess.DEVNULL).strip()
    except (OSError,subprocess.CalledProcessError): commit='unversioned-working-copy'
    sid,raw=add_source(runtime,'Fork / Story_events_YARN_11.yarn','repository-working-copy',f'Local working copy based on skargan/BesiegedInConstantinopole@{commit}/Assets/Yarn/Test/Story_events_YARN_11.yarn; hash identifies current bytes, including local edits')
    parse_nodes(list(enumerate(raw.splitlines(),1)),sid,'runtime','story')
    for extra in sorted(runtime.parent.glob('*.yarn')):
        if extra == runtime: continue
        extra_id,extra_raw=add_source(extra,'Unity / '+extra.name,'repository-working-copy',str(extra))
        parse_nodes(list(enumerate(extra_raw.splitlines(),1)),extra_id,'runtime','random' if extra.name.startswith('Random') else 'event chain')
    for relative in ['Assets/Scripts/ReliefStormSystem.cs','Assets/Scripts/SiegeOpeningIntelligence.cs','Assets/Scripts/NavalState.cs','Assets/Scripts/SiegeHardships.cs','Assets/Scripts/EmperorCombat.cs','Assets/Scripts/DailyLifeEvents.cs','Assets/Scripts/CourtIntrigue.cs','Assets/Scripts/CharacterPreparations.cs','Assets/Scripts/PreparationCampaign.cs','Assets/Scripts/RecurringEvents.cs','Assets/Scripts/SultanGifts.cs','Assets/Scripts/SupplyEconomy.cs','Assets/Scripts/RestUI.cs','Assets/Scripts/VariableSingleton.cs','Assets/Yarn/Test/NewProject.yarnproject','Assets/Yarn/Test/NewProject.yarnproject.meta']:
        add_source(REPO/relative,'Fork / '+Path(relative).name,'repository',f'{commit}/{relative}')
    scene=REPO/'Assets/Scenes/Continue Game.unity'
    scene_text=scene.read_text(encoding='utf-8-sig')
    sched=re.search(r'possibleStory: ([0-9a-f]+)',scene_text)
    turns=list(struct.unpack('<'+'i'*(len(sched[1])//8),bytes.fromhex(sched[1]))) if sched else []
    sid_schedule='src-scene-schedule'
    excerpt='\n'.join(f'{i}: {t}' for i,t in enumerate(scene_text.splitlines(),1) if any(x in t for x in ('possibleStory:','possibleRandom:','yarnProject:')))
    sources.append(dict(id=sid_schedule,name='Fork / Continue Game scheduling',kind='repository-excerpt',locator=f'{commit}/Assets/Scenes/Continue Game.unity',sha256=hashlib.sha256(scene.read_bytes()).hexdigest(),text=excerpt))
    for e in events:
        if e['collection']=='runtime':
            e['external']=bool(re.fullmatch(r'story\d+',e['code']) and int(e['code'][5:]) in turns)
            e['entry']=e['code']=='story0'
            e['timing']='Turn '+e['code'][5:] if e['external'] else 'No direct scheduler entry in Continue Game'
    for a,b in zip(turns,turns[1:]):
        edges.append(dict(id=f'schedule:runtime:{a}:{b}',source=f'runtime:story{a}',target=f'runtime:story{b}',
                          kind='schedule',label=f'Next scheduled turn: {b}',choiceId='',conditions=['Continue Game possibleStory list; progression assumes turns advance'],
                          sourceId=sid_schedule,sourceLine=8402,origin='imported',note='Scheduler sequence, not a Yarn jump or proof of runtime reachability.'))
else: commit=None

data=dict(schemaVersion=1,title='Besieged Event Dependency Map',revision='2026-10-21-v2',
          repositoryCommit=commit,events=events,edges=edges,sources=sources,
          importNotes=['No sample events are included. Missing-target cards are diagnostic references, not imported events.',
                       'DOCX/XLSX imports use existing cached text extracts, not freshly opened originals.',
                       'Runtime snapshot, narrative design and legacy spreadsheet remain separate collections.',
                       'Source status Coded/Written is a reported label, not verification of compilation or playtesting.'])
OUT.mkdir(exist_ok=True)
(OUT/'data.json').write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
(OUT/'data.js').write_text('window.BESIEGED_DATA = '+json.dumps(data,ensure_ascii=False).replace('</','<\\/')+';\n',encoding='utf-8')
print(json.dumps(dict(events=len(events),choices=sum(len(e['choices']) for e in events),edges=len(edges),sources=len(sources),collections={c:sum(e['collection']==c for e in events) for c in ('runtime','design','catalogue')},commit=commit),indent=2))
