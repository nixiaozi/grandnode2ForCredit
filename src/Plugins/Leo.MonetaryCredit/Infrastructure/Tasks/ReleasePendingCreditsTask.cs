using Grand.Business.Core.Interfaces.System.ScheduleTasks;
using Leo.MonetaryCredit.Services;
using Microsoft.Extensions.Logging;

namespace Leo.MonetaryCredit.Infrastructure.Tasks;

/// <summary>
///     Scheduled task: release pending credits whose return-window has expired.
///     Should run once per day (TimeInterval = 1440 minutes).
///     During each run it fetches all PendingCreditRecord entries whose
///     ReleasableAfterUtc &lt;= UtcNow and status is Pending, then calls
///     ReleaseAsync on each one, which issues the actual credit to the user account.
/// </summary>
public class ReleasePendingCreditsTask(
    IPendingCreditService pendingCreditService,
    ILogger<ReleasePendingCreditsTask> logger)
    : IScheduleTask
{
    /// <summary>The ScheduleTask.ScheduleTaskName stored in DB — must match the keyed DI registration key.</summary>
    public const string TaskName = "Release pending monetary credits";

    public async Task Execute()
    {
        var releasable = await pendingCreditService.GetReleasableAsync();

        if (releasable.Count == 0)
        {
            logger.LogInformation("[{Task}] No pending credits to release.", TaskName);
            return;
        }

        logger.LogInformation("[{Task}] Releasing {Count} pending credit record(s).", TaskName, releasable.Count);

        var released = 0;
        var failed = 0;

        foreach (var record in releasable)
        {
            try
            {
                await pendingCreditService.ReleaseAsync(record);
                released++;
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogError(ex,
                    "[{Task}] Failed to release PendingCreditRecord {RecordId} for customer {CustomerId}.",
                    TaskName, record.Id, record.CustomerId);
            }
        }

        logger.LogInformation("[{Task}] Done. Released={Released}, Failed={Failed}.", TaskName, released, failed);
    }
}
