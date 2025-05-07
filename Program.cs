using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EasyDotnet.RPC;
class Program
{
  private const string PipeName = "EasyDotnetPipe";
  private static readonly ConcurrentDictionary<Guid, NamedPipeServerStream> ActiveConnections = new();
  private static readonly CancellationTokenSource Cts = new();

  public static async Task<int> Main(string[] args)
  {
    Console.WriteLine($"Named pipe server started: {PipeName}");
    var serverTask = StartServerAsync(Cts.Token);

    //TODO: There has to be a better way to do this??
    Console.ReadKey();
    Cts.Cancel();

    try
    {
      await serverTask;
    }
    catch (OperationCanceledException)
    {
      Console.WriteLine("Server shutdown initiated");
    }

    ActiveConnections.ToList().ForEach(x =>
    {
      x.Value.Disconnect();
      x.Value.Dispose();
    });


    Console.WriteLine("Server shut down successfully");
    return 0;
  }

  private static async Task StartServerAsync(CancellationToken token)
  {
    var connectionTasks = new List<Task>();

    while (!token.IsCancellationRequested)
    {
      try
      {
        var pipe = new NamedPipeServerStream(
            PipeName,
            PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        Console.WriteLine("Waiting for client connection...");
        await pipe.WaitForConnectionAsync(token);

        Guid clientId = Guid.NewGuid();
        Console.WriteLine($"Client {clientId} connected");

        ActiveConnections[clientId] = pipe;

        var clientTask = HandleClientAsync(pipe, clientId, token);
        connectionTasks.Add(clientTask);
      }
      catch (OperationCanceledException)
      {
        break;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error in server loop: {ex.Message}");
        await Task.Delay(1000, token);
      }
    }

    await Task.WhenAll(connectionTasks);
  }

  private static async Task HandleClientAsync(NamedPipeServerStream pipe, Guid clientId, CancellationToken token)
  {
    try
    {
      using StreamReader reader = new(pipe);
      using StreamWriter writer = new(pipe) { AutoFlush = true };

      while (pipe.IsConnected && !token.IsCancellationRequested)
      {
        try
        {
          string message = await reader.ReadLineAsync(token);
          if (message == null)
            break;

          string response = await MessageProcesser.ProcessMessage(message);
          await writer.WriteLineAsync(response);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Error reading from client {clientId}: {ex.Message}");
          if (!pipe.IsConnected)
            break;
        }
      }
    }
    catch (IOException ex)
    {
      Console.WriteLine($"Client {clientId} disconnected: {ex.Message}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error handling client {clientId}: {ex.Message}");
    }
    finally
    {
      Console.WriteLine($"Client {clientId} disconnected");
      ActiveConnections.TryRemove(clientId, out _);

      if (pipe.IsConnected)
      {
        pipe.Disconnect();
      }
      pipe.Dispose();
    }
  }
}