using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ParcelPilot.Api.Realtime;

public static class DeliveryEventHub
{
    private static readonly ConcurrentDictionary<string, Channel<string>> Subscribers = new();

    public static string Subscribe()
    {
        var id = Guid.NewGuid().ToString("N");
        var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });

        Subscribers[id] = channel;
        return id;
    }

    public static ChannelReader<string>? GetReader(string subscriberId)
    {
        if (Subscribers.TryGetValue(subscriberId, out var channel))
        {
            return channel.Reader;
        }

        return null;
    }

    public static void Unsubscribe(string subscriberId)
    {
        Subscribers.TryRemove(subscriberId, out _);
    }

    public static async Task BroadcastAsync(Guid deliveryId)
    {
        var payload = $"{{\"deliveryId\":\"{deliveryId}\"}}";

        foreach (var channel in Subscribers.Values)
        {
            await channel.Writer.WriteAsync(payload);
        }
    }
}
