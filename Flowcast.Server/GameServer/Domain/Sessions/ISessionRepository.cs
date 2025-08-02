using SharedKernel;

namespace Domain.Sessions;

public interface ISessionRepository
{
    Result<Session> GetById(SessionId id);
    IReadOnlyCollection<Session> GetAll();
    void Save(Session session);
    void Delete(SessionId id);
}