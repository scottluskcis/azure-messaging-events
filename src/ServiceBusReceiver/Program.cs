﻿using System.IO;
using Common.Client;
using Common.ServiceBus;
using Common.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ServiceBusReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            // create a new service collection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // create a service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // create the app and run
            var app = serviceProvider.GetService<ServiceBusReceiverApp>();

            // run
            app.RunAsync().GetAwaiter().GetResult();
        }
        
        private static void ConfigureServices(IServiceCollection serviceCollection)
        {  
            // build configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings.local.json", true)
                .AddEnvironmentVariables()
                .Build();

            // add logging
            serviceCollection.AddLogging((logging) =>
            {
                logging.AddConfiguration(config.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            });

            // add options
            serviceCollection.AddOptions();
            serviceCollection.Configure<MessageSenderSettings>(config.GetSection("ServiceBusSettings"));

            // add services
            serviceCollection.AddTransient<IMessageReceiver, MessageReceiver>();
            serviceCollection.AddSingleton<IClientFactory, QueueClientFactory>();

            // add the application
            serviceCollection.AddTransient<ServiceBusReceiverApp>();
        }
    }
}
