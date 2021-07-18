using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace grpcServer.Services
{
    public class MessageService : Message.MessageBase
    {
        public override Task<MessageResponse> SendMessageUnary(MessageRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Type : Unary | Message : {request.Message} | Name : {request.Name}");
            return Task.FromResult(new MessageResponse
            {
                Message = "Message received successfully!"
            });
        }

        public override async Task SendMessageServerStreaming(MessageRequest request, IServerStreamWriter<MessageResponse> responseStream, ServerCallContext context)
        {
            Console.WriteLine($"Type : ServerStreaming | Message : {request.Message} | Name : {request.Name}");

            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(200);
                await responseStream.WriteAsync(new MessageResponse
                {
                    Message = "Hello " + i
                });
            }
        }

        public override async Task<MessageResponse> SendMessageClientStreaming(IAsyncStreamReader<MessageRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext(context.CancellationToken))
            {
                Console.WriteLine($"Type : ClientStreaming | Message : {requestStream.Current.Message} | Name : {requestStream.Current.Name}");
            }
            return new MessageResponse
            {
                Message = "Message has stream data!"
            };
        }

        public override async Task SendMessageBiDirectionalStreaming(IAsyncStreamReader<MessageRequest> requestStream, IServerStreamWriter<MessageResponse> responseStream, ServerCallContext context)
        {
            try
            {
                if(!await requestStream.MoveNext())
                {
                    return;
                }

                var requestTask = Task.Run(async () =>
                {
                    while (await requestStream.MoveNext(context.CancellationToken))
                    {
                        Console.WriteLine($"Type : BiDirectionalStreaming | Message : {requestStream.Current.Message} | Name : {requestStream.Current.Name}");
                    }
                });

                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    await responseStream.WriteAsync(new MessageResponse
                    {
                        Message = "Message " + i
                    });
                }

                await requestTask;
            }
            catch (Exception)
            {
                Console.WriteLine("Client was closed");
                
            }
        }
    }
}
