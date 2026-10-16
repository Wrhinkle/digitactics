"""Digi-Tactics desktop app.

Serves the game files from a local HTTP server on a free port and shows them in a native window
(pywebview, which uses the Edge WebView2 runtime on Windows). No console window when built with
build.py. Errors go to %LOCALAPPDATA%\\DigiTactics\\app.log.

Run from source:   desktop\\.venv\\Scripts\\python desktop\\digitactics.py
Server only:       ... digitactics.py --server-only [--port 8090]   (for testing; no window)
"""
import argparse
import functools
import http.server
import logging
import os
import sys
import threading
from pathlib import Path

APP_NAME = "Digi-Tactics"
DATA_DIR = Path(os.environ.get("LOCALAPPDATA", Path.home())) / "DigiTactics"
DATA_DIR.mkdir(parents=True, exist_ok=True)
logging.basicConfig(filename=DATA_DIR / "app.log", level=logging.INFO,
                    format="%(asctime)s %(levelname)s %(message)s")
log = logging.getLogger("digitactics")


def game_root() -> Path:
    # PyInstaller unpacks bundled data under sys._MEIPASS/game; from source the repo root is one level up.
    if getattr(sys, "frozen", False):
        return Path(sys._MEIPASS) / "game"
    return Path(__file__).resolve().parent.parent


class Handler(http.server.SimpleHTTPRequestHandler):
    extensions_map = {
        **http.server.SimpleHTTPRequestHandler.extensions_map,
        ".js": "text/javascript", ".mjs": "text/javascript", ".json": "application/json",
        ".glb": "model/gltf-binary", ".gltf": "model/gltf+json", ".bin": "application/octet-stream",
        ".png": "image/png", ".jpg": "image/jpeg", ".jpeg": "image/jpeg", ".webp": "image/webp",
    }

    def end_headers(self):
        self.send_header("Cache-Control", "no-cache")
        super().end_headers()

    def log_message(self, fmt, *args):  # keep the log quiet; only errors matter
        pass

    def log_error(self, fmt, *args):
        log.warning("http: " + fmt, *args)


def start_server(root: Path, port: int = 0) -> http.server.ThreadingHTTPServer:
    handler = functools.partial(Handler, directory=str(root))
    server = http.server.ThreadingHTTPServer(("127.0.0.1", port), handler)
    threading.Thread(target=server.serve_forever, daemon=True).start()
    log.info("serving %s on port %d", root, server.server_address[1])
    return server


def main() -> int:
    ap = argparse.ArgumentParser(description=APP_NAME)
    ap.add_argument("--server-only", action="store_true", help="run the file server without a window")
    ap.add_argument("--port", type=int, default=0, help="server port (default: any free port)")
    args = ap.parse_args()

    root = game_root()
    if not (root / "index.html").exists():
        log.error("game files not found in %s", root)
        return 1
    server = start_server(root, args.port)
    url = f"http://127.0.0.1:{server.server_address[1]}/index.html"

    if args.server_only:
        print(url, flush=True)
        try:
            threading.Event().wait()
        except KeyboardInterrupt:
            pass
        return 0

    import webview  # imported here so --server-only works without a GUI stack

    window = webview.create_window(APP_NAME, url, width=1500, height=940, min_size=(1200, 760),
                                   background_color="#0d1017")
    window.events.closed += lambda: server.shutdown()
    # private_mode=False keeps WebView storage between runs; storage lives in the app data folder.
    webview.start(private_mode=False, storage_path=str(DATA_DIR / "webview"))
    log.info("window closed")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except Exception:
        log.exception("crashed")
        raise
