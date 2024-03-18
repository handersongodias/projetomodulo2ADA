dot.net versao 8.0.202,
//instalar package
dotnet add package Newtonsoft.Json,
dotnet add package RabbitMQ.Client,
dotnet add package NRedisStack,

dotnet add package Minio // no projeto Received,

//adicionar referencia do projeto Received no projeto Send, pois utiliza a Classe Mensagem
dotnet add reference 'url projeto Received'

//executar Docker
//configurar RabbitMQ
    guest:guest
    docker run -d --hostname myRabbit --name rabbitMQ -p 15672:15672 -p 5672:5672 rabbitmq:3.13-management
//configurar Redis
    docker run -it --rm --name redis-stack -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
//configurar Minio
    docker run  --name minio --rm -p 9000:9000 -p 9001:9001 quay.io/minio/minio server /data --console-address ":9001"
    minioadmin:minioadmin

