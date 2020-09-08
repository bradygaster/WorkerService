using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static HeartbeatService.Heartbeat;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        //private HeartbeatClient _heartbeatClient;
        private HttpClient _httpClient;

        public Worker(ILogger<Worker> logger, 
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            try
            {
                // -----------------
                // you'd want to remove this line for production use, once your certs are prod
                //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                // -----------------
                
                //var channel = GrpcChannel.ForAddress("http://grpcservice");
                //_heartbeatClient = new HeartbeatClient(channel);
                
                _httpClient = _httpClientFactory.CreateClient("heartbeat");
            }
            catch(Exception x)
            {
                _logger.LogError(x, "Erorr during startup");
            }
            await base.StartAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _httpClient?.Dispose();
            await base.StopAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

#pragma warning disable CS4014
                    
                    Task.Run(() => {
                        var json = _httpClient.GetStringAsync("http://heartbeat/").Result;
                        var instanceInfo = JsonSerializer.Deserialize<InstanceInfo>(json);
                        _logger.LogInformation($"Internal API from host {instanceInfo.HostName} received at {instanceInfo.HostTimeStamp}");

                        //try
                        //{
                        //    var reply = _heartbeatClient.ReceiveHeartbeat(new HeartbeatService.HeartbeatMessage
                        //    {
                        //        HostName = instanceInfo.HostName,
                        //        HostTimeStamp = Timestamp.FromDateTime(instanceInfo.HostTimeStamp)
                        //    });

                        //    _logger.LogInformation($"Heartbeat received with success: {reply.Success}");
                        //}
                        //catch(Exception x)
                        //{
                        //    _logger.LogError(x, "Error calling gRPC service");
                        //}
                    });   

#pragma warning restore CS4014

                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error during HTTP request");
                }

                await Task.Delay(500, stoppingToken);
            }
        }
    }
}
