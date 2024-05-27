module client

open System
open System.Net.Sockets
open System.Text
open System.Threading

let startClient() =
    let client = new TcpClient()
    client.Connect("127.0.0.1", 8080)
    let stream = client.GetStream()

    let helloArray = Array.zeroCreate 32
    let helloRBytes = stream.Read(helloArray, 0, helloArray.Length)
    let helloMessage = Encoding.ASCII.GetString(helloArray, 0, helloRBytes)
    printfn "%s" helloMessage

    let mutable keepRunning = true

    // Thread to listen to server messages
    let listenToServer () = 
        Thread(ThreadStart(fun () ->
            while keepRunning do
                let bytes = Array.zeroCreate 1024
                let readBytes = stream.Read(bytes, 0, bytes.Length)
                if readBytes = 0 then  // server disconnected
                    printfn "Server has disconnected."
                    keepRunning <- false
                else
                    let responseData = Encoding.ASCII.GetString(bytes, 0, readBytes)
                    printfn "Server response: %s" responseData
                    // Check for a terminate broadcast from the server
                    if responseData.Trim() = "terminate" || responseData.Trim() = "-5" then
                        printfn "Received termination signal from server. Exiting..."
                        keepRunning <- false
        ))

    // Start listening to server messages
    let serverThread = listenToServer()
    serverThread.Start()

    try
        while keepRunning do
            if Console.KeyAvailable then
                printf "Enter numbers to send (or type 'bye' to quit or 'terminate' to shutdown server): "
                let input = Console.ReadLine()

                if input = "bye" || input = "terminate" then
                    keepRunning <- false

                let data = Encoding.ASCII.GetBytes(input)
                stream.Write(data, 0, data.Length)
                printfn "Sending command: %s" input

            Thread.Sleep(100)  // sleep to prevent high CPU usage

    finally
        stream.Close()
        client.Close()