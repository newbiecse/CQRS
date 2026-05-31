namespace CqrsDemo.BuildingBlocks.Domain;

public abstract class AggregateRoot : Entity
{
    public long Version { get; private set; }
    public void SetVersion(long version) => Version = version;
}
