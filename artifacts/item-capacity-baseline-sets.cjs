const fs = require('fs'), path = require('path'), cp = require('child_process');
const root = path.resolve(__dirname, '..');
const original = new Map(['js/items.js','js/game.js','js/combat.js','js/sets/arcane-mech.js'].map(p => [path.join(root,p), cp.execFileSync('git',['show','HEAD:'+p],{cwd:root})]));
const read = fs.readFileSync;
fs.readFileSync = function(p, options) { const b = typeof p === 'string' && original.get(path.resolve(p)); if (b) return typeof options === 'string' || options?.encoding ? b.toString(typeof options === 'string' ? options : options.encoding) : b; return read.apply(this, arguments); };
require('../tests/sets.cjs');
