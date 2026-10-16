// Rebuilds js/bundle.js (three.js + render3d + ui as one classic script) so index.html also works from file://.
// Run after editing render3d.js or ui.js: node tools/build.cjs [path-to-esbuild-package]
const path = require('path');
const given = process.argv[2];
const esbuild = require(given && (given.startsWith('.') || path.isAbsolute(given)) ? path.resolve(given) : given || 'esbuild');
const root = path.join(__dirname, '..');
esbuild.buildSync({
  entryPoints: [path.join(root, 'js/main.js')],
  bundle: true,
  format: 'iife',
  outfile: path.join(root, 'js/bundle.js'),
  alias: {
    three: path.join(root, 'vendor/three/build/three.module.js'),
    'three/addons': path.join(root, 'vendor/three/addons'),
  },
  logLevel: 'warning',
});
console.log('wrote js/bundle.js');
