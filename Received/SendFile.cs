using Minio;
using Minio.Exceptions;
using Minio.DataModel.Args;
using StackExchange.Redis;
internal class SendFile{

/// <summary>
/// Enviar o arquivo para o Minio
/// </summary>
/// <param name="nameFile"></param>
/// <param name="nmArquivo"></param>
public void EnviarArquivoMinio(string nameFile, string nmArquivo)
    {

        var endpoint = "127.0.0.1:9000";
        var accessKey = "minioadmin";
        var secretKey = "minioadmin";
        string bucketName = "bucketproject";
        try
        {
            IMinioClient minioClient = new MinioClient()
                              .WithEndpoint(endpoint)
                              .WithCredentials(accessKey, secretKey)
                              .Build();
            BucketExistsArgs args = new BucketExistsArgs().WithBucket(bucketName);
            MakeBucketArgs args2 = new MakeBucketArgs().WithBucket(bucketName);

            //Verificar se existe o bucket.
            bool found =  minioClient.BucketExistsAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
            if (found)
            {
              //  Console.WriteLine(bucketName + " já existe");
            }
            else
            {
                // Cria bucket.
                minioClient.MakeBucketAsync(args2).ConfigureAwait(false).GetAwaiter().GetResult();
                Console.WriteLine(bucketName + " criado com sucesso!");
            }
            PutObjectArgs putObjectArgs = new PutObjectArgs()
                            .WithBucket(bucketName)
                            .WithObject(nmArquivo).WithFileName(nameFile)
                            .WithContentType("application/text");
            var result = minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false).GetAwaiter().GetResult();
            var urlFile = minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs().WithBucket(bucketName).WithExpiry(25200).WithObject(nmArquivo)).ConfigureAwait(false).GetAwaiter().GetResult();
        
            Console.WriteLine(urlFile);
            minioClient.Dispose();
            ///enviar link para o redis
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redis.GetDatabase();         
            var hash = new HashEntry[] { 
            new HashEntry("link", urlFile)
            };
            
            db.HashSet("link_"+nmArquivo, hash);

        }
        catch (MinioException e)
        {
            Console.WriteLine("Houve um erro ao enviar o arquivo: " + e);
        }

    }



}