import pathlib,zipfile,shutil,hashlib,json,subprocess,urllib.request
root=pathlib.Path.cwd();src=root/'Builds/Version-45';dest=pathlib.Path('C:/Users/Dainius/Desktop/Saving Constantinople - Version 45');dest.mkdir(exist_ok=True)
exclude={'Saves','SallyChecks'}
files=[p for p in src.rglob('*') if p.is_file() and not any(x in exclude or 'DoNotShip' in x for x in p.relative_to(src).parts)]
for p in files:
 q=dest/p.relative_to(src);q.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,q)
shutil.copy2(root/'Documentation/Version45 release notes.md',dest/'CHANGELOG.txt')
archive=root/'Builds/Saving-Constantinople-Version-45-Windows.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=1) as z:
 for p in files:z.write(p,p.relative_to(src))
 z.write(dest/'CHANGELOG.txt','CHANGELOG.txt')
with zipfile.ZipFile(archive) as z:assert z.testzip() is None
print(json.dumps({'desktop':str(dest),'zip':str(archive),'bytes':archive.stat().st_size,'files':len(files)}))
