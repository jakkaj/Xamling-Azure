namespace Xamling.Azure.Portable.Entity
{
    public class QueueMessageWrapper<T>
        where T : class, new()
    {
        public QueueMessageWrapper(T message, string messageId, string popReceipt, int dequeueCount)
        {
            DequeueCount = dequeueCount;
            MessageId = messageId;
            PopReceipt = popReceipt;
            Message = message;
        }

        public T Message { get; }

        public string MessageId { get; }

        public string PopReceipt { get; }

        public int DequeueCount { get; }
    }
    
    
}
