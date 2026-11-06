import pathlib,subprocess,json,urllib.request,http.client,urllib.parse,hashlib
root=pathlib.Path.cwd();repo='skargan/BesiegedInConstantinopole'
cred=subprocess.run(['git','credential','fill'],input='protocol=https\nhost=github.com\n\n',text=True,capture_output=True,check=True)
d=dict(x.split('=',1) for x in cred.stdout.splitlines() if '=' in x);token=d['password'];headers={'Authorization':'Bearer '+token,'Accept':'application/vnd.github+json','User-Agent':'Saving-Constantinople-publication'}
sha=subprocess.check_output(['git','rev-parse','HEAD'],text=True).strip();body=(root/'Documentation/Version45 release notes.md').read_text(encoding='utf-8')
def api(path,data=None):
 req=urllib.request.Request('https://api.github.com/repos/'+repo+path,data=json.dumps(data).encode() if data else None,headers=headers);return json.load(urllib.request.urlopen(req,timeout=60))
r=api('/releases',{'tag_name':'v45','target_commitish':sha,'name':'Version 45 — Cavalry sallies','body':body,'draft':True})
p=root/'Builds/Saving-Constantinople-Version-45-Windows.zip';url=urllib.parse.urlsplit(r['upload_url'].split('{')[0]+'?name='+p.name)
c=http.client.HTTPSConnection(url.netloc,timeout=600);c.putrequest('POST',url.path+'?'+url.query)
for k,v in headers.items():c.putheader(k,v)
c.putheader('Content-Type','application/zip');c.putheader('Content-Length',str(p.stat().st_size));c.endheaders();h=hashlib.sha256()
with p.open('rb') as f:
 while chunk:=f.read(4*1024*1024):h.update(chunk);c.send(chunk)
response=c.getresponse();result=json.loads(response.read());assert response.status==201,(response.status,str(result)[:200]);assert result['size']==p.stat().st_size
req=urllib.request.Request('https://api.github.com/repos/'+repo+'/releases/'+str(r['id']),data=json.dumps({'draft':False}).encode(),headers=headers,method='PATCH');published=json.load(urllib.request.urlopen(req,timeout=60))
record={'release':published['html_url'],'download':result['browser_download_url'],'commit':sha,'bytes':p.stat().st_size,'sha256':h.hexdigest(),'desktop':'C:/Users/Dainius/Desktop/Saving Constantinople - Version 45'}
(root/'Documentation/Version45 publication.json').write_text(json.dumps(record,indent=2),encoding='utf-8');print(json.dumps(record))
