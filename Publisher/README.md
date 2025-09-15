# Publisher CLI for BrokerSocket

This project provides a modular, interactive command-line interface (CLI) for publishing messages to a BrokerSocket server. It supports both JSON and XML message formats, interactive REPL usage, and streaming from standard input.

## Features

- **Interactive CLI**: Connect to a broker and send messages interactively or via stdin.
- **JSON/XML Support**: Choose between JSON and XML formats for message serialization.
- **REPL Mode**: Use a rich command-line interface with commands for changing topics, formats, and sending messages.
- **Heartbeat & Connection Management**: Automatic heartbeat and reconnection logic for robust operation.
- **Thread-Safe Publishing**: Safe for concurrent use in multi-threaded scenarios.

## File Overview

- `cli.py`: Entry point for the CLI tool. Handles argument parsing and launches REPL or stdin streaming.
- `client.py`: Implements `BrokerPublisher`, managing TCP connections, heartbeats, and message publishing.
- `serialize.py`: Utilities for serializing messages to JSON or XML formats.
- `repl.py`: Implements the REPL interface and stdin streaming logic.
- `__init__.py`: Package initialization.

## Usage

### Installation

Clone the repository and ensure you have Python 3.8+ installed.

### Running the CLI

From the project root, run:

```sh
python -m Publisher.cli --host 127.0.0.1 --port 8080 --topic logs/app --fmt json
```

#### Options

- `--host`: Broker host (e.g., `127.0.0.1`)
- `--port`: Broker port (e.g., `8080`)
- `--topic`: Default topic to publish to (default: `logs/app`)
- `--fmt`: Message format (`json` or `xml`, default: `json`)
- `--mode`: Input interpretation mode (`auto`, `json`, or `text`)
- `--stdin`: Read messages from stdin instead of REPL
- `--timeout`: Connection timeout in seconds (default: `3.0`)

### REPL Commands

Once connected, you can use the following commands in the REPL:

- `:topic <name>` — Set the current topic
- `:fmt json|xml` — Set the message format
- `:json <object>` — Send a JSON object (e.g., `:json {"k":"v"}`)
- `:text <message>` — Send a plain text message
- `:switch` — Switch broker and CLI format
- `:subscribe <name>` — Subscribe to a topic
- `:help` — Show help
- `:quit` — Exit the CLI

### Example: Streaming from stdin

```sh
echo '{"level":"info","msg":"hello"}' | python -m Publisher.cli --host 127.0.0.1 --port 8080 --stdin
```

## Extending

- Add new serialization formats by extending `serialize.py`.
- Add new REPL commands in `repl.py`.

## License

MIT License

---

*This tool is intended for development and operational use with compatible BrokerSocket servers.*