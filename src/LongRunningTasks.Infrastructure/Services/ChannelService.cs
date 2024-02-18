using LongRunningTasks.Application.Services;
using System.Threading.Channels;

namespace LongRunningTasks.Infrastructure.Services
{
    public class ChannelService<T> : IChannelService<T>
    {
        private readonly Channel<T> _channel;

        public ChannelService(int capacity)
        {
            BoundedChannelOptions options = new(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<T>(options);
        }

        public async Task QueueAsync(T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            await _channel.Writer.WriteAsync(item);
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken) =>
            await _channel.Reader.ReadAsync(cancellationToken);
    }
}