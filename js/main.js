// Bundle entry: tools/build.cjs packs this into js/bundle.js so the game also runs from file://.
import './audio.js';
import './music.js';
import './icons.js';
import './render3d.js';
import './legends.js';
import './legendevo.js'; // legend levels, care and digivolution trees; before ui.js, which shows the current form
import './dances.js'; // 'dance' cosmetics: skeleton dances for legends and units
import './portraits.js';
import './boards.js'; // before ui.js, which creates the renderer
import './itemcards.js'; // item preview cards, used by ui.js
import './ui.js';
import './net.js';
import './client.js'; // client shell: nav, Home, Play, Collection, Profile, Settings; before shop/farm/codex so its Esc handler runs first
import './profile-backup.js';
import './emotes.js';
import './shop.js';
import './legendevo-ui.js'; // Digivolve panel, opened from the Locker
import './farm.js'; // Digi-Farm care rules and save (adds farm care to the digivolution odds)
import './farm-ui.js'; // Digi-Farm room and training mini-games, opened from the title screen and the Locker
import './killscreens.js';
import './soundboard.js';
import './elimsounds.js'; // 'elimsound' cosmetics; after soundboard.js, whose clips it reuses
import './mystery-register.js';
import './mystery-loot.js';
import './soundui.js';
import './scout.js'; // after emotes.js, so an open emote wheel gets the digit keys first
import './codex.js'; // trait encyclopedia: side panel (C) and title-screen Codex
import './setpicker.js'; // set picker on the Play page; net.js reads it for new lobbies
import './theme.js'; // design system and shell styles; last, so it follows every module's own styles
