"""Extract original workstream messages from available Codex session logs.

Only messages sent to the three named Besieged workstreams are retained.
Source paths and line numbers make every entry independently recoverable.
"""
import json, re, hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parent
IDS = {'01a0a8e2-b837-7953-bbb3-acf52c000d0c':'ConstantineX',
       '01a15e50-9560-7403-9c1d-159fb220af65':'JohanX',
       '6ae9c0d1-fbfc-83eb-baf4-0595d0c4a746':'OrbanX'}
SOURCES = [Path.home()/'.codex/sessions/2026/09/16/rollout-2026-09-16T09-23-45-01a0a8e2-b837-7953-bbb3-acf52c000d0c.jsonl',
           Path.home()/'.codex/sessions/2026/10/21/rollout-2026-10-21T14-55-01-01a15e50-9560-7403-9c1d-159fb220af65.jsonl']
records=[]; unresolved=[]; duplicate_count=0; seen={}
literal = r'"(?:[^"\\]|\\.)*"|\'(?:[^\'\\]|\\.)*\''
def value(code,key):
    m=re.search(r'(?:["\']?'+key+r'["\']?)\s*:\s*('+literal+r')',code,re.S)
    if not m:return None
    raw=m.group(1)
    if raw.startswith('"'):return json.loads(raw)
    # Single-quoted JS literals used by a few old calls; preserve Unicode.
    return re.sub(r'\\([\\\'"nrt])',lambda x:{'n':'\n','r':'\r','t':'\t'}.get(x[1],x[1]),raw[1:-1])
for source in SOURCES:
    sender=next(v for k,v in IDS.items() if k in source.name)
    with source.open(encoding='utf-8') as stream:
        for lineno,line in enumerate(stream,1):
            if 'send_message_to_thread' not in line:continue
            try:data=json.loads(line)
            except json.JSONDecodeError:continue
            p=data.get('payload',{})
            if data.get('type')!='response_item' or p.get('type') not in ('custom_tool_call','function_call'):continue
            code=p.get('input',p.get('arguments',''))
            if not isinstance(code,str):continue
            for piece in code.split('send_message_to_thread(')[1:]:
                target=value(piece,'threadId')
                if target not in IDS:continue
                body=value(piece,'prompt')
                if body is None:
                    alias=re.search(r'\bprompt\s*:\s*([A-Za-z_$][\w$]*)',piece)
                    alias=alias.group(1) if alias else 'prompt'
                    definition=re.search(r'\b(?:const|let|var)\s+'+re.escape(alias)+r'\s*=\s*('+literal+r')\s*;',code,re.S)
                    if definition:body=value('prompt:'+definition.group(1),'prompt')
                if body is None:
                    unresolved.append({'source':str(source),'line':lineno,'timestamp':data.get('timestamp'),'reason':'Nonliteral prompt requires manual recovery','code':piece[:2000]});continue
                key=(sender,IDS[target],body)
                evidence={'source':str(source),'line':lineno,'call_id':p.get('call_id'),'timestamp':data.get('timestamp')}
                if key in seen:
                    # Identical mirrored/retried text is retained as one body, all occurrences listed.
                    seen[key]['occurrences'].append(evidence);duplicate_count+=1;continue
                record={'timestamp':data.get('timestamp'),'sender':sender,'recipient':IDS[target],'text':body,'occurrences':[evidence]}
                records.append(record);seen[key]=record
records.sort(key=lambda r:r['timestamp'] or '')
out={'coverage':{'sources':[str(s) for s in SOURCES],'method':'Original send_message_to_thread tool-call prompts, restricted to named workstream IDs. Verbatim message body; transport timestamps in UTC. Identical sender, recipient and text merged with every occurrence retained. Delivery acknowledgement is not inferred.','gaps':'OrbanX original session is not available in these local archives. Incoming-only deliveries and nonliteral dynamic prompts are not guaranteed captured. Other conversations and internal subagents are outside this archive.','unique_messages':len(records),'duplicate_occurrences':duplicate_count,'unparsed_calls':len(unresolved)},'messages':records,'unresolved':unresolved}
(ROOT/'agent_coordination_messages.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps(out['coverage'],ensure_ascii=False,indent=2))
for r in unresolved:print('UNRESOLVED',r['timestamp'],r['code'][:300])
