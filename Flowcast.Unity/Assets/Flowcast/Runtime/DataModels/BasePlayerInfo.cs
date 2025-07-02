namespace Flowcast.Data
{
    public class BasePlayerInfo
    {
        public long PlayerId { get; set; }
        public string DisplayName { get; set; }
        public PlayerStatus Status { get; set; } = PlayerStatus.Unknown;
        public int Score { get; set; } = 0;
    }

}
