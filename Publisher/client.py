# publisher_cli/client.py
import socket
from typing import Any, Optional
from .serialize import to_wire

class BrokerPublisher:
    def __init__(self, host: str, port: int, timeout: float = 3.0):
        self.host, self.port, self.timeout = host, port, timeout
        self.sock: Optional[socket.socket] = None

    def connect(self):
        self.close()
        self.sock = socket.create_connection((self.host, self.port), timeout=self.timeout)
        self.sock.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)

    def close(self):
        if self.sock:
            try:
                self.sock.shutdown(socket.SHUT_RDWR)
            except OSError:
                pass
            try:
                self.sock.close()
            finally:
                self.sock = None

    def send_raw(self, line: str):
        """Send a control line like SWITCHTYPE or SUBSCRIBE:<topic>."""
        assert self.sock is not None
        data = (line.rstrip("\r\n") + "\n").encode("utf-8")
        self.sock.sendall(data)

    def publish(self, topic: str, message: Any, fmt: str):
        """Send one body line in the currently selected format (JSON or XML)."""
        assert self.sock is not None
        self.sock.sendall(to_wire(topic, message, fmt))
