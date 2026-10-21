"""Check the shipped HTML/data contract, then make standalone and Unity ZIPs."""
from pathlib import Path
from html.parser import HTMLParser
from zipfile import ZipFile, ZIP_DEFLATED
import json, subprocess, shutil, hashlib

base=Path(__file__).resolve().parents[1]
class Page(HTMLParser):
    def __init__(self): super().__init__(); self.scripts=[]; self.external=[]; self.current=None
    def handle_starttag(self,tag,attrs):
        a=dict(attrs)
        if tag=='script':
            if 'src' in a: self.external.append(a['src'])
            self.current=''
        if tag=='link' and a.get('rel')=='stylesheet': self.external.append(a.get('href',''))
    def handle_data(self,data):
        if self.current is not None: self.current+=data
    def handle_endtag(self,tag):
        if tag=='script' and self.current is not None:self.scripts.append(self.current);self.current=None

page=Page();page.feed((base/'Besieged Event Map.html').read_text(encoding='utf-8-sig'))
assert not page.external,page.external
assert len(page.scripts)==4,len(page.scripts)
node=shutil.which('node') or 'C:/Users/Dainius/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/bin/node.exe'
for script in page.scripts:subprocess.run([node,'--check'],input=script.encode('utf8'),check=True,capture_output=True)
data=json.loads((base/'dist/data.json').read_text(encoding='utf8'))
try:
    import jsonschema
    jsonschema.validate(data,json.loads((base/'data.schema.json').read_text(encoding='utf8')))
    schema_status='passed independent JSON Schema validation'
except ImportError:schema_status='JSON Schema library unavailable; browser validator and automated tests used'
for p in (base/'reference-extracts').glob('*.txt'):
    counterpart=base/'dist/sources'/p.name
    assert p.read_bytes()==counterpart.read_bytes(),p
repo=base.parent/'game-repo'
assert (repo/'Tools/BesiegedEventMap/Besieged Event Map.html').read_bytes()==(base/'Besieged Event Map.html').read_bytes()
with ZipFile(base/'BesiegedEventMap.zip','w',ZIP_DEFLATED) as z:
    for p in base.rglob('*'):
        if p.is_file() and p.suffix not in ('.zip','.pyc') and '__pycache__' not in p.parts:
            z.write(p,p.relative_to(base).as_posix())
with ZipFile(base/'BesiegedEventMap-Unity.zip','w',ZIP_DEFLATED) as z:
    editor=repo/'Assets/Editor/BesiegedEventMap.cs'
    z.write(editor,editor.relative_to(repo).as_posix())
    for p in (repo/'Tools/BesiegedEventMap').rglob('*'):
        if p.is_file() and p.suffix!='.pyc':z.write(p,p.relative_to(repo).as_posix())
for p in base.glob('*.zip'):
    with ZipFile(p) as z:assert z.testzip() is None
print(json.dumps({'portable_html_external_dependencies':page.external,'inline_scripts_syntax_checked':len(page.scripts),'schema':schema_status,'source_records':len(data['events']),'archives':[{'name':p.name,'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in base.glob('*.zip')]},indent=2))
