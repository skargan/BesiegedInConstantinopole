"""Preserve unmerged work outside Unity Assets, with source hashes and branch tips."""
from pathlib import Path
import json, subprocess, zipfile, shutil, hashlib
root=Path(__file__).resolve().parents[1]; workspace=root.parent
audit=json.loads((root/'Documentation/Release39/branch-audit.json').read_text())
archive=root/'FutureDevelopment'; archive.mkdir(exist_ok=True)
def git(*args):return subprocess.check_output(['git','-c',f'safe.directory={root.as_posix()}','-C',str(root),*args])
records=[]
for branch in audit['branches']:
    ref=branch['ref']; short=ref.rsplit('/',1)[-1]
    if short not in ['emperor-court-dialogues','brutal-murder-investigation','city-map-navigation','court-integration-base','candar-secret-negotiations-20261103']:continue
    out=archive/'Prototypes'/f'{short}.zip';out.parent.mkdir(exist_ok=True)
    manifest=[]
    with zipfile.ZipFile(out,'w',zipfile.ZIP_DEFLATED) as z:
        for item in branch['changes']:
            if item['state']=='deleted-at-source':continue
            data=git('show',f"{branch['sha']}:{item['path']}")
            z.writestr(item['path'],data);manifest.append({'path':item['path'],'sha256':hashlib.sha256(data).hexdigest()})
        z.writestr('ARCHIVE-PROVENANCE.json',json.dumps({'source':branch,'files':manifest},indent=2))
    records.append({'path':str(out.relative_to(root)),'source':ref,'commit':branch['sha'],'files':len(manifest),'sha256':hashlib.sha256(out.read_bytes()).hexdigest()})
# Copy curated staging inputs, excluding compiler output, caches and full old checkouts.
for name in ['ragusa-next','candar-next','diplomatic-expansion','ending-art','tavern-event','giustiniani-art']:
    source=workspace/name
    if not source.exists():continue
    for p in source.rglob('*'):
        if not p.is_file():continue
        rel=p.relative_to(source)
        if any(x.lower() in ['.git','library','builds','compilecheck','__pycache__','temp','obj','bin'] for x in rel.parts):continue
        if p.suffix.lower() in ['.dll','.exe','.pdb','.rsp','.log']:continue
        target=archive/'Staged'/name/rel;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,target)
        records.append({'path':str(target.relative_to(root)),'source':str(p),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
art=next(b for b in audit['branches'] if b['ref'].endswith('final-art-handover-20261102'))
for item in art['changes']:
    if item['state']=='deleted-at-source':continue
    target=archive/'OriginalArt'/Path(item['path']).relative_to('ArtHandover/FinalEventArt');target.parent.mkdir(parents=True,exist_ok=True)
    data=git('show',f"{art['sha']}:{item['path']}");target.write_bytes(data)
    records.append({'path':str(target.relative_to(root)),'source':art['ref'],'commit':art['sha'],'sha256':hashlib.sha256(data).hexdigest()})
(archive/'manifest.json').write_text(json.dumps(records,indent=2),encoding='utf8')
for item in records:
    assert hashlib.sha256((root/item['path']).read_bytes()).hexdigest()==item['sha256']
print('Archived and hash-verified',len(records),'files; all outside Unity Assets.')
