## RabbitMQ .NET Client (Unofficial CoreClr version)
This repository contains source code of the [RabbitMQ .NET client](http://www.rabbitmq.com/dotnet.html).
The original RabbitMQ .Net client is maintained by the [RabbitMQ team at Pivotal](http://github.com/rabbitmq/).


## Working in IPv6 networks (including working in Docker)
var factory = new ConnectionFactory()
{
  HostName = "localhost" ,
  SocketFactory = family =>
  {
    var tcpClient = new TcpClient(AddressFamily.InterNetwork);
    return new TcpClientAdapter(tcpClient);
  }
};


## NuGet artifacts
https://www.nuget.org/packages/RabbitMQ.Client.CoreClrUnofficial/


## Tutorials and Documentation

 * [Tutorials](http://www.rabbitmq.com/getstarted.html)
 * [Documentation guide](http://www.rabbitmq.com/dotnet.html)


## Contributing

See [Contributing](./CONTRIBUTING.md) and [How to Run Tests](./RUNNING_TESTS.md).


## License

This package, the RabbitMQ .NET client library, is double-licensed under
the Mozilla Public License 1.1 ("MPL") and the Apache License version 2 ("ASL").
