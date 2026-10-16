// Offline check of the native struct layouts desktop-steam/steam.js hands to koffi: every size and offset koffi computes
// must equal what MSVC reports for the Steamworks SDK headers on Windows x64 (steamnetworkingtypes.h, isteamfriends.h;
// SteamNetworkingIdentity is #pragma pack(1), the rest pack(8)). The expected numbers come from compiling the SDK 1.64
// headers with sizeof/offsetof (the layouts have not changed since SDK 1.53, older than the steam_api64.dll shipped in
// steamworks.js 0.4.0). No Steam needed. Run: node tests/steam-structs.cjs
const path = require('path');
const koffi = require(path.join(__dirname, '..', 'desktop-steam', 'node_modules', 'koffi'));
const steam = require(path.join(__dirname, '..', 'desktop-steam', 'steam.js'));

const SDK = {
  DT_SteamNetworkingIdentity: { size: 136, offsets: { eType: 0, cbSize: 4, steamID64: 8 } },
  DT_SteamNetworkingMessage_t: {
    size: 216,
    offsets: { data: 0, size: 8, conn: 12, peer: 16, connUserData: 152, timeReceived: 160, messageNumber: 168, freeData: 176, release: 184, channel: 192, flags: 196, userData: 200, lane: 208 },
  },
  DT_FriendGameInfo_t: { size: 24, offsets: { gameID: 0, ip: 8, port: 12, queryPort: 14, lobby: 16 } },
};

const fails = [];
const check = (ok, msg) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${msg}`); if (!ok) fails.push(msg); };
const types = steam._test.defineTypes(koffi);
for (const t of [types.Identity, types.Message, types.FriendGameInfo]) {
  const want = SDK[t.name];
  check(!!want, `${t.name} has SDK reference values`);
  if (!want) continue;
  check(koffi.sizeof(t) === want.size, `sizeof ${t.name} = ${koffi.sizeof(t)} (SDK ${want.size})`);
  for (const [m, off] of Object.entries(want.offsets)) check(koffi.offsetof(t, m) === off, `offsetof ${t.name}.${m} = ${koffi.offsetof(t, m)} (SDK ${off})`);
}

// the identity steam.js builds by hand must match SteamNetworkingIdentity::SetSteamID64
const id = steam._test.identity('76561198042484646');
check(id.length === 136, 'identity buffer is 136 bytes');
check(id.readInt32LE(0) === 16 && id.readInt32LE(4) === 8, 'identity m_eType = k_ESteamNetworkingIdentityType_SteamID (16), m_cbSize = 8');
check(id.readBigUInt64LE(8) === 76561198042484646n, 'identity m_steamID64 at offset 8');
check(id.subarray(16).every((b) => b === 0), 'rest of the identity union is zero');

// decoding a message written with the SDK offsets gives the right fields
const buf = Buffer.alloc(216);
buf.writeBigUInt64LE(0x1234n, 0);
buf.writeInt32LE(42, 8);
buf.writeInt32LE(16, 16);
buf.writeInt32LE(8, 20);
buf.writeBigUInt64LE(76561198042484646n, 24);
buf.writeInt32LE(3, 192);
const m = koffi.decode(buf, types.Message);
check(Number(m.size) === 42 && m.peer.eType === 16 && BigInt(m.peer.steamID64) === 76561198042484646n && m.channel === 3, 'decoded message fields land on the SDK offsets');

if (fails.length) { console.log(`\n${fails.length} FAILED`); process.exit(1); }
console.log('\nall struct checks passed');
