namespace CqrsDemo.BuildingBlocks.Domain;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(Guid streamId, long expectedVersion, long actualVersion)
        : base($"Concurrency conflict on stream {streamId}. Expected {expectedVersion}, actual {actualVersion}.")
    {
        StreamId = streamId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public Guid StreamId { get; }
    public long ExpectedVersion { get; }
    public long ActualVersion { get; }
}
