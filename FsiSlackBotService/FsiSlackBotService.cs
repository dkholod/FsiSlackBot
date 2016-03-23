namespace FsiSlackBotService
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using BotHost;

    using Microsoft.ServiceFabric.Services.Runtime;

    public class FsiSlackBotService : StatelessService
    {
        protected override async Task RunAsync(CancellationToken cancelServiceInstance)
        {
            BotBootstrap.initBot();

            var iterations = 0;
            while (!cancelServiceInstance.IsCancellationRequested)
            {
                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", iterations++);
                await Task.Delay(TimeSpan.FromSeconds(5), cancelServiceInstance);
            }
        }
    }
}