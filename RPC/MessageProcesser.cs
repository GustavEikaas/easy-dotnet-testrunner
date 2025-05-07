using System;
using System.Text.Json;
using System.Threading.Tasks;

using EasyDotnet.MTP;
using EasyDotnet.VSTest;

namespace EasyDotnet.RPC;

public static class MessageProcesser
{

  public static readonly JsonSerializerOptions SerializerOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
  };

  private const string VSTest_Discover = "VSTest_Discover";
  private const string VSTest_Run = "VSTest_Run";
  private const string MTP_Discover = "MTP_Discover";
  private const string MTP_RUN = "MTP_Run";

  public static async Task<string> ProcessMessage(string message)
  {
    try
    {
      var request = JsonSerializer.Deserialize<RpcRequest>(message, SerializerOptions);
      if (request == null) return Messages.InvalidJson();

      switch (request.Method)
      {
        case MTP_Discover:
          try
          {
            var mtpDiscover = request.Params.Deserialize<MTP.DiscoverRequest>(SerializerOptions);
            await MTPHandler.RunDiscoverAsync(mtpDiscover);
            return Messages.Success(request.Id, mtpDiscover.OutFile);
          }
          catch (Exception ex)
          {
            Console.Error.WriteLine(ex.ToString());
            return Messages.InvalidRequest(request.Id);
          }

        case MTP_RUN:
          try
          {
            var mtpRun = request.Params.Deserialize<MTP.RunRequest>(SerializerOptions);
            await MTPHandler.RunTestsAsync(mtpRun);
            return Messages.Success(request.Id, mtpRun.OutFile);
          }
          catch (Exception ex)
          {
            Console.Error.WriteLine(ex.ToString());
            return Messages.InvalidRequest(request.Id);
          }

        case VSTest_Discover:
          try
          {
            var vsTestDiscover = request.Params.Deserialize<VSTest.DiscoverRequest>(SerializerOptions);
            VsTestHandler.RunDiscover(vsTestDiscover);
            return Messages.Success(request.Id, vsTestDiscover.OutFile);
          }
          catch (Exception ex)
          {
            Console.Error.WriteLine(ex.ToString());
            return Messages.InvalidRequest(request.Id);
          }

        case VSTest_Run:
          try
          {
            var vsTestRun = request.Params.Deserialize<VSTest.IRunRequest>(SerializerOptions);
            await VsTestHandler.RunTestsAsync(vsTestRun);
            return Messages.Success(request.Id, vsTestRun.OutFile);
          }
          catch (Exception ex)
          {
            Console.Error.WriteLine(ex.ToString());
            return Messages.InvalidRequest(request.Id);
          }

        default:
          return Messages.MethodNotFound(request.Id);
      }
    }
    catch (JsonException)
    {
      return Messages.InvalidJson();
    }
  }
}