namespace DesktopNode.HyperV;

// 콘솔 입력의 VM 존재/실행 상태. GetVms 전체 요약과 분리한다. 구현이 없으면 adapter 가 목록을 스캔한다.
public interface IDesktopNodeHyperVVmRunStateProvider
{
    DesktopNodeHyperVVmRunState? FindVmRunState(string vmName, CancellationToken cancellationToken);
}

public sealed record DesktopNodeHyperVVmRunState(string Id, string Name, string State, bool TemplateLock = false);
