# publisher_cli/repl.py
import json
import sys
import time
from typing import Any
from .client import BrokerPublisher

HELP = """\
Commands:
  :topic <name>     - set current Topic
  :fmt json|xml     - set local send format
  :json <object>    - send a JSON object (e.g. :json {"k":"v"})
  :text <message>   - send a plain text Message
  :switch           - send SWITCHTYPE (and toggle local fmt too)
  :subscribe <name> - send SUBSCRIBE:<name>
  :help             - show this help
  :quit             - exit
"""

def _interpret_line(line: str, mode: str) -> Any:
    """Return a Python object to be used as Message depending on mode."""
    if mode == "json" or (mode == "auto" and line.lstrip().startswith("{")):
        try:
            return json.loads(line)
        except json.JSONDecodeError:
            if mode == "json":
                print("Invalid JSON.")
                return None
            # auto fallback to text
            return line
    return line

def run_repl(pub: BrokerPublisher, topic: str, fmt: str, mode: str):
    current_topic = topic
    current_fmt = fmt

    print(f"Connected to {pub.host}:{pub.port}")
    print(f"Topic = {current_topic} | fmt = {current_fmt} | mode = {mode}")
    print("(type :help for commands)")

    while True:
        try:
            line = input(f"{current_topic}> ").strip()
        except EOFError:
            print()
            break
        except KeyboardInterrupt:
            print()
            continue

        if not line:
            continue

        if line.startswith(":"):
            cmd, *rest = line[1:].split(" ", 1)
            arg = rest[0].strip() if rest else ""
            c = cmd.lower()

            if c in ("quit", "q", "exit"):
                break
            if c in ("help", "h"):
                print(HELP); continue
            if c == "topic":
                if arg: current_topic = arg
                else: print("Usage: :topic <name>")
                continue
            if c == "fmt":
                if arg in ("json", "xml"):
                    current_fmt = arg
                else:
                    print("Usage: :fmt json|xml")
                continue
            if c == "json":
                if not arg:
                    print('Usage: :json {"k":"v"}'); continue
                try:
                    payload = json.loads(arg)
                except json.JSONDecodeError as e:
                    print(f"Invalid JSON: {e}"); continue
                pub.publish(current_topic, payload, current_fmt); continue
            if c == "text":
                pub.publish(current_topic, arg, current_fmt); continue
            if c == "switch":
                pub.send_raw("SWITCHTYPE")
                # keep CLI and broker in sync
                current_fmt = "xml" if current_fmt == "json" else "json"
                print(f"Switched broker & CLI format to {current_fmt.upper()}"); continue
            if c == "subscribe":
                if not arg:
                    print("Usage: :subscribe <topic>"); continue
                pub.send_raw(f"SUBSCRIBE:{arg}"); continue

            print("Unknown command. :help for help.")
            continue

        # free text → interpret per mode and publish
        msg = _interpret_line(line, mode)
        if msg is None:
            continue
        try:
            pub.publish(current_topic, msg, current_fmt)
        except Exception as e:
            print(f"Send failed: {e}")

def stream_stdin(pub: BrokerPublisher, topic: str, fmt: str, mode: str):
    """Keep connection and stream stdin lines as messages."""
    for raw in sys.stdin:
        line = raw.rstrip("\r\n")
        if not line:
            continue
        msg = _interpret_line(line, mode)
        if msg is None:
            continue
        try:
            pub.publish(topic, msg, fmt)
        except Exception:
            # quick retry after a short backoff
            time.sleep(0.25)
            pub.publish(topic, msg, fmt)
