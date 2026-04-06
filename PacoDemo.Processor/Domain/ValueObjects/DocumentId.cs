namespace PacoDemo.Processor.Domain.ValueObjects;

public record DocumentId
{
    public Guid Value { get; }

    public DocumentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DocumentId cannot be empty.");
        Value = value;
    }

    public static DocumentId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
