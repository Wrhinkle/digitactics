// Stand-in steam_api64.dll for tests/steam-native.cjs: exports the flat Steamworks functions desktop-steam/steam.js binds
// with koffi, with the struct layouts of the Steamworks SDK (Windows x64; the static_asserts below hold the sizes MSVC
// reports for the real SDK headers). It lets the native message, session and avatar paths run inside Electron without
// Steam or a second account, and it checks what steam.js passes in (identity type and size, buffer sizes, double
// releases). FakeSteam_Stats reports the counts.
// Build: tests/native/build.cmd <outDir> (MSVC x64 tools).
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <stddef.h>
#include <deque>
#include <map>
#include <set>
#include <string>
#include <vector>

#define EXPORT extern "C" __declspec(dllexport)

#pragma pack(push, 1)
struct SteamNetworkingIdentity { int32_t m_eType; int32_t m_cbSize; union { uint64_t m_steamID64; char m_szUnknownRawString[128]; uint32_t m_reserved[32]; }; };
#pragma pack(pop)
#pragma pack(push, 8)
struct SteamNetworkingMessage_t {
  void *m_pData; int m_cbSize; uint32_t m_conn; SteamNetworkingIdentity m_identityPeer; int64_t m_nConnUserData; int64_t m_usecTimeReceived;
  int64_t m_nMessageNumber; void (*m_pfnFreeData)(SteamNetworkingMessage_t *); void (*m_pfnRelease)(SteamNetworkingMessage_t *);
  int m_nChannel; int m_nFlags; int64_t m_nUserData; uint16_t m_idxLane; uint16_t _pad1__;
};
struct FriendGameInfo_t { uint64_t m_gameID; uint32_t m_unGameIP; uint16_t m_usGamePort; uint16_t m_usQueryPort; uint64_t m_steamIDLobby; };
#pragma pack(pop)
static_assert(sizeof(SteamNetworkingIdentity) == 136, "identity");
static_assert(offsetof(SteamNetworkingIdentity, m_steamID64) == 8, "identity id");
static_assert(sizeof(SteamNetworkingMessage_t) == 216, "message");
static_assert(offsetof(SteamNetworkingMessage_t, m_identityPeer) == 16, "peer");
static_assert(offsetof(SteamNetworkingMessage_t, m_nConnUserData) == 152, "conn user data");
static_assert(offsetof(SteamNetworkingMessage_t, m_pfnRelease) == 184, "release");
static_assert(offsetof(SteamNetworkingMessage_t, m_nChannel) == 192, "channel");
static_assert(sizeof(FriendGameInfo_t) == 24, "friend game info");
static_assert(offsetof(FriendGameInfo_t, m_steamIDLobby) == 16, "lobby");

static const uint32_t MAGIC = 0x5354454du;
struct Owned { SteamNetworkingMessage_t m; uint32_t magic; };
static std::deque<SteamNetworkingMessage_t *> queue;
static std::set<uint64_t> pendingSessions, acceptedSessions;
static std::map<uint64_t, int> avatarCalls;
static uint64_t self = 76561190000000001ull;
static int64_t nextNumber = 1;
static struct { int64_t allocated, released, badRelease, badIdentity, badBuffer, sent, sentBytes, accepted, acceptFalse, receiveCalls; uint64_t lastTo; } st;
static char ifaces[5][64];
static std::string lastSent;
static int lastFlags, lastChannel;
static bool captureSent;
static std::deque<std::string> sentPackets;
static std::string poppedPacket;
EXPORT void FakeSteam_CaptureSent(int enabled) { captureSent=enabled!=0; sentPackets.clear(); }
EXPORT const char *FakeSteam_PopSent() { if(sentPackets.empty()) return ""; poppedPacket=sentPackets.front(); sentPackets.pop_front(); return poppedPacket.c_str(); }
EXPORT int FakeSteam_LastFlags() { return lastFlags; }
EXPORT int FakeSteam_LastChannel() { return lastChannel; }

// Lifecycle and manual dispatch controls for the native C# client.
static int initFailure, initCalls, shutdownCalls, callbackReleases, callbackErrors;
static bool invalidIdentity;
EXPORT void FakeSteam_InvalidIdentity(int value) { invalidIdentity = value != 0; }
struct CallbackMsg { int user, id; void *data; int size; };
static_assert(sizeof(CallbackMsg) == 24, "callback message");
struct FakeCallback { int id, size; std::vector<uint8_t> bytes; };
static std::deque<FakeCallback> callbacks;
static bool callbackActive;
static std::map<uint64_t, FakeCallback> results;
static uint64_t nextCall = 100;
static uint64_t result(int id, const void *data, int size) {
  uint64_t call=++nextCall;
  results[call]={id,size,std::vector<uint8_t>((const uint8_t*)data,(const uint8_t*)data+size)};
  struct Completion { uint64_t call; int id; uint32_t size; } completed={call,id,(uint32_t)size};
  callbacks.push_back({703,16,std::vector<uint8_t>((uint8_t*)&completed,(uint8_t*)&completed+16)});
  return call;
}
EXPORT void FakeSteam_FailInit(int status) { initFailure = status; }
EXPORT int FakeSteam_InitCount() { return initCalls; }
EXPORT int FakeSteam_ShutdownCount() { return shutdownCalls; }
EXPORT int FakeSteam_CallbackReleases() { return callbackReleases; }
EXPORT int FakeSteam_CallbackErrors() { return callbackErrors; }
EXPORT void FakeSteam_Callback(int id, const void *data, int size) {
  FakeCallback cb; cb.id=id; cb.size=size;
  if (size > 0) cb.bytes.assign((const uint8_t *)data, (const uint8_t *)data + size);
  callbacks.push_back(cb);
}
EXPORT int SteamAPI_InitFlat(char *error) { initCalls++; if (initFailure && error) strcpy(error, "Fake Steam is unavailable"); return initFailure; }
EXPORT void SteamAPI_Shutdown() { shutdownCalls++; }
EXPORT void SteamAPI_ManualDispatch_Init() {}
EXPORT int SteamAPI_GetHSteamPipe() { return 1; }
EXPORT void SteamAPI_ManualDispatch_RunFrame(int pipe) { if (pipe != 1) callbackErrors++; }
EXPORT bool SteamAPI_ManualDispatch_GetNextCallback(int pipe, CallbackMsg *out) {
  if (pipe != 1 || !out || callbackActive) { callbackErrors++; return false; }
  if (callbacks.empty()) return false;
  auto &cb=callbacks.front(); out->user=1; out->id=cb.id; out->data=cb.bytes.empty()?nullptr:cb.bytes.data(); out->size=cb.size;
  callbackActive=true; return true;
}
EXPORT void SteamAPI_ManualDispatch_FreeLastCallback(int pipe) {
  if (pipe != 1 || !callbackActive) { callbackErrors++; return; }
  callbacks.pop_front(); callbackActive=false; callbackReleases++;
}
EXPORT bool SteamAPI_ManualDispatch_GetAPICallResult(int pipe, uint64_t call, void *data, int size, int expected, bool *failed) {
  auto found=results.find(call);
  if(found!=results.end()) {
    auto &r=found->second;
    bool ok=pipe==1 && data && size==r.size && expected==r.id;
    if(failed) *failed=!ok;
    if(ok) memcpy(data,r.bytes.data(),size);
    results.erase(found); return ok;
  }
  if (failed) *failed = call != 77;
  if (pipe != 1 || call != 77 || size != 4 || expected != 504 || !data) return false;
  uint32_t value=0x12345678; memcpy(data,&value,4); return true;
}
EXPORT void *SteamAPI_SteamUser_v023() { return ifaces[1]; }
EXPORT uint64_t SteamAPI_ISteamUser_GetSteamID(void *) { return invalidIdentity ? 0 : self; }
EXPORT const char *SteamAPI_ISteamFriends_GetPersonaName(void *) { return "Native Test Tamer"; }
EXPORT uint32_t SteamAPI_ISteamUtils_GetAppID(void *) { return 480; }
EXPORT bool SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(void *, int key, int32_t value) { return key == 104 && value == 0; }
EXPORT bool SteamAPI_ISteamNetworkingMessages_CloseSessionWithUser(void *, const SteamNetworkingIdentity *id) {
  if (!id) return false;
  acceptedSessions.erase(id->m_steamID64); pendingSessions.erase(id->m_steamID64); return true;
}

static bool goodIdentity(const SteamNetworkingIdentity *id) {
  if (!id || id->m_eType != 16 || id->m_cbSize != 8 || id->m_steamID64 == 0) { st.badIdentity++; return false; }
  return true;
}
static void releaseMsg(SteamNetworkingMessage_t *m) {
  Owned *o = (Owned *)m;
  if (o->magic != MAGIC) { st.badRelease++; return; }
  o->magic = 0xdeadbeefu;
  free(m->m_pData);
  m->m_pData = nullptr;
  st.released++;
  // the block stays allocated (poisoned) so a second release is counted instead of corrupting the heap
}
static void push(uint64_t from, const void *data, int size) {
  Owned *o = (Owned *)calloc(1, sizeof(Owned));
  o->magic = MAGIC;
  o->m.m_pData = malloc(size > 0 ? size : 1);
  if (size > 0) memcpy(o->m.m_pData, data, size);
  o->m.m_cbSize = size;
  o->m.m_identityPeer.m_eType = 16;
  o->m.m_identityPeer.m_cbSize = 8;
  o->m.m_identityPeer.m_steamID64 = from;
  o->m.m_nMessageNumber = nextNumber++;
  o->m.m_pfnRelease = releaseMsg;
  queue.push_back(&o->m);
  st.allocated++;
  if (!acceptedSessions.count(from)) pendingSessions.insert(from);
}

// ---- test controls ----
EXPORT void FakeSteam_SetSelf(uint64_t id) { self = id; }
EXPORT void FakeSteam_Inject(uint64_t from, const void *data, int size) { push(from, data, size); }
EXPORT void FakeSteam_InjectRaw(int identityType, uint64_t from, const void *data, int size) {
  push(from, data, size);
  queue.back()->m_identityPeer.m_eType = identityType;
}
EXPORT int FakeSteam_Stats(char *out, int cap) {
  return snprintf(out, cap, "{\"allocated\":%lld,\"released\":%lld,\"badRelease\":%lld,\"badIdentity\":%lld,\"badBuffer\":%lld,\"sent\":%lld,\"sentBytes\":%lld,\"accepted\":%lld,\"acceptFalse\":%lld,\"receiveCalls\":%lld,\"queued\":%zu,\"lastTo\":\"%llu\"}",
    st.allocated, st.released, st.badRelease, st.badIdentity, st.badBuffer, st.sent, st.sentBytes, st.accepted, st.acceptFalse, st.receiveCalls, queue.size(), st.lastTo);
}
EXPORT const char *FakeSteam_LastSent() { return lastSent.c_str(); }

// ---- interface accessors ----
EXPORT void *SteamAPI_SteamNetworkingMessages_SteamAPI_v002() { return ifaces[0]; }
EXPORT void *SteamAPI_SteamMatchmaking_v009() { return ifaces[1]; }
EXPORT void *SteamAPI_SteamFriends_v017() { return ifaces[2]; }
EXPORT void *SteamAPI_SteamUtils_v010() { return ifaces[3]; }
EXPORT void *SteamAPI_SteamNetworkingUtils_SteamAPI_v004() { return ifaces[4]; }

// ---- ISteamNetworkingMessages ----
EXPORT int SteamAPI_ISteamNetworkingMessages_SendMessageToUser(void *self_, const SteamNetworkingIdentity *id, const void *data, uint32_t size, int flags, int channel) {
  if (self_ != ifaces[0] || !goodIdentity(id)) return 8; // k_EResultInvalidParam
  if (!data || size == 0 || size > 512 * 1024) { st.badBuffer++; return 8; }
  st.sent++;
  lastFlags = flags; lastChannel = channel;
  st.sentBytes += size;
  st.lastTo = id->m_steamID64;
  lastSent.assign((const char *)data, size);
  if(captureSent) sentPackets.push_back(lastSent);
  acceptedSessions.insert(id->m_steamID64);
  pendingSessions.erase(id->m_steamID64);
  if (id->m_steamID64 == self) push(self, data, (int)size); // a message to yourself comes back
  return 1;
}
EXPORT int SteamAPI_ISteamNetworkingMessages_ReceiveMessagesOnChannel(void *self_, int channel, SteamNetworkingMessage_t **out, int max) {
  st.receiveCalls++;
  if (self_ != ifaces[0] || !out || max < 1) return -1;
  int n = 0;
  while (n < max && !queue.empty()) { out[n++] = queue.front(); queue.pop_front(); }
  return n;
}
EXPORT bool SteamAPI_ISteamNetworkingMessages_AcceptSessionWithUser(void *self_, const SteamNetworkingIdentity *id) {
  if (self_ != ifaces[0] || !goodIdentity(id)) return false;
  if (!pendingSessions.count(id->m_steamID64)) { st.acceptFalse++; return false; }
  pendingSessions.erase(id->m_steamID64);
  acceptedSessions.insert(id->m_steamID64);
  st.accepted++;
  return true;
}
EXPORT void SteamAPI_SteamNetworkingMessage_t_Release(SteamNetworkingMessage_t *m) {
  if (!m) { st.badRelease++; return; }
  m->m_pfnRelease(m);
}
EXPORT void SteamAPI_ISteamNetworkingUtils_InitRelayNetworkAccess(void *) {}
// not bound by steam.js any more (it builds the identity itself); kept so the 1.0.0/1.0.1 steam.js can run as a control
EXPORT void SteamAPI_SteamNetworkingIdentity_SetSteamID64(SteamNetworkingIdentity *id, uint64_t steamId) {
  id->m_eType = 16; id->m_cbSize = 8; id->m_steamID64 = steamId;
}

// ---- ISteamMatchmaking ----
EXPORT void SteamAPI_ISteamMatchmaking_AddRequestLobbyListStringFilter(void *, const char *, const char *, int) {}
EXPORT void SteamAPI_ISteamMatchmaking_AddRequestLobbyListDistanceFilter(void *, int) {}
EXPORT void SteamAPI_ISteamMatchmaking_AddRequestLobbyListResultCountFilter(void *, int) {}
static std::map<uint64_t,std::map<std::string,std::string>> lobbyData;
static uint64_t lobbyOwner, activeLobby=109775240000000001ull;
static int lobbyCapacity=8, leaves;
static int memberMask=3;
EXPORT void FakeSteam_Members(int mask) { memberMask=mask; }
EXPORT int FakeSteam_Leaves() { return leaves; }
EXPORT uint64_t SteamAPI_ISteamMatchmaking_CreateLobby(void *, int type, int maximum) {
  lobbyOwner=self; lobbyCapacity=maximum; lobbyData[activeLobby].clear();
  struct Created { int status; uint64_t lobby; } created={1,activeLobby};
  static_assert(sizeof(Created)==16,"created"); return result(513,&created,16);
}
EXPORT uint64_t SteamAPI_ISteamMatchmaking_JoinLobby(void *, uint64_t lobby) {
  struct Entered { uint64_t lobby; uint32_t permissions; bool locked; uint32_t response; } entered={lobby,0,false,lobby==activeLobby?1u:2u};
  static_assert(sizeof(Entered)==24,"entered"); return result(504,&entered,24);
}
EXPORT uint64_t SteamAPI_ISteamMatchmaking_RequestLobbyList(void *) { uint32_t count=1; return result(510,&count,4); }
EXPORT uint64_t SteamAPI_ISteamMatchmaking_GetLobbyByIndex(void *, int index) { return index==0?activeLobby:0; }
EXPORT uint64_t SteamAPI_ISteamMatchmaking_GetLobbyMemberByIndex(void *, uint64_t, int index) {
  if(memberMask&1) { if(index==0) return self; index--; }
  return (memberMask&2) && index==0?76561190000000002ull:0;
}
EXPORT int SteamAPI_ISteamMatchmaking_GetLobbyMemberLimit(void *, uint64_t) { return lobbyCapacity; }
EXPORT uint64_t SteamAPI_ISteamMatchmaking_GetLobbyOwner(void *, uint64_t) { return lobbyOwner; }
EXPORT void SteamAPI_ISteamMatchmaking_LeaveLobby(void *, uint64_t) { leaves++; }
EXPORT bool SteamAPI_ISteamMatchmaking_SetLobbyData(void *, uint64_t lobby, const char *key, const char *value) { lobbyData[lobby][key]=value; return true; }
EXPORT const char *SteamAPI_ISteamMatchmaking_GetLobbyData(void *, uint64_t lobby, const char *key) {
  auto &fields=lobbyData[lobby]; auto found=fields.find(key);
  return found!=fields.end()?found->second.c_str():strcmp(key,"dt_game")==0?"digitactics":"";
}
static int dataRequestMode;
EXPORT void FakeSteam_DataRequestMode(int mode) { dataRequestMode=mode; }
EXPORT bool SteamAPI_ISteamMatchmaking_RequestLobbyData(void *, uint64_t lobby) {
  if(dataRequestMode==1) return false;
  if(dataRequestMode==2) return true;
  struct Updated { uint64_t lobby,member; bool success; } updated={lobby,lobby,true};
  static_assert(sizeof(Updated)==24,"data update"); FakeSteam_Callback(505,&updated,24); return true;
}
EXPORT int SteamAPI_ISteamMatchmaking_GetNumLobbyMembers(void *, uint64_t) { return ((memberMask&1)?1:0)+((memberMask&2)?1:0); }
EXPORT bool SteamAPI_ISteamMatchmaking_SetLobbyOwner(void *, uint64_t, uint64_t target) { lobbyOwner=target; return true; }

// ---- ISteamFriends ----
static const uint64_t FRIEND_A = 76561190000000002ull, FRIEND_B = 76561190000000003ull, LOBBY = 109775240000000001ull;
EXPORT int SteamAPI_ISteamFriends_GetFriendCount(void *, int) { return 2; }
EXPORT uint64_t SteamAPI_ISteamFriends_GetFriendByIndex(void *, int i, int) { return i == 0 ? FRIEND_A : i == 1 ? FRIEND_B : 0; }
EXPORT bool SteamAPI_ISteamFriends_GetFriendGamePlayed(void *, uint64_t id, FriendGameInfo_t *g) {
  if (!g) return false;
  memset(g, 0, sizeof(*g));
  if (id != FRIEND_A) return false;
  g->m_gameID = 480;
  g->m_steamIDLobby = LOBBY;
  g->m_usQueryPort = 0xffff;
  return true;
}
static char nameBuf[64];
EXPORT const char *SteamAPI_ISteamFriends_GetFriendPersonaName(void *, uint64_t id) { snprintf(nameBuf, sizeof nameBuf, "Tamer %llu", id % 1000); return nameBuf; }
EXPORT bool SteamAPI_ISteamFriends_RequestUserInformation(void *, uint64_t, bool) { return true; }
// someone else: -1 on the first ask (Steam still downloading), then image 7. Ourselves: image 7. Id 0: no avatar.
static int imageMode, imageReads;
EXPORT void FakeSteam_ImageMode(int mode) { imageMode=mode; }
EXPORT int FakeSteam_ImageReads() { return imageReads; }
EXPORT int SteamAPI_ISteamFriends_GetMediumFriendAvatar(void *, uint64_t id) {
  if (id == 0) return 0;
  if (id == self) return 7+imageMode;
  return avatarCalls[id]++ == 0 ? -1 : 7+imageMode;
}

// ---- ISteamUtils ----
EXPORT bool SteamAPI_ISteamUtils_GetImageSize(void *, int image, uint32_t *w, uint32_t *h) {
  if (!w || !h) { st.badBuffer++; return false; }
  if (image < 7 || image > 10) return false;
  *w = image==8?512:image==10?0:64; *h = 64;
  return true;
}
EXPORT bool SteamAPI_ISteamUtils_GetImageRGBA(void *, int image, uint8_t *dest, int size) {
  imageReads++;
  if (image != 7) return false;
  if (!dest || size != 64 * 64 * 4) { st.badBuffer++; return false; }
  for (int i = 0; i < size; i += 4) { dest[i] = 200; dest[i + 1] = 100; dest[i + 2] = 50; dest[i + 3] = 255; }
  return true;
}
