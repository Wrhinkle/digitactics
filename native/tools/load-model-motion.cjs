// Load the same source modules used by the browser without a renderer or a
// package-resolution dependency. Reference exporters must share this entry point
// so adding a source module cannot silently leave one fixture on an older path.
const fs=require('node:fs'),path=require('node:path');
const {pathToFileURL}=require('node:url');
module.exports=async function(){
  const root=path.resolve(__dirname,'../..');
  const three=pathToFileURL(path.join(root,'vendor/three/build/three.module.js')).href;
  const data=source=>'data:text/javascript;base64,'+Buffer.from(source).toString('base64');
  const read=file=>fs.readFileSync(path.join(root,'js',file),'utf8').replace("from 'three'",`from '${three}'`);
  const anatomy=data(read('model-anatomy.js'));
  const rigid=data(read('model-rigid-parts.js'));
  const motion=read('model-motion.js').replace("from './model-anatomy.js'",`from '${anatomy}'`).replace("from './model-rigid-parts.js'",`from '${rigid}'`);
  return import(data(motion));
};
