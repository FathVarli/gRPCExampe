using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace grpcClient
{
    class Program
    {
        static Grpc.Core.ChannelBase channel = GrpcChannel.ForAddress("https://localhost:5001");
        static Message.MessageClient messageClient = new Message.MessageClient(channel);

        static async Task Main(string[] args)
        {

            //Unary
            await UnaryStreaming();

            //Server Streaming
            //await ServerStreaming();

            //ClientStreaming
            //await ClientStreaming();

            //Bi-Directional Streaming
            //await BiDirectionalStreaming();

            Console.ReadKey();
        }

        static async Task UnaryStreaming()
        {
            MessageResponse response = await messageClient.SendMessageUnaryAsync(new MessageRequest
            {
                Message = "Hello",
                Name = "Fatih"
            });
            Console.WriteLine(response.Message);
        }

        static async Task ServerStreaming()
        {
            var response = messageClient.SendMessageServerStreaming(new MessageRequest
            {
                Message = "Hello",
                Name = "Fatih"
            });

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            while (await response.ResponseStream.MoveNext(cancellationTokenSource.Token))
            {
                Console.WriteLine(response.ResponseStream.Current.Message);
            }

        }

        static async Task ClientStreaming()
        {
            var request = messageClient.SendMessageClientStreaming();
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                await request.RequestStream.WriteAsync(new MessageRequest
                {
                    Name = "Fatih",
                    Message = "Hello " + i
                });
            }
            await request.RequestStream.CompleteAsync(); //Stream data finished

            var response = await request.ResponseAsync;
            Console.WriteLine(response.Message);
        }

        static async Task BiDirectionalStreaming()
        {

            var request = messageClient.SendMessageBiDirectionalStreaming();

            var requestTask = Task.Run(async () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    await request.RequestStream.WriteAsync(new MessageRequest
                    {
                        Name = "Fatih",
                        Message = "Hello " + i
                    });
                }
            });
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            while (await request.ResponseStream.MoveNext(cancellationTokenSource.Token))
            {

                Console.WriteLine(request.ResponseStream.Current.Message);
            }
            await request.RequestStream.CompleteAsync();
            await requestTask;



        }
    }
}


