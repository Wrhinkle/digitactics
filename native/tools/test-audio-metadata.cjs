// Compare the packaged track names and durations with the original sequencer.
// No browser, audio device or synthesis is needed for these metadata checks.
// node native/tools/test-audio-metadata.cjs [native-audio-directory]
const fs=require('fs'),path=require('path'),vm=require('vm'),assert=require('assert/strict');
const root=path.resolve(__dirname,'../..');
const directory=path.resolve(process.argv[2]||path.join(__dirname,'../artifacts/audio-content'));
const manifest=JSON.parse(fs.readFileSync(path.join(directory,'manifest.json'),'utf8'));
const context={window:{DT:{Sfx:{onChange(){}}},addEventListener(){}}};
vm.createContext(context);
vm.runInContext(fs.readFileSync(path.join(root,'js/music.js'),'utf8').replace('own.load();',''),context);
const tracks=context.window.DT.Music.TRACKS;
assert.deepEqual(Object.keys(manifest.tracks).sort(),Object.keys(tracks).sort(),'Every original music phase is named.');
let checks=1;
for(const [phase,track] of Object.entries(tracks)){
  const metadata=manifest.tracks[phase],clip=manifest.clips['music_'+phase];
  assert.equal(metadata.name,track.name,phase+' title matches the original score.');
  assert.equal(metadata.duration,track.bars.length*240/track.bpm,phase+' duration matches the compiled original score.');
  assert.ok(metadata.name&&clip,phase+' metadata has a title and corresponding PCM asset.');
  assert.equal(clip.endFrame,Math.round(metadata.duration*manifest.sampleRate),phase+' metadata names the actual exported music clip.');
  checks+=4;
}
console.log(`Built-in music metadata passed: ${checks} source-derived title, phase, duration and PCM-bound checks.`);
