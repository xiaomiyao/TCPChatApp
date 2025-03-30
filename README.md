🚀 # TCP Chat App

😊 ## Overview  
💡 This TCP-based chat application consists of a server and a client. The server listens on port 5000 for incoming connections and broadcasts messages to all connected clients (except the sender). The client connects to the server and provides a simple UI for sending and receiving chat messages.

🔄 ## Application Flows

### 🚀 Server Flow

1. **Initialization**

   - 🔌 Server starts on port 5000.
   - 👂 Listens for and accepts client connections.
   - 💾 **Database Integration:** The server now uses a SQL Server database through a `UserRepository` for secure user storage. 🗄️

2. **Handling Clients**

   - For every new client connection:
     - 📡 A `ClientHandler` is created.
     - ➕ The client is added to the server’s client list.
     - 🧵 A dedicated thread listens for messages from that client.
     - 🔥 Improved thread handling and error logging have been implemented.

3. **Message Handling**
   - When a message is received:
     - 🔓 The server uses the `MessageProcessor` to decrypt and deserialize the incoming message.
     - 📨 The `ClientHandler` now delegates:
       - **Chat messages** to `ChatMessageHandler` for logging and broadcasting.
       - **Registration requests** to `AuthenticationHandler` for validating and storing user info via `UserRepository`.
       - **Login requests** to `AuthenticationHandler` for credential validation and updating the online users list.

### 💻 Client Flow

1. **Connection and UI**

   - The client connects to the server using `TcpClient` (127.0.0.1:5000) with robust error handling 🚀.
   - The UI is started from WPF (`MainWindow.xaml.cs` and `LoginWindow.xaml.cs`).
   - The online users list is updated in the chat window upon successful login.
   - Enhanced UI responsiveness and error handling have been added 👍.

2. **Sending Messages & User Authentication**

   - ✍️ The user types a message, registration details, or login credentials.
   - 💬 The message is bundled into a `Message` object and included in an `Envelope`.
   - 🗂️ The envelope is serialized to JSON and encrypted.
   - 📤 The encrypted message is sent to the server using the helper function in `NetworkHelper`.
   - 🚀 New encryption flows ensure that all messages are securely transmitted.

3. **Receiving Messages**
   - 👂 A background thread listens for incoming messages.
   - 🔍 Each message is read, decrypted, and deserialized into an `Envelope`.
   - 🖥️ Depending on the envelope type:
     - `"ChatMessage"`: The sender and content are displayed in the chat window.
     - `"RegistrationResponse"` or `"LoginResponse"`: The client displays the corresponding response message. For login responses, the message now also includes the current online users list.
     - Otherwise, the plain text is displayed.

### 🛠 Helper Functions & UI Enhancements

- **NetworkHelper (TCPChatApp.Client\Helpers\NetworkHelper.cs):**  
  This helper function facilitates network communication by encapsulating the logic for connecting to the server, sending an encrypted message, and reading the server's response. 🧩 It improves code reusability and error handling for the client-side network communication.

- **LoginWindow Updates (TCPChatApp.Client\LoginWindow.xaml.cs):**  
  The login window now leverages the helper function for sending messages to the server. It processes both registration and login requests by:
  - Collecting user input.
  - Creating and encrypting the message envelope.
  - Sending the encrypted message via the helper.
  - Decrypting and processing the server response, opening the main chat window upon successful login and updating the online users list.
  - 💥 Enhanced error response handling is now in place.

📦 ## Data Models

- **Envelope** ✉️
  - Properties:
    - `Type` (string): Indicates the type of the message (e.g., `"ChatMessage"`, `"Register"`, `"Login"`, `"RegistrationResponse"`, `"LoginResponse"`, `"OnlineUsers"`).
    - `Message` (`Message`): Contains the chat message details.
    - `User` (optional): Contains user information if needed.
    - `Users` (optional): A list of online users returned with certain responses (such as a successful login).
- **Message** 💬
  - Properties:
    - `Sender` (string)
    - `Recipient` (string)
    - `Content` (string)
    - `Timestamp` (DateTime)
- **User** 👤
  - Properties:
    - `Id` (`Guid`): Uniquely identifies the user.
    - `Username` (string)
    - `PasswordHash` (string)
- **Contact** 📇
  - Properties:
    - `Id` (int)
    - `OwnerUserId` (`Guid`): The ID of the user who owns the contact.
    - `ContactUserId` (`Guid`): The ID of the contact user.
    - `ContactName` (string): The display name for the contact.
    - `AddedDate` (`DateTime`): The date the contact was added.

##- **Contact** 📇

- Properties:
  - `Id` (int)
  - `OwnerUserId` (`Guid`): The ID of the user who owns the contact.
  - `ContactUserId` (`Guid`): The ID of the contact user.
  - `ContactName` (string): The display name for the contact.
  - `AddedDate` (`DateTime`): The date the contact was added.

# 📝 User Registration & Authentication

- **Registration Message Structure:**  
  The envelope now supports registration details under the type `"Register"`. Registration messages include a `User` object with a username and a password hash. 🔐
- **Login Message Structure:**  
  The envelope supports login requests under the type `"Login"`. The login message includes a `User` object containing the username and password hash.
- **Server-Side Logic:**
  - Validates that the username and password hash are provided.
  - Checks against duplicates using the database.
  - Adds new users through `UserRepository` and returns proper encrypted responses. ✅
  - Compares provided credentials during login and returns an encrypted response that includes the current online users list upon success. 🔑

🔐 ## Encryption and Serialization

- **Encryption**: All messages are encrypted and decrypted using the `CryptoHelper`, while the `MessageProcessor` handles serialization and deserialization. 🔒
- **Serialization**: Messages are serialized to JSON for transport between client and server. ✨

📁 ## File Structure

- **Server**
  - `TCPChatApp.Server\ClientHandler.cs`: Manages client connections. The message processing logic now leverages `MessageProcessor` to decrypt incoming messages and delegates authentication to `AuthenticationHandler` and chat message handling to `ChatMessageHandler`.
  - `TCPChatApp.Server\AuthenticationHandler.cs`: Handles registration and login logic separately from the client connection logic.
  - `TCPChatApp.Server\ChatMessageHandler.cs`: Handles chat message logging and broadcasting.
  - `TCPChatApp.Server\DataAccess\UserRepository.cs`: Provides methods to retrieve and store user data in SQL Server.
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
   - On the server side, the `MessageProcessor` is used for all encryption/decryption, and the response (including updated online users list for login) is sent back accordingly.

4. **Display (Client)** 👀
   - Clients decrypt received messages.
   - The envelope is deserialized.
   - The message (sender and content), registration response, or login response (with the updated online users list) is displayed in the UI.

✨ Happy coding and enjoy the new enhancements! ✨
