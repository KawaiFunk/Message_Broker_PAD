# publisher_cli/client.py
import socket
import threading
import time
from typing import Any, Optional
from .serialize import to_wire

class BrokerPublisher:
    def __init__(
        self,
        host: str,
        port: int,
        timeout: float = 3.0,
        heartbeat_interval: float = 10.0,
        heartbeat_timeout: float = 20.0,
    ):
        self.host, self.port, self.timeout = host, port, timeout
        self.sock: Optional[socket.socket] = None
        self._rfile = None  # for reading server lines (PONG, notices)
        self._wlock = threading.Lock()
        self._stop = threading.Event()
        self._recv_thread: Optional[threading.Thread] = None
        self._hb_thread: Optional[threading.Thread] = None
        self._hb_interval = heartbeat_interval
        self._hb_timeout = heartbeat_timeout
        self._last_pong = time.time()

    def _print(self, msg: str):
        print(msg, flush=True)

    def connect(self):
        self.close()
        s = socket.create_connection((self.host, self.port), timeout=self.timeout)
        # send immediately
        s.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
        # best-effort keepalive (helps detect half-open after idle)
        try:
            s.setsockopt(socket.SOL_SOCKET, socket.SO_KEEPALIVE, 1)
        except OSError:
            pass

        self.sock = s
        self._rfile = s.makefile("r", encoding="utf-8", newline="\n")
        self._stop.clear()
        self._last_pong = time.time()

        # start background receiver + heartbeat
        self._recv_thread = threading.Thread(target=self._receiver_loop, daemon=True)
        self._recv_thread.start()
        self._hb_thread = threading.Thread(target=self._heartbeat_loop, daemon=True)
        self._hb_thread.start()

    def _receiver_loop(self):
        try:
            while not self._stop.is_set():
                # If broker closes the socket, readline() returns ''
                line = self._rfile.readline()
                if not line:
                    self._print("Broker connection closed.")
                    self.close()
                    return
                line = line.rstrip("\r\n")
                if line.upper().startswith("PONG"):
                    self._last_pong = time.time()
                else:
                    # Optional: print any server-side notices
                    self._print(f"← {line}")
        except Exception as e:
            if not self._stop.is_set():
                self._print(f"Receiver error: {e}")
                self.close()

    def _heartbeat_loop(self):
        # Periodically send PING; if no PONG within timeout → consider down
        while not self._stop.is_set():
            time.sleep(self._hb_interval)
            if self.sock is None:
                return
            try:
                self.send_raw("PING")
            except Exception as e:
                self._print(f"Broker unreachable (heartbeat send failed): {e}")
                self.close()
                return

            if time.time() - self._last_pong > self._hb_timeout:
                self._print("Broker not responding to heartbeats — considered DOWN.")
                self.close()
                return

    def close(self):
        if self._stop.is_set():
            return
        self._stop.set()
        # Close read file first (unblocks receiver thread)
        try:
            if self._rfile:
                try:
                    self._rfile.close()
                except Exception:
                    pass
        finally:
            self._rfile = None

        if self.sock:
            try:
                try:
                    self.sock.shutdown(socket.SHUT_RDWR)
                except OSError:
                    pass
                self.sock.close()
            except OSError:
                pass
            finally:
                self.sock = None

    def _send_bytes(self, data: bytes):
        if not self.sock:
            raise RuntimeError("Not connected")
        try:
            with self._wlock:
                self.sock.sendall(data)
        except (BrokenPipeError, ConnectionResetError, OSError) as e:
            self._print(f"Broker connection lost: {e}")
            self.close()
            raise

    def send_raw(self, line: str):
        """Send a control line like FORMAT:xml/json, SWITCHTYPE, SUBSCRIBE:<topic>, PING."""
        data = (line.rstrip("\r\n") + "\n").encode("utf-8")
        self._send_bytes(data)

    def publish(self, topic: str, message: Any, fmt: str):
        """Send one body line in the selected format (JSON or XML)."""
        wire = to_wire(topic, message, fmt)
        self._send_bytes(wire)
