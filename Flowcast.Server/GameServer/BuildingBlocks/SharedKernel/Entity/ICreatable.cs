namespace SharedKernel;

public interface ICreatable
{
	DateTime CreatedOnUtc { get; set; }

	string? CreatorUser { get; set; }
}
