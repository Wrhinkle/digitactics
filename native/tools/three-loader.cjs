// Headless access to the unchanged vendored glTF parser for reference fixtures.
const fs = require('node:fs'), path = require('node:path');
const {pathToFileURL} = require('node:url');
const root = path.resolve(__dirname, '../..');
const read = file => fs.readFileSync(path.join(root, file), 'utf8');
const moduleUrl = source => 'data:text/javascript;base64,' + Buffer.from(source).toString('base64');
module.exports = async function () {
  const threeUrl = pathToFileURL(path.join(root, 'vendor/three/build/three.module.js')).href;
  const THREE = await import(threeUrl);
  const utils = moduleUrl(read('vendor/three/addons/utils/BufferGeometryUtils.js').replace("from 'three'", `from '${threeUrl}'`));
  const source = read('vendor/three/addons/loaders/GLTFLoader.js')
    .replace("from 'three'", `from '${threeUrl}'`)
    .replace("from '../utils/BufferGeometryUtils.js'", `from '${utils}'`)
    + '\nexport {GLTFParser, GLTFMaterialsUnlitExtension, GLTFMaterialsSpecularExtension, GLTFMaterialsEmissiveStrengthExtension};';
  return {THREE, ...await import(moduleUrl(source))};
};
