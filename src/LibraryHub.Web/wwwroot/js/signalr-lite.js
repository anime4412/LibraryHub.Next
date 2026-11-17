window.SignalRLite = class {
  constructor(path, receive) {
    this.path = path;
    this.receive = receive;
    this.socket = null;
    this.stopped = false;
    this.retry = 1000;
  }
  async start() {
    const response = await fetch(`${this.path}/negotiate?negotiateVersion=1`, {
      method: "POST", credentials: "same-origin"
    });
    if (!response.ok) throw new Error(`Không thể kết nối realtime (${response.status}).`);
    const negotiated = await response.json();
    const scheme = location.protocol === "https:" ? "wss" : "ws";
    this.socket = new WebSocket(`${scheme}://${location.host}${this.path}?id=${encodeURIComponent(negotiated.connectionToken)}`);
    this.socket.addEventListener("open", () => {
      this.retry = 1000;
      this.socket.send('{"protocol":"json","version":1}\u001e');
    });
    this.socket.addEventListener("message", event => {
      for (const frame of String(event.data).split("\u001e").filter(Boolean)) {
        let payload;
        try { payload = JSON.parse(frame); } catch { continue; }
        if (payload.type === 1) this.receive(payload.target, payload.arguments || []);
        if (payload.type === 6) this.socket?.send('{"type":6}\u001e');
      }
    });
    this.socket.addEventListener("close", () => {
      if (this.stopped) return;
      setTimeout(() => this.start().catch(() => {}), this.retry);
      this.retry = Math.min(this.retry * 2, 15000);
    });
  }
  invoke(target, args = []) {
    if (this.socket?.readyState !== WebSocket.OPEN) return;
    this.socket.send(`${JSON.stringify({ type: 1, invocationId: crypto.randomUUID(), target, arguments: args })}\u001e`);
  }
  stop() { this.stopped = true; this.socket?.close(); }
};
