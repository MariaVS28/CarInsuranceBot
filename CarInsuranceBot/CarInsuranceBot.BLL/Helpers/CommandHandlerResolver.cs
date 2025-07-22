using Microsoft.Extensions.DependencyInjection;

namespace CarInsuranceBot.BLL.Helpers
{
    public class CommandHandlerResolver : ICommandHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandHandlerResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Resolve<T>() => _serviceProvider.GetRequiredService<T>();
    }

}
