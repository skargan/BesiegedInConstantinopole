import {readFile,writeFile,mkdir,cp} from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {existsSync} from 'node:fs';
const base=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..'),dist=path.join(base,'dist');
let html=await readFile(path.join(dist,'index.html'),'utf8');
const css=await readFile(path.join(dist,'style.css'),'utf8');
html=html.replace('<link rel="stylesheet" href="style.css">',()=>'<style>'+css+'</style>');
for(const f of ['data.js','graph-core.js','preparation-core.js','app.js']){const code=await readFile(path.join(dist,f),'utf8');html=html.replace(`<script src="${f}"></script>`,()=>'<script>'+code+'</script>');}
await writeFile(path.join(base,'Besieged Event Map.html'),html);
console.log('Built portable HTML with all data, styles and code embedded.');
const repoFlag=process.argv.indexOf('--repo');
const repo=repoFlag>=0?path.resolve(process.argv[repoFlag+1]):path.resolve(base,'../game-repo');
if(!existsSync(path.join(repo,'Assets'))){console.log('Standalone build complete. To also install in Unity: node scripts/package.mjs --repo /path/to/project');process.exit(0);}
const target=path.join(repo,'Tools/BesiegedEventMap');
await mkdir(target,{recursive:true});
if(path.resolve(target)!==path.resolve(base))for(const f of ['dist','scripts','reference-extracts','tests','planning','README.md','Preparation and Galata review.html','Besieged Event Map.html','Open Event Map.cmd','data.schema.json'])await cp(path.join(base,f),path.join(target,f),{recursive:true});
await mkdir(path.join(repo,'Assets/Editor'),{recursive:true});
if(existsSync(path.join(base,'unity/BesiegedEventMap.cs')))await cp(path.join(base,'unity/BesiegedEventMap.cs'),path.join(repo,'Assets/Editor/BesiegedEventMap.cs'));
console.log('Installed editor launcher and tool in the local fork checkout.');
