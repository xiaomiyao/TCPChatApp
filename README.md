🚀 # TCP Chat App

😊 ## Overview  
💡 This TCP-based chat application consists of a server and a client. The server listens on port 5000 for incoming connections and broadcasts messages to all connected clients (except the sender). The client connects to the server and provides a simple UI for sending and receiving chat messages.

🔄 ## Application Flows

### 🚀 Server Flow

1. **Initialization**

   - 🔌 Server starts on port 5000.
   - 👂 Listens for and accepts client connections.

2. **Handling Clients**

   - For every new client connection:
     - 📡 A `ClientHandler` is created.
     - ➕ The client is added to the server’s client list.
     - 🧵 A dedicated thread listens for messages from that client.

3. **Message Handling**
   - When a message is received:
     - 🔓 The server decrypts the incoming message.
     - 📨 The decrypted text is deserialized into an `Envelope` object.
     - 📝 If the envelope type is `"ChatMessage"`, the server logs the sender and content.
     - 📢 The server then broadcasts the original plain text message (after encryption) to all other clients.

### 💻 Client Flow

1. **Connection and UI**

   - 🌐 The client connects to the server using `TcpClient` (localhost:5000).
   - 🎨 The UI is started from WPF (`MainWindow.xaml.cs`).

2. **Sending Messages**

   - ✍️ The user types a message into the chat input.
   - 💬 The message is wrapped in a `Message` object and included in an `Envelope`.
   - 🗂️ The envelope is serialized to JSON and encrypted.
   - 📤 The encrypted message is sent to the server.

3. **Receiving Messages**
   - 👂 A background thread listens for incoming messages.
   - 🔍 Each message is read, decrypted, and deserialized into an `Envelope`.
   - 🖥️ If the envelope type is `"ChatMessage"`, the client displays the sender and message content in the chat window.
   - 👉 Otherwise, the plain text of the message is displayed.

📦 ## Data Models

- **Envelope** ✉️
  - Properties:
    - `Type` (string): Indicates the type of the message.
    - `Message` (`Message`): Contains the chat message details.
    - `User` (optional): Contains user information if needed.
- **Message** 💬
  - Properties:
    - `Sender` (string)
    - `Recipient` (string)
    - `Content` (string)
    - `Timestamp` (DateTime)

🔐 ## Encryption and Serialization

- **Encryption**: All messages are encrypted before sending and decrypted upon receipt using a shared `CryptoHelper`.
- **Serialization**: Messages are serialized to JSON for transport between client and server.

📁 ## File Structure

- **Server**
  - `TCPChatApp.Server\ClientHandler.cs`: Manages client connections and broadcasts messages.
- **Client**
  - `TCPChatApp.Client\MainWindow.xaml.cs`: Contains UI logic for sending and displaying messages.
- **Common**
  - `TCPChatApp.Common\Models\Envelope.cs`: Defines the `Envelope` structure.
  - `TCPChatApp.Common\Models\Message.cs`: Defines the `Message` structure.

🔄 ## Message Flow

1. **User Input (Client)** ⌨️

   - Type your message and click send.

2. **Processing** ⚙️

   - The client creates message and envelope objects.
   - The envelope is serialized to JSON and encrypted.

3. **Transmission** 🚀

   - The encrypted message is sent from the client to the server.
   - The server decrypts, deserializes, logs, and broadcasts the message to other clients.

4. **Display (Client)** 👀
   - Clients decrypt received messages.
   - The envelope is deserialized.
   - The message (sender and content) is displayed in the chat window.
