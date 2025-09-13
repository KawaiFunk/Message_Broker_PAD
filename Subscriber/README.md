# Subscriber – TCP Message Broker Project

The **Subscriber** is a client application that connects to the **Message Broker** and listens for messages on one or more topics.  

## Features
- Connects to the broker via **TCP sockets**.
- Subscribes to one or more **topics**.
- Continuously listens for incoming messages from the broker.
- Displays received messages in real time.

## Example Workflow
1. Start the **Message Broker**.
2. Run the **Subscriber** and choose which topics to subscribe to.
3. Messages published to those topics are immediately received and displayed.
