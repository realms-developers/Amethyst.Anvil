using System.Linq.Expressions;
using Amethyst.Kernel;
using Amethyst.Storages.Mongo;

namespace Anvil.Audit;

public sealed class AuditInstance
{
    internal AuditInstance(string name)
    {
        Name = name;

        _logs = AuditModule.Database.Get<AuditLog>(Name);
    }

    private MongoModels<AuditLog> _logs;

    public string Name { get; set; }

    public IEnumerable<AuditLog> GetLogs(Expression<Func<AuditLog, bool>> predicate)
    {
        return _logs.FindAll(predicate);
    }

    public IEnumerable<AuditLog> GetLogs()
    {
        return _logs.FindAll();
    }

    public void Log(
        string action,
        string message,
        string[] tags,
        string? user = null,
        Dictionary<string, string>? data = null)
    {
        List<string> tagList = tags?.ToList() ?? new List<string>();
        if (!tagList.Any(p => p.StartsWith("user:")))
        {
            if (user != null)
            {
                tagList.Add($"user:{user}");
            }
        }

        if (!tagList.Any(p => p.StartsWith("server:")))
        {
            tagList.Add($"server:{AmethystSession.Profile.Name}");
        }

        var log = new AuditLog($"anvil_audit-{Name}-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.log")
        {
            Server = AmethystSession.Profile.Name,
            Action = action,
            Timestamp = DateTime.UtcNow,
            Message = message,
            User = user,
            Tags = tagList.ToArray(),
            Objects = data ?? new Dictionary<string, string>()
        };

        _logs.InternalCollection.InsertOne(log);
    }
}