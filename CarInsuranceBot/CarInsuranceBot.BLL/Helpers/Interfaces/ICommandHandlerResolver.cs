namespace CarInsuranceBot.BLL.Helpers
{
    public interface ICommandHandlerResolver
    {
        T Resolve<T>();
    }
}
