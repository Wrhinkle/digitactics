// Digi-Tactics design system: components, the client shell (js/client.js) and the restyle of every out-of-game
// window on one token set, in the style of a League-type launcher: dark navy surfaces, thin gold frames, angled and
// beveled buttons, our brand orange and cyan for energy. Tokens (--c-*, --lc-*, --r-*, --rad-*, --sp-*, --fs-*,
// --sh-*, --dur-*) are in style.css :root so the classic HUD and every module share them. Imported last in
// js/main.js, so these rules come after every module's own <style> and win ties. :where() keeps the generic button
// rules at zero extra specificity so a module's own button classes still win.
// Art: assets/client/*.webp (see js/client.js).
const A = (n) => `url('assets/client/${n}.webp')`;
const css = String.raw`
/* =====================================================================
   Base
   ===================================================================== */
:where(#overlay, #cnav, #csocial, #boot, #cmodal, #ctoasts, #cload, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop, #picker, #tooltip, #modal, #banner, #cxPanel) {
  font-family: var(--font-body); font-feature-settings: "cv11", "ss01"; -webkit-font-smoothing: antialiased; }
:where(#overlay, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #cload) h1, :where(#overlay, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #cload) h2 { font-family: var(--font-display); letter-spacing: 0.02em; }
:where(#overlay, #cnav, #csocial, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop) ::-webkit-scrollbar { width: 8px; height: 8px; }
:where(#overlay, #cnav, #csocial, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop) ::-webkit-scrollbar-thumb { background: var(--lc-gold-5); border-radius: 0; border: 2px solid transparent; background-clip: padding-box; }
:where(#overlay, #cnav, #csocial, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop) ::-webkit-scrollbar-thumb:hover { background-color: var(--lc-gold-4); }
:where(#overlay, #cnav, #csocial, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop) ::-webkit-scrollbar-track { background: rgba(1, 10, 19, 0.5); }
:where(#overlay, #cnav, #csocial, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop, #picker, #modal) :focus-visible { outline: 2px solid var(--c-accent); outline-offset: 2px; }
:where(#overlay, #cmodal) a { color: var(--c-accent); text-decoration: none; }
:where(#overlay, #cmodal) a:hover { text-decoration: underline; }
.r-common { --rc: var(--r-common); } .r-uncommon { --rc: var(--r-uncommon); } .r-rare { --rc: var(--r-rare); }
.r-epic { --rc: var(--r-epic); } .r-legendary { --rc: var(--r-legendary); }
.muted.small, .small { font-size: var(--fs-sm); }
.hidden { display: none !important; }
.eyebrow { font-family: var(--font-display); font-size: var(--fs-xs); font-weight: 700; letter-spacing: 0.16em; text-transform: uppercase; color: var(--lc-gold-2); }
.page-title { margin: 2px 0 0; font-family: var(--font-display); font-size: var(--fs-2xl); font-weight: 700; line-height: 1.1; color: var(--lc-gold-1); text-transform: uppercase; letter-spacing: 0.04em; }
.page-head { display: flex; align-items: flex-end; justify-content: space-between; gap: var(--sp-4); margin-bottom: var(--sp-3); }
.empty:not(.card) { padding: var(--sp-5); text-align: center; color: var(--c-text-3); border: 1px dashed var(--lc-gold-5); grid-column: 1 / -1; }
.empty.small { padding: var(--sp-3); font-size: var(--fs-sm); }
kbd { font-family: var(--font-body); background: var(--c-surface-3); border: 1px solid var(--c-border-2); border-bottom-width: 2px; border-radius: var(--rad-xs); padding: 0 6px; font-size: var(--fs-xs); color: var(--c-text-1); }

/* =====================================================================
   Buttons: beveled navy with gold trim; CTA in hextech blue; PLAY in brand orange
   ===================================================================== */
:where(#overlay, #cnav, #csocial, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop, #picker, #modal) button {
  font-family: var(--font-body); font-weight: 600; color: var(--lc-text); background: linear-gradient(180deg, #1e2328, #141a22);
  border: 1px solid var(--lc-gold-4); border-radius: 2px; transition: background var(--dur-1), border-color var(--dur-1), color var(--dur-1), box-shadow var(--dur-1), transform var(--dur-1); }
:where(#overlay, #cnav, #csocial, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop, #picker, #modal) button:hover:not(:disabled) { border-color: var(--lc-gold-2); color: var(--lc-gold-1);
  background: linear-gradient(180deg, #262d36, #1a212b); box-shadow: 0 0 10px rgba(200, 170, 110, 0.18); }
:where(#overlay, #cnav, #csocial, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop, #picker, #modal) button:active:not(:disabled) { transform: translateY(1px); }
:where(#overlay, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop, #picker, #modal) button.primary,
.btn-primary { color: var(--c-text-inv) !important; background: linear-gradient(180deg, #ffd27a, var(--c-brand-2) 30%, var(--c-brand)) !important; border-color: #f0d28c !important; font-weight: 700;
  box-shadow: 0 2px 10px rgba(255, 138, 61, 0.25), inset 0 1px 0 rgba(255, 255, 255, 0.35); }
:where(#overlay, #cmodal, #dtStore, #cxFull, #dtFarm, #dtEvo, #sndPop, #picker, #modal) button.primary:hover:not(:disabled),
.btn-primary:hover:not(:disabled) { filter: brightness(1.08); box-shadow: var(--glow-brand), inset 0 1px 0 rgba(255, 255, 255, 0.35); }
.btn { display: inline-flex; align-items: center; justify-content: center; gap: var(--sp-2); padding: 8px 16px; font-size: var(--fs-md); line-height: 1.2; white-space: nowrap; cursor: pointer; }
.btn:disabled { opacity: 0.45; cursor: not-allowed; }
.btn-sm { padding: 5px 11px; font-size: var(--fs-sm); }
.btn-lg { padding: 11px 22px; font-size: var(--fs-lg); }
.btn-xl { padding: 14px 44px; font-size: var(--fs-xl); }
.btn-block { width: 100%; }
.btn-secondary { font-family: var(--font-display) !important; letter-spacing: 0.08em; text-transform: uppercase; }
.btn-ghost { background: transparent !important; border-color: transparent !important; color: var(--lc-mute) !important; }
.btn-ghost:hover:not(:disabled) { background: rgba(200, 170, 110, 0.08) !important; color: var(--lc-gold-1) !important; border-color: var(--lc-gold-5) !important; box-shadow: none !important; }
.btn-danger { color: #fff !important; background: linear-gradient(180deg, #ff6b7c, #d63a4e) !important; border-color: #ff8b98 !important; }
.btn-danger:hover:not(:disabled) { box-shadow: 0 0 0 1px rgba(255, 84, 104, 0.6), 0 6px 20px rgba(255, 84, 104, 0.35); }
.btn-icon { width: 34px; height: 34px; padding: 0; display: inline-grid; place-items: center; }
/* the hextech call to action: navy-teal fill, gold frame, bright text */
.lc-cta { position: relative; font-family: var(--font-display) !important; font-weight: 700 !important; letter-spacing: 0.12em; text-transform: uppercase; padding: 10px 26px !important;
  color: #cdfafa !important; background: linear-gradient(180deg, #0b3a4a 0%, #062033 100%) !important; border: 2px solid transparent !important; border-radius: 0 !important;
  border-image: linear-gradient(180deg, var(--lc-gold-2), var(--lc-gold-4)) 1 !important; box-shadow: inset 0 0 14px rgba(63, 214, 255, 0.25) !important; }
.lc-cta:hover:not(:disabled) { color: #fff !important; background: linear-gradient(180deg, #0f5266, #08304a) !important; border-image: linear-gradient(180deg, var(--lc-gold-1), var(--lc-gold-2)) 1 !important;
  box-shadow: inset 0 0 18px rgba(63, 214, 255, 0.45), 0 0 18px rgba(63, 214, 255, 0.3) !important; }
.lc-cta:disabled { filter: grayscale(0.9) brightness(0.7); }
/* the big launcher button */
.btn-play { position: relative; font-family: var(--font-display) !important; font-weight: 700; text-transform: uppercase; letter-spacing: 0.14em;
  color: var(--c-text-inv) !important; padding: 12px 34px; font-size: var(--fs-lg); border-radius: 0 !important;
  background: linear-gradient(180deg, #ffd26a 0%, var(--c-brand-2) 35%, var(--c-brand) 100%) !important; border: 1px solid #ffe09a !important;
  clip-path: polygon(10px 0, 100% 0, 100% calc(100% - 10px), calc(100% - 10px) 100%, 0 100%, 0 10px);
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.55), inset 0 -3px 0 rgba(120, 50, 0, 0.25); overflow: hidden; }
.btn-play::after { content: ""; position: absolute; inset: 0; pointer-events: none;
  background: linear-gradient(105deg, transparent 42%, rgba(255, 255, 255, 0.5) 50%, transparent 58%) no-repeat; background-size: 260% 100%; animation: dtShine 3.6s ease-in-out infinite; }
.btn-play:hover:not(:disabled) { filter: brightness(1.08) saturate(1.05); }
.btn-play:disabled { filter: grayscale(0.85) brightness(0.7); }
.btn-play:disabled::after { display: none; }
.btn-play.btn-xl { font-size: var(--fs-2xl); padding: 14px 56px; }
@keyframes dtShine { 0%, 55% { background-position: 160% 0; } 85%, 100% { background-position: -60% 0; } }

/* =====================================================================
   Cards, badges, chips, progress, inputs, toggles, tabs
   ===================================================================== */
.box { position: relative; background: linear-gradient(180deg, rgba(10, 20, 40, 0.94), rgba(1, 10, 19, 0.95)); border: 1px solid var(--lc-gold-5);
  padding: var(--sp-4); box-shadow: var(--sh-2); min-width: 0; }
.box::before { content: ""; position: absolute; left: -1px; right: -1px; top: -1px; height: 1px; background: linear-gradient(90deg, transparent, var(--lc-gold-3), transparent); pointer-events: none; }
.card-h, .tile-h { display: flex; align-items: center; justify-content: space-between; gap: var(--sp-2); margin: 0 0 var(--sp-3); font-family: var(--font-display); font-size: var(--fs-md);
  font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: var(--lc-gold-1); }
.card-h .muted, .tile-h .muted { font-family: var(--font-body); text-transform: none; letter-spacing: 0; font-weight: 500; font-size: var(--fs-xs); color: var(--lc-mute); white-space: nowrap; }
.tile-h > span:first-child { min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.card-h.sp { margin-top: var(--sp-5); }
.badge { display: inline-flex; align-items: center; gap: 4px; padding: 2px 8px; border-radius: 0; font-size: 10.5px; font-weight: 800; letter-spacing: 0.08em;
  text-transform: uppercase; line-height: 1.5; white-space: nowrap; background: rgba(1, 10, 19, 0.85); color: var(--lc-text); border: 1px solid var(--lc-gold-4); }
.badge-rar { color: var(--rc); border-color: color-mix(in srgb, var(--rc) 55%, transparent); background: color-mix(in srgb, var(--rc) 14%, rgba(1, 10, 19, 0.85)); }
.badge-eq { color: var(--c-text-inv); background: var(--lc-gold-2); border-color: var(--lc-gold-1); }
.badge-own { color: var(--c-success); border-color: rgba(79, 224, 143, 0.45); background: rgba(79, 224, 143, 0.1); }
.badge-lock { color: var(--c-text-3); background: rgba(1, 10, 19, 0.85); border-color: var(--c-border-1); }
.badge-new { color: var(--c-text-inv); background: var(--c-accent); border-color: #a8f0ff; }
.badge-live { color: #ffd0d6; background: rgba(255, 84, 104, 0.15); border-color: rgba(255, 84, 104, 0.5); }
.badge-open { color: var(--c-success); background: rgba(79, 224, 143, 0.1); border-color: rgba(79, 224, 143, 0.4); }
.badge-host { color: var(--lc-gold-1); border-color: var(--lc-gold-3); background: rgba(200, 155, 60, 0.15); }
.badge-you { color: var(--c-text-inv); background: var(--c-accent); border-color: var(--c-accent); }
.badge-off { color: var(--c-text-3); }
.sale { display: inline-block; padding: 1px 6px; margin-right: 6px; font: 800 10.5px var(--font-display); letter-spacing: 0.06em; color: #fff; background: linear-gradient(180deg, #e8433f, #a5221f); border: 1px solid #ff8a7a; }
.bit { display: inline-block; width: 11px; height: 11px; margin: 0 3px 0 1px; vertical-align: -1px; transform: rotate(45deg); border-radius: 2px;
  background: linear-gradient(135deg, #c8fdff, var(--c-bits) 55%, #1b5fd0); box-shadow: 0 0 6px rgba(79, 200, 255, 0.7); }
.chip-bits { display: inline-flex; align-items: center; gap: 6px; padding: 5px 12px 5px 10px; border-radius: 0; font-weight: 800; font-variant-numeric: tabular-nums;
  background: rgba(1, 10, 19, 0.7) !important; border: 1px solid var(--lc-gold-5) !important; color: var(--lc-gold-1) !important; }
.chip-bits:hover:not(.static) { border-color: var(--lc-gold-3) !important; box-shadow: 0 0 14px rgba(79, 200, 255, 0.25); }
.chip-bits.pulse { animation: dtPulse 0.6s var(--ease-out); }
@keyframes dtPulse { 0% { transform: scale(1); } 35% { transform: scale(1.12); box-shadow: 0 0 22px rgba(79, 200, 255, 0.6); } 100% { transform: scale(1); } }
.progress { position: relative; height: 8px; background: rgba(1, 10, 19, 0.85); border: 1px solid var(--lc-gold-5); overflow: hidden; }
.progress.thin { height: 6px; }
.progress i { display: block; height: 100%; background: linear-gradient(90deg, #0a6f8f, var(--c-accent)); box-shadow: 0 0 10px rgba(63, 214, 255, 0.5); transition: width var(--dur-3) var(--ease-out); }
.input, :where(#overlay, #cmodal) input:not([type=checkbox]):not([type=radio]):not([type=range]), :where(#overlay, #cmodal) select {
  font: inherit; font-size: var(--fs-md); color: var(--c-text-1); background: rgba(1, 10, 19, 0.85); border: 1px solid var(--lc-gold-5); border-radius: 0;
  padding: 8px 11px; outline: none; min-width: 0; transition: border-color var(--dur-1), box-shadow var(--dur-1); }
.input:focus, :where(#overlay, #cmodal) input:focus, :where(#overlay, #cmodal) select:focus { border-color: var(--lc-gold-2); box-shadow: 0 0 0 2px rgba(200, 170, 110, 0.18); }
:where(#overlay, #cmodal) select option { background: var(--c-surface-2); }
.field { display: flex; flex-direction: column; gap: 5px; min-width: 0; }
.field > span { font-size: var(--fs-xs); font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: var(--lc-mute); }
.field.grow { flex: 1; }
.seg { display: inline-flex; padding: 2px; gap: 2px; background: rgba(1, 10, 19, 0.85); border: 1px solid var(--lc-gold-5); }
.seg button { border: 0 !important; background: transparent !important; padding: 5px 12px; font-size: var(--fs-sm); color: var(--lc-mute) !important; box-shadow: none !important; }
.seg button.on { background: linear-gradient(180deg, #1e2328, #2a3240) !important; color: var(--lc-gold-1) !important; box-shadow: inset 0 0 0 1px var(--lc-gold-4) !important; }
.ctabs, .subnav { display: flex; gap: 4px; border-bottom: 1px solid var(--lc-gold-5); margin-bottom: var(--sp-3); overflow-x: auto; scrollbar-width: none; flex: none; }
.ctab, .subnav button { position: relative; flex: none; border: 0 !important; background: transparent !important; border-radius: 0 !important; padding: 9px 14px 10px !important; box-shadow: none !important;
  font-family: var(--font-display) !important; font-size: var(--fs-md); font-weight: 600; letter-spacing: 0.1em; text-transform: uppercase; color: var(--lc-mute) !important; }
.ctab span { font-family: var(--font-body); font-size: var(--fs-xs); color: var(--c-text-3); margin-left: 3px; letter-spacing: 0; }
.ctab:hover, .subnav button:hover { color: var(--lc-gold-1) !important; }
.ctab.on, .subnav button.on { color: var(--lc-gold-1) !important; }
.ctab.on::after, .subnav button.on::after { content: ""; position: absolute; left: 12px; right: 12px; bottom: -1px; height: 2px; background: var(--lc-gold-2); box-shadow: 0 0 10px var(--lc-gold-3); }
.toggle { display: flex; align-items: center; gap: var(--sp-3); padding: 8px 0; cursor: pointer; }
.toggle input { position: absolute; opacity: 0; pointer-events: none; }
.toggle .tg { position: relative; flex: none; width: 40px; height: 20px; background: rgba(1, 10, 19, 0.9); border: 1px solid var(--lc-gold-4); transition: background var(--dur-1); }
.toggle .tg::after { content: ""; position: absolute; top: 2px; left: 2px; width: 14px; height: 14px; background: var(--lc-mute); transition: transform var(--dur-2) var(--ease-out); }
.toggle input:checked + .tg { background: linear-gradient(90deg, #0a5f7a, #0b3a4a); border-color: var(--lc-gold-2); }
.toggle input:checked + .tg::after { transform: translateX(20px); background: #cdfafa; box-shadow: 0 0 8px var(--c-accent); }
.toggle input:focus-visible + .tg { outline: 2px solid var(--c-accent); outline-offset: 2px; }
.toggle .tl { display: flex; flex-direction: column; }
.toggle .tl small { color: var(--c-text-3); font-size: var(--fs-xs); }
.srow { display: grid; grid-template-columns: 130px 1fr 36px; align-items: center; gap: var(--sp-3); padding: 6px 0; }
.srow span { color: var(--c-text-2); font-size: var(--fs-sm); }
.srow output { text-align: right; font-variant-numeric: tabular-nums; color: var(--c-text-2); font-size: var(--fs-sm); }
input[type=range] { accent-color: var(--lc-gold-2); }
.spinner { display: inline-block; width: 14px; height: 14px; border-radius: 50%; border: 2px solid rgba(200, 170, 110, 0.25); border-top-color: var(--lc-gold-2); animation: dtSpin 0.8s linear infinite; vertical-align: -2px; }
@keyframes dtSpin { to { transform: rotate(360deg); } }
.price { display: inline-flex; align-items: center; gap: 2px; font-weight: 800; font-size: var(--fs-lg); color: var(--lc-gold-1); }
.price s { color: var(--c-text-3); font-weight: 500; font-size: var(--fs-sm); margin-right: 6px; }
.tchip { --tc: #9aa4b1; display: inline-flex; align-items: center; gap: 5px; padding: 2px 8px 2px 4px; font-size: var(--fs-xs); font-weight: 700;
  background: color-mix(in srgb, var(--tc) 14%, rgba(1, 10, 19, 0.8)); border: 1px solid color-mix(in srgb, var(--tc) 45%, transparent); color: var(--c-text-1); white-space: nowrap; }
.tchip i { width: 12px; height: 13px; flex: none; clip-path: polygon(50% 0, 100% 25%, 100% 75%, 50% 100%, 0 75%, 0 25%); background: var(--tc); }
.tchip b { color: var(--c-text-2); }
.tchip.lv1 i { background: var(--bronze); } .tchip.lv2 i { background: var(--silver); } .tchip.lv3 i { background: var(--goldt); box-shadow: 0 0 6px var(--goldt); }
[data-tip] { position: relative; }
[data-tip]:hover::after { content: attr(data-tip); position: absolute; left: 50%; bottom: calc(100% + 8px); transform: translateX(-50%); z-index: 20; white-space: nowrap; pointer-events: none;
  padding: 5px 9px; background: var(--lc-navy-0); border: 1px solid var(--lc-gold-4); color: var(--c-text-1); font-size: var(--fs-xs); font-weight: 600; box-shadow: var(--sh-2); animation: dtFade var(--dur-1) ease-out; }
.tile-link { align-self: flex-start; margin-top: auto; padding: 0 !important; border: 0 !important; background: none !important; box-shadow: none !important; color: var(--c-accent) !important;
  font: 700 var(--fs-xs) var(--font-display) !important; letter-spacing: 0.1em; text-transform: uppercase; cursor: pointer; white-space: nowrap; }
.tile-link:hover { color: #cdfafa !important; text-shadow: 0 0 8px var(--c-accent); }

/* =====================================================================
   Shell: top bar, social column, page area, boot, modal, toasts
   ===================================================================== */
#cnav { display: none; position: fixed; left: 0; right: 0; top: 0; height: var(--nav-h); z-index: 56; align-items: stretch; gap: 0; padding: 0;
  background: linear-gradient(180deg, #010a13 0%, #07111f 70%, #0a1428 100%); box-shadow: 0 6px 24px rgba(0, 0, 0, 0.55); }
#cnav::after { content: ""; position: absolute; left: 0; right: 0; bottom: 0; height: 1px; background: linear-gradient(90deg, var(--lc-gold-4), var(--lc-gold-5) 40%, var(--lc-gold-5) 70%, var(--lc-gold-4)); }
body.client-on #cnav { display: flex; }
/* PLAY: the angled crest button in the top-left */
#cnav .cn-play { position: relative; flex: none; display: flex; align-items: center; gap: 10px; margin: 14px 10px 14px 14px; padding: 0 34px 0 10px; height: auto; min-width: 162px;
  border: 0 !important; background: none !important; box-shadow: none !important; color: #cdfafa; isolation: isolate; cursor: pointer; }
#cnav .cn-play::before { content: ""; position: absolute; inset: 0; z-index: -2; background: linear-gradient(180deg, var(--lc-gold-1), var(--lc-gold-3) 45%, var(--lc-gold-4));
  clip-path: polygon(0 0, calc(100% - 22px) 0, 100% 50%, calc(100% - 22px) 100%, 0 100%); }
#cnav .cn-play::after { content: ""; position: absolute; inset: 2px 3px 2px 2px; z-index: -1; background: linear-gradient(180deg, #0f4a5c, #062033 60%, #041624);
  clip-path: polygon(0 0, calc(100% - 21px) 0, calc(100% - 1px) 50%, calc(100% - 21px) 100%, 0 100%); box-shadow: inset 0 0 18px rgba(63, 214, 255, 0.35); transition: filter var(--dur-2); }
#cnav .cn-play:hover::after { filter: brightness(1.35) saturate(1.2); }
#cnav .cn-play:hover { color: #fff; }
#cnav .cn-play.on::after { filter: brightness(1.25); }
.cn-play .cn-mark { width: 36px; height: 36px; filter: drop-shadow(0 2px 8px rgba(255, 138, 61, 0.45)); }
.cn-play-t { font: 700 20px var(--font-display); letter-spacing: 0.16em; text-transform: uppercase; text-shadow: 0 0 12px rgba(63, 214, 255, 0.55); }
.cn-tabs { display: flex; align-items: stretch; height: 100%; gap: 0; min-width: 0; margin-left: 4px; overflow: hidden; }
#cnav .cn-tab { position: relative; height: 100%; padding: 0 13px; border: 0; border-radius: 0; background: transparent; color: var(--lc-mute); box-shadow: none;
  font-family: var(--font-display); font-size: 14px; font-weight: 700; letter-spacing: 0.12em; text-transform: uppercase; white-space: nowrap; }
#cnav .cn-tab:hover:not(:disabled) { background: radial-gradient(60% 60% at 50% 100%, rgba(200, 170, 110, 0.14), transparent); color: var(--lc-gold-1); border: 0; box-shadow: none; }
#cnav .cn-tab.on { color: var(--lc-gold-1); background: radial-gradient(70% 70% at 50% 100%, rgba(200, 170, 110, 0.2), transparent); }
#cnav .cn-tab.on::after { content: ""; position: absolute; left: 10px; right: 10px; bottom: 1px; height: 2px; background: linear-gradient(90deg, transparent, var(--lc-gold-1), transparent); box-shadow: 0 0 12px var(--lc-gold-2); }
#cnav .cn-tab:focus-visible { outline-offset: -4px; }
.cn-right { margin-left: auto; display: flex; align-items: center; gap: 6px; flex: none; padding: 0 12px; }
#cnav .cn-status { display: inline-flex; align-items: center; gap: 7px; padding: 6px 10px; font-size: var(--fs-xs); color: var(--lc-mute); background: rgba(1, 10, 19, 0.6); border: 1px solid var(--lc-gold-5); box-shadow: none; }
.cn-status .dot, .cn-st .dot { width: 7px; height: 7px; border-radius: 50%; background: var(--c-text-3); flex: none; }
.cn-status.on .dot { background: var(--c-success); box-shadow: 0 0 8px var(--c-success); }
.cn-status.live .dot { background: var(--c-brand); box-shadow: 0 0 8px var(--c-brand); animation: dtBlink 1.4s ease-in-out infinite; }
.cn-status.live { color: var(--c-text-1); border-color: rgba(255, 138, 61, 0.45) !important; }
@keyframes dtBlink { 50% { opacity: 0.35; } }
#cnav .cn-icon { width: 34px; height: 34px; padding: 0; display: grid; place-items: center; border-radius: 50%; color: var(--lc-gold-2); background: rgba(1, 10, 19, 0.6); border-color: var(--lc-gold-4); }
#cnav .cn-icon.on, #cnav .cn-icon:hover { color: var(--lc-gold-1); border-color: var(--lc-gold-2); }
/* profile block over the social column */
#cnav .cn-profile { flex: none; display: flex; align-items: center; gap: 10px; width: var(--social-w); height: 100%; padding: 0 44px 0 14px; border: 0; border-left: 1px solid var(--lc-gold-5);
  border-radius: 0; background: linear-gradient(180deg, #010a13, #06101d); box-shadow: none; text-align: left; }
#cnav .cn-profile:hover:not(:disabled) { background: linear-gradient(180deg, #06121f, #0b1a2c); border: 0; border-left: 1px solid var(--lc-gold-5); box-shadow: none; }
#cnav .cn-profile.on { background: linear-gradient(180deg, #08182a, #0d2236); }
.cn-ring { position: relative; flex: none; width: 54px; height: 54px; display: grid; place-items: center; }
.cn-ring-svg, .prof-ring { position: absolute; inset: 0; width: 100%; height: 100%; transform: rotate(-90deg); }
.rg-bg { fill: none; stroke: var(--lc-gold-5); stroke-width: 3; }
.rg-fg { fill: none; stroke: var(--lc-gold-2); stroke-width: 3; filter: drop-shadow(0 0 3px rgba(240, 210, 140, 0.6)); transition: stroke-dashoffset var(--dur-3) var(--ease-out); }
.cn-av .pt { width: 42px; height: 42px; border-radius: 50%; background: radial-gradient(circle at 50% 30%, #2a3a5c, #0a1428); display: block; overflow: hidden; }
.cn-lv { position: absolute; left: 50%; bottom: -4px; transform: translateX(-50%); min-width: 22px; padding: 0 5px; font: 700 10px/15px var(--font-display); text-align: center; color: var(--lc-gold-1);
  background: var(--lc-navy-0); border: 1px solid var(--lc-gold-3); }
.cn-who { display: flex; flex-direction: column; min-width: 0; gap: 2px; }
.cn-pn { font: 700 var(--fs-md) var(--font-display); color: var(--lc-gold-1); letter-spacing: 0.04em; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.cn-st { display: flex; align-items: center; gap: 6px; font-size: var(--fs-xs); color: var(--c-success); white-space: nowrap; }
.cn-st .dot { background: var(--c-success); box-shadow: 0 0 6px var(--c-success); }
.cn-st.busy { color: #6ab9ff; } .cn-st.busy .dot { background: #4fa6ff; box-shadow: 0 0 6px #4fa6ff; }
.cn-win { position: absolute; top: 4px; right: 6px; display: flex; gap: 2px; z-index: 2; }
#cnav .cn-wb { width: 24px; height: 22px; padding: 0; display: grid; place-items: center; border: 0; background: transparent; color: var(--lc-mute); box-shadow: none; }
#cnav .cn-wb:hover, #cnav .cn-wb.on { color: var(--lc-gold-1); background: rgba(200, 170, 110, 0.12); border: 0; box-shadow: none; }

#csocial { display: none; position: fixed; top: var(--nav-h); right: 0; bottom: 0; width: var(--social-w); z-index: 55; flex-direction: column;
  background: linear-gradient(180deg, #06101d, #010a13 40%); border-left: 1px solid var(--lc-gold-5); color: var(--c-text-1); }
body.client-on #csocial { display: flex; }
body.client-on.social-off #csocial { display: none; }
.cs-head { flex: none; display: flex; align-items: center; justify-content: space-between; padding: 8px 10px 8px 14px; border-bottom: 1px solid var(--lc-gold-5);
  font: 700 var(--fs-xs) var(--font-display); letter-spacing: 0.18em; text-transform: uppercase; color: var(--lc-gold-2); }
.cs-acts { display: flex; gap: 4px; }
#csocial .cs-btn { width: 28px; height: 26px; padding: 0; display: grid; place-items: center; color: var(--lc-gold-2); background: transparent; border-color: transparent; box-shadow: none; }
#csocial .cs-btn:hover { border-color: var(--lc-gold-4); }
.cs-body { flex: 1; min-height: 0; overflow-y: auto; padding: 4px 0 10px; }
.cs-sec { margin-top: 2px; }
#csocial .cs-sh { display: flex; align-items: center; gap: 6px; width: 100%; padding: 7px 12px; border: 0; background: transparent; box-shadow: none; text-align: left;
  font: 700 var(--fs-xs) var(--font-display); letter-spacing: 0.14em; text-transform: uppercase; color: var(--lc-mute); }
#csocial .cs-sh:hover { color: var(--lc-gold-1); background: rgba(200, 170, 110, 0.06); border: 0; box-shadow: none; }
.cs-sh span { color: var(--c-text-3); letter-spacing: 0.04em; }
.cs-car { width: 0; height: 0; border-left: 5px solid currentColor; border-top: 4px solid transparent; border-bottom: 4px solid transparent; transition: transform var(--dur-2); }
.cs-sec.open .cs-car { transform: rotate(90deg); }
.cs-list { display: none; }
.cs-sec.open .cs-list { display: block; animation: dtFade var(--dur-2) ease-out; }
.cs-row { position: relative; display: flex; align-items: center; gap: 9px; padding: 5px 10px 5px 14px; min-height: 44px; }
.cs-row:hover { background: linear-gradient(90deg, rgba(200, 170, 110, 0.1), transparent); }
.cs-av { position: relative; flex: none; width: 34px; height: 34px; border-radius: 50%; border: 1px solid var(--lc-gold-4); background: radial-gradient(circle at 50% 30%, #2a3a5c, #0a1428); display: grid; place-items: center; }
.cs-av > img, .cs-av .pt { width: 100%; height: 100%; border-radius: 50%; overflow: hidden; display: block; }
.cs-av > b { font: 700 11px var(--font-display); color: var(--lc-gold-1); }
.cs-dot { position: absolute; right: -2px; bottom: -1px; width: 10px; height: 10px; border-radius: 50%; border: 2px solid #010a13; background: var(--c-text-3); }
.cs-row.s-online .cs-dot { background: var(--c-success); } .cs-row.s-ingame .cs-dot { background: #4fa6ff; }
.cs-row.s-offline { opacity: 0.6; }
.cs-txt { display: flex; flex-direction: column; min-width: 0; flex: 1; }
.cs-txt b { font-size: var(--fs-sm); font-weight: 700; color: var(--lc-gold-1); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.cs-txt small { font-size: var(--fs-xs); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; color: var(--c-success); }
.cs-row.s-ingame small { color: #6ab9ff; } .cs-row.s-offline small { color: var(--c-text-3); }
.cs-row.me .cs-txt b { color: var(--c-accent); }
#csocial .cs-act { flex: none; padding: 3px 8px; font: 700 10.5px var(--font-display); letter-spacing: 0.08em; text-transform: uppercase; opacity: 0; transition: opacity var(--dur-1); }
.cs-row:hover .cs-act, .cs-act:focus-visible { opacity: 1; }
.cs-empty { padding: 4px 14px 8px 26px; font-size: var(--fs-xs); color: var(--c-text-3); line-height: 1.4; }
.cs-foot { flex: none; display: flex; align-items: center; gap: 8px; padding: 10px 12px; border-top: 1px solid var(--lc-gold-5); background: rgba(1, 10, 19, 0.85); }
.cs-room { display: flex; flex-direction: column; min-width: 0; flex: 1; }
.cs-room b { font-size: var(--fs-sm); color: var(--lc-gold-1); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.cs-room small { font-size: var(--fs-xs); color: var(--c-text-3); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
#csocial .cs-fbtn { flex: none; padding: 5px 10px; font: 700 var(--fs-xs) var(--font-display); letter-spacing: 0.1em; text-transform: uppercase; }

#overlay { padding-top: var(--nav-h); overflow: hidden; background: radial-gradient(900px 500px at 15% 0%, rgba(255, 138, 61, 0.06), transparent 60%), radial-gradient(900px 600px at 90% 10%, rgba(63, 214, 255, 0.07), transparent 60%), #010a13; }
body.client-on #overlay { padding-right: var(--social-w); }
body.client-on.social-off #overlay { padding-right: 0; }
#overlay::before { content: ""; position: absolute; inset: 0; pointer-events: none; opacity: 0.55; background: ${A('tex-hex')} 0 0 / 256px; mask-image: radial-gradient(ellipse at 50% 40%, #000 20%, transparent 85%); }
.cpages { position: relative; height: 100%; }
.cpage { display: none; height: 100%; overflow-y: auto; overflow-x: hidden; }
.cpage.on { display: block; animation: dtPageIn 240ms var(--ease-out); }
@keyframes dtPageIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: none; } }
@keyframes dtFade { from { opacity: 0; } to { opacity: 1; } }
@keyframes dtPop { from { opacity: 0; transform: scale(0.96) translateY(6px); } to { opacity: 1; transform: none; } }
@keyframes dtRiseIn { from { opacity: 0; transform: translateY(14px); } to { opacity: 1; transform: none; } }

#boot { position: fixed; inset: 0; z-index: 100; display: grid; place-items: center; transition: opacity 260ms ease-out; background: #010a13; }
#boot.art { background: linear-gradient(180deg, rgba(1, 10, 19, 0.2), rgba(1, 10, 19, 0.35) 55%, rgba(1, 10, 19, 0.95)), var(--boot-art) center / cover no-repeat, #010a13; }
#boot.done { opacity: 0; pointer-events: none; }
.boot-in { display: flex; flex-direction: column; align-items: center; gap: 14px; font-family: var(--font-display); margin-top: 38vh; }
.boot-mark { width: 72px; height: 72px; filter: drop-shadow(0 8px 28px rgba(255, 138, 61, 0.55)); animation: dtBob 2.4s ease-in-out infinite; }
@keyframes dtBob { 50% { transform: translateY(-6px); } }
.boot-title { font-size: 40px; font-weight: 700; letter-spacing: 0.14em; text-transform: uppercase; color: var(--lc-gold-1); text-shadow: 0 0 24px rgba(200, 170, 110, 0.45), 0 2px 0 #000; }
.boot-bar { width: 340px; height: 4px; background: rgba(1, 10, 19, 0.8); border: 1px solid var(--lc-gold-4); overflow: hidden; }
.boot-bar i { display: block; width: 0; height: 100%; background: linear-gradient(90deg, var(--lc-gold-3), var(--c-accent)); box-shadow: 0 0 12px rgba(63, 214, 255, 0.6); transition: width 260ms var(--ease-out); }
.boot-step { font-family: var(--font-body); font-size: var(--fs-sm); color: var(--lc-mute); letter-spacing: 0.06em; min-height: 18px; }

#cmodal { position: fixed; inset: 0; z-index: 90; display: flex; align-items: center; justify-content: center; padding: var(--sp-4);
  background: rgba(1, 5, 10, 0.75); backdrop-filter: blur(3px); animation: dtFade var(--dur-1) ease-out; }
#cmodal.hidden { display: none; }
.cm-box { width: min(460px, 100%); max-height: calc(100vh - 48px); overflow: auto; padding: var(--sp-5); background: linear-gradient(180deg, #0a1428, #010a13);
  border: 1px solid var(--lc-gold-3); box-shadow: var(--sh-3), 0 0 0 4px rgba(1, 10, 19, 0.6); animation: dtPop var(--dur-2) var(--ease-out); }
.cm-box.wide { width: min(860px, 100%); }
.cm-box.danger { border-color: rgba(255, 84, 104, 0.6); }
.cm-title { margin: 0 0 6px; font-size: var(--fs-xl); color: var(--lc-gold-1); text-transform: uppercase; letter-spacing: 0.06em; }
.cm-box p { color: var(--c-text-2); line-height: 1.45; }
.cm-btns { display: flex; justify-content: flex-end; gap: var(--sp-2); margin-top: var(--sp-4); flex-wrap: wrap; }
.cm-box .credits { max-height: 50vh; overflow-y: auto; font-size: var(--fs-sm); border-top: 1px solid var(--lc-gold-5); }
.cr-row { display: grid; grid-template-columns: minmax(120px, 0.6fr) 1fr; gap: var(--sp-3); padding: 5px 0; border-bottom: 1px solid var(--c-border-1); }
.cr-row b { color: var(--c-text-1); font-weight: 600; } .cr-row span { color: var(--c-text-2); }
.av-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(104px, 1fr)); gap: var(--sp-2); max-height: 52vh; overflow-y: auto; padding: 2px; }
.av-pick { display: flex !important; flex-direction: column; align-items: center; gap: 6px; padding: 8px !important; }
.av-pick .pt { width: 72px; height: 72px; border-radius: 50%; background: radial-gradient(circle at 50% 30%, #2a3a5c, #0a1428); }
.av-pick span { font-size: var(--fs-xs); max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.av-pick.on { border-color: var(--lc-gold-1) !important; box-shadow: 0 0 0 1px var(--lc-gold-2); }

#ctoasts { position: fixed; top: calc(var(--nav-h) + 12px); right: calc(var(--social-w) + 16px); z-index: 95; display: flex; flex-direction: column; align-items: flex-end; gap: 8px; pointer-events: none; }
body.social-off #ctoasts { right: 16px; }
.ctoast { padding: 9px 14px 9px 12px; background: rgba(1, 10, 19, 0.96); border: 1px solid var(--lc-gold-4); border-left: 3px solid var(--c-accent);
  color: var(--c-text-1); font-family: var(--font-body); font-weight: 600; font-size: var(--fs-sm); box-shadow: var(--sh-2); animation: dtToastIn var(--dur-2) var(--ease-out); transition: opacity var(--dur-2), transform var(--dur-2); max-width: 360px; }
.ctoast.good { border-left-color: var(--c-success); } .ctoast.bad { border-left-color: var(--c-danger); }
.ctoast.out { opacity: 0; transform: translateX(16px); }
@keyframes dtToastIn { from { opacity: 0; transform: translateX(20px); } to { opacity: 1; transform: none; } }

/* =====================================================================
   Home: sub tabs, hero carousel, Little Legend card, tiles
   ===================================================================== */
.home { display: flex; flex-direction: column; height: 100%; padding: 10px var(--sp-5) var(--sp-4); max-width: 1720px; margin: 0 auto; }
.subnav { margin-bottom: 10px; }
.home-view { flex: 1; min-height: 0; display: flex; flex-direction: column; gap: 12px; }
.home-top { flex: 1 1 auto; min-height: 0; display: grid; grid-template-columns: minmax(0, 1fr) clamp(240px, 21vw, 320px); gap: 12px; }
.home-tiles { flex: none; height: clamp(176px, 25vh, 250px); display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; }
#homeResults.hidden, #homeHero.hidden { display: none; }
.hero { position: relative; min-height: 0; overflow: hidden; border: 1px solid var(--lc-gold-4); background: #010a13; box-shadow: var(--sh-2); }
.hero::after { content: ""; position: absolute; inset: 0; pointer-events: none; box-shadow: inset 0 0 0 1px rgba(1, 10, 19, 0.8), inset 0 0 60px rgba(1, 10, 19, 0.6); }
.hero-arts, .hero-art { position: absolute; inset: 0; }
.hero-art { background-size: cover; background-position: center 40%; opacity: 0; transform: scale(1.06); transition: opacity 0.9s ease, transform 9s linear; }
.hero-art.on { opacity: 1; transform: scale(1); }
.hero-shade { position: absolute; inset: 0; pointer-events: none; background: linear-gradient(90deg, rgba(1, 10, 19, 0.92) 0%, rgba(1, 10, 19, 0.6) 34%, transparent 62%), linear-gradient(0deg, rgba(1, 10, 19, 0.85), transparent 38%); }
.hero-renders { position: absolute; right: 2%; bottom: 0; top: 4%; width: 58%; pointer-events: none; }
.hero-render { position: absolute; bottom: -2%; height: 96%; object-fit: contain; filter: drop-shadow(0 16px 30px rgba(0, 0, 0, 0.7)) drop-shadow(0 0 22px rgba(63, 214, 255, 0.18)); animation: dtRenderIn 0.8s var(--ease-out) both; }
.hero-render.r0 { right: 2%; }
.hero-render.r1 { right: 36%; height: 78%; filter: drop-shadow(0 16px 30px rgba(0, 0, 0, 0.7)) brightness(0.85); animation-delay: 0.12s; }
@keyframes dtRenderIn { from { opacity: 0; transform: translateX(30px); } to { opacity: 1; transform: none; } }
.hero-prev { position: absolute; right: 4%; top: 6%; bottom: 74px; width: 44%; display: flex; align-items: center; justify-content: center; pointer-events: none; }
.hero-prev:empty { display: none; }
.hero-prev-in { position: relative; width: min(100%, 360px); aspect-ratio: 1; max-height: 100%; display: flex; align-items: center; justify-content: center; }
.hero-prev-in.wide { width: 100%; aspect-ratio: 16 / 10; }
.hero-prev-in > img, .hero-prev-in > .pt img { max-width: 100%; max-height: 100%; object-fit: contain; filter: drop-shadow(0 12px 30px rgba(0, 0, 0, 0.7)); }
.hero-prev-in > img.board-preview { border: 1px solid var(--lc-gold-3); box-shadow: var(--sh-3); }
.hero-prev canvas { position: absolute; inset: 0; width: 100% !important; height: 100% !important; }
.hero-prev .pt { width: 100% !important; height: 100% !important; background: transparent !important; }
.hero-prev .cp-emo { font-size: 110px; } .hero-prev .cp-art { font-size: 80px; }
.hero-txt { position: absolute; left: 0; bottom: 70px; width: min(52%, 560px); padding: 0 var(--sp-6); display: flex; flex-direction: column; gap: 8px; z-index: 1; }
.hero-txt.in > * { animation: dtRiseIn 0.55s var(--ease-out) both; }
.hero-txt.in > *:nth-child(2) { animation-delay: 0.05s; } .hero-txt.in > *:nth-child(3) { animation-delay: 0.1s; } .hero-txt.in > *:nth-child(4) { animation-delay: 0.15s; } .hero-txt.in > *:nth-child(5) { animation-delay: 0.2s; }
.hero-title { margin: 0; font-size: clamp(34px, 3.8vw, 64px); line-height: 0.95; font-weight: 700; color: var(--lc-gold-1); text-transform: uppercase; letter-spacing: 0.03em; text-shadow: 0 4px 24px rgba(0, 0, 0, 0.7); overflow-wrap: anywhere; }
.hero-meta { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; color: var(--lc-text); font-size: var(--fs-sm); font-weight: 600; }
.hero-desc { margin: 0; color: var(--c-text-2); max-width: 52ch; line-height: 1.45; font-size: var(--fs-md); display: -webkit-box; -webkit-line-clamp: 3; -webkit-box-orient: vertical; overflow: hidden; text-shadow: 0 1px 3px #000; }
.hero-act { display: flex; align-items: center; gap: var(--sp-3); margin-top: 6px; flex-wrap: wrap; }
.hero-thumbs { position: absolute; left: var(--sp-6); right: var(--sp-4); bottom: 14px; z-index: 2; display: flex; gap: 6px; justify-content: flex-end; }
#overlay .hero-thumb { position: relative; flex: 0 1 112px; min-width: 0; height: 42px; padding: 0 8px; overflow: hidden; display: flex; align-items: flex-end; border-radius: 0;
  background: linear-gradient(0deg, rgba(1, 10, 19, 0.9), rgba(1, 10, 19, 0.2)), var(--art) center / cover; border: 1px solid var(--lc-gold-5); box-shadow: none; filter: saturate(0.6) brightness(0.8); }
#overlay .hero-thumb:hover { filter: none; border-color: var(--lc-gold-3); }
#overlay .hero-thumb.on { filter: none; border-color: var(--lc-gold-1); box-shadow: 0 0 12px rgba(240, 210, 140, 0.3); }
.hero-thumb span { position: relative; padding-bottom: 6px; font: 700 10.5px var(--font-display); letter-spacing: 0.08em; text-transform: uppercase; color: var(--lc-gold-1); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.hero-thumb i { position: absolute; left: 0; bottom: 0; height: 2px; width: 0; background: var(--c-accent); box-shadow: 0 0 6px var(--c-accent); }
.hero-thumb.on i { animation: dtProg 8s linear forwards; }
.hero:hover .hero-thumb.on i { animation-play-state: paused; }
@keyframes dtProg { to { width: 100%; } }
/* your Little Legend and quick play */
.legend-card { position: relative; display: flex; flex-direction: column; min-height: 0; overflow: hidden; border: 1px solid var(--lc-gold-4);
  background: radial-gradient(260px 220px at 50% 40%, rgba(63, 214, 255, 0.16), transparent 70%), linear-gradient(180deg, rgba(1, 10, 19, 0.4), #010a13 80%), ${A('lobby-bg')} center / cover; }
.stage-host { position: relative; flex: 1 1 auto; min-height: 110px; overflow: hidden; }
.stage-canvas { position: absolute; inset: 0; width: 100%; height: 100%; display: block; cursor: grab; }
.stage-canvas:active { cursor: grabbing; }
.legend-info { flex: none; padding: 8px 12px; display: flex; flex-direction: column; gap: 6px; border-top: 1px solid var(--lc-gold-5); background: rgba(1, 10, 19, 0.7); }
.li-top { display: flex; align-items: center; justify-content: space-between; gap: var(--sp-2); }
.li-name { font-family: var(--font-display); font-size: var(--fs-lg); font-weight: 700; color: var(--lc-gold-1); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.lvl { font-family: var(--font-display); font-weight: 700; font-size: var(--fs-xs); color: var(--lc-gold-1); background: var(--lc-navy-0); border: 1px solid var(--lc-gold-3); padding: 1px 7px; white-space: nowrap; }
.li-bar { display: grid; grid-template-columns: 1fr auto; gap: var(--sp-3); align-items: center; font-size: var(--fs-xs); }
.ready { color: var(--c-success); }
.li-btns { display: flex; gap: 4px; flex-wrap: wrap; }
.qp-btns { flex: none; display: grid; grid-template-columns: 1fr; gap: 6px; padding: 8px 12px 12px; background: #010a13; }
.qp-btns .btn-play { padding: 11px 10px; }
.qp-btns .btn-secondary { padding: 7px 10px; }
/* tiles */
.tile-card { position: relative; display: flex; flex-direction: column; min-width: 0; min-height: 0; overflow: hidden; padding: 10px 12px; border: 1px solid var(--lc-gold-5);
  background: linear-gradient(180deg, rgba(10, 20, 40, 0.92), rgba(1, 10, 19, 0.95)); transition: border-color var(--dur-2); }
.tile-card:hover { border-color: var(--lc-gold-4); }
.tile-card .tile-h { margin-bottom: 6px; font-size: var(--fs-sm); }
.tile-art { flex: none; position: relative; margin: -10px -12px 8px; height: 62px; background-size: cover; background-position: center; border-bottom: 1px solid var(--lc-gold-5); }
.tile-art::after { content: ""; position: absolute; inset: 0; background: linear-gradient(0deg, rgba(1, 10, 19, 0.75), transparent 70%); }
.tile-render { position: absolute; right: 6px; bottom: -2px; z-index: 1; height: 118%; object-fit: contain; filter: drop-shadow(0 4px 10px rgba(0, 0, 0, 0.8)); }
.tile-art .badge { position: absolute; left: 10px; bottom: 8px; z-index: 1; }
.tile-b { flex: 1; min-height: 0; overflow: hidden; }
.tile-p { margin: 0 0 8px; color: var(--c-text-2); font-size: var(--fs-sm); line-height: 1.45; }
.mission { padding: 3px 0 6px; }
.mi-top { display: flex; align-items: center; justify-content: space-between; gap: var(--sp-2); margin-bottom: 4px; }
.mi-name { font-weight: 600; font-size: var(--fs-sm); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; min-width: 0; }
.mi-bits { display: inline-flex; align-items: center; font-weight: 800; font-size: var(--fs-sm); color: var(--lc-gold-1); white-space: nowrap; }
.mi-sub { font-size: var(--fs-xs); color: var(--c-text-3); margin-top: 3px; }
.mission.done .progress i { background: linear-gradient(90deg, #2fb86c, var(--c-success)); box-shadow: 0 0 10px rgba(79, 224, 143, 0.5); }
.mission.done .mi-name { color: var(--c-text-2); }
.daily { display: grid; grid-template-columns: 26px 1fr auto; gap: 10px; align-items: center; padding-top: 6px; border-top: 1px solid var(--lc-gold-5); margin-top: 2px; }
.daily-ic { width: 26px; height: 26px; display: grid; place-items: center; font-weight: 800; color: var(--c-text-inv); background: linear-gradient(180deg, var(--c-brand-2), var(--c-brand)); clip-path: polygon(50% 0, 100% 25%, 100% 75%, 50% 100%, 0 75%, 0 25%); }
.daily.done .daily-ic { background: linear-gradient(180deg, #6ff0a8, #2fb86c); }
.daily b { font-size: var(--fs-sm); }
.patch-list { list-style: none; margin: 0 0 6px; padding: 0; display: flex; flex-direction: column; gap: 6px; min-height: 0; overflow: hidden; }
.patch-list li { display: flex; flex-direction: column; gap: 1px; padding-left: 10px; border-left: 2px solid var(--lc-gold-4); }
.patch-list b { font-size: var(--fs-sm); color: var(--lc-gold-1); }
.patch-list span { font-size: var(--fs-xs); color: var(--c-text-2); line-height: 1.4; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.patch-list.big span { -webkit-line-clamp: unset; display: block; font-size: var(--fs-sm); }
.feat-list { display: flex; flex-direction: column; gap: 6px; min-height: 0; overflow: hidden; }
#overlay .feat { display: grid; grid-template-columns: 40px minmax(0, 1fr) auto; gap: 8px; align-items: center; padding: 4px 8px 4px 4px; text-align: left; white-space: normal;
  background: rgba(1, 10, 19, 0.6); border: 1px solid color-mix(in srgb, var(--rc) 35%, var(--lc-gold-5)); }
#overlay .feat:hover { border-color: var(--rc); }
.feat-prev { width: 40px; height: 40px; overflow: hidden; display: grid; place-items: center; background: radial-gradient(circle, color-mix(in srgb, var(--rc) 30%, #0a1428), #010a13); }
.feat-prev > img, .feat-prev .pt { max-width: 100%; max-height: 100%; width: 100% !important; height: 100% !important; object-fit: cover; }
.feat-prev canvas { max-width: 100%; max-height: 100%; }
.feat-prev .cp-emo { font-size: 22px; } .feat-prev .cp-art { font-size: 13px; }
.feat-txt { display: flex; flex-direction: column; min-width: 0; }
.feat-txt b { font-size: var(--fs-sm); color: var(--lc-gold-1); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.feat-txt small { font-size: var(--fs-xs); color: var(--c-text-3); }
.feat-price { display: inline-flex; align-items: center; font-weight: 800; font-size: var(--fs-sm); color: var(--lc-gold-1); white-space: nowrap; }
/* patch notes and events */
.article, .events { height: 100%; overflow-y: auto; display: flex; flex-direction: column; gap: 12px; }
.art-banner, .ev-banner { flex: none; position: relative; height: clamp(150px, 28vh, 260px); display: flex; align-items: flex-end; padding: var(--sp-5) var(--sp-6); border: 1px solid var(--lc-gold-4);
  background-size: cover; background-position: center; }
.art-banner::before, .ev-banner::before { content: ""; position: absolute; inset: 0; background: linear-gradient(90deg, rgba(1, 10, 19, 0.92), rgba(1, 10, 19, 0.3) 70%), linear-gradient(0deg, rgba(1, 10, 19, 0.7), transparent 50%); }
.art-banner > div, .ev-banner > div { position: relative; }
.art-banner h1, .ev-banner h1 { margin: 4px 0; font-size: clamp(30px, 3vw, 48px); color: var(--lc-gold-1); text-transform: uppercase; letter-spacing: 0.04em; }
.art-banner p, .ev-banner p { margin: 0; color: var(--c-text-2); }
.art-body { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1.2fr); gap: 12px; }
.art-body section { padding: var(--sp-4); border: 1px solid var(--lc-gold-5); background: rgba(1, 10, 19, 0.75); }
.art-body h3 { margin: 0 0 10px; font: 700 var(--fs-md) var(--font-display); letter-spacing: 0.12em; text-transform: uppercase; color: var(--lc-gold-2); }
.art-sets { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 10px; }
.art-set { position: relative; display: flex; flex-direction: column; gap: 4px; justify-content: flex-end; min-height: 200px; padding: 12px; border: 1px solid var(--lc-gold-4);
  background: linear-gradient(0deg, rgba(1, 10, 19, 0.95) 30%, rgba(1, 10, 19, 0.1)), var(--art) center 25% / cover; }
.art-set b { font: 700 var(--fs-lg) var(--font-display); color: var(--lc-gold-1); text-transform: uppercase; }
.art-set span { font-size: var(--fs-xs); color: var(--c-text-2); line-height: 1.4; }
.art-set small { font-size: var(--fs-xs); color: var(--c-accent); }
.ev-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px; }
.ev-grid .tile-card { min-height: 220px; }

/* ---------- end of game ---------- */
#homeResults:not(.hidden) { flex: 1 1 auto; display: flex; min-height: 0; height: 100%; }
.results { position: relative; flex: 1; display: grid; grid-template-rows: auto minmax(0, 1fr) auto; gap: 12px; padding: var(--sp-4) var(--sp-5); overflow: hidden; border: 1px solid var(--lc-gold-4);
  background: linear-gradient(180deg, rgba(1, 10, 19, 0.55), rgba(1, 10, 19, 0.92) 55%), var(--bg) center 30% / cover; animation: dtPop 0.4s var(--ease-out); --pc: var(--c-text-2); }
.results.p-gold { --pc: #ffd56a; border-color: var(--lc-gold-2); }
.results.p-good { --pc: var(--c-accent); }
.res-hero { display: grid; grid-template-columns: auto minmax(0, 1fr); gap: var(--sp-5); align-items: center; }
.res-place { position: relative; display: flex; flex-direction: column; align-items: center; gap: 0; width: 170px; }
.res-emblem { width: 130px; height: 130px; object-fit: contain; filter: drop-shadow(0 0 22px color-mix(in srgb, var(--pc) 45%, transparent)); animation: dtReveal 0.7s var(--ease-out) both; }
.res-num { margin-top: -14px; font-family: var(--font-display); font-size: 46px; font-weight: 700; line-height: 1; color: var(--pc); text-shadow: 0 0 24px color-mix(in srgb, var(--pc) 55%, transparent), 0 2px 0 #000; }
.res-lbl { font-family: var(--font-display); font-size: var(--fs-xs); font-weight: 700; letter-spacing: 0.16em; text-transform: uppercase; color: var(--lc-gold-2); }
.res-body { display: flex; flex-direction: column; gap: 6px; min-width: 0; }
.res-title { margin: 0; font-size: clamp(38px, 4vw, 64px); line-height: 1; color: var(--lc-gold-1); text-transform: uppercase; letter-spacing: 0.06em; text-shadow: 0 0 30px color-mix(in srgb, var(--pc) 40%, transparent), 0 3px 0 #000; animation: dtRiseIn 0.6s 0.1s var(--ease-out) both; }
.res-body p { margin: 0; }
.res-traits, .m-traits { display: flex; flex-wrap: wrap; gap: 5px; }
.res-units, .m-units { display: flex; flex-wrap: wrap; gap: 5px; }
.res-units .pt { width: 40px; height: 40px; border: 1px solid var(--lc-gold-4); }
.res-cols { display: grid; grid-template-columns: minmax(0, 1.1fr) minmax(0, 1fr) minmax(0, 1fr); gap: 12px; min-height: 0; }
.res-col { min-height: 0; overflow: auto; padding: 10px 12px; border: 1px solid var(--lc-gold-5); background: rgba(1, 10, 19, 0.8); animation: dtRiseIn 0.5s 0.2s var(--ease-out) both; }
.res-col .tile-h { margin-bottom: 6px; }
.standings { display: flex; flex-direction: column; gap: 2px; }
.st-row { display: grid; grid-template-columns: 30px 28px minmax(0, 1fr) auto; gap: 8px; align-items: center; padding: 2px 6px; font-size: var(--fs-sm); border-left: 2px solid transparent; }
.st-row b { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; color: var(--c-text-1); font-weight: 600; }
.st-row .muted { font-size: var(--fs-xs); white-space: nowrap; }
.st-row.me { background: linear-gradient(90deg, rgba(200, 170, 110, 0.18), transparent); border-left-color: var(--lc-gold-2); }
.st-row.me b { color: var(--lc-gold-1); }
.st-place { font: 700 var(--fs-sm) var(--font-display); color: var(--lc-gold-2); }
.st-av { width: 26px; height: 26px; border-radius: 50%; overflow: hidden; border: 1px solid var(--lc-gold-5); background: #0a1428; }
.st-av .pt { width: 100%; height: 100%; display: block; }
.res-xp { display: flex; flex-direction: column; gap: 6px; margin-bottom: 10px; font-size: var(--fs-sm); }
.res-missions .mi-sub { display: none; }
.res-btns { display: flex; gap: var(--sp-2); justify-content: flex-end; flex-wrap: wrap; }
.res-btns .lc-cta { min-width: 200px; }
.res-x { position: absolute !important; top: 10px; right: 10px; z-index: 2; }
.res-award .dts-award { margin: 0; max-width: none; background: transparent !important; border: 0 !important; padding: 0 !important; }
@keyframes dtReveal { 0% { opacity: 0; transform: scale(0.4) rotate(-6deg); filter: brightness(3); } 60% { opacity: 1; transform: scale(1.08) rotate(1deg); filter: brightness(1.4); } 100% { transform: none; filter: none; } }

/* ---------- match loading screen ---------- */
#cload { position: fixed; inset: 0; z-index: 120; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 22px; padding: 24px; pointer-events: none;
  background: #010a13; animation: dtFade 0.25s ease-out; transition: opacity 0.45s ease; color: var(--c-text-1); }
#cload.out { opacity: 0; }
.ld-bg { position: absolute; inset: 0; background-size: cover; background-position: center; opacity: 0.55; filter: saturate(0.8); }
#cload::before { content: ""; position: absolute; inset: 0; z-index: 1; background: radial-gradient(ellipse at 50% 45%, transparent 30%, rgba(1, 10, 19, 0.85) 80%); }
.ld-head, .ld-cards, .ld-foot { position: relative; z-index: 2; }
.ld-head { text-align: center; }
.ld-head h1 { margin: 4px 0 0; font-size: clamp(30px, 3.2vw, 52px); color: var(--lc-gold-1); text-transform: uppercase; letter-spacing: 0.1em; text-shadow: 0 0 24px rgba(200, 170, 110, 0.4), 0 3px 0 #000; }
.ld-cards { display: grid; grid-template-columns: repeat(8, minmax(0, 1fr)); gap: 10px; width: min(1500px, 100%); }
.ld-card { position: relative; display: flex; flex-direction: column; aspect-ratio: 9 / 15; min-width: 0; overflow: hidden; border: 1px solid var(--lc-gold-4);
  background: radial-gradient(circle at 50% 35%, rgba(63, 214, 255, 0.22), transparent 60%), linear-gradient(180deg, #0b1a2c, #010a13); animation: dtCardIn 0.5s var(--ease-out) both; box-shadow: 0 10px 30px rgba(0, 0, 0, 0.6); }
.ld-card:nth-child(2) { animation-delay: 0.05s; } .ld-card:nth-child(3) { animation-delay: 0.1s; } .ld-card:nth-child(4) { animation-delay: 0.15s; }
.ld-card:nth-child(5) { animation-delay: 0.2s; } .ld-card:nth-child(6) { animation-delay: 0.25s; } .ld-card:nth-child(7) { animation-delay: 0.3s; } .ld-card:nth-child(8) { animation-delay: 0.35s; }
.ld-card.me { border-color: var(--lc-gold-1); box-shadow: 0 0 0 1px var(--lc-gold-2), 0 0 30px rgba(240, 210, 140, 0.35); }
.ld-card.human:not(.me) { border-color: var(--c-accent); }
.ld-pic { flex: 1; min-height: 0; display: grid; place-items: center; padding: 8px; }
.ld-pic .pt { width: 100% !important; height: auto !important; aspect-ratio: 1; background: transparent !important; display: block; }
.ld-pic .pt img { width: 100%; height: 100%; object-fit: contain; filter: drop-shadow(0 10px 18px rgba(0, 0, 0, 0.6)); }
.ld-pic .pt b { font: 700 28px var(--font-display); color: var(--lc-gold-1); }
.ld-name { flex: none; display: flex; flex-direction: column; align-items: center; padding: 8px 6px 10px; border-top: 1px solid var(--lc-gold-5); background: linear-gradient(180deg, rgba(1, 10, 19, 0.6), #010a13); }
.ld-name b { font: 700 var(--fs-md) var(--font-display); color: var(--lc-gold-1); max-width: 100%; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; text-transform: uppercase; letter-spacing: 0.04em; }
.ld-name small { font-size: var(--fs-xs); color: var(--lc-mute); }
.ld-foot { display: flex; flex-direction: column; align-items: center; gap: 8px; font-size: var(--fs-sm); color: var(--lc-mute); letter-spacing: 0.06em; }
.ld-bar { width: 320px; height: 3px; background: rgba(1, 10, 19, 0.85); border: 1px solid var(--lc-gold-5); overflow: hidden; }
.ld-bar i { display: block; height: 100%; width: 0; background: linear-gradient(90deg, var(--lc-gold-3), var(--c-accent)); animation: dtLoadBar 2s ease-out forwards; }
@keyframes dtLoadBar { to { width: 100%; } }
@keyframes dtCardIn { from { opacity: 0; transform: translateY(24px) scale(0.96); } to { opacity: 1; transform: none; } }

/* =====================================================================
   Play: mode select, party, lobbies
   ===================================================================== */
.play { height: 100%; padding: 12px var(--sp-5) var(--sp-4); max-width: 1720px; margin: 0 auto; }
.play-select, .play-party { height: 100%; display: flex; flex-direction: column; gap: 12px; animation: dtFade var(--dur-2) ease-out; }
.ps-head { flex: none; display: flex; align-items: flex-end; justify-content: space-between; gap: var(--sp-4); }
.play-status { display: flex; align-items: center; gap: 10px; padding: 6px 14px 6px 6px; border: 1px solid var(--lc-gold-5); background: rgba(1, 10, 19, 0.7); }
.play-status:empty { display: none; }
.ps-emblem { width: 46px; height: 46px; object-fit: contain; }
.ps-emblem.dim, .prof-rank img.dim { filter: grayscale(1) brightness(0.6); }
.ps-row { display: flex; align-items: baseline; gap: 5px; flex-wrap: wrap; }
.ps-row b { font-family: var(--font-display); font-size: var(--fs-lg); color: var(--lc-gold-1); }
.ps-row span { color: var(--c-text-3); font-size: var(--fs-xs); margin-right: 6px; }
.play-sets { flex: 1; min-height: 0; }
.ps-foot { flex: none; display: flex; align-items: stretch; justify-content: space-between; gap: var(--sp-4); padding-top: 12px; border-top: 1px solid var(--lc-gold-5); }
.play-modes { display: flex; gap: 10px; min-width: 0; }
#overlay .mode-card { display: flex; align-items: center; gap: 10px; padding: 8px 14px 8px 8px; text-align: left; white-space: normal; min-width: 0; max-width: 330px;
  background: rgba(1, 10, 19, 0.75); border: 1px solid var(--lc-gold-5); }
#overlay .mode-card:hover { border-color: var(--lc-gold-3); }
#overlay .mode-card.on { border-color: var(--lc-gold-1); box-shadow: 0 0 0 1px var(--lc-gold-2), inset 0 0 22px rgba(63, 214, 255, 0.12); background: linear-gradient(90deg, rgba(11, 58, 74, 0.7), rgba(1, 10, 19, 0.8)); }
.mode-art { position: relative; flex: none; width: 46px; height: 46px; border-radius: 50%; border: 1px solid var(--lc-gold-4); background-size: cover; background-position: center; }
.mode-art.solo { background-image: ${A('mode-solo')}; }
.mode-art.mp { background-image: ${A('mode-multiplayer')}; }
.mode-card.on .mode-art { border-color: var(--lc-gold-1); box-shadow: 0 0 12px rgba(240, 210, 140, 0.4); }
.mode-txt { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
.mode-txt b { font-family: var(--font-display); font-size: var(--fs-md); text-transform: uppercase; letter-spacing: 0.08em; color: var(--lc-gold-1); }
.mode-txt small { color: var(--c-text-2); font-size: var(--fs-xs); line-height: 1.35; font-weight: 500; }
.ps-confirm { display: flex; align-items: center; gap: var(--sp-4); flex: none; }
.ps-pick { display: flex; align-items: center; gap: 6px; font-size: var(--fs-sm); white-space: nowrap; }
.ps-pick b { color: var(--lc-gold-1); font-family: var(--font-display); letter-spacing: 0.04em; }
#overlay .lc-confirm { min-width: 220px; height: 52px; font: 700 var(--fs-xl) var(--font-display); letter-spacing: 0.18em; text-transform: uppercase; color: #cdfafa;
  background: linear-gradient(180deg, #0f5266, #062033); border: 2px solid transparent; border-image: linear-gradient(180deg, var(--lc-gold-1), var(--lc-gold-4)) 1;
  box-shadow: inset 0 0 20px rgba(63, 214, 255, 0.35); clip-path: polygon(14px 0, calc(100% - 14px) 0, 100% 50%, calc(100% - 14px) 100%, 14px 100%, 0 50%); }
#overlay .lc-confirm:hover { color: #fff; filter: brightness(1.2); box-shadow: inset 0 0 26px rgba(63, 214, 255, 0.6); }
/* party */
.play-solo, .play-mp { flex: 1; min-height: 0; display: flex; flex-direction: column; overflow: hidden; border: 1px solid var(--lc-gold-4);
  background: linear-gradient(180deg, rgba(1, 10, 19, 0.55), rgba(1, 10, 19, 0.92) 70%), ${A('lobby-bg')} center 40% / cover; box-shadow: var(--sh-2); }
.play-solo.hidden, .play-mp.hidden { display: none; }
.play-solo-body { flex: 1; min-height: 0; overflow-y: auto; padding: var(--sp-4) var(--sp-5); display: flex; flex-direction: column; gap: 14px; }
.party-head { display: flex; align-items: flex-start; gap: var(--sp-3); }
.party-back { margin-top: 2px; }
.party-sub { margin: 4px 0 0; color: var(--c-text-2); font-size: var(--fs-sm); max-width: 760px; }
.party-slots { flex: 1 1 auto; min-height: 220px; max-height: 470px; display: grid; grid-template-columns: 1.6fr repeat(7, minmax(0, 1fr)); gap: 8px; align-items: stretch; }
.party-slot { position: relative; display: flex; flex-direction: column; min-width: 0; min-height: 0; overflow: hidden; border: 1px solid var(--lc-gold-5);
  background: linear-gradient(180deg, rgba(10, 20, 40, 0.5), rgba(1, 10, 19, 0.92)); animation: dtCardIn 0.45s var(--ease-out) both; }
.party-slot:nth-child(2) { animation-delay: 0.04s; } .party-slot:nth-child(3) { animation-delay: 0.08s; } .party-slot:nth-child(4) { animation-delay: 0.12s; } .party-slot:nth-child(5) { animation-delay: 0.16s; }
.party-slot:nth-child(6) { animation-delay: 0.2s; } .party-slot:nth-child(7) { animation-delay: 0.24s; } .party-slot:nth-child(8) { animation-delay: 0.28s; }
.party-slot.me { border-color: var(--lc-gold-2); box-shadow: 0 0 24px rgba(200, 170, 110, 0.2); background: radial-gradient(circle at 50% 55%, rgba(63, 214, 255, 0.18), transparent 60%), linear-gradient(180deg, rgba(10, 20, 40, 0.3), rgba(1, 10, 19, 0.9)); }
.party-stage { flex: 1; min-height: 120px; }
.ps-pic { position: relative; flex: 1; min-height: 0; display: flex; align-items: flex-end; justify-content: center; padding: 8px 8px 14px; opacity: 0.9; }
.ps-pic::after { content: ""; position: absolute; left: 14%; right: 14%; bottom: 8px; height: 16px; border-radius: 50%; background: radial-gradient(rgba(63, 214, 255, 0.35), transparent 70%); }
.ps-pic .pt { width: 100% !important; height: auto !important; aspect-ratio: 1; background: transparent !important; display: block; }
.ps-pic .pt img { width: 100%; height: 100%; object-fit: contain; filter: grayscale(0.3); }
.ps-plate { flex: none; display: flex; flex-direction: column; align-items: center; padding: 6px 4px 8px; border-top: 1px solid var(--lc-gold-5); background: rgba(1, 10, 19, 0.85); text-align: center; }
.ps-plate b { font: 700 var(--fs-sm) var(--font-display); color: var(--lc-gold-1); max-width: 100%; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; text-transform: uppercase; letter-spacing: 0.04em; }
.ps-plate small { font-size: var(--fs-xs); color: var(--lc-mute); max-width: 100%; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.party-slot.me .ps-plate b { color: #fff; }
.ps-crown { position: absolute; top: 8px; left: 8px; width: 18px; height: 12px; background: var(--lc-gold-2); clip-path: polygon(0 100%, 0 20%, 25% 60%, 50% 0, 75% 60%, 100% 20%, 100% 100%); filter: drop-shadow(0 0 4px var(--lc-gold-3)); }
.howto { flex: none; margin: 0; padding: 0; list-style: none; display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px; }
.howto li { padding: 8px 10px; background: rgba(1, 10, 19, 0.75); border: 1px solid var(--lc-gold-5); border-left: 2px solid var(--lc-gold-3); color: var(--c-text-2); font-size: var(--fs-xs); line-height: 1.45; }
.howto li b { color: var(--lc-gold-1); }
.play-foot { flex: none; display: flex; align-items: center; justify-content: space-between; gap: var(--sp-4); padding: 12px var(--sp-5); border-top: 1px solid var(--lc-gold-4); background: rgba(1, 10, 19, 0.9); }
.play-keys { color: var(--c-text-3); font-size: var(--fs-sm); display: flex; align-items: center; gap: 6px; flex-wrap: wrap; }
.play-keys kbd { margin-left: 6px; }
/* lobbies (DOM from js/net.js) */
#lobbyPanel { flex: 1; min-height: 0; overflow-y: auto; padding: var(--sp-4) var(--sp-5); display: flex; flex-direction: column; }
#lobbyPanel .sub { margin: 4px 0 0; color: var(--c-text-2); font-size: var(--fs-sm); }
.mp-head { display: flex; align-items: flex-end; justify-content: space-between; gap: var(--sp-4); flex-wrap: wrap; margin-bottom: var(--sp-3); }
.mp-wait { display: flex; align-items: center; gap: var(--sp-2); color: var(--c-text-2); padding: var(--sp-4) 0; }
.mp-empty { padding: var(--sp-5); text-align: center; color: var(--c-text-3); border: 1px dashed var(--lc-gold-5); background: rgba(1, 10, 19, 0.6); }
.mp-create { display: flex; align-items: flex-end; gap: var(--sp-3); flex-wrap: wrap; padding: var(--sp-3) var(--sp-4); background: rgba(1, 10, 19, 0.82) !important; }
.mp-create .field:first-child { width: 170px; }
.mph { margin: var(--sp-4) 0 var(--sp-2); font-family: var(--font-display); font-size: var(--fs-md); letter-spacing: 0.12em; text-transform: uppercase; color: var(--lc-gold-1); display: flex; gap: var(--sp-2); align-items: baseline; }
.mplist { display: flex; flex-direction: column; gap: 6px; }
.mplobby { display: grid; grid-template-columns: minmax(0, 1fr) auto auto; align-items: center; gap: var(--sp-4); padding: 10px var(--sp-4);
  background: rgba(1, 10, 19, 0.8); border: 1px solid var(--lc-gold-5); transition: border-color var(--dur-1), background var(--dur-1); }
.mplobby:hover { border-color: var(--lc-gold-3); background: linear-gradient(90deg, rgba(200, 170, 110, 0.1), rgba(1, 10, 19, 0.8)); }
.mplobby.friend { border-left: 3px solid var(--c-accent); }
.mpl-main { min-width: 0; }
.mpl-top { display: flex; align-items: center; gap: var(--sp-2); min-width: 0; }
.mpl-top b { font-size: var(--fs-lg); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; color: var(--lc-gold-1); font-family: var(--font-display); }
.mplobby .muted { font-size: var(--fs-xs); }
.mp-pips { display: grid; grid-template-columns: repeat(8, 20px); gap: 4px; }
.mp-pip { width: 20px; height: 20px; border-radius: 50%; border: 1px dashed var(--lc-gold-5); display: grid; place-items: center; font-size: 8px; font-weight: 800; color: var(--c-text-inv); }
.mp-pip.on { border: 1px solid var(--lc-gold-1); background: linear-gradient(180deg, var(--lc-gold-2), var(--lc-gold-4)); }
.mp-invite { display: flex; gap: var(--sp-2); align-items: center; }
.mp-invite input { width: 300px; font-size: var(--fs-sm) !important; color: var(--c-text-2) !important; }
.mpseats { flex: 1 1 auto; max-height: 520px; min-height: 0; display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); grid-auto-rows: minmax(0, 1fr); gap: 8px; }
.mpseat { position: relative; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 6px; text-align: center; padding: 10px 12px; min-height: 120px;
  background: rgba(1, 10, 19, 0.75); border: 1px dashed var(--lc-gold-5); transition: border-color var(--dur-1), box-shadow var(--dur-1); animation: dtCardIn 0.4s var(--ease-out) both; }
.mpseat .n { position: absolute; top: 4px; right: 8px; font: 700 var(--fs-xs) var(--font-display); color: var(--c-text-3); }
.mpseat .mps-body { align-items: center; max-width: 100%; }
.mpseat .mps-tags { justify-content: center; }
.mpseat.taken { border-style: solid; border-color: var(--lc-gold-4); background: linear-gradient(180deg, rgba(11, 58, 74, 0.45), rgba(1, 10, 19, 0.85)); }
.mpseat.me { border-color: var(--lc-gold-1); box-shadow: 0 0 0 1px var(--lc-gold-3) inset, 0 0 20px rgba(200, 170, 110, 0.2); }
.mp-av { flex: none; width: clamp(56px, 10vh, 104px) !important; height: clamp(56px, 10vh, 104px) !important; border-radius: 50% !important; background: radial-gradient(circle at 50% 30%, #2a3a5c, #0a1428) !important; border: 2px solid var(--lc-gold-4); }
.mpseat.me .mp-av { border-color: var(--lc-gold-1); box-shadow: 0 0 12px rgba(240, 210, 140, 0.4); }
.mp-ai { opacity: 0.55; } .mp-ai b, .mp-ini b { visibility: visible !important; font-size: 14px !important; color: var(--lc-gold-1); }
.mps-body { display: flex; flex-direction: column; gap: 3px; min-width: 0; }
.mps-body b { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; color: var(--lc-gold-1); font-family: var(--font-display); letter-spacing: 0.03em; }
.mps-tags { display: flex; gap: 4px; flex-wrap: wrap; }
.mpfoot { display: flex; justify-content: space-between; align-items: center; gap: var(--sp-3); margin-top: auto; padding-top: var(--sp-4); }
.mp-waithost { display: inline-flex; align-items: center; gap: var(--sp-2); color: var(--c-text-2); }

/* =====================================================================
   Collection
   ===================================================================== */
.coll { display: grid; grid-template-rows: minmax(0, 1fr); grid-template-columns: minmax(0, 1fr) clamp(270px, 24vw, 360px); gap: 12px; padding: 12px var(--sp-5) var(--sp-4); height: 100%; max-width: 1720px; margin: 0 auto; }
.coll-main { display: flex; flex-direction: column; min-height: 0; min-width: 0; }
.coll-banner { flex: none; position: relative; display: flex; align-items: flex-end; justify-content: space-between; gap: var(--sp-4); height: 92px; padding: 0 var(--sp-5) 12px; margin-bottom: 8px;
  border: 1px solid var(--lc-gold-4); background-size: cover; background-position: center 45%; }
.coll-banner::before { content: ""; position: absolute; inset: 0; background: linear-gradient(90deg, rgba(1, 10, 19, 0.92), rgba(1, 10, 19, 0.35) 60%, rgba(1, 10, 19, 0.8)); }
.coll-banner > div { position: relative; }
.coll-count { display: flex; flex-direction: column; align-items: flex-end; gap: 3px; font-family: var(--font-display); color: var(--lc-text); font-size: var(--fs-md); white-space: nowrap; min-width: 160px; }
.coll-count b { color: var(--lc-gold-1); font-size: var(--fs-xl); }
.coll-count small { font-family: var(--font-body); font-size: var(--fs-xs); color: var(--c-text-3); }
.coll-count .progress { width: 100%; }
.ctools { display: flex; gap: var(--sp-2); align-items: center; margin-bottom: 10px; flex-wrap: wrap; }
.ctools .search { flex: 1 1 200px; }
.cgrid { flex: 1; min-height: 0; overflow-y: auto; display: grid; grid-auto-rows: max-content; grid-template-columns: repeat(auto-fill, minmax(132px, 1fr)); gap: 8px; align-content: start; padding: 2px 6px 16px 2px; }
#overlay .ccard { position: relative; display: flex; flex-direction: column; align-items: stretch; gap: 2px; padding: 0 0 8px; text-align: left; white-space: normal; overflow: hidden;
  background: linear-gradient(180deg, #0b1a2c, #010a13); border: 1px solid color-mix(in srgb, var(--rc) 25%, var(--lc-gold-5)); }
#overlay .ccard:hover { border-color: var(--lc-gold-2); transform: translateY(-2px); box-shadow: 0 8px 20px rgba(0, 0, 0, 0.5), 0 0 14px color-mix(in srgb, var(--rc) 30%, transparent); }
#overlay .ccard.sel { border-color: var(--lc-gold-1); box-shadow: 0 0 0 1px var(--lc-gold-2), 0 0 18px rgba(240, 210, 140, 0.25); }
#overlay .ccard.locked .ccard-prev { filter: grayscale(0.8) brightness(0.55); }
#overlay .ccard.locked .ccard-name { color: var(--c-text-2); }
.ccard-prev { position: relative; flex: none; height: 104px; overflow: hidden; display: flex; align-items: center; justify-content: center; border-bottom: 1px solid var(--lc-gold-5);
  background: radial-gradient(circle at 50% 40%, color-mix(in srgb, var(--rc) 30%, #0b1a2c), #010a13 85%); }
.ccard-prev::after { content: ""; position: absolute; left: 0; right: 0; bottom: 0; height: 2px; background: var(--rc); opacity: 0.7; }
.ccard-prev > img { max-width: 100%; max-height: 100%; object-fit: contain; }
.ccard-prev > img.board-preview { width: 100%; height: 100%; object-fit: cover; }
.ccard-prev canvas { max-width: 100%; max-height: 100%; }
.ccard-prev .pt { width: 100% !important; height: 100% !important; min-height: 0 !important; }
.ccard-name { font-weight: 700; font-size: var(--fs-sm); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; margin: 5px 8px 0; color: var(--lc-gold-1); }
.ccard-meta { display: flex; align-items: center; gap: 5px; margin: 0 8px; font-size: 10.5px; text-transform: uppercase; letter-spacing: 0.08em; color: var(--rc); font-weight: 700; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.ccard-meta i { width: 7px; height: 7px; flex: none; transform: rotate(45deg); background: var(--rc); }
.ccard .badge { position: absolute; top: 6px; right: 6px; }
.cp-emo { font-size: 46px; line-height: 1; } .cp-art { font-size: 30px; font-weight: 800; color: color-mix(in srgb, var(--rc, #fff) 70%, #fff); text-shadow: 0 2px 6px #000; }
.cprev { min-height: 0; min-width: 0; }
.cprev-card { height: 100%; display: flex; flex-direction: column; overflow: hidden; border: 1px solid var(--lc-gold-4);
  background: radial-gradient(420px 300px at 50% 30%, color-mix(in srgb, var(--rc) 22%, transparent), transparent 70%), linear-gradient(180deg, #0b1a2c, #010a13); box-shadow: var(--sh-2); }
.cprev-stage, .cprev-big { flex: 1 1 auto; min-height: 170px; }
.cprev-big { position: relative; display: flex; align-items: center; justify-content: center; overflow: hidden; padding: var(--sp-3); }
.cprev-big > img { max-width: 100%; max-height: 100%; object-fit: contain; }
.cprev-big > img.board-preview { border: 1px solid var(--lc-gold-4); box-shadow: var(--sh-3); }
.cprev-big canvas { position: absolute; inset: 0; width: 100% !important; height: 100% !important; }
.cprev-big .cp-emo { font-size: 110px; } .cprev-big .cp-art { font-size: 70px; }
.cprev-info { flex: none; display: flex; flex-direction: column; gap: var(--sp-2); padding: var(--sp-4); border-top: 1px solid var(--lc-gold-4); background: rgba(1, 10, 19, 0.85); }
.cprev-top { display: flex; align-items: center; gap: var(--sp-2); flex-wrap: wrap; }
.cprev-name { margin: 0; font-size: var(--fs-2xl); line-height: 1.05; overflow-wrap: anywhere; color: var(--lc-gold-1); text-transform: uppercase; letter-spacing: 0.03em; }
.cprev-desc { margin: 0; color: var(--c-text-2); font-size: var(--fs-sm); line-height: 1.45; }
.cprev-acts { display: flex; flex-direction: column; gap: var(--sp-2); margin-top: var(--sp-1); }
.cprev-acts p { margin: 0; }
.slot-h { font-size: var(--fs-xs); font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: var(--c-text-3); }
.slot-h .muted { text-transform: none; letter-spacing: 0; font-weight: 500; }
.slots { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 6px; }
#overlay .slot { display: flex; flex-direction: column; align-items: flex-start; gap: 1px; padding: 6px 8px; text-align: left; min-width: 0; }
.slot b { font-family: var(--font-display); font-size: var(--fs-xs); color: var(--c-text-3); }
.slot span { font-size: var(--fs-xs); max-width: 100%; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
#overlay .slot.cur { border-color: var(--lc-gold-1); box-shadow: 0 0 0 1px var(--lc-gold-2) inset; }
.evo-box { display: flex; flex-direction: column; gap: 6px; padding: var(--sp-3); background: rgba(1, 10, 19, 0.6); border: 1px solid var(--lc-gold-5); }

/* =====================================================================
   Loot (crafting workshop)
   ===================================================================== */
.loot { display: grid; grid-template-columns: minmax(0, 1fr) clamp(280px, 26vw, 380px); gap: 12px; height: 100%; padding: 12px var(--sp-5) var(--sp-4); max-width: 1720px; margin: 0 auto; }
.loot-stage { position: relative; display: flex; flex-direction: column; min-height: 0; overflow: hidden; border: 1px solid var(--lc-gold-4); background-size: cover; background-position: center; }
.loot-stage::before { content: ""; position: absolute; inset: 0; background: radial-gradient(ellipse at 50% 55%, rgba(1, 10, 19, 0.15), rgba(1, 10, 19, 0.85) 75%); }
.loot-head { position: relative; display: flex; align-items: flex-start; justify-content: space-between; gap: var(--sp-4); padding: var(--sp-4) var(--sp-5); flex-wrap: wrap; }
.loot-mats { display: flex; gap: 8px; flex-wrap: wrap; }
.mat { display: grid; grid-template-columns: auto auto; grid-template-rows: auto auto; column-gap: 8px; align-items: center; padding: 6px 12px 6px 8px; background: rgba(1, 10, 19, 0.85); border: 1px solid var(--lc-gold-5); }
.mat img { grid-row: 1 / span 2; width: 34px; height: 34px; object-fit: contain; }
.mat.bits { grid-template-columns: auto; }
.mat b { font: 700 var(--fs-lg) var(--font-display); color: var(--lc-gold-1); line-height: 1; }
.mat small { font-size: var(--fs-xs); color: var(--lc-mute); }
.altar { position: relative; flex: 1; min-height: 0; display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); align-items: center; gap: var(--sp-5); padding: 0 var(--sp-6) var(--sp-5); }
.altar-item { position: relative; display: grid; place-items: center; aspect-ratio: 1; max-height: 100%; max-width: 380px; justify-self: center; width: 100%; }
.altar-item::before { content: ""; position: absolute; inset: 4%; border-radius: 50%; border: 2px solid rgba(200, 170, 110, 0.5);
  background: conic-gradient(from 0deg, transparent 0 10%, rgba(63, 214, 255, 0.35) 15%, transparent 20% 45%, rgba(255, 138, 61, 0.35) 50%, transparent 55% 80%, rgba(63, 214, 255, 0.35) 85%, transparent 90%);
  -webkit-mask: radial-gradient(circle, transparent 54%, #000 56%); mask: radial-gradient(circle, transparent 54%, #000 56%); animation: dtSpin 12s linear infinite; }
.altar-item::after { content: ""; position: absolute; inset: 18%; border-radius: 50%; background: radial-gradient(circle, rgba(63, 214, 255, 0.35), transparent 70%); filter: blur(8px); animation: dtGlow 3s ease-in-out infinite; }
@keyframes dtGlow { 50% { opacity: 0.55; transform: scale(0.92); } }
.altar-egg, .altar-item .pt { position: relative; z-index: 1; width: 62% !important; height: auto !important; aspect-ratio: 1; object-fit: contain; animation: dtBob 3s ease-in-out infinite; background: transparent !important; }
.altar-egg { filter: hue-rotate(var(--hue, 0deg)) drop-shadow(0 18px 30px rgba(0, 0, 0, 0.7)); }
.altar-item .pt img { width: 100%; height: 100%; object-fit: contain; filter: drop-shadow(0 18px 30px rgba(0, 0, 0, 0.7)); }
.altar-info { position: relative; display: flex; flex-direction: column; gap: 8px; padding: var(--sp-4) var(--sp-5); background: rgba(1, 10, 19, 0.82); border: 1px solid var(--lc-gold-4); animation: dtRiseIn 0.4s var(--ease-out) both; }
.altar-info h2 { margin: 0; font-size: var(--fs-3xl); line-height: 1; color: var(--lc-gold-1); text-transform: uppercase; letter-spacing: 0.04em; }
.altar-info p { margin: 0; color: var(--c-text-2); font-size: var(--fs-sm); line-height: 1.45; }
.altar-acts { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; margin-top: 4px; }
.odds { display: flex; flex-wrap: wrap; gap: 6px; }
.odd { display: inline-flex; align-items: center; gap: 5px; padding: 2px 8px; font-size: var(--fs-xs); font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: var(--rc, var(--c-text-2));
  background: rgba(1, 10, 19, 0.7); border: 1px solid color-mix(in srgb, var(--rc, #888) 45%, transparent); }
.odd i { width: 7px; height: 7px; transform: rotate(45deg); background: var(--rc, #888); }
.loot-inv { display: flex; flex-direction: column; min-height: 0; overflow-y: auto; padding: 12px; border: 1px solid var(--lc-gold-5); background: linear-gradient(180deg, rgba(10, 20, 40, 0.92), rgba(1, 10, 19, 0.96)); }
.loot-inv .tile-h { margin: 4px 0 8px; font-size: var(--fs-sm); }
.loot-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(76px, 1fr)); gap: 6px; margin-bottom: 12px; }
#overlay .loot-item { position: relative; display: flex; flex-direction: column; align-items: center; gap: 2px; padding: 6px 4px; min-width: 0; text-align: center; white-space: normal; background: rgba(1, 10, 19, 0.75); border: 1px solid var(--lc-gold-5); }
#overlay .loot-item:hover { border-color: var(--lc-gold-3); transform: translateY(-1px); }
#overlay .loot-item.sel { border-color: var(--lc-gold-1); box-shadow: 0 0 0 1px var(--lc-gold-2), 0 0 14px rgba(240, 210, 140, 0.25); }
#overlay .loot-item.ready { border-color: var(--c-success); box-shadow: 0 0 12px rgba(79, 224, 143, 0.35); }
.loot-item img { width: 44px; height: 44px; object-fit: contain; filter: hue-rotate(var(--hue, 0deg)); }
.loot-item .pt { width: 44px !important; height: 44px !important; border-radius: 50%; background: radial-gradient(circle at 50% 30%, #2a3a5c, #0a1428) !important; overflow: hidden; display: block; }
.loot-item span { font-size: 10.5px; font-weight: 700; color: var(--lc-gold-1); max-width: 100%; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.loot-item small { font-size: 10px; color: var(--lc-mute); display: inline-flex; align-items: center; }

/* =====================================================================
   Profile
   ===================================================================== */
.prof { display: flex; flex-direction: column; gap: 12px; height: 100%; padding: 12px var(--sp-5) var(--sp-4); max-width: 1720px; margin: 0 auto; }
.prof-banner { flex: none; position: relative; height: clamp(150px, 24vh, 220px); display: flex; align-items: flex-end; justify-content: space-between; gap: var(--sp-4); padding: var(--sp-4) var(--sp-5);
  border: 1px solid var(--lc-gold-4); background-size: cover; background-position: center 40%; }
.prof-banner::before { content: ""; position: absolute; inset: 0; background: linear-gradient(0deg, rgba(1, 10, 19, 0.92), rgba(1, 10, 19, 0.15) 70%), linear-gradient(90deg, rgba(1, 10, 19, 0.6), transparent 50%); }
.prof-id-wrap { position: relative; display: flex; align-items: center; gap: var(--sp-4); min-width: 0; flex: 1; }
#overlay .prof-av { position: relative; flex: none; width: 112px; height: 112px; padding: 0; border-radius: 50%; overflow: visible; border: 0; background: none; box-shadow: none; }
#overlay .prof-av:hover { border: 0; background: none; box-shadow: none; }
.prof-av > .pt { position: absolute; inset: 10px; width: auto !important; height: auto !important; border-radius: 50%; overflow: hidden; background: radial-gradient(circle at 50% 30%, #2a3a5c, #0a1428); }
.prof-av .pav-l { position: absolute; left: 10px; right: 10px; bottom: 30px; padding: 3px 0; font-size: 10px; font-weight: 800; letter-spacing: 0.08em; text-transform: uppercase; text-align: center;
  background: rgba(1, 10, 19, 0.85); opacity: 0; transition: opacity var(--dur-1); }
.prof-av:hover .pav-l, .prof-av:focus-visible .pav-l { opacity: 1; }
.prof-lv { position: absolute; left: 50%; bottom: -6px; transform: translateX(-50%); min-width: 30px; padding: 1px 8px; font: 700 var(--fs-sm) var(--font-display); color: var(--lc-gold-1); background: var(--lc-navy-0); border: 1px solid var(--lc-gold-2); }
.prof-ring .rg-bg, .prof-ring .rg-fg { stroke-width: 4; }
.prof-id { display: flex; flex-direction: column; gap: 4px; min-width: 0; max-width: 420px; flex: 1; }
.prof-name { font-family: var(--font-display) !important; font-size: var(--fs-xl) !important; font-weight: 700; padding: 5px 10px !important; width: 100%; color: var(--lc-gold-1) !important; background: rgba(1, 10, 19, 0.6) !important; }
.prof-rank { position: relative; display: flex; align-items: center; gap: 10px; padding: 6px 14px 6px 6px; background: rgba(1, 10, 19, 0.75); border: 1px solid var(--lc-gold-4); min-width: 0; }
.prof-rank img { width: 64px; height: 64px; object-fit: contain; flex: none; }
.prof-rank div { display: flex; flex-direction: column; min-width: 0; }
.prof-rank b { font: 700 var(--fs-lg) var(--font-display); color: var(--lc-gold-1); text-transform: uppercase; letter-spacing: 0.08em; }
.prof-rank small { font-size: var(--fs-xs); color: var(--c-text-2); }
.prof-banners { position: absolute; top: 10px; right: 10px; display: flex; gap: 4px; }
#overlay .pb { width: 46px; height: 26px; padding: 0; background-size: cover; background-position: center; border: 1px solid var(--lc-gold-5); opacity: 0.7; }
#overlay .pb:hover { opacity: 1; border-color: var(--lc-gold-3); }
#overlay .pb.on { opacity: 1; border-color: var(--lc-gold-1); box-shadow: 0 0 8px rgba(240, 210, 140, 0.4); }
.prof-cols { flex: 1; min-height: 0; display: grid; grid-template-columns: clamp(300px, 27vw, 400px) minmax(0, 1fr); gap: 12px; }
.prof-left { display: flex; flex-direction: column; gap: 10px; min-height: 0; overflow-y: auto; padding-right: 2px; }
.prof-left .box { padding: 10px 12px; }
.prof-left .card-h { margin-bottom: 6px; }
.prof-right { display: grid; grid-template-rows: minmax(0, 1fr) auto; gap: 12px; min-height: 0; min-width: 0; }
.stat-tiles { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 6px; flex: none; }
.tile { display: flex; flex-direction: column; align-items: center; gap: 2px; padding: 8px 4px; background: rgba(1, 10, 19, 0.8); border: 1px solid var(--lc-gold-5); min-width: 0; }
.tile b { font-family: var(--font-display); font-size: var(--fs-xl); line-height: 1.1; color: var(--lc-gold-1); }
.tile span { font-size: 10px; font-weight: 700; letter-spacing: 0.06em; text-transform: uppercase; color: var(--c-text-3); white-space: nowrap; }
.chart { display: block; width: 100%; height: auto; overflow: visible; }
.ch-base, .ch-mid { stroke: var(--lc-gold-5); stroke-width: 1; }
.ch-mid { stroke-dasharray: 3 3; }
.ch-x { fill: var(--c-text-3); font: 600 9px var(--font-body); text-anchor: middle; }
.ch-v { fill: var(--c-text-2); font: 700 9px var(--font-body); text-anchor: middle; }
.ch-bar:hover path { filter: brightness(1.25); }
.ch-line { fill: none; stroke: var(--c-accent); stroke-width: 2; stroke-linejoin: round; stroke-linecap: round; }
.ch-dot { fill: var(--c-text-3); stroke: #010a13; stroke-width: 2; }
.ch-dot.good { fill: var(--c-accent); }
.fav { display: flex; align-items: center; justify-content: space-between; gap: var(--sp-2); padding: 4px 0; }
.favs-u { display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: var(--sp-2); }
.fav.u { justify-content: flex-start; padding: 0; }
.fav.u .pt { width: 38px; height: 38px; border: 1px solid var(--lc-gold-4); }
.fav.u div { display: flex; flex-direction: column; min-width: 0; }
.fav.u b { font-size: var(--fs-sm); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.fav.u .muted { font-size: var(--fs-xs); }
.hist-card { display: flex; flex-direction: column; min-height: 0; }
.hist-list { flex: 1; min-height: 0; overflow-y: auto; display: flex; flex-direction: column; gap: 6px; padding-right: 4px; }
.match { display: grid; grid-template-columns: 60px minmax(0, 1fr) auto; gap: var(--sp-3); align-items: center; padding: 8px var(--sp-3);
  background: linear-gradient(90deg, rgba(10, 20, 40, 0.8), rgba(1, 10, 19, 0.8)); border: 1px solid var(--lc-gold-5); border-left: 3px solid var(--c-text-3); }
.match.p-gold { border-left-color: var(--lc-gold-2); background: linear-gradient(90deg, rgba(200, 155, 60, 0.18), rgba(1, 10, 19, 0.8)); }
.match.p-good { border-left-color: var(--c-accent); }
.m-place { display: flex; flex-direction: column; align-items: center; }
.m-place b { font-family: var(--font-display); font-size: var(--fs-2xl); line-height: 1; }
.match.p-gold .m-place b { color: var(--lc-gold-1); } .match.p-good .m-place b { color: var(--c-accent); }
.m-place span { font-size: 10px; font-weight: 700; letter-spacing: 0.08em; text-transform: uppercase; color: var(--c-text-3); }
.m-body { display: flex; flex-direction: column; gap: 5px; min-width: 0; }
.m-top { display: flex; justify-content: space-between; gap: var(--sp-2); font-size: var(--fs-sm); font-weight: 600; flex-wrap: wrap; }
.m-top .muted { font-weight: 500; font-size: var(--fs-xs); }
.m-units .pt { width: 30px; height: 30px; border: 1px solid var(--lc-gold-5); }
.m-bits { display: inline-flex; align-items: center; font-weight: 800; font-size: var(--fs-sm); color: var(--lc-gold-1); white-space: nowrap; }
.bits-card { max-height: 200px; display: flex; flex-direction: column; }
.bits-list { overflow-y: auto; min-height: 0; }
.bh-row { display: grid; grid-template-columns: 1fr auto 86px; gap: var(--sp-3); padding: 4px 0; border-bottom: 1px solid var(--c-border-1); font-size: var(--fs-sm); }
.bh-row span { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; color: var(--c-text-2); }
.bh-row b { color: var(--c-bits); } .bh-row i { font-style: normal; color: var(--c-text-3); text-align: right; font-size: var(--fs-xs); }

/* =====================================================================
   Settings
   ===================================================================== */
.settings { display: flex; flex-direction: column; gap: var(--sp-4); padding: var(--sp-4) var(--sp-5) var(--sp-6); max-width: 1280px; margin: 0 auto; }
.set-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: var(--sp-4); align-items: start; }
.set-music { display: flex; align-items: center; justify-content: space-between; gap: var(--sp-3); margin-top: var(--sp-3); padding-top: var(--sp-3); border-top: 1px solid var(--lc-gold-5); font-size: var(--fs-sm); flex-wrap: wrap; }
.keytable { width: 100%; border-collapse: collapse; font-size: var(--fs-sm); }
.keytable td { padding: 6px 4px; border-bottom: 1px solid var(--c-border-1); color: var(--c-text-2); vertical-align: middle; }
.keytable td:first-child { white-space: nowrap; width: 1%; padding-right: var(--sp-3); }
.set-about p { margin: 6px 0; font-size: var(--fs-sm); }

/* =====================================================================
   Store, Codex and Digi-Farm docked as pages under the top bar, left of the social column
   ===================================================================== */
body.client-on #dtStore, body.client-on #cxFull, body.client-on #dtFarm { top: var(--nav-h); right: var(--social-w); z-index: 50; align-items: stretch; padding: 12px var(--sp-5) var(--sp-4);
  background: radial-gradient(900px 500px at 90% 0%, rgba(63, 214, 255, 0.07), transparent 60%), #010a13; }
body.client-on.social-off #dtStore, body.client-on.social-off #cxFull, body.client-on.social-off #dtFarm { right: 0; }
body.client-on .dts-win, body.client-on .cx-win, body.client-on .df-win { width: 100%; max-width: 1720px; height: 100%; margin: 0 auto; border-radius: 0 !important; }
body.client-on #dtStore:not(.hidden) .dts-win, body.client-on #cxFull:not(.hidden) .cx-win, body.client-on #dtFarm:not(.hidden) .df-win { animation: dtPageIn 240ms var(--ease-out); }
body.client-on .dts-close, body.client-on .cx-close { display: none; }

/* window chrome shared by the Store, Codex, Farm and Digivolve panel */
.dts-win, .cx-win, .df-win, .dte-win { background: linear-gradient(180deg, #0a1428, #010a13) !important; border: 1px solid var(--lc-gold-4) !important; border-radius: 0 !important; box-shadow: var(--sh-3) !important; }
.dts-head, .cx-head, .dte-head { background: linear-gradient(180deg, rgba(10, 20, 40, 0.95), rgba(1, 10, 19, 0.7)) !important; border-bottom: 1px solid var(--lc-gold-5) !important; }
.df-head { background: linear-gradient(180deg, rgba(14, 40, 40, 0.9), rgba(1, 10, 19, 0.7)) !important; border-bottom: 1px solid var(--lc-gold-5) !important; }
.cx-head h2, .df-head h2, .dte-head h2, .dts-head h2 { font-family: var(--font-display); letter-spacing: 0.08em; text-transform: uppercase; color: var(--lc-gold-1); }
.cx-head h2 { background: none !important; -webkit-background-clip: initial !important; background-clip: initial !important; color: var(--lc-gold-1) !important; }

/* Store */
#dtStore { --r-common: #9aa6bb; --r-uncommon: #5fd36a; --r-rare: #4fb3ff; --r-epic: #c77dff; --r-legendary: #ffb347; }
.dts-head { position: relative; background: linear-gradient(90deg, rgba(1, 10, 19, 0.95), rgba(1, 10, 19, 0.55) 55%, rgba(1, 10, 19, 0.85)), ${A('store-hero')} center 55% / cover !important; min-height: 64px; }
.dts-modes { padding: 2px; gap: 2px; background: rgba(1, 10, 19, 0.85); border: 1px solid var(--lc-gold-5); border-radius: 0 !important; }
.dts-modes button { border: 0 !important; background: transparent !important; font-family: var(--font-display); letter-spacing: 0.1em; text-transform: uppercase; font-size: 15px !important; color: var(--lc-mute); padding: 5px 16px !important; box-shadow: none !important; }
.dts-modes button.on { background: linear-gradient(180deg, #1e2328, #2a3240) !important; color: var(--lc-gold-1) !important; box-shadow: inset 0 0 0 1px var(--lc-gold-4) !important; }
.dts-mis { background: rgba(1, 10, 19, 0.75) !important; border-color: var(--lc-gold-5) !important; border-radius: 0 !important; }
.dts-wallet { background: rgba(1, 10, 19, 0.85) !important; border-color: var(--lc-gold-4) !important; border-radius: 0 !important; }
.dts-wallet > span { font-variant-numeric: tabular-nums; }
.dts-hist { background: #0a1428 !important; border-color: var(--lc-gold-4) !important; border-radius: 0 !important; animation: dtPop var(--dur-1) ease-out; }
.dts-tabs { gap: 2px !important; padding: 0 16px !important; border-bottom: 1px solid var(--lc-gold-5) !important; background: rgba(1, 10, 19, 0.6); }
.dts-tabs button { position: relative; border: 0 !important; background: transparent !important; border-radius: 0 !important; padding: 11px 14px 12px !important; box-shadow: none !important;
  font-family: var(--font-display); font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: var(--lc-mute); }
.dts-tabs button:hover { color: var(--lc-gold-1); }
.dts-tabs button.on { color: var(--lc-gold-1) !important; background: transparent !important; }
.dts-tabs button.on::after { content: ""; position: absolute; left: 10px; right: 10px; bottom: -1px; height: 2px; background: var(--lc-gold-2); box-shadow: 0 0 10px var(--lc-gold-3); }
.dts-body h3 { font-family: var(--font-display); letter-spacing: 0.12em; text-transform: uppercase; color: var(--lc-gold-1); }
.dts-body > section:not(.dts-feat) > h3 { position: relative; margin: 0 0 12px; padding: 26px 18px 12px; border: 1px solid var(--lc-gold-5);
  background: linear-gradient(90deg, rgba(1, 10, 19, 0.92), rgba(1, 10, 19, 0.35) 70%), var(--sec-art, ${A('store-hero')}) center 45% / cover; }
.dts-body > section:not(.dts-feat):has(.dts-card[data-kind="legend"]) > h3 { --sec-art: ${A('store-legends')}; }
.dts-body > section:not(.dts-feat):has(.dts-card[data-kind="board"]) > h3 { --sec-art: ${A('store-boards')}; }
.dts-body > section:not(.dts-feat):has(.dts-card:is([data-kind="killscreen"], [data-kind="dance"], [data-kind="emote"], [data-kind="sound"])) > h3 { --sec-art: ${A('store-fx')}; }
.dts-note { padding: 34px 18px 14px !important; border: 1px solid var(--lc-gold-5); color: var(--c-text-1) !important;
  background: linear-gradient(90deg, rgba(1, 10, 19, 0.95), rgba(1, 10, 19, 0.45) 75%), ${A('store-eggs')} center 40% / cover !important; }
.dts-feat { position: relative; background: linear-gradient(90deg, rgba(1, 10, 19, 0.92), rgba(1, 10, 19, 0.6) 60%, rgba(1, 10, 19, 0.9)), ${A('store-hero')} center 40% / cover !important;
  border: 1px solid var(--lc-gold-3) !important; border-radius: 0 !important; }
.dts-feat .dts-card::before { content: "-25%"; position: absolute; z-index: 2; top: 6px; left: 6px; padding: 1px 6px; font: 800 10.5px var(--font-display); color: #fff; background: linear-gradient(180deg, #e8433f, #a5221f); border: 1px solid #ff8a7a; }
.dts-card { position: relative; background: linear-gradient(180deg, #0b1a2c, #010a13) !important; border: 1px solid color-mix(in srgb, var(--rc) 30%, var(--lc-gold-5)) !important; border-radius: 0 !important;
  box-shadow: inset 0 3px 0 -1px color-mix(in srgb, var(--rc) 70%, transparent), 0 2px 8px rgba(0, 0, 0, 0.3) !important; transition: transform var(--dur-1), box-shadow var(--dur-1), border-color var(--dur-1); }
.dts-card:hover { transform: translateY(-2px); border-color: var(--lc-gold-2) !important; box-shadow: inset 0 3px 0 -1px var(--rc), 0 10px 24px rgba(0, 0, 0, 0.5), 0 0 14px color-mix(in srgb, var(--rc) 35%, transparent) !important; }
.dts-card.eq { border-color: var(--lc-gold-1) !important; }
.dts-prev { border-radius: 0 !important; }
.dts-name { font-weight: 700; color: var(--lc-gold-1); }
.dts-rar { font-weight: 700; }
.dts-buy:not(.poor) { color: #cdfafa; border-color: var(--lc-gold-3) !important; background: linear-gradient(180deg, #0b3a4a, #062033) !important; }
.dts-buy:not(.poor):hover { border-color: var(--lc-gold-1) !important; box-shadow: 0 0 14px rgba(63, 214, 255, 0.3); }
.dts-eggs { position: relative; }
.dts-egg-card { background: linear-gradient(180deg, rgba(1, 10, 19, 0.5), #010a13 80%), ${A('store-eggs')} center 30% / cover !important;
  border-color: var(--lc-gold-5) !important; border-radius: 0 !important; transition: transform var(--dur-1), border-color var(--dur-1); }
.dts-egg-card:hover { transform: translateY(-2px); border-color: var(--lc-gold-3) !important; }
.dts-egg-card:hover .dts-egg { animation: dtsWobble 0.6s ease-in-out 1; }
.dts-egg-art { position: relative; }
.dts-egg-art::before { content: ""; position: absolute; left: 50%; top: 58%; width: 90px; height: 16px; transform: translate(-50%, 0); border-radius: 50%; background: radial-gradient(rgba(0, 0, 0, 0.55), transparent 70%); }
.dts-egg-art.lg::before { width: 160px; height: 26px; top: 70%; }
.dts-msg { background: rgba(1, 10, 19, 0.96) !important; border-color: var(--lc-gold-4) !important; border-radius: 0 !important; box-shadow: var(--sh-2); }
.dts-modal { background: rgba(1, 5, 10, 0.78) !important; backdrop-filter: blur(3px); animation: dtFade var(--dur-1) ease-out; }
.dts-mbox { background: linear-gradient(180deg, #0a1428, #010a13) !important; border-color: var(--lc-gold-3) !important; border-radius: 0 !important;
  box-shadow: var(--sh-3) !important; animation: dtPop var(--dur-2) var(--ease-out); padding: var(--sp-5) var(--sp-6) !important; }
.dts-mbox h2 { font-family: var(--font-display); color: var(--lc-gold-1); text-transform: uppercase; letter-spacing: 0.05em; }
.dts-mprev { position: relative; border-radius: 0 !important; box-shadow: 0 0 30px color-mix(in srgb, var(--rc) 35%, transparent); }
.dts-hatch { position: relative; }
.dts-hatch .dts-egg-art.lg { overflow: visible; }
.dts-hatch .dts-egg-art.lg::after { content: ""; position: absolute; left: 50%; top: 50%; width: 300px; height: 300px; transform: translate(-50%, -50%); border-radius: 50%; pointer-events: none; z-index: -1;
  background: conic-gradient(from 0deg, transparent 0 8%, var(--hc, rgba(255,255,255,0.4)) 10%, transparent 12% 33%, var(--hc) 35%, transparent 37% 58%, var(--hc) 60%, transparent 62% 83%, var(--hc) 85%, transparent 87%);
  -webkit-mask: radial-gradient(circle, #000 25%, transparent 70%); mask: radial-gradient(circle, #000 25%, transparent 70%); opacity: 0.35; animation: dtSpin 6s linear infinite; }
.dts-reveal { position: relative; isolation: isolate; }
.dts-reveal::before { content: ""; position: absolute; left: 50%; top: 110px; width: 460px; height: 460px; transform: translate(-50%, -50%); z-index: -1; border-radius: 50%; pointer-events: none;
  background: conic-gradient(from 0deg, transparent 0 5%, var(--hc) 7%, transparent 9% 20%, var(--hc) 22%, transparent 24% 45%, var(--hc) 47%, transparent 49% 70%, var(--hc) 72%, transparent 74% 95%, var(--hc) 97%, transparent 99%);
  -webkit-mask: radial-gradient(circle, #000 15%, transparent 68%); mask: radial-gradient(circle, #000 15%, transparent 68%); opacity: 0.55; animation: dtSpin 9s linear infinite; }
.dts-reveal h2 { font-size: var(--fs-3xl) !important; margin: 10px 0 2px !important; animation: dtRise 0.45s 0.1s var(--ease-out) both; }
.dts-reveal .dts-rar { font-family: var(--font-display); font-size: var(--fs-md); letter-spacing: 0.16em; animation: dtRise 0.45s 0.2s var(--ease-out) both; }
.dts-reveal .dts-mprev { animation: dtReveal 0.55s var(--ease-out) both; }
.dts-hatch.r-legendary .dts-reveal h2, .dts-hatch.r-epic .dts-reveal h2 { text-shadow: 0 0 24px var(--hc); }
@keyframes dtRise { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: none; } }
/* game-over Bits breakdown */
.dts-award { background: rgba(1, 10, 19, 0.7) !important; border: 1px solid var(--lc-gold-5) !important; border-radius: 0 !important; padding: var(--sp-3) var(--sp-4) !important; }
.dts-award-h { font-family: var(--font-display); letter-spacing: 0.1em; text-transform: uppercase; font-size: var(--fs-sm); color: var(--lc-gold-1); }
.dts-aline b { color: var(--c-bits) !important; }
.dts-aline.tot b { color: #fff !important; }

/* Codex: set switcher (League-style art tabs) */
.cx-sets { display: flex; gap: 6px; padding: 10px 16px 0; overflow-x: auto; scrollbar-width: none; flex: none; min-width: 0; max-width: 100%; box-sizing: border-box; }
.cx-set { position: relative; flex: 0 0 auto; display: flex; flex-direction: column; justify-content: flex-end; align-items: flex-start; width: 170px; height: 58px; padding: 0 10px 7px !important; overflow: hidden; text-align: left; cursor: pointer;
  background: linear-gradient(0deg, rgba(1, 10, 19, 0.92), rgba(1, 10, 19, 0.25)), var(--art) center 30% / cover !important; border: 1px solid var(--lc-gold-5) !important; border-radius: 0 !important;
  filter: saturate(0.55) brightness(0.8); transition: filter var(--dur-2), border-color var(--dur-2) !important; }
.cx-set:hover { filter: none; border-color: var(--lc-gold-3) !important; }
.cx-set.on { filter: none; border-color: var(--lc-gold-1) !important; box-shadow: 0 0 0 1px var(--lc-gold-2), 0 0 14px rgba(240, 210, 140, 0.25) !important; }
.cx-set b { display: block; max-width: 100%; font: 700 var(--fs-md) var(--font-display); letter-spacing: 0.06em; text-transform: uppercase; color: var(--lc-gold-1); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.cx-set small { display: block; max-width: 100%; font-size: 10px; color: var(--lc-mute); letter-spacing: 0.04em; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.cx-set .cx-live { position: absolute; top: 5px; right: 6px; padding: 0 5px; font: 800 9px/14px var(--font-display); letter-spacing: 0.08em; text-transform: uppercase; color: var(--c-text-inv); background: var(--c-accent); }
.cx-tabs button, .cx-filters button { border-radius: 0 !important; }
.cx-tabs button.on { background: linear-gradient(180deg, #1e2328, #2a3240) !important; border-color: var(--lc-gold-2) !important; color: var(--lc-gold-1) !important; box-shadow: 0 0 0 1px var(--lc-gold-3) inset !important; }
.cx-search { background: rgba(1, 10, 19, 0.85) !important; border-color: var(--lc-gold-5) !important; border-radius: 0 !important; }
.cx-search:focus { border-color: var(--lc-gold-2) !important; box-shadow: 0 0 0 2px rgba(200, 170, 110, 0.18); }
.cx-line, .cx-card, .cx-rules section.cx-rule { background: linear-gradient(180deg, #0b1a2c, #010a13) !important; border-radius: 0 !important; transition: transform var(--dur-1), border-color var(--dur-1); }
.cx-line:hover { transform: translateY(-1px); }
.cx-body h3 { font-family: var(--font-display); letter-spacing: 0.12em; }
.cx-mbox { background: linear-gradient(180deg, #0a1428, #010a13) !important; border-radius: 0 !important; border-color: var(--lc-gold-3) !important; animation: dtPop var(--dur-2) var(--ease-out); }
.cx-modal { backdrop-filter: blur(3px); }
#cxPanel { background: linear-gradient(180deg, #0a1428, #010a13) !important; border-right-color: var(--lc-gold-5) !important; }
.cx-ph { background: linear-gradient(180deg, rgba(10, 20, 40, 0.95), rgba(1, 10, 19, 0.85)) !important; }
.cx-ptitle { font-family: var(--font-display); letter-spacing: 0.14em !important; color: var(--lc-gold-2) !important; }

/* Digi-Farm chrome */
.df-pill { background: rgba(1, 10, 19, 0.75) !important; border-color: var(--lc-gold-5) !important; border-radius: 0 !important; }
.df-card { background: linear-gradient(180deg, rgba(10, 20, 40, 0.6), rgba(1, 10, 19, 0.92)); border-left-color: var(--lc-gold-5) !important; }
.df-box { border-radius: 0 !important; }
.df-mbox { background: linear-gradient(180deg, #0a1428, #010a13) !important; border-color: var(--lc-gold-3) !important; border-radius: 0 !important; animation: dtPop var(--dur-2) var(--ease-out); }

/* Digivolve panel */
#dtEvo { backdrop-filter: blur(3px); }
.dte-win { animation: dtPop var(--dur-2) var(--ease-out); }
.dte-bsec { border-radius: 0 !important; }

/* sound settings popover */
#sndPop { background: linear-gradient(180deg, #0a1428, #010a13) !important; border-color: var(--lc-gold-4) !important; border-radius: 0 !important; box-shadow: var(--sh-3) !important; animation: dtPop var(--dur-1) ease-out; }
#sndPop h4 { font-family: var(--font-display); letter-spacing: 0.06em; }

/* =====================================================================
   In-game overlays: token colors and type only, same layout and hooks
   ===================================================================== */
#tooltip:not(.icard) { background: linear-gradient(180deg, rgba(10, 20, 40, 0.98), rgba(1, 10, 19, 0.98)); border-color: var(--lc-gold-4); border-radius: 0; box-shadow: var(--sh-3); }
#tooltip h3 { font-family: var(--font-display); letter-spacing: 0.02em; }
.ic { background: linear-gradient(180deg, var(--c-surface-2), var(--c-bg-1) 75%) !important; }
.ic-nm { font-family: var(--font-display); letter-spacing: 0.02em; }
#modal .mpanel { background: linear-gradient(180deg, rgba(10, 20, 40, 0.98), rgba(1, 10, 19, 0.98)); border-color: var(--lc-gold-4); box-shadow: var(--sh-3); animation: dtPop var(--dur-2) var(--ease-out); }
#modal h2 { font-family: var(--font-display); letter-spacing: 0.04em; }
#modal .choice { background: linear-gradient(180deg, var(--c-surface-2), var(--c-surface-1)); transition: transform var(--dur-1), filter var(--dur-1), border-color var(--dur-1); }
#modal .choice:not(.taken):hover { border-color: var(--lc-gold-2); }
#banner { font-family: var(--font-display); letter-spacing: 0.03em; background: linear-gradient(180deg, rgba(10, 20, 40, 0.95), rgba(1, 10, 19, 0.95)); border-color: var(--lc-gold-4); }
#banner .small { font-family: var(--font-body); letter-spacing: 0; }
#toast { font-family: var(--font-body); background: rgba(1, 10, 19, 0.94); border: 1px solid var(--lc-gold-4); }
#ksLayer .ksMain { font-family: var(--font-display); letter-spacing: 0.06em; text-transform: uppercase; }
#ksLayer .ksSub { font-family: var(--font-body); }
/* Results open on elimination: keep the victim's final attack/dance visible above them. Ordinary menus and
   modules still suppress queued kill screens. */
body.client-on:not(:has(#pgHome.on #homeResults:not(.hidden))) #ksLayer,
body.client-on.client-module #ksLayer { display: none !important; }
#picker .pk { background: linear-gradient(180deg, var(--c-surface-2), var(--c-surface-1)); border-color: var(--c-border-2); }

/* =====================================================================
   Motion preferences and sizes (1280x720 is the smallest target)
   ===================================================================== */
body.reduce-motion :is(#overlay, #cnav, #csocial, #cmodal, #ctoasts, #cload, #dtStore, #cxFull, #dtFarm, #dtEvo) *,
body.reduce-motion :is(#overlay, #cmodal, #dtStore, #cxFull, #dtFarm, #cload) { animation-duration: 1ms !important; animation-iteration-count: 1 !important; transition-duration: 1ms !important; }
@media (prefers-reduced-motion: reduce) {
  :is(#overlay, #cnav, #csocial, #cmodal, #ctoasts, #cload) *, #overlay { animation-duration: 1ms !important; animation-iteration-count: 1 !important; transition-duration: 1ms !important; }
}
@media (max-width: 1700px) {
  #cnav .cn-tab { padding: 0 10px; font-size: 13px; letter-spacing: 0.1em; }
  .howto li:nth-child(n+4) { display: none; }
  .howto { grid-template-columns: repeat(3, minmax(0, 1fr)); }
}
@media (max-width: 1500px) {
  :root { --social-w: 210px; }
  #cnav .cn-play { min-width: 140px; padding-right: 28px; margin-left: 10px; }
  .cn-play-t { font-size: 17px; }
  #cnav .cn-tab { padding: 0 8px; font-size: 12px; letter-spacing: 0.08em; }
  #cnav .cn-status span { display: none; }
  .home, .play, .coll, .prof, .loot { padding-left: var(--sp-4); padding-right: var(--sp-4); }
  .hero-txt { padding: 0 var(--sp-5); width: 56%; }
  .hero-thumbs { left: var(--sp-5); }
  #overlay .hero-thumb { flex-basis: 96px; }
  .mpseats { grid-template-columns: repeat(4, minmax(0, 1fr)); }
  .mp-invite input { width: 220px; }
  .mplobby { grid-template-columns: minmax(0, 1fr) auto; }
  .mp-pips { display: none; }
  .party-slots { grid-template-columns: 1.5fr repeat(7, minmax(0, 1fr)); gap: 6px; }
  .set-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .set-grid .keys { grid-column: 1 / -1; }
  .res-cols { grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); }
  .res-cols .res-award { grid-column: 1 / -1; }
  .ev-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
@media (max-width: 1380px) {
  #cnav .cn-icon#cnStoreIc { display: none; }
  #cnav .cn-tab { padding: 0 7px; font-size: 11.5px; letter-spacing: 0.06em; }
  .cn-right { padding: 0 8px; gap: 4px; }
  #cnav .cn-profile { padding-left: 10px; gap: 8px; }
  .hero-render.r1 { display: none; }
  .home-tiles { grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px; }
  .home-top { gap: 8px; }
  .home-view { gap: 8px; }
  .tile-card { padding: 8px 10px; }
  .tile-art { margin: -8px -10px 6px; height: 48px; }
  .mode-txt small { display: none; }
  #overlay .lc-confirm { min-width: 180px; }
  .ps-pick { display: none; }
  .res-cols { grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); }
  .coll { grid-template-columns: minmax(0, 1fr) 270px; }
  .loot { grid-template-columns: minmax(0, 1fr) 290px; }
  .altar { grid-template-columns: minmax(0, 0.9fr) minmax(0, 1.1fr); gap: var(--sp-4); padding: 0 var(--sp-4) var(--sp-4); }
  .prof-cols { grid-template-columns: 300px minmax(0, 1fr); }
  .ctab, .dts-tabs button { padding: 9px 9px 10px !important; }
  .ctab span, .dts-tabs button span { display: none; }
  .mpseat { padding: 8px 10px; }
  .cx-set { width: 140px; }
}
@media (max-width: 1100px) {
  .home-top { grid-template-columns: minmax(0, 1fr) 220px; }
  .home-tiles { grid-template-columns: repeat(2, minmax(0, 1fr)); height: auto; }
  .party-slots { grid-template-columns: repeat(4, minmax(0, 1fr)); }
  .stat-tiles { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .mpseats { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
@media (max-width: 1180px) {
  body.client-on #csocial { display: none; }
  body.client-on #overlay { padding-right: 0; }
  body.client-on #dtStore, body.client-on #cxFull, body.client-on #dtFarm { right: 0; }
  #ctoasts { right: 16px; }
  #cnav .cn-profile { width: auto; padding-right: 54px; }
  .cn-who { display: none; }
  .cn-tabs { margin-left: 0; }
  #cnav .cn-play { min-width: 120px; padding-right: 26px; }
  .cn-play .cn-mark { width: 28px; height: 28px; }
}
@media (max-height: 820px) {
  :root { --nav-h: 72px; }
  #cnav .cn-play { margin-top: 12px; margin-bottom: 12px; }
  .home, .play, .coll, .prof, .loot { padding-top: 8px; padding-bottom: 10px; }
  .subnav { margin-bottom: 6px; }
  .subnav button { padding: 6px 12px 7px !important; }
  .home-tiles { height: 168px; }
  .tile-art { height: 40px; }
  .tile-card .patch-list li:nth-child(n+3) { display: none; }
  .hero-txt { bottom: 62px; }
  .hero-desc { -webkit-line-clamp: 2; }
  .box { padding: var(--sp-3) var(--sp-4); }
  .card-h { margin-bottom: var(--sp-2); }
  .howto { display: none; }
  .play-solo-body, #lobbyPanel { padding: var(--sp-3) var(--sp-4); }
  .play-foot { padding: 8px var(--sp-4); }
  .play-foot .btn-xl { padding: 10px 40px; font-size: var(--fs-xl); }
  .coll-banner { height: 72px; }
  .ccard-prev { height: 88px; }
  .prof-banner { height: 140px; padding: var(--sp-3) var(--sp-4); }
  #overlay .prof-av { width: 92px; height: 92px; }
  .prof-rank img { width: 50px; height: 50px; }
  .res-emblem { width: 96px; height: 96px; }
  .res-num { font-size: 36px; }
  .results { padding: var(--sp-3) var(--sp-4); gap: 8px; }
  .res-units .pt { width: 32px; height: 32px; }
  .mpseat { min-height: 96px; gap: 4px; }
  .mph { margin-top: var(--sp-3); }
  .loot-head { padding: var(--sp-3) var(--sp-4); }
  .altar-info h2 { font-size: var(--fs-2xl); }
  body.client-on #dtStore, body.client-on #cxFull, body.client-on #dtFarm { padding: 8px var(--sp-4) 10px; }
  .art-banner, .ev-banner { height: 140px; }
  .ld-cards { width: min(1180px, 100%); }
}
/* Flexible pages: preserve readable content and let short windows scroll. */
.cn-tabs { overflow-x: auto; scrollbar-width: thin; }
#cnav .cn-tab { flex: none; }
.play:has(.play-select:not(.hidden)) { height: auto; min-height: 100%; }
.play-select { height: auto; min-height: 100%; }
.play-sets { flex: none; min-width: 0; }
.ps-foot { flex-wrap: wrap; }
.ps-confirm { flex: 1; justify-content: flex-end; min-width: 0; }
.ps-pick { display: flex; flex-wrap: wrap; white-space: normal; }
.ps-head, .play-foot, .mp-invite { flex-wrap: wrap; }
.mp-invite input { max-width: 100%; min-width: 0; }
.play-modes { flex-wrap: wrap; }
.home-top { min-height: 280px; }
.home-view { flex-shrink: 0; }
.home { min-height: 100%; height: auto; }
.home-tiles { flex-shrink: 0; }
@media (min-width: 1900px) and (min-height: 1000px) {
  .home { height: 100%; }
  .home-top { min-height: 360px; }
}
@media (max-width: 1000px) {
  :root { --nav-h: 110px; }
  #cnav { flex-wrap: wrap; align-content: flex-start; }
  #cnav .cn-play { height: 46px; margin: 4px 0 4px 10px; }
  .cn-tabs { order: 5; width: 100%; height: 52px; flex: 0 0 100%; border-top: 1px solid var(--lc-gold-5); }
  #cnav .cn-tab { flex: 1 0 auto; padding: 0 14px; font-size: 12px; }
  .cn-right { margin-left: auto; height: 54px; }
  #cnav .cn-profile { height: 54px; }
  .ps-confirm { flex-wrap: wrap; }
  .ps-pick { flex: 1; }
  .home-top { grid-template-columns: minmax(0, 1fr); }
  .hero { min-height: 340px; }
  .df-head { flex-wrap: wrap; }
  .df-body { grid-template-columns: minmax(0, 1fr) 280px; }
  .df-bar { flex-wrap: wrap; justify-content: center; }
  .df-act { flex: 0 0 48px; }
  .df-menu { min-width: min(300px, calc(100% - 20px)); bottom: 130px; }
  .coll { grid-template-columns: minmax(0, 1fr) 240px; }
  .loot { grid-template-columns: minmax(0, 1fr) 250px; }
  .prof-cols { grid-template-columns: 260px minmax(0, 1fr); }
}
@media (max-width: 700px) {
  #cnav .cn-play { min-width: 104px; margin-left: 6px; padding-right: 22px; }
  .cn-play-t { font-size: 15px; }
  .cn-play .cn-mark { width: 24px; height: 24px; }
  .cn-right { padding: 0 4px; gap: 4px; }
  #cnav .cn-profile { width: 64px; padding: 0 28px 0 0; }
  .cn-ring { width: 36px; height: 36px; }
  #cnav .cn-av, #cnav .cn-av .pt { width: 28px; height: 28px; }
  #cnav .cn-status { padding: 6px; }
  #cnav .cn-icon { width: 28px; height: 28px; }
  #cnav #cnMin { display: none; }
  .home, .play, .coll, .prof, .loot { padding: 12px; }
  .ps-head { align-items: flex-start; }
  .play-status { padding: 4px; }
  .ps-emblem { width: 30px; height: 30px; }
  .play-modes, .ps-confirm { width: 100%; }
  #overlay .mode-card { flex: 1; }
  .ps-confirm { justify-content: space-between; }
  .ps-pick { flex-basis: 100%; }
  #overlay .lc-confirm { width: 100%; }
  .home-tiles, .set-grid, .prof-cols, .res-cols { grid-template-columns: minmax(0, 1fr); }
  .home-tiles { height: auto; }
  .tile-card { min-height: 150px; }
  .hero-txt { width: 100%; padding: 0 18px; }
  .hero-txt h1 { font-size: 36px; }
  .hero-render { opacity: .35; }
  .hero-thumbs { left: 18px; right: 18px; overflow-x: auto; justify-content: flex-start; scrollbar-width: thin; }
  #overlay .hero-thumb { flex: 0 0 80px; }
  .coll, .loot { height: auto; min-height: 100%; grid-template-columns: minmax(0, 1fr); grid-template-rows: auto auto; }
  .coll-main { min-height: 440px; }
  .cgrid, .loot-inv { max-height: 65vh; min-height: 240px; }
  .cprev { min-height: 340px; }
  .prof { height: auto; }
  .prof-cols { flex: none; }
  .hist-list { max-height: 60vh; }
  .prof-banner { flex-wrap: wrap; height: auto; min-height: 140px; }
  .party-slots { grid-template-columns: repeat(2, minmax(0, 1fr)); min-height: 640px; }
  .play-foot .btn-xl { padding: 10px 24px; }
  .altar { grid-template-columns: 1fr; }
  .res-cols .res-award { grid-column: auto; }
  .dts-head, .cx-head, .df-head { flex-wrap: wrap; }
  .df-body { grid-template-columns: minmax(0, 1fr); grid-template-rows: 380px auto; overflow-y: auto; }
  .df-card { overflow: visible; border-left: 0; border-top: 1px solid var(--lc-gold-5); }
  .dts-feat { flex-wrap: wrap; }
  .dts-mbox { max-width: calc(100vw - 24px); padding: 16px !important; }
  .set-grid .keys { grid-column: auto; }
  #overlay .prof-av { width: 64px; height: 64px; }
}

`;
const tag = document.createElement('style');
tag.id = 'dt-theme';
tag.textContent = css;
document.head.appendChild(tag);
