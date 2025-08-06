using SharedKernel;

namespace Domain.Games
{
    public class UserProgress : Entity<Guid>
    {
        public Guid UserId { get; private set; }
        public string NickName { get; private set; }
        public int Score { get; private set; }
        public int Level { get; private set; }
        public List<string> Achievements { get; private set; }

        public UserProgress(Guid userId, string nickName)
        {
            UserId = userId;
            NickName = nickName;
            Achievements = [];
        }

        public void UpdateScore(int score)
        {
            Score = score;
        }

        public void UpdateLevel(int level)
        {
            Level = level;
        }

        public void AddAchievement(string achievement)
        {
            Achievements.Add(achievement);
        }
    }
}
