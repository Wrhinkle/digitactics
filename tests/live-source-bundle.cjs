// Browser tests exercise current source without rewriting the shipped bundle.
const path=require('path'),root=path.join(__dirname,'..');
const esbuild=require(require.resolve('esbuild',{paths:[path.join(root,'native/artifacts/audio-tools'),root]}));
let code;
module.exports=async page=>{
  code??=esbuild.buildSync({absWorkingDir:root,entryPoints:['js/main.js'],bundle:true,format:'iife',write:false,alias:{three:path.join(root,'vendor/three/build/three.module.js'),'three/addons':path.join(root,'vendor/three/addons')},logLevel:'warning'}).outputFiles[0].contents;
  await page.route('**/js/bundle.js',route=>route.fulfill({status:200,contentType:'text/javascript',body:Buffer.from(code)}));
};
