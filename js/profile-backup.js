// Lossless backup of durable game storage. Export never modifies the source profile.
(function (G) {
  const MAX_MUSIC = 256 * 1024 * 1024;
  function storage() {
    const result = {};
    for (const key of Object.keys(localStorage).sort())
      if (key.startsWith('dt.') || key.startsWith('dt-')) result[key] = localStorage.getItem(key);
    return result;
  }
  function musicRecords() {
    return new Promise((resolve, reject) => {
      if (!G.indexedDB) { reject(new Error('Music storage is unavailable; backup was not created.')); return; }
      let missing = false;
      const request = indexedDB.open('dt-music');
      request.onupgradeneeded = () => { missing = true; request.transaction.abort(); };
      request.onerror = () => missing ? resolve([]) : reject(request.error);
      request.onblocked = () => reject(new Error('Close other game windows and try the backup again.'));
      request.onsuccess = () => {
        const db = request.result;
        if (!db.objectStoreNames.contains('tracks')) { db.close(); reject(new Error('Unrecognized music storage; backup was not created.')); return; }
        const tx = db.transaction('tracks', 'readonly'), query = tx.objectStore('tracks').getAll();
        tx.oncomplete = () => { db.close(); resolve(query.result); };
        tx.onerror = tx.onabort = () => { db.close(); reject(tx.error || new Error('Could not read music storage.')); };
      };
    });
  }
  async function snapshot() {
    const before = storage(), records = await musicRecords();
    if (records.reduce((sum, record) => sum + (record.blob?.size || 0), 0) > MAX_MUSIC)
      throw new Error('Your music exceeds the 256 MB backup limit. Keep a separate copy of the files before reducing your in-game music library.');
    const music = [];
    for (const record of records) {
      if (!(record.blob instanceof Blob)) throw new Error('A music file is unreadable; backup was not created.');
      const { blob, ...metadata } = record;
      const bytes = new Uint8Array(await blob.arrayBuffer());
      let binary = '';
      for (let i = 0; i < bytes.length; i += 8192) binary += String.fromCharCode(...bytes.subarray(i, i + 8192));
      const hash = new Uint8Array(await crypto.subtle.digest('SHA-256', bytes));
      music.push({ metadata, blob: { type: blob.type, name: blob instanceof File ? blob.name : null,
        lastModified: blob instanceof File ? blob.lastModified : null },
        data: btoa(binary), sha256: [...hash].map(b => b.toString(16).padStart(2, '0')).join('') });
    }
    if (JSON.stringify(before) !== JSON.stringify(storage())) throw new Error('Your save changed during backup. Try again while the game is idle.');
    return { format: 'digitactics-profile', version: 1, exportedAt: new Date().toISOString(), storage: before, music };
  }
  async function download() {
    const backup = await snapshot();
    const blob = new Blob([JSON.stringify(backup)], { type: 'application/json' });
    const url = URL.createObjectURL(blob), link = document.createElement('a');
    link.href = url; link.download = `digi-tactics-profile-${new Date().toISOString().slice(0, 10)}.json`;
    document.body.appendChild(link); link.click(); link.remove();
    setTimeout(() => URL.revokeObjectURL(url), 60000);
  }
  G.DT.ProfileBackup = { snapshot, download };
  document.addEventListener('click', async event => {
    const button = event.target.closest('[data-profile-backup]');
    if (!button || button.disabled) return;
    button.disabled = true; button.textContent = 'Preparing backup...';
    try { await download(); button.textContent = 'Backup downloaded'; }
    catch (error) { button.textContent = 'Export save backup'; G.DT.Client?.toast(error.message || 'Backup failed.', 'bad'); }
    finally { button.disabled = false; }
  });
})(window);
