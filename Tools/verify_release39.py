"""Run focused suites in the actual release player with isolated test saves."""
from pathlib import Path
import subprocess, time, json
root=Path(__file__).resolve().parents[1]; build=root/'Builds/Version-39'
tests=[('provisions38','Provisions38'),('technology-desk','TechnologyDesk'),('ionian','Ionian'),('chatter','Chatter'),('art-integration','ArtIntegration'),('event-frame','EventFrames'),('provincial-art','ProvincialArt'),('chamber','Chamber')]
out=root/'Documentation/Release39';out.mkdir(exist_ok=True)
results=[]
for flag,folder in tests:
    start=time.time();log=build/f'{flag}-verification.log'
    si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
    try:
        proc=subprocess.run([str(build/'Besieged.exe'),f'-{flag}-capture','-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',str(log)],cwd=build,startupinfo=si,timeout=180)
        checks=build/'Captures'/folder/'checks.txt'
        # Older probes use a slightly different evidence folder; find only fresh reports.
        fresh=[p for p in (build/'Captures').rglob('checks.txt') if p.stat().st_mtime>=start-1]
        if not checks.exists() or checks.stat().st_mtime<start-1:
            checks=fresh[0] if len(fresh)==1 else checks
        report=checks.read_text(encoding='utf-8-sig') if checks.exists() and checks.stat().st_mtime>=start-1 else 'MISSING FRESH REPORT'
        passed=proc.returncode==0 and 'passed' in report.lower() and not any(x in report for x in ['FAILED','NEEDS REVIEW'])
        result={'suite':flag,'exitCode':proc.returncode,'passed':passed,'seconds':round(time.time()-start,1),'report':report,'evidence':str(checks.relative_to(root))}
    except subprocess.TimeoutExpired:
        result={'suite':flag,'passed':False,'report':'TIMEOUT after180seconds'}
    results.append(result);(out/'runtime-checks.json').write_text(json.dumps(results,indent=2),encoding='utf8')
    print(flag,result,flush=True)
    if not result['passed']:break
raise SystemExit(0 if len(results)==len(tests) and all(x['passed'] for x in results) else 1)
