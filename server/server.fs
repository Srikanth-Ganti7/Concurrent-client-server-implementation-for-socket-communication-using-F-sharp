module server

open System
open System.Net
open System.Net.Sockets
open System.Text
open System.Collections.Concurrent
open System.Threading.Tasks
open System.Threading

let connectedClients = ConcurrentDictionary<TcpClient, unit>()
let serverLock = obj() // Lock object
let mutable terminating = false

let handleClientAsync (counter: int) (client: TcpClient) (cts: CancellationTokenSource) =
    async {
        try
            use stream = client.GetStream()
            let bytes = Array.zeroCreate 1024
            let mutable continueHandling = true

            let hello = Encoding.ASCII.GetBytes("Hello!")
            do! stream.WriteAsync(hello, 0, hello.Length) |> Async.AwaitTask

            let tryParseInt (str: string) = 
                let mutable value = 0
                let success = Int32.TryParse(str, &value)
                if success then Some value else None

            let getResponseErrorCode (numbers: string[]) =
                if not(numbers.[0].ToLower() = "add" || numbers.[0].ToLower() = "subtract" || numbers.[0].ToLower() ="multiply") then -1
                elif numbers.Length < 3 then -2
                elif numbers.Length > 5 then -3
                elif numbers.[1..] |> Array.exists (fun x -> tryParseInt x = None) then -4
                else 0        

            let disconnectAllClients () =
                lock serverLock (fun () ->
                    for client in connectedClients.Keys do
                        try
                            client.Close()
                            connectedClients.TryRemove(client) |> ignore
                        with
                            | ex -> printfn "Error disconnecting client: %A" ex
                )

            let broadcastMessage (message: string) =
                let data = Encoding.ASCII.GetBytes(message)
                for kvp in connectedClients do
                    try
                        let clientStream = kvp.Key.GetStream()
                        if clientStream.CanWrite then
                            clientStream.Write(data, 0, data.Length)
                    with
                    | :? System.IO.IOException -> 
                        printfn "Failed to send message to client: %A. It might have disconnected." kvp.Key.Client.RemoteEndPoint
                    | ex ->
                        printfn "Error sending message: %s" ex.Message

            while continueHandling do
                let! readBytes = stream.ReadAsync(bytes, 0, bytes.Length) |> Async.AwaitTask
                if readBytes > 0 then
                    let data = Encoding.ASCII.GetString(bytes, 0, readBytes)
                    if data = "terminate" then
                        lock serverLock (fun () ->
                            terminating <- true
                            continueHandling <- false
                            let response = "-5"
                            printfn "Responding to client %d with result %s" counter response
                            let responseData = Encoding.ASCII.GetBytes(response)
                            stream.Write(responseData, 0, responseData.Length)
                            broadcastMessage "terminate"
                            cts.Cancel()
                            disconnectAllClients()
                        )
                    elif data = "bye" then
                        continueHandling <- false
                        let response = "-5"
                        printfn "Responding to client %d with result %s" counter response
                        let responseData = Encoding.ASCII.GetBytes(response)
                        stream.Write(responseData, 0, responseData.Length)
                    else
                        printfn "Received: %s" data
                        let numbers = data.Split(' ')

                        let errorCode = getResponseErrorCode numbers
                        let response =
                            match errorCode with
                            | -1 -> "-1"
                            | -2 -> "-2"
                            | -3 -> "-3"
                            | -4 -> "-4"
                            | _ ->
                                let operation = numbers.[0].ToLower()
                                let operands = [
                                    for x in numbers.[1..] do
                                        let parsed = tryParseInt x
                                        if parsed.IsSome then yield parsed.Value
                                ]
                            
                                if List.length operands >= 2 && List.length operands <= 4 then
                                    let mutable result = operands.[0]
                                    match operation with
                                    | "add" ->
                                        for i in 1..(List.length operands - 1) do
                                            result <- result + operands.[i]
                                        result.ToString()
                                    | "multiply" ->
                                        for i in 1..(List.length operands - 1) do
                                            result <- result * operands.[i]
                                        result.ToString()
                                    | "subtract" when List.length operands = 2 ->
                                        result <- result - operands.[1]
                                        result.ToString()
                                    | _ -> "-1"
                                else
                                    "-4"
                        printfn "Responding to client %d with result %s" counter response
                        let responseData = Encoding.ASCII.GetBytes(response)
                        stream.Write(responseData, 0, responseData.Length)
                else
                    continueHandling <- false

            printfn "Finished serving client: %A" client.Client.RemoteEndPoint
            connectedClients.TryRemove(client) |> ignore
        finally
            client.Close()
    }

let startServer () =
    let localEndPoint = IPEndPoint(IPAddress.Loopback, 8080)
    let listener = new TcpListener(localEndPoint)
    listener.Start()

    let cts = new CancellationTokenSource()

    let port = (listener.LocalEndpoint :?> IPEndPoint).Port
    printfn "Server is running and listening on port %d" port

    let mutable clientCounter = 1
    let mutable isRunning = true

    while isRunning && not cts.Token.IsCancellationRequested do
        if not terminating then
            let clientTask = listener.AcceptTcpClientAsync()
            let hasClientConnected = clientTask.Wait(500)

            if hasClientConnected then
                let client = clientTask.Result
                lock serverLock (fun () ->
                    if not terminating then
                        let _ = connectedClients.TryAdd(client, ())
                        handleClientAsync clientCounter client cts |> Async.Start
                        clientCounter <- clientCounter + 1
                )
        else
            Thread.Sleep(500)  // Wait for a bit before checking the token again if we're terminating

        if cts.Token.IsCancellationRequested then
            isRunning <- false
            for kvp in connectedClients do
                kvp.Key.Close()
            connectedClients.Clear()

    printfn "Termination requested. Stopping the server..."
    listener.Stop()
