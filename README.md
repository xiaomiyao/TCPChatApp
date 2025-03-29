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
     - 📝 Depending on the envelope type:
       - If `"ChatMessage"`, the server logs the sender and content, then broadcasts the message.
       - If `"Register"`, the server validates and registers a new user.
       - If `"Login"`, the server validates the user’s credentials and returns a login response.
       - For unknown types, a warning is logged.

### 💻 Client Flow

1. **Connection and UI**

   - 🌐 The client connects to the server using `TcpClient` (localhost:5000).
   - 🎨 The UI is started from WPF (`MainWindow.xaml.cs` and `LoginWindow.xaml.cs`).

2. **Sending Messages & User Authentication**

   - ✍️ The user types a message, registration details, or login credentials.
   - 💬 The message is bundled into a `Message` object and included in an `Envelope`.
   - 🗂️ The envelope is serialized to JSON and encrypted.
   - 📤 The encrypted message is sent to the server.

3. **Receiving Messages**
   - 👂 A background thread listens for incoming messages.
   - 🔍 Each message is read, decrypted, and deserialized into an `Envelope`.
   - 🖥️ Depending on the envelope type:
     - `"ChatMessage"`: The sender and content are displayed in the chat window.
     - `"RegistrationResponse"` or `"LoginResponse"`: The client displays the corresponding response message.
     - Otherwise, the plain text is displayed.

### 🛠 Helper Functions & UI Enhancements

- **NetworkHelper (TCPChatApp.Client\Helpers\NetworkHelper.cs):**  
  This helper function facilitates network communication by encapsulating the logic for connecting to the server, sending an encrypted message, and reading the server's response. It improves code reusability and error handling for the client-side network communication.

- **LoginWindow Updates (TCPChatApp.Client\LoginWindow.xaml.cs):**  
  The login window now leverages the helper function for sending messages to the server. It processes both registration and login requests by:
  - Collecting user input.
  - Creating and encrypting the message envelope.
  - Sending the encrypted message via the helper.
  - Decrypting and processing the server response, opening the main chat window upon successful login.

📦 ## Data Models

- **Envelope** ✉️
  - Properties:
    - `Type` (string): Indicates the type of the message (e.g., `"ChatMessage"`, `"Register"`, `"Login"`, `"RegistrationResponse"`, `"LoginResponse"`).
    - `Message` (`Message`): Contains the chat message details.
    - `User` (optional): Contains user information if needed.
- **Message** 💬
  - Properties:
    - `Sender` (string)
    - `Recipient` (string)
    - `Content` (string)
    - `Timestamp` (DateTime)

### 📝 User Registration & Authentication

- **Registration Message Structure:**  
  The envelope now supports registration details under the type `"Register"`. Registration messages include a `User` object with a username and a password hash.
- **Login Message Structure:**  
  The envelope supports login requests under the type `"Login"`. The login message includes a `User` object containing the username and password hash.

- **Server-Side Logic for Registration & Login:**  
  Upon receiving a registration request:

  - Validates that the username and password hash are not empty.
  - Checks for duplicate usernames in the in-memory store.
  - Adds the new user if valid, returning an encrypted registration response.

  Upon receiving a login request:

  - Validates the incoming login details.
  - Compares the provided credentials against the registered users.
  - Returns an encrypted login response indicating success or failure.

🔐 ## Encryption and Serialization

- **Encryption**: All messages are encrypted before sending and decrypted upon receipt using a shared `CryptoHelper`.
- **Serialization**: Messages are serialized to JSON for transport between client and server.

📁 ## File Structure

- **Server**
  - `TCPChatApp.Server\ClientHandler.cs`: Manages client connections, processes registration and login requests, and broadcasts messages.
- **Client**
  - `TCPChatApp.Client\MainWindow.xaml.cs`: Contains UI logic for sending and displaying messages.
  - `TCPChatApp.Client\LoginWindow.xaml.cs`: Handles user login and registration using updated network helper functions.
- **Common**
  - `TCPChatApp.Common\Models\Envelope.cs`: Defines the `Envelope` structure.
  - `TCPChatApp.Common\Models\Message.cs`: Defines the `Message` structure.

🔄 ## Message Flow

1. **User Input (Client)** ⌨️

   - Type your message, registration details, or login credentials and click send.

2. **Processing** ⚙️

   - The client creates message and envelope objects.
   - The envelope is serialized to JSON and encrypted.

3. **Transmission** 🚀

   - The encrypted message is sent from the client to the server using the `NetworkHelper`.
   - The server decrypts, deserializes, processes, logs, and (if appropriate) broadcasts the message to other clients while sending a response for registration or login requests.

4. **Display (Client)** 👀
   - Clients decrypt received messages.
   - The envelope is deserialized.
   - The message (sender and content) or response information is displayed in the chat window.
