// Specialize authored template expressions while executing their original closures for static text.
// Unsupported expressions stay explicit so they cannot silently become approximate formulas.
const fs = require('fs'), path = require('path');
const underlordsSource = fs.readFileSync(path.join(__dirname, '../../js/sets/underlords-mech.js'), 'utf8');
const delegated = new Map();
for (const block of underlordsSource.split(/(?=^    ul_\w+: \{)/m)) {
  const id = block.match(/^    (ul_\w+): \{/);
  const text = block.match(/^      text: T\((.+)\),\s*$/m);
  if (id && text) delegated.set(id[1], text[1]);
}
module.exports = function customTemplate(DT, line, stage) {
  const formatter = line.abilText || DT.abilityDef?.(line.type)?.text;
  if (typeof formatter !== 'function') return null;
  let source = formatter.toString();
  if (source === '(line, stage, v) => s(line, stage, Math.round(v))') {
    source = delegated.get(line.type);
    if (!source) throw Error(`Missing original Underlords delegated formatter: ${line.type}`);
    // abilityText already rounds v; the wrapper's second round is therefore an identity.
  } else if (formatter.length < (line.abilText ? 2 : 3)) {
    // These authored functions accept only line/stage, never the computed AP value.
    const description = DT.abilityText(line,stage);
    for (const ap of [0,.1,1,33.3,99.5,100,127.25,200,999.9,10000])
      if (description !== DT.abilityText(line,stage,ap)) throw Error(`Unexpected AP dependence: ${line.id}/${stage}`);
    return { scaledDescription: description, abilityValue: 0, formulas: [] };
  }
  const expressions = [...source.matchAll(/\$\{([^}]*)\}/g)].map(match => match[1]).filter(expr => /\bv\b/.test(expr));
  const body = source.slice(source.indexOf('=>') + 2).replace(/\$\{[^}]*\}/g, '');
  if (/\bv\b/.test(body) || expressions.length === 0) return null;
  const formulas = [];
  for (let expression of [...new Set(expressions)]) {
    let rounded = false;
    if (/^Math\.round\(.*\)$/.test(expression)) { rounded = true; expression = expression.slice(11,-1); }
    const match = expression.trim().match(/^v(?:\s*([*/])\s*(\d+(?:\.\d+)?))?$/);
    if (!match) return null;
    formulas.push({ operation: match[1] || '*', operand: Number(match[2] || 1), rounded });
  }
  const sentinel = 1000000007;
  const values = formulas.map(f => {const v = f.operation === '/' ? sentinel / f.operand : sentinel * f.operand; return String(f.rounded ? Math.round(v) : v);});
  if (new Set(values).size !== values.length) return null;
  const original = DT.abilityValue;
  let template;
  try { DT.abilityValue = () => sentinel; template = DT.abilityText(line,stage); }
  finally { DT.abilityValue = original; }
  // Longest first prevents a shorter numeric value matching within a longer one.
  for (const i of values.map((_,i) => i).sort((a,b) => values[b].length-values[a].length))
    template = template.replaceAll(values[i], `{value:${i}}`);
  const evaluate = ap => {
    const value = Math.round(original(line,stage) * ap / 100);
    return formulas.reduce((text,f,i) => {
      const n = f.operation === '/' ? value / f.operand : value * f.operand;
      return text.replaceAll(`{value:${i}}`, String(f.rounded ? Math.round(n) : n));
    },template);
  };
  for (const ap of [0, .1, 1, 33.3, 99.5, 100, 127.25, 200, 999.9, 10000])
    if (evaluate(ap) !== DT.abilityText(line,stage,ap)) throw Error(`Custom ability template mismatch: ${line.id}/${stage}/${ap}`);
  return { scaledDescription: template, abilityValue: original(line,stage), formulas };
};
