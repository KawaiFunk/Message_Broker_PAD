# publisher_cli/serialize.py
from typing import Any
import json

def _xml_escape(s: str) -> str:
    return (
        s.replace("&", "&amp;")
         .replace("<", "&lt;")
         .replace(">", "&gt;")
         .replace('"', "&quot;")
         .replace("'", "&apos;")
    )

def to_wire(topic: str, message: Any, fmt: str) -> bytes:
    """Return a single line (ending with '\n') in the broker's body format."""
    if fmt == "json":
        body = {"Topic": topic, "Message": message}
        return (json.dumps(body, separators=(",", ":"), ensure_ascii=False) + "\n").encode("utf-8")
    if fmt == "xml":
        # Broker's XML model expects Message as string; stringify non-strings.
        if not isinstance(message, str):
            message = json.dumps(message, separators=(",", ":"), ensure_ascii=False)
        xml = f"<Payload><Topic>{_xml_escape(topic)}</Topic><Message>{_xml_escape(message)}</Message></Payload>\n"
        return xml.encode("utf-8")
    raise ValueError("fmt must be 'json' or 'xml'")
