using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using System.Text;
namespace GameRankAuth.Services.RabbitMQ;

public class RabbitMQService
{
    
    public RabbitMQService()
    {
        
        
        
    }

    public async Task Send()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",

        };
        using var connection = await factory.CreateConnectionAsync();
        using var chanell = await connection.CreateChannelAsync();
       
        await chanell.QueueDeclareAsync(queue:"Admin" , durable:false , exclusive:false , autoDelete:false , arguments:null );
        const string message = $"User is Admin ";
        var body = Encoding.UTF8.GetBytes(message);
        
        await chanell.BasicPublishAsync(exchange: string.Empty, routingKey: "Admin", body: body);
        
    }
}