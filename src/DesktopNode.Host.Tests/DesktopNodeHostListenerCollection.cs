namespace DesktopNode.Host.Tests;

// GetFreeLoopbackPort는 포트를 고른 뒤 소켓을 닫고, StartListener가 그 뒤에 HttpListener를 연다.
// 그 사이에 다른 시험이 같은 포트를 등록하면 HttpListenerException이 난다.
// 이 collection 안의 시험만 서로 병렬로 돌지 않는다. collection 밖 시험은 그대로 병렬이다.
[CollectionDefinition(DesktopNodeHostListenerCollection.Name)]
public sealed class DesktopNodeHostListenerCollection
{
    public const string Name = "DesktopNode host listener";
}
