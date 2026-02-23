namespace SharedKernel;

public interface IModifiable
{
	DateTimeOffset? ModifiedAtUtc { get; set; }

	string? ModifierUser { get; set; }
}
