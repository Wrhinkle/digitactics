const fs=require('fs'),path=require('path'),Module=require('module');
const filename=path.resolve(__dirname,'../tests/sets.cjs');
const source=fs.readFileSync(filename,'utf8').replace('check(client.pace === pace && Lock.digest(host)',"if(id==='_template')console.log('Template diagnostic',JSON.stringify({pace,stage:host.stage,round:host.round,over:host.over,sameDigest:Lock.digest(host)===Lock.digest(client),entries:n})); check(client.pace === pace && Lock.digest(host)");
const m=new Module(filename,module);m.filename=filename;m.paths=Module._nodeModulePaths(path.dirname(filename));m._compile(source,filename);
