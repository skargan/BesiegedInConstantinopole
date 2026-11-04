"""Read-only Git comparison. Writes a provenance inventory, never merges files."""
from pathlib import Path
import subprocess, json
root=Path(__file__).resolve().parents[1]
def git(*args):
    return subprocess.check_output(['git','-c',f'safe.directory={root.as_posix()}','-C',str(root),*args],text=True,encoding='utf-8').strip()
def tree(ref):
    result={}
    for row in git('ls-tree','-rl',ref).splitlines():
        fields,path=row.split('\t',1); mode,kind,sha,size=fields.split()
        if kind=='blob':result[path]={'sha':sha,'bytes':int(size)}
    return result
head=tree('HEAD'); records=[]
for ref in git('for-each-ref','--format=%(refname:short)','refs/remotes/origin').splitlines():
    base=git('merge-base','HEAD',ref)
    paths=git('diff','--name-only',base,ref).splitlines()
    source=tree(ref);changes=[]
    for path in paths:
        item=source.get(path)
        state='deleted-at-source' if item is None else 'missing' if path not in head else 'identical' if head[path]['sha']==item['sha'] else 'different'
        changes.append({'path':path,'state':state,**(item or {})})
    records.append({'ref':ref,'sha':git('rev-parse',ref),'base':base,'aheadBehind':git('rev-list','--left-right','--count',f'HEAD...{ref}'),'changes':changes})
out=root/'Documentation'/'Release39';out.mkdir(parents=True,exist_ok=True)
(out/'branch-audit.json').write_text(json.dumps({'baseline':git('rev-parse','HEAD'),'branches':records},indent=2),encoding='utf-8')
for r in records:
    counts={s:sum(c['state']==s for c in r['changes']) for s in ['missing','different','identical','deleted-at-source']}
    print(r['ref'],counts,'missing MB',round(sum(c.get('bytes',0) for c in r['changes'] if c['state']=='missing')/1024**2,1))
