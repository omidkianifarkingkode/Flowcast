namespace SharedKernel;

public interface ISoftDeletable
{
	bool IsDeleted { get; set; }

	DateTime? DeletedOnUtc { get; set; }

    string? DeleterUser { get; set; }

    bool MentionToSoftDelete { get; set; }
}
