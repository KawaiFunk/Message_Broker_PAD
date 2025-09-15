# publisher_cli/cli.py
import argparse
from .client import BrokerPublisher
from .repl import run_repl, stream_stdin

def build_parser() -> argparse.ArgumentParser:
    ap = argparse.ArgumentParser(description="Interactive CLI publisher for BrokerSocket")
    ap.add_argument("--host", required=True, help="Broker host, e.g. 127.0.0.1")
    ap.add_argument("--port", required=True, type=int, help="Broker port, e.g. 8080")
    ap.add_argument("--topic", default="logs/app", help="Default topic")
    ap.add_argument("--fmt", default="json", choices=["json", "xml"], help="Local send format")
    ap.add_argument("--mode", default="auto", choices=["auto", "json", "text"],
                    help="Interpretation of input lines")
    ap.add_argument("--stdin", action="store_true", help="Read from stdin instead of REPL")
    ap.add_argument("--timeout", type=float, default=3.0, help="Connect timeout seconds")
    return ap

def main():
    args = build_parser().parse_args()
    pub = BrokerPublisher(args.host, args.port, timeout=args.timeout)
    pub.connect()
    try:
        if args.stdin:
            stream_stdin(pub, args.topic, args.fmt, args.mode)
        else:
            run_repl(pub, args.topic, args.fmt, args.mode)
    finally:
        pub.close()

if __name__ == "__main__":
    main()

