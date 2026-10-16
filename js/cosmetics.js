// Cosmetics: one registry shared by the store, Little Legends, emotes, boards, kill screens and soundboard.
// Classic script (loads before bundle.js) so every module can register at import time.
//
// Kinds: 'legend' | 'emote' | 'board' | 'killscreen' | 'sound' | 'dance' (js/dances.js) | 'elimsound' (js/elimsounds.js).
// Older saves and older clients' loadouts without a kind get its first free item.
// Item: { id, name, price (Bits), rarity: 'common'|'rare'|'epic'|'legendary', desc?, icon? (URL or emoji),
//         free?: true (owned by everyone), egg?: false (left out of egg rolls), data?: anything the feature needs,
//         linked?: [kind, id] (owned whenever that other item is owned; not sold on its own) }
//
// DT.Cosmetics.register(kind, items)        add catalog items (call again to add more)
// DT.Cosmetics.list(kind) / get(kind, id)
// DT.Cosmetics.owns(kind, id) / grant(kind, id)
// DT.Cosmetics.equipped(kind)               id; for 'emote' and 'sound' an array of up to SLOTS ids (the wheel)
// DT.Cosmetics.equip(kind, id, slot?)       slot index for 'emote' / 'sound'
// DT.Cosmetics.loadout()                    { legend, legendForm, board, killscreen, dance, elimsound, emote: [...], sound: [...] } (sent to other players)
// DT.Cosmetics.onChange(fn)                 fn(kind) after any grant / equip
//
// DT.Wallet.bits / add(n, reason) / spend(n) -> bool / onChange(fn)
// Everything is saved in this browser's localStorage.
(function (G) {
  const DT = G.DT;
  const KEY = 'dt.cosmetics.v1';
  const KINDS = ['legend', 'emote', 'board', 'killscreen', 'sound', 'dance', 'elimsound'];
  const SLOTS = 6;
  const catalog = {};
  for (const k of KINDS) catalog[k] = [];
  const listeners = new Set();
  const walletListeners = new Set();

  let save = { bits: 0, owned: {}, equipped: {}, history: [] };
  try { const s = JSON.parse(localStorage.getItem(KEY)); if (s && typeof s === 'object') save = Object.assign(save, s); } catch (e) { /* blocked or corrupt */ }
  for (const k of KINDS) save.owned[k] = save.owned[k] || [];
  const persist = () => { try { localStorage.setItem(KEY, JSON.stringify(save)); } catch (e) { /* private mode */ } };
  const emit = (kind) => { for (const fn of listeners) { try { fn(kind); } catch (e) { console.warn(e); } } };

  const multi = (kind) => kind === 'emote' || kind === 'sound';

  const Cosmetics = {
    KINDS,
    SLOTS,
    register(kind, items) {
      if (!catalog[kind]) throw new Error(`unknown cosmetic kind ${kind}`);
      for (const it of items) {
        const i = catalog[kind].findIndex((x) => x.id === it.id);
        if (i >= 0) catalog[kind][i] = it; else catalog[kind].push(it);
      }
      emit(kind);
    },
    list(kind) { return catalog[kind] || []; },
    get(kind, id) { return (catalog[kind] || []).find((x) => x.id === id) || null; },
    owns(kind, id) {
      const it = this.get(kind, id);
      if (it && it.linked) return this.owns(it.linked[0], it.linked[1]);
      return !!it && (it.free || save.owned[kind].includes(id));
    },
    grant(kind, id) {
      if (!save.owned[kind].includes(id)) save.owned[kind].push(id);
      persist();
      emit(kind);
    },
    equipped(kind) {
      const owned = (id) => id && this.owns(kind, id);
      if (multi(kind)) {
        const cur = Array.isArray(save.equipped[kind]) ? save.equipped[kind] : null;
        if (cur) return cur.map((id) => (owned(id) ? id : null));
        // default wheel: the free items, in catalog order
        const free = this.list(kind).filter((x) => x.free).map((x) => x.id).slice(0, SLOTS);
        while (free.length < SLOTS) free.push(null);
        return free;
      }
      const id = save.equipped[kind];
      if (owned(id)) return id;
      const def = this.list(kind).find((x) => x.free);
      return def ? def.id : null;
    },
    equip(kind, id, slot) {
      if (id != null && !this.owns(kind, id)) return false;
      if (multi(kind)) {
        const cur = this.equipped(kind).slice();
        const s = slot == null ? Math.max(0, cur.indexOf(null)) : slot;
        if (s < 0 || s >= SLOTS) return false;
        const dup = cur.indexOf(id);
        if (dup >= 0 && id != null) cur[dup] = cur[s];
        cur[s] = id;
        save.equipped[kind] = cur;
      } else save.equipped[kind] = id;
      persist();
      emit(kind);
      return true;
    },
    loadout() {
      const o = {};
      for (const k of KINDS) o[k] = this.equipped(k);
      // the legend's digivolved form (js/legendevo.js), or null for its base form
      o.legendForm = DT.LegendEvo ? DT.LegendEvo.formOf(o.legend) : null;
      return o;
    },
    onChange(fn) { listeners.add(fn); return () => listeners.delete(fn); },
    notify(kind) { emit(kind); }, // something about an item changed (a legend digivolved)
  };

  const Wallet = {
    get bits() { return save.bits; },
    add(n, reason) {
      n = Math.max(0, Math.round(n));
      if (!n) return;
      save.bits += n;
      save.history = [{ n, reason: String(reason || ''), at: Date.now() }, ...save.history].slice(0, 30);
      persist();
      for (const fn of walletListeners) { try { fn(save.bits, n, reason); } catch (e) { console.warn(e); } }
    },
    spend(n) {
      if (n > save.bits) return false;
      save.bits -= n;
      persist();
      for (const fn of walletListeners) { try { fn(save.bits, -n); } catch (e) { console.warn(e); } }
      return true;
    },
    history() { return save.history.slice(); },
    onChange(fn) { walletListeners.add(fn); return () => walletListeners.delete(fn); },
  };

  // Social: what other Tamers wear, and live emotes / soundboard clips. Works in solo too.
  // DT.Social.lookOf(player)         loadout for a Game player (yours, a remote human's, or a random one for AI Tamers)
  // DT.Social.send(kind, id, slot)   'emote' | 'sound': plays here at once and goes to everyone in a multiplayer game
  // DT.Social.on(kind, fn)           fn(slot, id) for every emote / sound, yours included
  // net.js sets DT.Social.net = { lookOf(slot) -> loadout|null, send(kind, id) } while a multiplayer game runs.
  const fxFns = {};
  const aiLooks = new Map();
  const hash = (s) => { let h = 2166136261; for (let i = 0; i < s.length; i++) { h ^= s.charCodeAt(i); h = Math.imul(h, 16777619) >>> 0; } return h; };
  // look.legend comes back as 'id@form' when the legend has digivolved; DT.Legends.createView / html take either
  const withForm = (l) => (l && DT.LegendEvo ? DT.LegendEvo.look(l) : l);
  const Social = {
    net: null,
    lookOf(player) {
      if (!player) return null;
      const g = DT.UI && DT.UI.game ? DT.UI.game() : null;
      if (g && g.human === player) return withForm(Cosmetics.loadout());
      if (player.isHuman && Social.net) { const l = Social.net.lookOf(player.id); if (l) return withForm(l); }
      // AI Tamers (and humans without a loadout yet) wear a stable random pick from the whole catalog
      const key = `${player.id}:${player.name}`;
      if (!aiLooks.has(key)) {
        const o = {};
        for (const k of KINDS) {
          const items = Cosmetics.list(k);
          const pick = (n) => (items.length ? items[hash(`${key}:${k}:${n}`) % items.length].id : null);
          o[k] = k === 'emote' || k === 'sound' ? Array.from({ length: SLOTS }, (_, n) => pick(n)) : pick(0);
        }
        o.legendForm = DT.LegendEvo ? DT.LegendEvo.aiForm(o.legend, key) : null;
        aiLooks.set(key, o);
      }
      return withForm(aiLooks.get(key));
    },
    send(kind, id, slot) {
      Social.emit(kind, slot, id);
      if (Social.net) Social.net.send(kind, id);
    },
    on(kind, fn) { (fxFns[kind] = fxFns[kind] || new Set()).add(fn); return () => fxFns[kind].delete(fn); },
    emit(kind, slot, id) { for (const fn of fxFns[kind] || []) { try { fn(slot, id); } catch (e) { console.warn(e); } } },
    clearAiLooks() { aiLooks.clear(); },
  };

  DT.Cosmetics = Cosmetics;
  DT.Social = Social;
  DT.Wallet = Wallet;
})(typeof window !== 'undefined' ? window : globalThis);
