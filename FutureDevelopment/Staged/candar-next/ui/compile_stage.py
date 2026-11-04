from pathlib import Path
import subprocess

stage=Path(__file__).resolve().parent
workspace=stage.parents[1]
managed=workspace/'releases/verify-37/Version-37/Besieged_Data/Managed'
out=stage/'checks';out.mkdir(exist_ok=True)
extra=list((stage/'Assets').rglob('*.cs'))+[stage.parent/'mechanics/CandarDiplomacy.cs',stage.parent/'mechanics/updated/AI.cs',stage.parent/'mechanics/updated/OttomanDirector.cs',stage/'checks/updated/CouncilDiplomacyUI.cs']
overrides={p.name for p in extra}
sources=[p for p in (workspace/'consolidation/Assets').rglob('*.cs') if 'Editor' not in p.parts and p.name not in overrides]+extra
args=['-nologo','-target:library','-langversion:9.0','-nostdlib+','-define:DEVELOPMENT_BUILD','-out:"'+str(out/'Assembly-CSharp.dll')+'"']
args+=['-r:"'+str(p)+'"' for p in managed.glob('*.dll') if not p.name.startswith('Assembly-CSharp')]
args+=['"'+str(p)+'"' for p in sources]
rsp=out/'compile.rsp';rsp.write_text('\n'.join(args),encoding='utf-8')
r=subprocess.run(['C:/Program Files/Unity/Hub/Editor/2022.3.15f1/Editor/Data/NetCoreRuntime/dotnet.exe','C:/Program Files/Unity/Hub/Editor/2022.3.15f1/Editor/Data/DotNetSdkRoslyn/csc.dll','@'+str(rsp)],capture_output=True,text=True)
(out/'compile.log').write_text(r.stdout+r.stderr,encoding='utf-8')
print('Compiler exit:',r.returncode)
print('\n'.join(s for s in r.stdout.splitlines() if 'error ' in s));print(r.stderr[-500:])
raise SystemExit(r.returncode)
