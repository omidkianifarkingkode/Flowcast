namespace Realtime.Transport.Routing.Options;

public sealed class RoutingOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>Number of worker partitions. Defaults to Environment.ProcessorCount.</summary>
    public int Partitions { get; set; } = Math.Max(1, Environment.ProcessorCount);

    /// <summary>Queue capacity per partition (bounded backpressure).</summary>
    public int PartitionQueueCapacity { get; set; } = 512;
}