using Domain.Entities;

namespace Domain.Players;

public interface IPlayerRepository
{
    Player GetById(long playerId);
    void Save(Player player);
}
