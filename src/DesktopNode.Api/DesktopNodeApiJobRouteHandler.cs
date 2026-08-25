using DesktopNode.Runtime;

namespace DesktopNode.Api;

// job 조회/취소/재시도 경로. 지금은 Func 다섯 개를 processor 로
// 되돌려 보내는 callback adapter 로 dispatch 된다. reconcile 은 별도 소유자가 가져간다.
internal sealed class DesktopNodeApiJobRouteHandler
{
    private readonly DesktopNodeJobRuntime jobRuntime;

    public DesktopNodeApiJobRouteHandler(DesktopNodeJobRuntime jobRuntime)
    {
        this.jobRuntime = jobRuntime;
    }

    public DesktopNodeApiResponse? TryHandle(DesktopNodeApiRequest request, string method, string normalizedPath)
    {
        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "ListJobs", out _))
        {
            return BuildJobListResponse(request.Path);
        }

        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "GetJob", out var getMatch))
        {
            return HandleJobGet(getMatch.Parameters["jobId"]);
        }

        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "CancelJob", out var cancelMatch))
        {
            return HandleJobCancel(cancelMatch.Parameters["jobId"]);
        }

        if (DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "RetryJob", out var retryMatch))
        {
            return HandleJobRetry(retryMatch.Parameters["jobId"], request.RequestId!);
        }

        return null;
    }

    private DesktopNodeApiResponse BuildJobListResponse(string rawPath)
    {
        var page = DesktopNodeApiRequestParsing.ParseJobListPage(rawPath);
        if (!page.Ok)
        {
            return page.Response!;
        }

        var snapshot = jobRuntime.Snapshot();
        var allJobRows = snapshot.Jobs
            .OrderByDescending(job => job.UpdatedAt)
            .ThenByDescending(job => job.CreatedAt)
            .Select(DesktopNodeApiResponseFactory.JobData)
            .ToArray();
        var pageRows = allJobRows
            .Skip(page.Offset)
            .Take(page.Limit)
            .ToArray();
        var nextOffset = page.Offset + pageRows.Length < allJobRows.Length
            ? page.Offset + pageRows.Length
            : (int?)null;

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "job.list", DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["count"] = allJobRows.Length,
            ["default_limit"] = DesktopNodeApiRequestParsing.DefaultJobListLimit,
            ["jobs"] = pageRows,
            ["limit"] = page.Limit,
            ["max_limit"] = DesktopNodeApiRequestParsing.MaxJobListLimit,
            ["next_offset"] = nextOffset,
            ["offset"] = page.Offset,
            ["returned"] = pageRows.Length,
            ["retention"] = new SortedDictionary<string, object?>
            {
                ["active_jobs_preserved"] = true,
                ["max_terminal_jobs"] = snapshot.MaxRetainedTerminalJobs,
                ["pruned_terminal_jobs"] = snapshot.PrunedTerminalJobs,
                ["terminal_statuses"] = new[] { "succeeded", "failed", "canceled" }
            }
        }), null));
    }

    private DesktopNodeApiResponse HandleJobGet(string jobId)
    {
        var result = jobRuntime.Get(jobId);
        if (result.Outcome == DesktopNodeJobCommandOutcome.NotFound)
        {
            return DesktopNodeApiResponseFactory.Json(404, DesktopNodeApiResponseFactory.Body(false, "job.get", null, DesktopNodeApiErrorMapping.ToApiError(result.Error)));
        }

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "job.get", DesktopNodeApiResponseFactory.JobData(result.Job!), null));
    }

    public DesktopNodeApiResponse HandleVmDeleteStatus(string vmName)
    {
        var latest = jobRuntime.Snapshot().Jobs
            .Where(job => string.Equals(job.Operation, "vm.delete", StringComparison.Ordinal) &&
                string.Equals(DesktopNodeApiJsonReader.ReadString(job.Parameters, "name") ?? DesktopNodeApiJsonReader.ReadString(job.Parameters, "vm_name"), vmName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(job => job.UpdatedAt, StringComparer.Ordinal)
            .ThenByDescending(job => job.CreatedAt, StringComparer.Ordinal)
            .FirstOrDefault();

        if (latest is null)
        {
            return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "vm.delete-status", new SortedDictionary<string, object?>
            {
                ["name"] = vmName,
                ["operation"] = "vm.delete",
                ["status"] = "not_started",
                ["job_id"] = null
            }, null));
        }

        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "vm.delete-status", new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["operation"] = latest.Operation,
            ["status"] = latest.Status,
            ["job_id"] = latest.JobId,
            ["updated_at"] = latest.UpdatedAt,
            ["result"] = latest.Result,
            ["error"] = latest.Error
        }, null));
    }

    private DesktopNodeApiResponse HandleJobCancel(string jobId)
    {
        var result = jobRuntime.Cancel(jobId);
        if (result.Outcome == DesktopNodeJobCommandOutcome.NotFound)
        {
            return DesktopNodeApiResponseFactory.Json(404, DesktopNodeApiResponseFactory.Body(false, "job.cancel", null, DesktopNodeApiErrorMapping.ToApiError(result.Error)));
        }

        if (result.Outcome == DesktopNodeJobCommandOutcome.Rejected)
        {
            return DesktopNodeApiResponseFactory.Json(409, DesktopNodeApiResponseFactory.Body(false, "job.cancel", null, DesktopNodeApiErrorMapping.ToApiError(result.Error)));
        }

        if (result.Error is not null)
        {
            return DesktopNodeApiResponseFactory.Json(503, DesktopNodeApiResponseFactory.Body(
                false,
                "job.cancel",
                result.Job is null ? null : DesktopNodeApiResponseFactory.JobData(result.Job),
                DesktopNodeApiErrorMapping.ToApiError(result.Error)));
        }

        var statusCode = result.Outcome == DesktopNodeJobCommandOutcome.CancellationRequested
            ? 202
            : 200;
        return DesktopNodeApiResponseFactory.Json(statusCode, DesktopNodeApiResponseFactory.Body(true, "job.cancel", DesktopNodeApiResponseFactory.JobData(result.Job!), null));
    }

    private DesktopNodeApiResponse HandleJobRetry(string jobId, string requestId)
    {
        var result = jobRuntime.Retry(jobId, new DesktopNodeJobRequestContext(requestId));
        if (result.Outcome == DesktopNodeJobCommandOutcome.NotFound)
        {
            return DesktopNodeApiResponseFactory.Json(404, DesktopNodeApiResponseFactory.Body(false, "job.retry", null, DesktopNodeApiErrorMapping.ToApiError(result.Error)));
        }

        if (result.Outcome == DesktopNodeJobCommandOutcome.Rejected)
        {
            return DesktopNodeApiResponseFactory.Json(409, DesktopNodeApiResponseFactory.Body(false, "job.retry", null, DesktopNodeApiErrorMapping.ToApiError(result.Error)));
        }

        return DesktopNodeApiResponseFactory.Json(202, DesktopNodeApiResponseFactory.Body(true, "job.retry", DesktopNodeApiResponseFactory.JobData(result.Job!), null));
    }
}
