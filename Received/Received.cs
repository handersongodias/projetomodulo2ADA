using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using Newtonsoft.Json;
using Minio;
using Minio.Exceptions;
using Minio.DataModel.Args;


internal class Received
{

    public string path = AppDomain.CurrentDomain.BaseDirectory.Split("bin")[0];
    /// <summary>
    /// Metodo Main
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        new Received().GetMensagem();
    }
    /// <summary>
    /// Recupera as mensagens enviadas que estao no RabbitMQ
    /// </summary>
    void GetMensagem()
    {
        var factory = new ConnectionFactory { HostName = "localhost", Port = 5672 };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "hello",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        Console.WriteLine(" [*] Aguardando mensagens...");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var mensagem = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] Mensagem Recebida {mensagem}");
            ///inserir no redis
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redis.GetDatabase();

            Mensagem? mensagemDeserializada = JsonConvert.DeserializeObject<Mensagem>(mensagem);

            ///verifica se a chave existe no REDIS
            var redisValue = db.StringGet(mensagemDeserializada.Id?.ToString());
            var getMensagemRedis = redisValue.HasValue ? JsonConvert.DeserializeObject<Mensagem>(redisValue) : null;
            ///caso a chave nao exista é criada no redis
            if (getMensagemRedis == null)
            {
                mensagemDeserializada.ValorMaiorCompra = mensagemDeserializada.ValorCompra;
                db.StringSet(mensagemDeserializada.Id, JsonConvert.SerializeObject(mensagemDeserializada));
            }
            else
            {//caso a chave exista é verificado os valores 
                ///validar se o valor esta muito mais alto e pode ser fraude
                if (Convert.ToInt32(mensagemDeserializada.ValorCompra) >= (Convert.ToInt32(getMensagemRedis.ValorMaiorCompra) * 1.5))
                {
                    string nmArquivo;
                    ///gerar o arquivo com os dados
                    GerarArquivo(mensagem, mensagemDeserializada.Id, out nmArquivo);
                    //escrever arquivo primeiro e depois enviar para minio
                    Console.WriteLine("Transação com suspeita de fraude!");
                    string nameFile = path + nmArquivo;
                    ///enviar o arquivo para o minio e gera o link
                    new SendFile().EnviarArquivoMinio(nameFile, nmArquivo);

                }
                else
                {
                    ///atualiza os valores no redis

                    if (Convert.ToInt32(mensagemDeserializada.ValorCompra) >= Convert.ToInt32(getMensagemRedis.ValorMaiorCompra))
                    {
                        mensagemDeserializada.ValorMaiorCompra = mensagemDeserializada.ValorCompra;

                    }
                    else
                    {
                        mensagemDeserializada.ValorMaiorCompra = getMensagemRedis.ValorMaiorCompra;
                    }
                    db.StringGetDelete(getMensagemRedis.Id);
                    db.StringSet(mensagemDeserializada.Id, JsonConvert.SerializeObject(mensagemDeserializada));

                }
            }

        };
        channel.BasicConsume(queue: "hello",
                             autoAck: true,
                             consumer: consumer);
        Console.WriteLine(" Pressione [enter] para sair!.");
        Console.ReadLine();
    }
    /// <summary>
    /// Gerar arquivo com os dados da fraude
    /// </summary>
    /// <param name="mensagem"></param>
    /// <param name="id"></param>
    /// <param name="nmArquivo"></param>
    void GerarArquivo(string mensagem, string id, out string nmArquivo)
    {
        nmArquivo = "fraude_usuario_" + id + "_" + Guid.NewGuid().ToString() + ".txt";

        var nameFile = path + nmArquivo;

        using (var arquivo = File.Open(nameFile, FileMode.OpenOrCreate))
        {
            arquivo.Write(Encoding.UTF8.GetBytes('\n' + mensagem));
            arquivo.Flush();

        };
    }

}