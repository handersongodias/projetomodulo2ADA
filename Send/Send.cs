using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using Newtonsoft.Json;
// using JsonSerializer = System.Text.Json.JsonSerializer;

internal class Send
{
    /// <summary>
    /// variaveis diversas
    /// </summary>
    static readonly string fileJson = @"fileJson\list.json";
    static void Main(string[] args) { GetList(); }
    /// <summary>
    /// Recuperar a lista de mensagens do arquivo , convertendo para objeto Mensagem
    /// </summary>
    static void GetList()
    {
       
        var nameFile = AppDomain.CurrentDomain.BaseDirectory.Split("bin")[0] + fileJson;

        List<Mensagem>? listMensagem = new List<Mensagem>();
        using (StreamReader openFile = new StreamReader(nameFile))
        {
            string json = openFile.ReadToEnd();

            if (!string.IsNullOrEmpty(json))
            {
                listMensagem = JsonConvert.DeserializeObject<List<Mensagem>>(json);
            }

            openFile.Close();
        }
        if (listMensagem != null)
        {
            foreach (Mensagem mensage in listMensagem)
            {
                SendMessage(System.Text.Json.JsonSerializer.Serialize(mensage));
                Thread.Sleep(5000);
            }
        }

    }
    /// <summary>
    /// Enviar mensagens para o RabbitMQ
    /// </summary>
    /// <param name="mensagem"></param>
    static void SendMessage(string mensagem)
    {
        var factory = new ConnectionFactory { HostName = "localhost", Port = 5672 };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "hello",
                      durable: false,
                      exclusive: false,
                      autoDelete: false,
                      arguments: null);
        Console.WriteLine("RabbitMQ conectado");

        var body = Encoding.UTF8.GetBytes(mensagem);

        channel.BasicPublish(exchange: string.Empty,
                             routingKey: "hello",
                             basicProperties: null,
                             body: body);
        Console.WriteLine($" [x] Mensagem enviada {mensagem}");
       
    }
}