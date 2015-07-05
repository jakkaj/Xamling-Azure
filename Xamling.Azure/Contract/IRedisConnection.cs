using StackExchange.Redis;

namespace Xamling.Azure.Contract
{
    public interface IRedisConnection
    {
        IDatabase GetDatabase();
        ISubscriber GetSubscriber();
    }
}