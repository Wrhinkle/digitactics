// Build-time adapter for the original Codex. The native runtime never executes JavaScript.
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const zlib = require('node:zlib');
const assert = require('node:assert/strict');
const { load, ROOT } = require('../../tools/headless.cjs');

// Evaluate the complete source module with only its stylesheet import removed. DOM
// wiring is inert; all formatting, search, rules and breakpoint helpers are original.
function sourceCodex(DT) {
  const context = {
    window: { DT, addEventListener() {} },
    document: { getElementById() { return null; }, querySelector() { return null; }, addEventListener() {} },
    setInterval() {},
  };
  const code = fs.readFileSync(path.join(ROOT, 'js/codex.js'), 'utf8').replace(/^import ['"]\.\/codexstyle\.js['"];\s*$/m, '');
  vm.runInNewContext(code + '\nthis.source = {trait,bpText,notesOf,kinds,sortTraits,allTraitIds,linesWith,crestsFor,tierColor,attrText,dropSource,rulesSections,tabsNow,traitHay,lineHay,itemHay,matches,traitHTML,unitHTML,itemCard};', context, { filename: 'js/codex.js' });
  return context.source;
}

// The original portrait's --fc is not the shop-tier color. Evaluate its own
// helper so explicit line colors and each line's native set remain authoritative.
function sourcePortraitColor(DT) {
  const render = fs.readFileSync(path.join(ROOT, 'js/render3d.js'), 'utf8');
  const begin = render.indexOf('export const familyColor =');
  const end = render.indexOf('// ---------- model library', begin);
  assert(begin >= 0 && end > begin, 'Original familyColor helper is missing');
  const context = { DT };
  vm.runInNewContext(render.slice(begin, end).replace('export const', 'const') + '\nthis.color = familyColor;', context);
  return context.color;
}

const entities = { amp: '&', lt: '<', gt: '>', quot: '"', apos: "'", nbsp: '\u00a0', ndash: '–', mdash: '—', times: '×', middot: '·', rarr: '→' };
function decode(text) {
  return text.replace(/&(#x[0-9a-f]+|#\d+|[a-z]+);/gi, (all, value) => {
    if (value[0] === '#') return String.fromCodePoint(value[1].toLowerCase() === 'x' ? parseInt(value.slice(2), 16) : Number(value.slice(1)));
    if (!(value in entities)) throw Error('Unknown entity in Codex source: ' + all);
    return entities[value];
  });
}
const clean = text => decode(text).replace(/\s+/g, ' ').trim();

// Small strict parser for the trusted generated rules markup. Preserve table rows
// and cells, paragraph boundaries and list entries instead of flattening the page.
function rulesBlocks(html) {
  const root = { tag: 'root', children: [] }, stack = [root];
  for (const token of html.match(/<[^>]+>|[^<]+/g) || []) {
    if (!token.startsWith('<')) { stack.at(-1).children.push(token); continue; }
    const match = token.match(/^<(\/)?([a-z][\w-]*)\b[^>]*>$/i);
    if (!match) throw Error('Unrecognized Codex markup: ' + token);
    const tag = match[2].toLowerCase();
    if (match[1]) {
      if (stack.at(-1).tag !== tag) throw Error('Unbalanced Codex markup: ' + token);
      stack.pop();
    } else {
      const node = { tag, children: [] }; stack.at(-1).children.push(node);
      if (!['br', 'img', 'hr'].includes(tag) && !token.endsWith('/>')) stack.push(node);
    }
  }
  if (stack.length !== 1) throw Error('Unclosed Codex markup');
  const text = node => typeof node === 'string' ? node : node.tag === 'br' ? '\n'
    : (node.tag === 'i' ? ' ' : '') + node.children.map(text).join(node.tag === 'div' || node.tag === 'tr' ? ' ' : '');
  const descendants = (node, tag) => node.children.flatMap(child => typeof child === 'string' ? [] : child.tag === tag ? [child] : descendants(child, tag));
  const blocks = [];
  const walk = node => {
    if (typeof node === 'string') { if (clean(node)) blocks.push({ kind: 'paragraph', text: clean(node), rows: [] }); return; }
    if (node.tag === 'table') {
      const rows = descendants(node, 'tr').map(row => row.children.filter(c => typeof c !== 'string' && ['td', 'th'].includes(c.tag)).map(cell => clean(text(cell))));
      blocks.push({ kind: 'table', text: '', rows });
    } else if (['p', 'li'].includes(node.tag) || !node.children.some(c => typeof c !== 'string' && ['p', 'li', 'div', 'ul', 'ol', 'table'].includes(c.tag))) {
      const value = clean(text(node)); if (value) blocks.push({ kind: 'paragraph', text: (node.tag === 'li' ? '• ' : '') + value, rows: [] });
    } else for (const child of node.children) walk(child);
  };
  for (const node of root.children) walk(node);
  return blocks;
}

function exportCodex() {
  const DT = load({ strict: true }), source = sourceCodex(DT), portraitColor = sourcePortraitColor(DT), variants = [], fixtures = [];
  const hay = (...fields) => fields.flat().join(' ').toLowerCase();
  const queries = ['', '  ', 'Agumon', '3 gold', 'wild', 'family', 'role', 'damage', 'health', 'armor', 'mana', 'digi egg', 'silver', 'gold', 'prismatic', 'normal', 'hyper roll', '20%', '3-2', 'NO SUCH CODEX ENTRY'];
  for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) {
    DT.Sets.activate(set.id, pace);
    const id = `${set.id}/${pace}`, core = new Set(DT.CORE_ITEMS);
    const traits = source.sortTraits(source.allTraitIds()).map(id => {
      const t = source.trait(id);
      return { id, name: t.name, kind: t.kind, color: t.color, icon: t.T.icon || '', description: t.T.desc || '', notes: source.notesOf(t.T),
        breakpoints: t.bp.map((count, i) => ({ count, text: source.bpText(t.T, i) })),
        lineIds: source.linesWith(id).map(l => l.id), crestIds: source.crestsFor(id).map(it => it.id), searchText: hay(source.traitHay(id)) };
    });
    const lines = DT.LINES.map(l => ({ id: l.id, tier: l.tier, shop: !!l.shop, color: source.tierColor(l.tier), portraitColor: portraitColor(l), attribute: l.attr,
      attributeColor: (DT.ATTR[l.attr] || {}).color || '#c8c8c8', attributeText: source.attrText(l.attr), type: l.type || '', traits: l.traits || [],
      forms: [0, 1, 2].filter(stage => l.forms[stage] != null).map(stage => {
        const stats = DT.unitStats(l, stage);
        return { stage, name: l.forms[stage], stageName: DT.stageName(l, stage), abilityName: (l.abil || [])[stage] || 'Ability', abilityText: DT.abilityText(l, stage),
          stats: Object.fromEntries(['maxHp', 'ad', 'as', 'range', 'armor', 'mr', 'mana', 'maxMana'].map(k => [k, stats[k]])), sellValue: l.shop ? DT.sellValue({ line: l.id, stage }) : 0 };
      }), searchText: hay(source.lineHay(l)) }));
    const items = Object.values(DT.ITEMS).map(it => ({ id: it.id, name: it.name, description: it.desc || '', stats: it.stats || {}, statsText: DT.statText && it.stats ? DT.statText(it.stats) : '',
      kind: it.component ? 'component' : it.emblem ? 'crest' : core.has(it.id) ? 'core' : 'special', emblem: it.emblem || '', drop: !!it.drop, dropSource: source.dropSource(),
      recipes: it.from && !it.component ? (DT.recipesOf ? DT.recipesOf(it.id) : [it.from]) : [], searchText: hay(source.itemHay(it)) }));
    const augments = DT.AUGMENTS.map(a => ({ id: a.id, name: a.name, description: a.desc || '', tier: a.tier, tierName: DT.AUG_TIER_NAME[a.tier] || `Tier ${a.tier}`,
      color: DT.AUG_TIER_COLOR[a.tier] || '#c9d1dc', searchText: hay(a.name, a.desc, DT.AUG_TIER_NAME[a.tier]) }));
    const wisps = DT.WISPS.map(w => ({ id: w.id, name: w.name, description: w.desc || '', kind: w.kind, basePrice: w.base || 0, minStage: w.minStage,
      exampleStage: Math.max(2, w.minStage), exampleCost: DT.wispCost(w, Math.max(2, w.minStage)), searchText: hay(w.name, w.desc, w.kind) }));
    const sourceRules = source.rulesSections();
    const rules = sourceRules.map(s => ({ id: s.id, title: s.title, html: s.html, blocks: rulesBlocks(s.html), searchText: hay(s.title, s.html.replace(/<[^>]+>/g, ' ')) }));
    const variant = { id, setId: set.id, name: DT.SET.name, pace, traits, lines, items, augments, wisps, rules,
      componentIds: DT.COMPONENT_IDS, recipeGrid: DT.COMPONENT_IDS.map(a => DT.COMPONENT_IDS.map(b => DT.combineItems(a, b) || '')),
      traitKinds: source.kinds(), tabs: source.tabsNow().map(([key]) => key) };
    variants.push(variant);

    // Reference queries are evaluated by the original search helpers, independently
    // of the exported search strings and native matching implementation.
    const searches = queries.map(query => ({ query,
      traits: source.sortTraits(source.allTraitIds()).filter(id => source.matches(query, source.traitHay(id))),
      digimon: DT.LINES.filter(l => source.matches(query, source.lineHay(l))).map(l => l.id),
      items: Object.values(DT.ITEMS).filter(it => source.matches(query, source.itemHay(it))).map(it => it.id),
      augments: DT.AUGMENTS.filter(a => source.matches(query, a.name, a.desc, DT.AUG_TIER_NAME[a.tier])).map(a => a.id),
      wisps: DT.WISPS.filter(w => source.matches(query, w.name, w.desc, w.kind)).map(w => w.id),
      rules: sourceRules.filter(s => source.matches(query, s.title, s.html.replace(/<[^>]+>/g, ' '))).map(s => s.id),
    }));
    const tables = sourceRules.flatMap(section => [...section.html.matchAll(/<table\b[^>]*>([\s\S]*?)<\/table>/g)].map((match, index) => ({ section: section.id, index,
      rows: [...match[1].matchAll(/<tr\b[^>]*>([\s\S]*?)<\/tr>/g)].map(row => [...row[1].matchAll(/<t[dh]\b[^>]*>([\s\S]*?)<\/t[dh]>/g)].map(cell => clean(cell[1].replace(/<[^>]*>/g, '')))) })));
    fixtures.push({ id, searches, tables,
      ruleText: Object.fromEntries(sourceRules.map(section => [section.id, clean(section.html.replace(/<[^>]+>/g, ' ')).replace(/\s+/g, '')])),
      traitCards: Object.fromEntries(DT.Codex.traitIds().map(id => [id, source.traitHTML(id, null)])),
      unitCards: Object.fromEntries(DT.LINES.map(line => [line.id, source.unitHTML(line.id, null)])),
      itemCards: Object.fromEntries(Object.values(DT.ITEMS).map(item => [item.id, source.itemCard(item)])),
    });
    for (const section of rules) {
      assert(section.blocks.length, `${id}/${section.id} has no readable blocks`);
      const rendered = section.blocks.map(block => block.kind === 'table' ? block.rows.flat().join(' ') : block.text.replace(/^• /, '')).join(' ').replace(/\s+/g, '');
      assert.equal(rendered, clean(section.html.replace(/<[^>]+>/g, ' ')).replace(/\s+/g, ''), `${id}/${section.id} lost source text`);
    }
  }
  assert.equal(variants.length, 22, 'Expected every playable set and both paces');
  const content = path.join(ROOT, 'native/content/codex.json'), fixture = path.join(ROOT, 'native/tests/fixtures/codex.json.gz');
  fs.mkdirSync(path.dirname(content), { recursive: true }); fs.mkdirSync(path.dirname(fixture), { recursive: true });
  fs.writeFileSync(content, JSON.stringify({ schema: 1, variants }) + '\n');
  fs.writeFileSync(fixture, zlib.gzipSync(JSON.stringify(fixtures), { level: 9 }));
  console.log(`Exported Codex for ${variants.length} variants: ${variants.reduce((n, v) => n + v.lines.length, 0)} lines, ${variants.reduce((n, v) => n + v.rules.length, 0)} original rules sections, and ${fixtures.length * queries.length * 6} search references.`);
}

if (require.main === module) exportCodex();
module.exports = { sourceCodex, sourcePortraitColor, rulesBlocks, exportCodex };
