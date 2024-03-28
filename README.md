<h1>#### Instalar vscode, <br>
Instalar .net versao 8.0.202,<br>
Instalar docker desktop.#### </h1>

>//no vscode instalar package nos projetos Send e Received <br>
>> dotnet add package Newtonsoft.Json,<br>
dotnet add package RabbitMQ.Client,<br>
dotnet add package NRedisStack,<br>

>// somente no projeto Received
>> dotnet add package Minio 

>//adicionar referencia do projeto Received no projeto Send, pois utiliza a Classe Mensagem <br>
>> dotnet add reference 'url projeto Received'

>//executar Docker <br>
>//configurar RabbitMQ <br>
 >>guest:guest <br>
   >> docker run -d --hostname myRabbit --name rabbitMQ -p 15672:15672 -p 5672:5672 rabbitmq:3.13-management <br>
   
>//configurar Redis <br>
  >>  docker run -it --rm --name redis-stack -p 6379:6379 -p 8001:8001 redis/redis-stack:latest  //Redis version=7.2.4 <br>
  
>//configurar Minio <br>
   >> docker run  --name minio --rm -p 9000:9000 -p 9001:9001 quay.io/minio/minio server /data --console-address ":9001" <br>
   >> minioadmin:minioadmin <br>
    
>//executar o projeto Received primeiro <br>
>no terminal dentro da pasta do projeto Received 
>> dotnet run <br>

>//e em outro terminal dentro da pasta do projeto Send <br>
>> dotnet run
