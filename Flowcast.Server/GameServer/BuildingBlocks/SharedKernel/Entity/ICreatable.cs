namespace SharedKernel;

public interface ICreatable
{
	DateTimeOffset CreatedAtUtc { get; set; }

	string? CreatorUser { get; set; }
}
