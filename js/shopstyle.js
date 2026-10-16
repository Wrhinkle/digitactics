// Styles for js/shop.js (Store, Locker, Digi-Eggs, Bits breakdown). Injected at import so the packaged
// builds (desktop/build.py, tools/publish-web.cjs) need no extra file.
const css = `
.bit { display: inline-block; width: 11px; height: 11px; margin: 0 2px 0 1px; vertical-align: -1px; transform: rotate(45deg); border-radius: 2px;
  background: linear-gradient(135deg, #b8fbff, #2f9bff 60%, #1b5fd0); box-shadow: 0 0 6px rgba(79, 200, 255, 0.7); }
#dtStore { --r-common: #9aa6bb; --r-uncommon: #5fd36a; --r-rare: #4fb3ff; --r-epic: #c77dff; --r-legendary: #ffb347;
  position: fixed; inset: 0; z-index: 50; display: flex; align-items: center; justify-content: center;
  background: radial-gradient(ellipse at 30% 20%, rgba(30, 60, 110, 0.6), rgba(5, 7, 12, 0.95) 70%); }
#dtStore .r-common { --rc: var(--r-common); } #dtStore .r-uncommon { --rc: var(--r-uncommon); } #dtStore .r-rare { --rc: var(--r-rare); }
#dtStore .r-epic { --rc: var(--r-epic); } #dtStore .r-legendary { --rc: var(--r-legendary); }
.dts-win { position: relative; display: flex; flex-direction: column; width: min(1480px, calc(100vw - 32px)); height: calc(100vh - 32px);
  background: var(--panel); border: 1px solid var(--line2); border-radius: 14px; box-shadow: 0 20px 60px rgba(0,0,0,0.6); overflow: hidden; }
.dts-head { display: flex; align-items: center; gap: 14px; padding: 12px 16px; background: linear-gradient(#182033, var(--panel)); border-bottom: 1px solid var(--line); }
.dts-modes { display: flex; gap: 4px; }
.dts-modes button { font-size: 17px; font-weight: 800; padding: 6px 16px; }
.dts-modes button.on, .dts-tabs button.on { background: var(--panel3); border-color: var(--gold); color: #fff; }
.dts-missions { flex: 1; display: flex; flex-wrap: wrap; gap: 6px 10px; align-items: center; font-size: 12px; min-width: 0; }
.dts-mis { padding: 3px 8px; border-radius: 12px; background: var(--panel2); border: 1px solid var(--line2); }
.dts-mis i { font-style: normal; color: var(--accent); }
.dts-mis.done { border-color: var(--good); color: var(--muted); }
.dts-mis.done i { color: var(--good); }
.dts-wallet { position: relative; display: flex; align-items: center; gap: 8px; padding: 4px 6px 4px 12px; border-radius: 8px; background: var(--bg); border: 1px solid var(--line2); }
.dts-wallet > span { font-size: 18px; font-weight: 800; }
.dts-wallet button { padding: 3px 8px; font-size: 12px; }
.dts-hist { position: absolute; right: 0; top: calc(100% + 6px); z-index: 3; width: 320px; max-height: 360px; overflow-y: auto; padding: 10px 12px;
  background: var(--panel2); border: 1px solid var(--line2); border-radius: 10px; box-shadow: 0 10px 30px rgba(0,0,0,0.6); }
.dts-hh { font-weight: 700; margin-bottom: 6px; }
.dts-hrow { display: grid; grid-template-columns: 1fr auto auto; gap: 8px; padding: 3px 0; border-bottom: 1px solid var(--line); font-size: 12px; }
.dts-hrow b { color: var(--accent); } .dts-hrow i { color: var(--muted); font-style: normal; }
.dts-tabs { display: flex; gap: 6px; padding: 10px 16px; border-bottom: 1px solid var(--line); flex-wrap: wrap; }
.dts-tabs button span { color: var(--muted); font-size: 12px; }
.dts-body { flex: 1; overflow-y: auto; padding: 14px 16px 24px; }
.dts-body h3 { margin: 6px 0 10px; font-size: 14px; text-transform: uppercase; letter-spacing: 1px; display: flex; gap: 10px; align-items: baseline; flex-wrap: wrap; }
.dts-body h3 .muted { text-transform: none; letter-spacing: 0; font-weight: 400; }
.dts-body h3 button { text-transform: none; letter-spacing: 0; font-size: 12px; padding: 2px 8px; }
.dts-body section + section { margin-top: 18px; }
.dts-feat { padding: 10px 12px 14px; border-radius: 12px; background: linear-gradient(135deg, rgba(255, 207, 74, 0.08), rgba(79, 179, 255, 0.05)); border: 1px solid rgba(255, 207, 74, 0.3); }
.dts-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(176px, 1fr)); gap: 12px; }
.dts-card { position: relative; display: flex; flex-direction: column; gap: 4px; padding: 10px; border-radius: 10px; background: var(--panel2);
  border: 2px solid color-mix(in srgb, var(--rc) 55%, transparent); box-shadow: inset 0 0 18px color-mix(in srgb, var(--rc) 14%, transparent); }
.dts-card.eq { border-color: var(--gold); }
.dts-card:hover { border-color: var(--rc); }
.dts-prev { position: relative; height: 120px; border-radius: 8px; overflow: hidden; display: flex; align-items: center; justify-content: center;
  background: radial-gradient(circle at 50% 35%, color-mix(in srgb, var(--rc, #667) 30%, #1c2333), #0d111a 85%); }
.dts-prev > img { max-width: 100%; max-height: 100%; object-fit: contain; }
.dts-prev canvas { max-width: 100%; max-height: 100%; }
.dts-prev.lg { height: 200px; width: 260px; }
.dts-prev.sm { height: 34px; width: 44px; border-radius: 6px; flex: none; }
.dts-emo { font-size: 54px; line-height: 1; }
.dts-prev.sm .dts-emo { font-size: 22px; }
.dts-art { font-size: 34px; font-weight: 800; letter-spacing: 1px; color: color-mix(in srgb, var(--rc, #fff) 70%, #fff); text-shadow: 0 2px 6px #000; }
.dts-prev.sm .dts-art { font-size: 13px; }
.dts-name { font-weight: 700; }
.dts-rar { font-size: 11px; text-transform: uppercase; letter-spacing: 1px; color: var(--rc); }
.dts-desc { font-size: 12px; color: var(--muted); }
.dts-act { margin-top: auto; padding-top: 4px; display: flex; }
.dts-act button { flex: 1; }
.dts-buy { font-weight: 700; }
.dts-buy s { color: var(--muted); font-weight: 400; margin-right: 2px; }
.dts-buy.poor { opacity: 0.6; }
.dts-eq { border-color: var(--gold) !important; color: var(--gold) !important; opacity: 1 !important; }
.dts-empty { padding: 30px; text-align: center; color: var(--muted); border: 1px dashed var(--line2); border-radius: 10px; }
.dts-empty.small { padding: 12px; font-size: 12px; }
.dts-note { margin: 0 0 12px; color: var(--muted); font-size: 13px; }
.dts-msg { position: absolute; left: 50%; bottom: 14px; transform: translateX(-50%); padding: 6px 14px; border-radius: 8px; background: rgba(11, 14, 21, 0.92);
  border: 1px solid var(--line2); color: var(--gold); opacity: 0; transition: opacity 0.2s; pointer-events: none; font-weight: 600; }
.dts-msg.on { opacity: 1; } .dts-msg.bad { color: var(--bad); }

/* eggs */
.dts-eggs { display: grid; grid-template-columns: repeat(auto-fill, minmax(210px, 1fr)); gap: 12px; }
.dts-egg-card { display: flex; flex-direction: column; gap: 6px; padding: 12px; border-radius: 10px; background: var(--panel2); border: 1px solid var(--line2); }
.dts-egg-card .dts-buy { margin-top: auto; }
.dts-egg-art { height: 110px; display: flex; align-items: center; justify-content: center; }
.dts-egg-art.lg { height: 220px; }
.dts-egg { --e1: #fff4d6; --e2: #f0c060; position: relative; width: 62px; height: 80px; border-radius: 50% 50% 50% 50% / 60% 60% 40% 40%;
  background: radial-gradient(circle at 35% 30%, #fff 0 8%, transparent 9%), radial-gradient(circle at 62% 62%, var(--e2) 0 12%, transparent 13%),
    radial-gradient(circle at 30% 70%, var(--e2) 0 9%, transparent 10%), radial-gradient(circle at 50% 40%, var(--e1), color-mix(in srgb, var(--e1) 60%, #555) 90%);
  box-shadow: 0 8px 16px rgba(0,0,0,0.5), inset -6px -8px 14px rgba(0,0,0,0.25); }
.dts-egg-art.lg .dts-egg { width: 120px; height: 154px; }
.dts-egg.e-mixed { --e1: #f6f0ff; --e2: #c77dff; } .dts-egg.e-legend { --e1: #fff4d6; --e2: #ffb347; }
.dts-egg.e-emote { --e1: #fff9c9; --e2: #ffd84a; } .dts-egg.e-board { --e1: #dcffe9; --e2: #3fbf72; }
.dts-egg.e-killscreen { --e1: #ffe0e4; --e2: #ff5a6a; } .dts-egg.e-sound { --e1: #dff3ff; --e2: #4fb3ff; }
.dts-egg i { position: absolute; display: none; background: #2a1d10; height: 3px; border-radius: 2px; transform-origin: left center; }
.dts-egg .c1 { left: 18%; top: 44%; width: 34%; transform: rotate(-24deg); }
.dts-egg .c2 { left: 46%; top: 36%; width: 30%; transform: rotate(28deg); }
.dts-egg .c3 { left: 40%; top: 52%; width: 26%; transform: rotate(70deg); }
.dts-egg.crack i { display: block; }
.dts-egg.wobble { animation: dtsWobble 0.42s ease-in-out infinite; }
.dts-egg.crack { animation: dtsShake 0.12s linear infinite; box-shadow: 0 0 34px 6px var(--hc, #fff), inset -6px -8px 14px rgba(0,0,0,0.25); }
.dts-egg.burst { animation: dtsBurst 0.38s ease-out forwards; }
.dts-hatch.r-common { --hc: rgba(200, 210, 230, 0.6); } .dts-hatch.r-uncommon { --hc: rgba(95, 211, 106, 0.8); } .dts-hatch.r-rare { --hc: rgba(79, 179, 255, 0.85); }
.dts-hatch.r-epic { --hc: rgba(199, 125, 255, 0.9); } .dts-hatch.r-legendary { --hc: rgba(255, 179, 71, 1); }
.dts-egg-art.gone { display: none; }
@keyframes dtsWobble { 0%, 100% { transform: rotate(0); } 25% { transform: rotate(-12deg); } 75% { transform: rotate(12deg); } }
@keyframes dtsShake { 0% { transform: translate(-2px, 0); } 50% { transform: translate(2px, -1px); } 100% { transform: translate(-1px, 1px); } }
@keyframes dtsBurst { to { transform: scale(1.7); opacity: 0; filter: brightness(3); } }
.dts-reveal { animation: dtsPop 0.4s ease-out; }
@keyframes dtsPop { from { transform: scale(0.6); opacity: 0; } to { transform: scale(1); opacity: 1; } }
.dts-hatch { cursor: pointer; min-width: 300px; }
.dts-hatch.done { cursor: default; }
.dts-hatch .dts-mprev { box-shadow: 0 0 40px 4px var(--hc); }
.dts-skip { margin: 4px 0 0; font-size: 12px; }
.dts-odds { width: 100%; border-collapse: collapse; font-size: 12px; }
.dts-odds td { padding: 2px 4px; border-bottom: 1px solid var(--line); }
.dts-odds td:first-child { color: var(--rc); text-transform: capitalize; font-weight: 700; }
.dts-odds td:nth-child(2) { text-align: right; font-weight: 700; }
.dts-odds td:last-child { text-align: right; }
.dts-eobadge { position: absolute; top: 14px; right: 14px; padding: 2px 7px; border-radius: 10px; font-size: 10px; font-weight: 800; letter-spacing: 0.5px;
  text-transform: uppercase; color: #1a1206; background: linear-gradient(135deg, #ffe7a8, #ffb347); box-shadow: 0 2px 6px rgba(0,0,0,0.5); }
.dts-eggonly { font-weight: 700; color: var(--gold); border-color: color-mix(in srgb, var(--gold) 55%, transparent); }
.dts-eolist { display: flex; flex-wrap: wrap; gap: 4px; font-size: 11px; }
.dts-eoh { width: 100%; font-size: 10px; text-transform: uppercase; letter-spacing: 1px; color: var(--muted); }
.dts-eolist span { padding: 1px 6px; border-radius: 8px; border: 1px solid color-mix(in srgb, var(--rc) 60%, transparent); color: var(--rc); }
.dts-eolist span i { font-style: normal; color: var(--muted); }
.dts-eolist span.own { opacity: 0.55; }

/* modal */
.dts-modal { position: absolute; inset: 0; z-index: 5; display: flex; align-items: center; justify-content: center; background: rgba(5, 7, 12, 0.7); }
.dts-mbox { padding: 20px 26px; border-radius: 14px; background: var(--panel); border: 1px solid var(--line2); text-align: center; box-shadow: 0 20px 60px rgba(0,0,0,0.7); max-width: calc(100vw - 48px); }
.dts-mbox h2 { margin: 0 0 10px; font-size: 22px; }
.dts-mbox p { margin: 8px 0; }
.dts-mprev { display: inline-block; padding: 3px; border-radius: 10px; border: 2px solid var(--rc); }
.dts-mbtns { display: flex; gap: 8px; justify-content: center; margin-top: 12px; flex-wrap: wrap; }

/* locker wheel */
.dts-wheelbox { display: flex; gap: 24px; align-items: center; flex-wrap: wrap; padding: 6px 0 4px; }
.dts-wheel { position: relative; width: 380px; height: 290px; flex: none; }
.dts-wheel::before { content: ''; position: absolute; left: 50%; top: 50%; width: 250px; height: 206px; transform: translate(-50%, -50%); border-radius: 50%; border: 2px dashed var(--line2); }
.dts-slot { position: absolute; transform: translate(-50%, -50%); width: 104px; height: 66px; padding: 4px; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 2px; white-space: normal; }
.dts-slot.on { border-color: var(--gold); box-shadow: 0 0 0 2px var(--gold); background: var(--panel3); }
.dts-slotn { position: absolute; left: 5px; top: 3px; font-size: 10px; font-weight: 800; color: var(--muted); }
.dts-slotname { font-size: 11px; line-height: 1.1; max-width: 96px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.dts-hub { position: absolute; left: 50%; top: 50%; transform: translate(-50%, -50%); font-size: 12px; color: var(--muted); text-transform: uppercase; letter-spacing: 1px; text-align: center; width: 70px; }
.dts-wheelhelp { max-width: 360px; color: var(--muted); display: flex; flex-direction: column; gap: 8px; }
.dts-wheelhelp b { color: var(--text); }

/* game-over breakdown */
.dts-award { margin: 12px auto 16px; padding: 10px 14px; max-width: 360px; text-align: left; border-radius: 10px; background: var(--panel2); border: 1px solid var(--line2); }
.dts-award-h { font-weight: 800; margin-bottom: 6px; }
.dts-aline { display: flex; justify-content: space-between; gap: 12px; padding: 2px 0; font-size: 13px; transition: opacity 0.2s, transform 0.2s; }
.dts-aline b { color: var(--accent); font-variant-numeric: tabular-nums; }
.dts-aline b::before { content: '+'; }
.dts-aline.tot { margin-top: 4px; padding-top: 6px; border-top: 1px solid var(--line); font-size: 16px; font-weight: 800; }
.dts-aline.tot b::before { content: ''; }
.dts-aline.pending { opacity: 0; transform: translateY(4px); }
.dts-award-f { display: flex; justify-content: space-between; align-items: center; margin-top: 8px; }
.dts-award-f button { padding: 3px 10px; font-size: 12px; }

@media (max-width: 1360px), (max-height: 820px) {
  .dts-win { width: calc(100vw - 20px); height: calc(100vh - 20px); }
  .dts-head { padding: 8px 12px; gap: 10px; }
  .dts-modes button { font-size: 15px; padding: 5px 12px; }
  .dts-grid { grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 10px; }
  .dts-prev { height: 100px; }
  .dts-tabs { padding: 8px 12px; }
  .dts-body { padding: 10px 12px 20px; }
  .dts-egg-art.lg { height: 180px; }
  .dts-prev.lg { height: 170px; width: 220px; }
}
`;
const tag = document.createElement('style');
tag.id = 'dt-shop-style';
tag.textContent = css;
document.head.appendChild(tag);
