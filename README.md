# TCP Chat App Flow

## Server Flow

1. 🎯 Server starts on port 5000
2. 👥 Waits for client connections
3. ⚡ For each new client:
   - Creates client handler
   - Starts message listener
   - Adds to client list
4. 📨 When message received:
   - Decrypts message
   - Broadcasts to all other clients
   - Logs activity

## Client Flow

1. 🔌 Connect to server (localhost:5000)
2. 🎮 Start UI
3. 🔄 Two parallel processes:

   ### Sending:

   - User types message
   - Message gets encrypted
   - Sends to server

   ### Receiving:

   - Listen for server messages
   - Decrypt incoming messages
   - Display in chat window

## Message Flow

1. ✍️ User Input
   - Type message
   - Click send
2. 🔒 Processing
   - Encrypt message
   - Create message object
   - Serialize data
3. 📤 Transmission
   - Client sends to server
   - Server receives
   - Server broadcasts
4. 📱 Display
   - Other clients receive
   - Decrypt message
   - Show in chat window
