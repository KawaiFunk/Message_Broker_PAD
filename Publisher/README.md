# Publisher – TCP Message Broker Project

The **Publisher** is a client application that connects to the **Message Broker** and sends messages to specific topics.  

## Features
- Connects to the broker via **TCP sockets**.
- Allows users to send messages tagged with a **topic**.
- Forwards the message to the broker, which then delivers it to all subscribers of that topic.

## Example Workflow
1. Start the **Message Broker**.
2. Start one or more **Subscribers** that subscribe to certain topics.
3. Run the **Publisher** to send messages to those topics.
4. The broker forwards messages to all matching subscribers.
