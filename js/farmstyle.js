// Styles for the Digi-Farm (js/farm-ui.js) and its training mini-games (js/farm-games.js).
const css = `
#dtFarm { position: fixed; inset: 0; z-index: 58; display: flex; align-items: center; justify-content: center;
  background: radial-gradient(ellipse at 50% 30%, rgba(60, 120, 90, 0.5), rgba(4, 8, 8, 0.94) 70%); }
#dtFarm.hidden { display: none; }
.df-win { position: relative; display: flex; flex-direction: column; width: min(1560px, calc(100vw - 16px)); height: min(900px, calc(100vh - 16px));
  background: var(--panel); border: 1px solid var(--line2); border-radius: 14px; box-shadow: 0 20px 60px rgba(0,0,0,0.7); overflow: hidden; }
.df-head { flex: none; display: flex; align-items: center; gap: 10px; padding: 8px 12px; background: linear-gradient(#1b2a26, var(--panel)); border-bottom: 1px solid var(--line); min-width: 0; }
.df-head h2 { margin: 0; font-size: 19px; white-space: nowrap; background: linear-gradient(90deg, #b6f58a, #ffe08a); -webkit-background-clip: text; background-clip: text; color: transparent; }
.df-theme { font-size: 12px; color: var(--muted); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; min-width: 0; }
.df-head .sp { flex: 1; min-width: 0; }
.df-pill { display: inline-flex; align-items: center; gap: 4px; font-size: 12px; padding: 3px 8px; border-radius: 9px; background: var(--bg); border: 1px solid var(--line2); white-space: nowrap; font-variant-numeric: tabular-nums; }
.df-pill b { color: var(--text); }
.df-pill.capped { border-color: #ffd36a88; color: #ffd36a; }
.df-head button { padding: 4px 10px; font-size: 13px; white-space: nowrap; }
.df-body { flex: 1; min-height: 0; display: grid; grid-template-columns: 1fr 350px; }
.df-scene { position: relative; min-width: 0; min-height: 0; overflow: hidden; background: linear-gradient(var(--sky1, #8fd3ff), var(--sky2, #d8f1ff)); }
.df-scene canvas { position: absolute; inset: 0; width: 100%; height: 100%; display: block; cursor: pointer; }
.df-night { position: absolute; inset: 0; pointer-events: none; opacity: 0; transition: opacity 0.8s;
  background: radial-gradient(ellipse at 50% 70%, rgba(20, 30, 80, 0.15), rgba(5, 8, 30, 0.6)); }
.df-scene.night .df-night { opacity: 1; }
.df-stars { position: absolute; left: 0; right: 0; top: 0; height: 40%; pointer-events: none; opacity: 0; transition: opacity 0.8s;
  background-image: radial-gradient(1.5px 1.5px at 10% 20%, #fff, transparent), radial-gradient(1px 1px at 30% 60%, #fff, transparent), radial-gradient(1.5px 1.5px at 55% 25%, #fff, transparent),
    radial-gradient(1px 1px at 70% 50%, #fff, transparent), radial-gradient(1.5px 1.5px at 85% 15%, #fff, transparent), radial-gradient(1px 1px at 45% 75%, #fff, transparent),
    radial-gradient(1px 1px at 20% 80%, #fff, transparent), radial-gradient(1.5px 1.5px at 92% 65%, #fff, transparent); }
.df-scene.night .df-stars { opacity: 0.9; }
.df-icons { position: absolute; inset: 0; pointer-events: none; overflow: hidden; }
.df-bub { position: absolute; transform: translate(-50%, -100%); display: flex; flex-direction: column; align-items: center; gap: 1px; transition: opacity 0.2s; }
.df-bub .ic { min-width: 26px; height: 26px; padding: 0 4px; border-radius: 13px; display: flex; align-items: center; justify-content: center; font-size: 16px; line-height: 1;
  background: rgba(255,255,255,0.92); color: #333; box-shadow: 0 2px 6px rgba(0,0,0,0.35); animation: dfBob 1.6s ease-in-out infinite; }
.df-bub .ic:empty { display: none; }
.df-bub .ic.note { color: #e2558f; font-weight: 800; }
.df-bub .nm { font-size: 11px; font-weight: 800; color: #fff; text-shadow: 0 1px 2px #000, 0 0 4px #000; white-space: nowrap; opacity: 0; transition: opacity 0.2s; }
.df-bub.sel .nm, .df-bub.hov .nm { opacity: 1; }
.df-pop { position: absolute; transform: translate(-50%, -100%); font-weight: 800; font-size: 15px; pointer-events: none; white-space: nowrap; color: #fff;
  text-shadow: 0 1px 3px #000, 0 0 6px rgba(0,0,0,0.6); animation: dfRise 1.4s ease-out forwards; }
.df-pop.gold { color: #ffd36a; font-size: 13px; }
@keyframes dfBob { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-3px); } }
@keyframes dfRise { 0% { opacity: 0; margin-top: 4px; } 15% { opacity: 1; } 80% { opacity: 1; } 100% { opacity: 0; margin-top: -34px; } }
.df-chips { position: absolute; left: 8px; top: 8px; right: 8px; display: flex; gap: 5px; flex-wrap: wrap; pointer-events: none; }
.df-chip { pointer-events: auto; position: relative; width: 42px; height: 42px; padding: 0; border-radius: 9px; overflow: visible; background: rgba(20, 26, 38, 0.75); }
.df-chip .pt { width: 100%; height: 100%; border-radius: 8px; background: transparent; border: 0; overflow: hidden; }
.df-chip .pt img { object-fit: contain !important; }
.df-chip.on { border-color: var(--gold); box-shadow: 0 0 0 2px var(--gold); }
.df-chip i { position: absolute; right: -5px; top: -6px; font-style: normal; font-size: 12px; line-height: 1; padding: 2px; border-radius: 8px; background: #fff; box-shadow: 0 1px 3px rgba(0,0,0,0.4); }
.df-chip i:empty { display: none; }
.df-toast { position: absolute; left: 50%; top: 58px; transform: translateX(-50%); max-width: 80%; padding: 6px 14px; border-radius: 10px; background: rgba(12, 16, 24, 0.86);
  border: 1px solid var(--line2); font-size: 13px; text-align: center; opacity: 0; transition: opacity 0.25s; pointer-events: none; }
.df-toast.on { opacity: 1; }
.df-bar { position: absolute; left: 50%; bottom: 10px; transform: translateX(-50%); display: flex; gap: 6px; padding: 6px; border-radius: 14px;
  background: rgba(14, 18, 28, 0.82); border: 1px solid var(--line2); box-shadow: 0 6px 20px rgba(0,0,0,0.45); max-width: calc(100% - 16px); }
.df-act { display: flex; flex-direction: column; align-items: center; gap: 1px; width: 64px; padding: 5px 2px 4px; font-size: 11px; font-weight: 700; border-radius: 10px; }
.df-act span { font-size: 20px; line-height: 1.1; }
.df-act.on { border-color: #ffd36a; color: #ffd36a; }
.df-act.alert { border-color: #ff8a5a; box-shadow: 0 0 10px rgba(255, 138, 90, 0.5); }
.df-menu { position: absolute; bottom: 82px; left: 50%; transform: translateX(-50%); min-width: 300px; max-width: calc(100% - 20px); padding: 8px; border-radius: 12px;
  background: rgba(16, 21, 32, 0.96); border: 1px solid var(--line2); box-shadow: 0 10px 30px rgba(0,0,0,0.6); display: flex; flex-direction: column; gap: 5px; }
.df-menu.hidden { display: none; }
.df-menu h4 { margin: 0 2px 2px; font-size: 12px; text-transform: uppercase; letter-spacing: 1px; color: var(--muted); }
.df-mi { display: grid; grid-template-columns: 30px 1fr auto; align-items: center; gap: 8px; text-align: left; padding: 6px 9px; }
.df-mi .e { font-size: 22px; text-align: center; }
.df-mi b { display: block; font-size: 13px; }
.df-mi small { display: block; font-size: 11px; color: var(--muted); line-height: 1.25; }
.df-mi .c { font-size: 12px; font-weight: 800; white-space: nowrap; }
.df-card { min-width: 0; min-height: 0; overflow-y: auto; overflow-x: hidden; padding: 10px 12px; display: flex; flex-direction: column; gap: 8px; border-left: 1px solid var(--line);
  background: linear-gradient(180deg, #171f2d, var(--panel)); scrollbar-width: thin; }
.df-who { display: flex; align-items: center; gap: 10px; }
.df-who .pt { width: 56px; height: 56px; border-radius: 12px; flex: none; }
.df-who .pt img { object-fit: contain !important; }
.df-who .t { min-width: 0; flex: 1; }
.df-who .t b { display: block; font-size: 17px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.df-who .t span { display: block; font-size: 12px; color: var(--muted); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.df-mood { font-size: 12px; font-weight: 700; padding: 2px 8px; border-radius: 9px; background: var(--bg); border: 1px solid var(--line2); white-space: nowrap; }
.df-box { padding: 8px 10px; border-radius: 10px; background: var(--panel2); border: 1px solid var(--line2); }
.df-box h3 { margin: 0 0 5px; font-size: 11px; text-transform: uppercase; letter-spacing: 1px; color: var(--muted); display: flex; justify-content: space-between; gap: 6px; }
.df-box h3 span { text-transform: none; letter-spacing: 0; font-weight: 400; }
.df-stat { display: grid; grid-template-columns: 18px 74px 1fr 30px; align-items: center; gap: 6px; font-size: 12px; padding: 1px 0; }
.df-stat .e { text-align: center; }
.df-stat .n { text-align: right; color: var(--muted); font-variant-numeric: tabular-nums; }
.df-meter { height: 9px; border-radius: 5px; background: var(--bg); border: 1px solid var(--line); overflow: hidden; }
.df-meter i { display: block; height: 100%; border-radius: 5px; transition: width 0.4s; }
.df-meter.low i { animation: dfBlink 1s ease-in-out infinite; }
@keyframes dfBlink { 50% { opacity: 0.45; } }
.df-tags { display: flex; flex-wrap: wrap; gap: 4px; }
.df-tag { font-size: 11px; padding: 2px 7px; border-radius: 9px; background: var(--bg); border: 1px solid var(--line2); white-space: nowrap; }
.df-tag.bad { color: #ff9a7a; border-color: #ff9a7a66; }
.df-tag.good { color: #9fe07a; border-color: #9fe07a66; }
.df-train { display: grid; grid-template-columns: 1fr 1fr; gap: 4px 10px; }
.df-train div { display: grid; grid-template-columns: 1fr auto; gap: 2px 6px; font-size: 12px; align-items: center; }
.df-train .df-meter { grid-column: 1 / -1; height: 6px; }
.df-train b { font-variant-numeric: tabular-nums; }
.df-odd { display: grid; grid-template-columns: 26px 1fr 64px 34px; align-items: center; gap: 6px; font-size: 12px; padding: 1px 0; }
.df-odd .pt { width: 26px; height: 26px; border-radius: 6px; }
.df-odd .pt img { object-fit: contain !important; }
.df-odd.unk .pt img { filter: brightness(0) drop-shadow(0 0 1px rgba(120, 170, 255, 0.9)); }
.df-odd .nm { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.df-odd .tg { font-size: 10px; padding: 1px 4px; border-radius: 8px; border: 1px solid var(--line2); text-align: center; white-space: nowrap; }
.df-odd .tg.bond { color: #ff8fb0; } .df-odd .tg.battle { color: #ff9a5a; } .df-odd .tg.food { color: #ffd36a; } .df-odd .tg.wild { color: #9fe07a; }
.df-odd .pc { text-align: right; font-weight: 800; font-variant-numeric: tabular-nums; }
.df-push { display: grid; grid-template-columns: 62px 1fr 34px; gap: 2px 6px; font-size: 11px; align-items: center; }
.df-push .k { font-weight: 800; }
.df-push .k.bond { color: #ff8fb0; } .df-push .k.battle { color: #ff9a5a; } .df-push .k.food { color: #ffd36a; } .df-push .k.wild { color: #9fe07a; }
.df-push .v { text-align: right; font-weight: 800; font-variant-numeric: tabular-nums; }
.df-push span { color: var(--muted); line-height: 1.25; }
.df-evobtn { width: 100%; margin-top: 6px; padding: 4px 8px; font-size: 12px; }
.df-modal { position: absolute; inset: 0; z-index: 5; display: flex; align-items: center; justify-content: center; background: rgba(4, 6, 10, 0.6); }
.df-modal.hidden { display: none; }
.df-mbox { position: relative; width: min(760px, calc(100% - 24px)); max-height: calc(100% - 24px); overflow: auto; padding: 14px 16px; border-radius: 14px;
  background: var(--panel); border: 1px solid var(--line2); box-shadow: 0 20px 50px rgba(0,0,0,0.7); }
.df-mbox h3 { margin: 0 0 8px; font-size: 17px; }
.df-mbox .x { position: absolute; right: 10px; top: 10px; padding: 3px 10px; }
.df-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(108px, 1fr)); gap: 8px; margin-bottom: 10px; }
.df-slot { display: flex; flex-direction: column; align-items: center; gap: 3px; padding: 6px; border-radius: 10px; background: var(--panel2); border: 1px solid var(--line2); font-size: 12px; text-align: center; }
.df-slot .pt { width: 58px; height: 58px; border-radius: 10px; }
.df-slot .pt img { object-fit: contain !important; }
.df-slot b { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 100%; }
.df-slot button { padding: 2px 8px; font-size: 11px; width: 100%; }
.df-slot.empty { opacity: 0.5; justify-content: center; min-height: 110px; border-style: dashed; }
.df-themes { display: grid; grid-template-columns: repeat(auto-fill, minmax(210px, 1fr)); gap: 8px; }
.df-th { display: flex; flex-direction: column; gap: 4px; padding: 8px; border-radius: 10px; background: var(--panel2); border: 1px solid var(--line2); font-size: 12px; }
.df-th.cur { border-color: var(--gold); box-shadow: 0 0 0 1px var(--gold) inset; }
.df-th .sw { height: 46px; border-radius: 8px; display: flex; align-items: flex-end; justify-content: flex-end; padding: 4px 6px; font-size: 22px; }
.df-th b { font-size: 13px; }
.df-th small { color: var(--muted); line-height: 1.25; }
/* mini-games */
.dg { display: flex; flex-direction: column; gap: 10px; user-select: none; }
.dg-top { display: flex; align-items: baseline; gap: 10px; padding-right: 70px; }
.dg-top h3 { margin: 0; }
.dg-top span { color: var(--muted); font-size: 12px; }
.dg-help { font-size: 13px; color: var(--muted); min-height: 18px; }
.dg-time { height: 6px; border-radius: 3px; background: var(--bg); overflow: hidden; }
.dg-time i { display: block; height: 100%; background: linear-gradient(90deg, #8ff0ff, #2f9bff); }
.dg-stage { position: relative; height: 280px; border-radius: 12px; overflow: hidden; border: 1px solid var(--line2);
  background: radial-gradient(circle at 50% 60%, #2a3b5c, #0c1220 80%); cursor: pointer; }
.dg-big { position: absolute; left: 0; right: 0; top: 18px; text-align: center; font-size: 30px; font-weight: 900; color: #fff; text-shadow: 0 3px 8px #000; pointer-events: none; }
.dg-score { display: flex; justify-content: space-between; font-size: 13px; }
.dg-sw-track { position: absolute; left: 8%; right: 8%; top: 150px; height: 34px; border-radius: 17px; background: #0b1220; border: 2px solid #3a4762; overflow: hidden; }
.dg-sw-zone { position: absolute; top: 0; bottom: 0; background: linear-gradient(90deg, #ffb347, #ffe7a8, #ffb347); }
.dg-sw-mark { position: absolute; top: -6px; bottom: -6px; width: 6px; margin-left: -3px; border-radius: 3px; background: #fff; box-shadow: 0 0 10px #fff; }
.dg-sw-log { position: absolute; left: 50%; top: 66px; width: 60px; height: 70px; margin-left: -30px; border-radius: 10px 10px 4px 4px;
  background: repeating-linear-gradient(180deg, #8a5a34 0 10px, #74492a 10px 13px); border: 2px solid #4a2e1a; }
.dg-sw-log.hit { animation: dgShake 0.3s; }
@keyframes dgShake { 25% { transform: translateX(-6px) rotate(-4deg); } 75% { transform: translateX(6px) rotate(4deg); } }
.dg-lane { position: absolute; left: 0; right: 0; height: 90px; border-top: 1px dashed rgba(255,255,255,0.1); border-bottom: 1px dashed rgba(255,255,255,0.1); }
.dg-lane .k { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); font-size: 11px; color: var(--muted); }
.dg-ring { position: absolute; left: 90px; top: 50%; width: 54px; height: 54px; margin: -27px 0 0 -27px; border-radius: 50%; border: 3px solid rgba(255,255,255,0.7); }
.dg-ring.flash { box-shadow: 0 0 18px #8ff0ff; border-color: #8ff0ff; }
.dg-note { position: absolute; top: 50%; width: 40px; height: 40px; margin: -20px 0 0 -20px; border-radius: 50%; background: radial-gradient(circle at 35% 35%, #fff, #4fb3ff 60%, #1f5fa8); box-shadow: 0 0 10px #4fb3ff; }
.dg-note.b { background: radial-gradient(circle at 35% 35%, #fff, #ff8fb0 60%, #a8325c); box-shadow: 0 0 10px #ff8fb0; }
.dg-pads { position: absolute; left: 50%; top: 50%; transform: translate(-50%, -42%); display: grid; grid-template-columns: 96px 96px; gap: 12px; }
.dg-pad { width: 96px; height: 96px; border-radius: 18px; font-size: 38px; display: flex; align-items: center; justify-content: center; border: 3px solid rgba(255,255,255,0.15);
  opacity: 0.55; transition: opacity 0.08s, transform 0.08s; position: relative; padding: 0; }
.dg-pad.lit { opacity: 1; transform: scale(1.06); box-shadow: 0 0 24px currentColor; border-color: #fff; }
.dg-pad i { position: absolute; left: 6px; top: 4px; font-size: 11px; font-style: normal; color: #fff; opacity: 0.8; }
.dg-meter { position: absolute; left: 50%; top: 40px; bottom: 30px; width: 56px; margin-left: -28px; border-radius: 12px; background: #0b1220; border: 2px solid #3a4762; overflow: hidden; }
.dg-meter .z { position: absolute; left: 0; right: 0; background: rgba(159, 224, 122, 0.25); border-top: 2px solid #9fe07a; border-bottom: 2px solid #9fe07a; }
.dg-meter .f { position: absolute; left: 0; right: 0; bottom: 0; background: linear-gradient(0deg, #ff9a3c, #ffd36a); }
.dg-runner { position: absolute; left: calc(50% + 60px); top: 50%; font-size: 44px; }
.dg-result { text-align: center; padding: 10px 0 2px; }
.dg-result b { font-size: 22px; }
.dg-result .gains { display: flex; justify-content: center; gap: 8px; flex-wrap: wrap; margin: 8px 0; }
@media (min-width: 1400px) { .df-body { grid-template-columns: 1fr 390px; } }
@media (max-height: 800px) {
  .dg-stage { height: 240px; }
  .dg-sw-track { top: 130px; }
  .df-act { width: 58px; padding: 4px 2px 3px; }
  .df-act span { font-size: 18px; }
  .df-card { gap: 6px; padding: 8px 10px; }
  .df-box { padding: 6px 9px; }
  .df-who .pt { width: 48px; height: 48px; }
}
`;
const tag = document.createElement('style');
tag.id = 'dt-farm-style';
tag.textContent = css;
document.head.appendChild(tag);
