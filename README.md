
# Concurrent Client/Server Implementation for Socket Communication using F#


## Introduction
This project focuses on the practical application of distributed operating system principles through the lens of socket programming using F# language. Our objective was to simulate a real-world scenario where a server manages multiple simultaneous client requests, processes their arithmetic command messages, and responds appropriately.

## Objectives
1. **Real-Time Client-Server Interactions**: Create a server application that runs continuously, accepting and processing incoming client requests.
2. **Managing Concurrent Requests**: Utilize asynchronous tasks for each client's requests, ensuring that multiple clients can interact with the server without significant delays.
3. **Command Interpretations and Response**: Handle arithmetic command messages such as addition (‘add’), subtraction (‘subtract’), and multiplication (‘multiply’), followed by their respective numeric values.
4. **Execution Handling**: Emphasize the importance of exception handling. The server is designed to recognize invalid commands or input and respond with predefined error codes.
5. **Graceful Termination**: Integrate mechanisms for clients and the server to terminate their operations gracefully.

## Environment Setup
### Basic Requirements
1. **Operating System**: Any platform compatible with the .NET SDK, including Windows, Linux, macOS.
2. **.NET SDK**: Required to execute F# applications.
3. **IDE**: Visual Studio is used for writing the F# code.

### Steps
1. Install the .NET SDK and Visual Studio.
2. Create a new directory for the F# project.
3. Write the code in multiple files in the directory, loading them in the correct order.
4. Start the server by calling `startServer()` and running it using `dotnet run`.
5. Start the client in a separate terminal by calling `startClient()` and running it.
6. Interact with the application through the client’s console interface.

## Compilation & Execution
### Server
1. Navigate to the server directory:
   ```
   cd OneDrive\Documents\Masters fall 2023\UF-CourseMaterial\Sem1\DOSP\PA1\DOSP\ServerClient\server
   ```
2. Compile the application:
   ```
   dotnet build
   ```
3. Run the application:
   ```
   dotnet run
   ```

### Client
1. Navigate to the client directory:
   ```
   cd OneDrive\Documents\Masters fall 2023\UF-CourseMaterial\Sem1\DOSP\PA1\DOSP\ServerClient\client
   ```
2. Compile the application:
   ```
   dotnet build
   ```
3. Run the application:
   ```
   dotnet run
   ```

## Code Structure
### Server Module
- Establishes a basic TCP server for client communication.
- Global variables:
  - `connectedClients`: Keeps track of connected clients.
  - `serverlock`: Ensures thread safety when altering shared resources.
  - `terminating`: Indicates whether the server is undergoing termination.

### Description of Functions
- **handleClientAsync**: Manages client-server interaction.
  - Local variables:
    - `Stream`: Retrieves a stream from the client for communication.
  - Helper functions:
    - `tryParseInt`: Parses a string into an integer.
    - `getResponseErrorCode`: Validates the data received for arithmetic operation requests.
    - `disconnectAllClients`: Safely disconnects all currently connected clients.
    - `continueHandling`: Decides if the server should continue processing client requests.

### Main Loop
- Listens to client commands like “bye”, “terminate”, or arithmetic operations like “add”.
- Sends error codes to the client as required.
- Parses and validates arithmetic operations, performs the operations, and sends results back to the client.
- Handles exceptions gracefully, closing connections after a client’s session ends.

### StartServer
- Launches the TCP server and waits for incoming client connections.
- Local variables:
  - `localEndPoint`: Specifies the port on which the server listens.
  - `Listener`: Waits for client connection requests.
  - `cancellationTokenSource`: Safely terminates asynchronous tasks.
  - `clientCounter`: Monitors the number of connected clients.
  - `isRunning`: Indicates if the server is still running.

### Termination
- Stops accepting new requests if termination is requested and disconnects existing clients.
- Displays a termination message.

## Execution Results
### Test Case 1: Arithmetic Operations
1. **Client 1 input**: `add 4 5`
   - Server Response: `Received: add 4 5`
   - Client Output: `Server response: 9`

2. **Client 2 input**: `subtract 20 7`
   - Server Response: `Received: subtract 20 7`
   - Client Output: `Server response: 13`

3. **Client 3 input**: `multiply 2 3 4`
   - Server Response: `Received: multiply 2 3 4`
   - Client Output: `Server response: 24`

### Test Case 2: Exception Handling
1. **Client 1 input**: `ads`
   - Server Response: `Received: ads`
   - Client Output: `Server response: -1`

2. **Client 2 input**: `add 1`
   - Server Response: `Received: add 1`
   - Client Output: `Server response: -2`

3. **Client 3 input**: `subtract 100 10 20 30 40 50`
   - Server Response: `Received: subtract 100 10 20 30 40 50`
   - Client Output: `Server response: -3`

4. **Client 2 input**: `multiply 6 d`
   - Server Response: `Received: multiply 6 d`
   - Client Output: `Server response: -4`

5. **Client 1 input**: `ads 1 2 3 4 5`
   - Server Response: `Received: ads 1 2 3 4 5`
   - Client Output: `Server response: -1`

### Test Case 3: Termination and Bye Commands
1. **Client 1 input**: `bye`
   - Server Response: `Received: bye`
   - Client Output: `Server response: -5`

2. **Client 3 input**: `terminate`
   - Server Response: `Received: terminate`
   - Client Output: `Server response: -5`

## Bugs, Missing Items, and Limitations
1. `Received terminate command` not printed out at server side.
2. `Received bye command` not printed out at server side.

## Additional Comments
This project was successfully realized through collective team effort. We worked through the ins and outs of the project to understand, execute, and fix all persisting issues during development.

