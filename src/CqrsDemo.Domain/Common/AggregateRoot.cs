namespace CqrsDemo.Domain.Common;

public abstract class AggregateRoot : Entity
{
  public long Version { get; private set; }

  internal void SetVersion(long version) => Version = version;
}
