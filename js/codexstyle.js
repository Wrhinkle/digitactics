// Styles for js/codex.js (side panel and full-screen codex). Injected at import like shopstyle.js,
// so the packaged builds need no extra file.
const css = `
/* ---------- shared pieces ---------- */
.cx-seg { display: inline-flex; gap: 2px; padding: 2px; border-radius: 7px; background: var(--bg); border: 1px solid var(--line); flex-wrap: wrap; }
.cx-seg button { padding: 3px 9px; font-size: 12px; border: 1px solid transparent; background: transparent; color: var(--muted); }
.cx-seg button.on { background: var(--panel3); border-color: var(--line2); color: var(--text); }
.cx-seg button[style*="--tc"].on { border-color: var(--tc); color: var(--tc); }
.cx-hex { --tc: #8e98ad; display: inline-flex; align-items: center; justify-content: center; flex: none; width: 28px; height: 30px; font-size: 11px; font-weight: 900;
  color: #10131a; background: var(--tc); clip-path: polygon(50% 0, 100% 25%, 100% 75%, 50% 100%, 0 75%, 0 25%); letter-spacing: 0; overflow: hidden; }
.cx-hex img { width: 100%; height: 100%; object-fit: cover; }
.cx-hex.xs { width: 14px; height: 15px; font-size: 7px; }
.cx-ii { display: inline-flex; align-items: center; justify-content: center; width: 34px; height: 34px; flex: none; border-radius: 6px; font-size: 11px; font-weight: 800; background: var(--panel3); }
.cx-ii.sm { width: 26px; height: 26px; } .cx-ii.xs { width: 18px; height: 18px; border-radius: 4px; }
.cx-pill { display: inline-block; padding: 0 7px; border-radius: 9px; font-size: 11px; font-weight: 700; border: 1px solid var(--line2); color: var(--muted); white-space: nowrap; }
.cx-pill.cost { color: var(--tc); border-color: color-mix(in srgb, var(--tc) 60%, transparent); }
.cx-empty { color: var(--muted); font-size: 13px; padding: 10px 4px; }
.cx-empty b { color: var(--text); }
.flash { animation: cxFlash 1.2s ease-out; }
@keyframes cxFlash { 0%, 30% { box-shadow: 0 0 0 2px var(--gold), 0 0 16px rgba(255, 207, 74, 0.5); } 100% { box-shadow: none; } }

/* trait entry */
.cx-trait { --tc: #8e98ad; position: relative; padding: 7px 8px 8px; margin-bottom: 6px; border-radius: 8px; background: var(--panel); border: 1px solid var(--line);
  border-left: 3px solid color-mix(in srgb, var(--tc) 70%, transparent); scroll-margin-top: 6px; }
.cx-trait.active { background: linear-gradient(90deg, color-mix(in srgb, var(--tc) 10%, var(--panel)), var(--panel) 70%); }
.cx-th { display: flex; align-items: center; gap: 7px; }
.cx-tn { flex: 1; min-width: 0; display: flex; flex-direction: column; line-height: 1.15; }
.cx-tn b { font-size: 13px; color: var(--tc); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.cx-kind { font-size: 10px; text-transform: uppercase; letter-spacing: 0.8px; color: var(--muted); font-weight: 700; }
.cx-count { font-size: 15px; font-weight: 900; color: var(--muted); font-variant-numeric: tabular-nums; }
.cx-count small { font-size: 11px; font-weight: 700; color: var(--muted); }
.cx-count.on { color: var(--gold); }
.cx-desc { font-size: 12px; color: #c6cddb; margin-top: 5px; line-height: 1.3; }
.cx-note { font-size: 11px; color: var(--muted); margin-top: 3px; padding-left: 8px; border-left: 2px solid var(--line2); }
.cx-bps { display: flex; flex-direction: column; gap: 2px; margin-top: 6px; }
.cx-bp { display: grid; grid-template-columns: 20px 1fr; gap: 6px; align-items: baseline; font-size: 12px; color: var(--muted); padding: 1px 4px 1px 0; border-radius: 4px; }
.cx-bp b { text-align: center; font-size: 11px; border-radius: 4px; background: var(--panel3); color: var(--muted); padding: 0 2px; }
.cx-bp.on { color: var(--text); }
.cx-bp.on b { background: var(--tc); color: #10131a; }
.cx-bp.cur { background: color-mix(in srgb, var(--tc) 14%, transparent); }
.cx-sub { font-size: 10px; text-transform: uppercase; letter-spacing: 0.8px; color: var(--muted); margin-top: 6px; }
.cx-units { display: flex; flex-wrap: wrap; gap: 4px; margin-top: 6px; }
.cx-u { position: relative; padding: 0; border: 0; background: none; border-radius: 6px; line-height: 0; }
.cx-u:hover:not(:disabled) { background: none; }
.cx-u .pt { width: 30px; height: 30px; border: 2px solid #556; border-radius: 6px; }
.cx-u .pt b { font-size: 9px; }
.cx-u:hover .pt { filter: brightness(1.2); box-shadow: 0 0 0 1px var(--text); }
.cx-u.wild .pt { border-style: dashed; border-color: #6a7488 !important; }
.cx-u:not(.owned) .pt img { opacity: 0.88; }
.cx-mark { position: absolute; right: -2px; top: -2px; width: 9px; height: 9px; border-radius: 50%; border: 2px solid #0e121b; }
.cx-mark.board { background: var(--good); }
.cx-mark.own { background: var(--accent); }
.cx-u .cx-stars { position: absolute; left: 0; right: 0; bottom: -1px; font-size: 7px; line-height: 1; color: var(--gold); text-align: center; font-style: normal; text-shadow: 0 0 2px #000, 0 0 2px #000; }
.cx-crest { display: flex; align-items: center; gap: 6px; margin-top: 6px; font-size: 11px; color: var(--muted); }

/* unit card */
.cx-unit { --tc: #9aa4b1; font-size: 12px; }
.cx-uh { display: flex; flex-direction: column; gap: 5px; padding-bottom: 7px; border-bottom: 1px solid var(--line); }
.cx-uname b { font-size: 16px; }
.cx-pills, .cx-tchips { display: flex; flex-wrap: wrap; gap: 4px; }
.cx-tchip { --tc: #8e98ad; display: inline-flex; align-items: center; gap: 4px; padding: 1px 7px 1px 3px; border-radius: 10px; font-size: 11px; font-weight: 700;
  color: var(--tc); background: color-mix(in srgb, var(--tc) 12%, var(--panel2)); border: 1px solid color-mix(in srgb, var(--tc) 45%, transparent); white-space: nowrap; }
button.cx-tchip:hover:not(:disabled) { border-color: var(--tc); background: color-mix(in srgb, var(--tc) 22%, var(--panel2)); }
.cx-attr, .cx-ownline { font-size: 11px; color: var(--muted); }
.cx-ownline { color: var(--good); }
.cx-common, .cx-lstats { display: flex; flex-wrap: wrap; gap: 3px 10px; font-size: 11px; color: var(--muted); padding: 6px 0; }
.cx-common b, .cx-lstats b, .cx-fstats b { color: var(--text); font-weight: 700; }
.cx-form { display: grid; grid-template-columns: 48px 1fr; gap: 8px; padding: 7px 0; border-top: 1px solid var(--line); }
.cx-form .pt.big { width: 48px; height: 48px; border: 2px solid #556; border-radius: 8px; }
.cx-fbody { min-width: 0; display: flex; flex-direction: column; gap: 2px; }
.cx-fname b { font-size: 13px; }
.cx-stars { color: var(--gold); font-size: 10px; letter-spacing: -1px; }
.cx-fname .muted { font-size: 11px; }
.cx-fstats { font-size: 11px; color: var(--muted); }
.cx-abil { font-size: 12px; color: #c6cddb; line-height: 1.3; }
.cx-abil b { color: #d9b8ff; margin-right: 3px; }
.cx-abil.small { font-size: 11px; color: var(--muted); }
.cx-foot { font-size: 11px; color: var(--muted); padding-top: 6px; border-top: 1px solid var(--line); }

/* rules */
.cx-rule p { margin: 4px 0 6px; font-size: 12px; line-height: 1.4; color: #c6cddb; }
.cx-rule p.muted { color: var(--muted); font-size: 11px; }
.cx-rule b { color: var(--text); }
.cx-rule summary { cursor: pointer; font-weight: 800; font-size: 12px; text-transform: uppercase; letter-spacing: 0.8px; color: var(--muted); padding: 6px 2px; list-style: none; }
.cx-rule summary::-webkit-details-marker { display: none; }
.cx-rule summary::before { content: '▸'; display: inline-block; width: 12px; color: var(--accent); }
.cx-rule[open] summary::before { content: '▾'; }
details.cx-rule { border-bottom: 1px solid var(--line); padding-bottom: 2px; }
.cx-rounds { display: flex; flex-direction: column; gap: 4px; margin: 6px 0; }
.cx-rounds > div { display: flex; flex-wrap: wrap; gap: 3px; align-items: center; }
.cx-rl { font-size: 10px; text-transform: uppercase; letter-spacing: 0.6px; color: var(--muted); width: 58px; }
.cx-rd { display: inline-flex; flex-direction: column; padding: 2px 6px; border-radius: 5px; font-size: 11px; background: var(--panel2); border: 1px solid var(--line2); line-height: 1.2; }
.cx-rd b { font-size: 11px; }
.cx-rd.pvp { border-color: #7a3442; } .cx-rd.pve { border-color: #2f6b45; } .cx-rd.carousel { border-color: #7a6420; }
.cx-rd i { font-style: normal; font-size: 10px; color: var(--gold); }
.cx-tbl { border-collapse: collapse; font-size: 11px; margin: 6px 0; font-variant-numeric: tabular-nums; }
.cx-tbl th, .cx-tbl td { padding: 2px 5px; text-align: center; border: 1px solid var(--line); }
.cx-tbl th { color: var(--muted); font-weight: 700; background: var(--panel2); }
.cx-tbl td:first-child { text-align: left; color: var(--muted); }

/* ---------- in-game side panel ---------- */
#cxPanel { position: fixed; z-index: 9; display: flex; flex-direction: column; min-width: 0; background: #0e121b; border-right: 1px solid var(--line2);
  box-shadow: 8px 0 22px rgba(0, 0, 0, 0.5); animation: cxIn 0.14s ease-out; }
@keyframes cxIn { from { transform: translateX(-14px); opacity: 0; } }
.cx-ph { display: flex; align-items: center; gap: 4px; padding: 7px 6px 6px 10px; border-bottom: 1px solid var(--line); background: linear-gradient(#182033, #0e121b); }
.cx-ptitle { font-size: 11px; font-weight: 800; text-transform: uppercase; letter-spacing: 1px; color: var(--gold); margin-right: auto; }
.cx-icon { padding: 2px 6px; font-size: 12px; line-height: 1.2; }
.cx-pbar { padding: 6px 8px; border-bottom: 1px solid var(--line); }
.cx-pbar .cx-seg { display: flex; }
.cx-pbar .cx-seg button { flex: 1 1 auto; padding: 3px 4px; font-size: 11.5px; }
.cx-pback { padding: 6px 8px 0; }
.cx-back { padding: 3px 9px; font-size: 12px; }
.cx-pbody { flex: 1; min-height: 0; overflow-y: auto; overflow-x: hidden; padding: 8px; }
.cx-pbody .cx-desc { font-size: 11.5px; }
#codexBtn.on { border-color: var(--gold); color: var(--gold); }

/* ---------- full-screen codex ---------- */
#cxFull { position: fixed; inset: 0; z-index: 45; display: flex; align-items: center; justify-content: center;
  background: radial-gradient(ellipse at 30% 20%, rgba(30, 60, 110, 0.6), rgba(5, 7, 12, 0.95) 70%); }
.cx-win { position: relative; display: flex; flex-direction: column; width: min(1480px, calc(100vw - 32px)); height: calc(100vh - 32px);
  background: var(--panel); border: 1px solid var(--line2); border-radius: 14px; box-shadow: 0 20px 60px rgba(0,0,0,0.6); overflow: hidden; }
.cx-head { display: flex; align-items: center; gap: 12px; padding: 10px 14px; background: linear-gradient(#182033, var(--panel)); border-bottom: 1px solid var(--line); flex-wrap: wrap; }
.cx-head h2 { margin: 0; font-size: 22px; background: linear-gradient(#ffe08a, #ffb347); -webkit-background-clip: text; background-clip: text; color: transparent; }
.cx-tabs { display: flex; gap: 4px; flex-wrap: wrap; flex: 1; min-width: 0; }
.cx-tabs button { padding: 5px 11px; }
.cx-tabs button.on { background: var(--panel3); border-color: var(--gold); color: #fff; }
.cx-tabs button span { color: var(--muted); font-size: 12px; }
.cx-search { width: 200px; font: inherit; color: var(--text); background: var(--bg); border: 1px solid var(--line2); border-radius: 6px; padding: 6px 10px; outline: none; }
.cx-search:focus { border-color: var(--accent); }
.cx-filters { padding: 8px 14px; border-bottom: 1px solid var(--line); }
.cx-body { flex: 1; min-height: 0; overflow-y: auto; overflow-x: hidden; padding: 14px 16px 24px; }
.cx-body h3 { margin: 4px 0 10px; font-size: 14px; text-transform: uppercase; letter-spacing: 1px; display: flex; gap: 10px; align-items: baseline; flex-wrap: wrap; }
.cx-body h3 .muted { text-transform: none; letter-spacing: 0; font-weight: 400; }
.cx-group + .cx-group { margin-top: 20px; }
.cx-intro { margin: 0 0 14px; color: var(--muted); font-size: 13px; }
.cx-tgrid { display: grid; grid-template-columns: repeat(auto-fill, minmax(290px, 1fr)); gap: 10px; align-items: start; }
.cx-tgrid .cx-trait { margin: 0; }
.cx-lgrid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 10px; }
.cx-line { --tc: #9aa4b1; display: flex; flex-direction: column; gap: 6px; padding: 9px 10px; border-radius: 10px; background: var(--panel2); cursor: pointer;
  border: 1px solid var(--line); border-top: 3px solid var(--tc); }
.cx-line:hover { border-color: var(--line2); border-top-color: var(--tc); background: var(--panel3); }
.cx-line.wild { border-top-style: dashed; }
.cx-lh { display: flex; align-items: center; gap: 6px; }
.cx-lh b { font-size: 14px; margin-right: auto; }
.cx-forms { display: grid; grid-template-columns: repeat(3, 1fr); gap: 4px; }
.cx-lf { display: flex; flex-direction: column; align-items: center; gap: 3px; text-align: center; min-width: 0; }
.cx-lf .pt { width: 54px; height: 54px; border: 2px solid #556; border-radius: 8px; }
.cx-lf div { display: flex; flex-direction: column; min-width: 0; max-width: 100%; line-height: 1.15; }
.cx-lf div b { font-size: 11px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.cx-lf div span { font-size: 10px; color: #c9a6ff; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.cx-cgrid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 10px; }
.cx-card { --tc: var(--line2); display: flex; flex-direction: column; gap: 5px; padding: 9px 10px; border-radius: 10px; background: var(--panel2); border: 1px solid var(--line); scroll-margin: 40px; }
.cx-card[data-cx-aug] { border-left: 3px solid var(--tc); }
.cx-ch { display: flex; align-items: center; gap: 8px; }
.cx-ch b { font-size: 13px; }
.cx-stat { font-size: 11px; color: var(--gold); }
.cx-meta { font-size: 11px; color: var(--muted); }
.cx-cost { margin-left: auto; font-size: 11px; font-weight: 700; color: var(--gold); white-space: nowrap; }
.cx-gem { width: 12px; height: 12px; transform: rotate(45deg); border-radius: 2px; background: var(--tc); box-shadow: 0 0 8px var(--tc); flex: none; }
.cx-wisp { width: 14px; height: 14px; border-radius: 50%; flex: none; background: radial-gradient(circle at 35% 35%, #fff, #7fe3ff 40%, #2a7bd8); box-shadow: 0 0 8px #4fc8ff; }
.cx-recipe { display: flex; align-items: center; gap: 4px; font-size: 11px; color: var(--muted); }
.cx-recipe i { font-style: normal; }
.cx-recipe span { margin-left: 4px; }
.cx-gridwrap { max-width: 100%; overflow-x: auto; }
.cx-rgrid { border-collapse: separate; border-spacing: 3px; }
.cx-rgrid th { padding: 0; }
.cx-rgrid td { padding: 0; width: 34px; height: 34px; text-align: center; background: var(--panel2); border-radius: 6px; }
.cx-rgrid td button { padding: 2px; border: 1px solid transparent; background: none; line-height: 0; border-radius: 6px; }
.cx-rgrid td button:hover:not(:disabled) { border-color: var(--gold); background: var(--panel3); }
.cx-rgrid td.dim { opacity: 0.2; }
.cx-rules { display: grid; grid-template-columns: repeat(auto-fill, minmax(360px, 1fr)); gap: 12px; align-items: start; }
.cx-rules section.cx-rule { padding: 10px 12px; border-radius: 10px; background: var(--panel2); border: 1px solid var(--line); }
.cx-rules section.cx-rule h3 { margin: 0 0 6px; font-size: 13px; color: var(--gold); }
.cx-rules section.cx-rule p { font-size: 13px; }
.cx-rules.compact { display: block; }
.cx-rules.compact .cx-rl { width: 100%; margin-top: 2px; }
.cx-rules.compact .cx-tbl th, .cx-rules.compact .cx-tbl td { padding: 2px 3px; }
.cx-modal { position: absolute; inset: 0; z-index: 5; display: flex; align-items: center; justify-content: center; background: rgba(5, 7, 12, 0.7); }
.cx-mbox { position: relative; width: min(460px, calc(100vw - 32px)); max-height: calc(100vh - 60px); overflow-y: auto; padding: 14px 16px; border-radius: 12px;
  background: var(--panel); border: 1px solid var(--line2); box-shadow: 0 20px 50px rgba(0,0,0,0.6); }
.cx-mclose { position: absolute; right: 10px; top: 10px; padding: 2px 8px; }
.cx-mbox .cx-form .pt.big { width: 64px; height: 64px; }
.cx-mbox .cx-form { grid-template-columns: 64px 1fr; }
@media (max-width: 1360px), (max-height: 800px) {
  .cx-search { width: 150px; }
  .cx-tabs button { padding: 4px 8px; font-size: 13px; }
}
`;
const tag = document.createElement('style');
tag.id = 'dt-codex-style';
tag.textContent = css;
document.head.appendChild(tag);
