namespace SharedKernel;

public interface IModifiable
{
	DateTime? ModifiedOnUtc { get; set; }

	string? ModifierUser { get; set; }
}
