const fs=require('node:fs'),path=require('node:path'),cp=require('node:child_process');
const root=path.resolve(__dirname,'..'), source=fs.readFileSync(path.join(root,'native/build.ps1'),'utf8');
const fixtureScripts=[...source.matchAll(/Invoke-Checked 'node' @\('tools\/(export-[^']+\.cjs)'\)/g)].map(m=>m[1]);
const first=fixtureScripts.indexOf('export-economy.cjs'),last=fixtureScripts.indexOf('export-ai-inventory.cjs');
const tasks=['export-lockstep.cjs',...fixtureScripts.slice(first,last+1),'export-reward-tracker.cjs','export-combat-fixtures.cjs','export-event-fixtures.cjs','export-scene-fixtures.cjs','export-choreography.cjs','export-audio-cues.cjs'];
for(const task of [...new Set(tasks)]) { console.log('Running '+task);cp.execFileSync(process.execPath,[path.join(root,'native/tools',task)],{cwd:root,stdio:'inherit',maxBuffer:16*1024*1024}); }

