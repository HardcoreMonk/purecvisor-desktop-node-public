using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using DesktopNode.Host;

namespace DesktopNode.Host.Tests;

// DesktopNodeHostServiceAction 은 CreatePlan/ExecuteAsync/token 표면만 소유한다. 도메인 native
// action 구현이 이 타입으로 되돌아오면 ExecuteAsync -> Ops -> ServiceAction 왕복이 되살아나므로
// 각 도메인마다 "떠났는지"와 "도착했는지"를 함께 잠근다.
//
// BindingFlags 대신 metadata 를 읽는 이유: csharp-architecture-test-migration.json 이 test 코드의
// private_reflection.current_occurrence_count 를 0 으로 고정하고 있고,
// RuntimeArchitectureOwnershipTests 가 PEReader 를 그 정책에 맞는 패턴으로 이미 세워 뒀다.
public sealed class HostServiceActionOwnershipTests
{
    private const string HostNamespace = "DesktopNode.Host";
    private const string OpsNamespace = "DesktopNode.Host.Ops";

    internal static string[] GetDeclaredMethodNames(string typeNamespace, string typeName)
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeHostServiceAction).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var typeHandle = metadata.TypeDefinitions.Single(handle =>
        {
            var definition = metadata.GetTypeDefinition(handle);
            return metadata.GetString(definition.Namespace) == typeNamespace &&
                metadata.GetString(definition.Name) == typeName;
        });

        return metadata.GetTypeDefinition(typeHandle)
            .GetMethods()
            .Select(handle => metadata.GetString(metadata.GetMethodDefinition(handle).Name))
            .ToArray();
    }

    internal static void AssertServiceActionDoesNotDeclare(params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(HostNamespace, nameof(DesktopNodeHostServiceAction));
        foreach (var methodName in methodNames)
        {
            Assert.DoesNotContain(methodName, declared);
        }
    }

    internal static void AssertServiceActionDeclares(params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(HostNamespace, nameof(DesktopNodeHostServiceAction));
        foreach (var methodName in methodNames)
        {
            Assert.Contains(methodName, declared);
        }
    }

    internal static void AssertOpsTypeDeclares(string opsTypeName, params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(OpsNamespace, opsTypeName);
        foreach (var methodName in methodNames)
        {
            Assert.Contains(methodName, declared);
        }
    }

    [Fact]
    public void FirewallDomainLivesInFirewallOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeFirewallActionForOps",
            "NativeFirewallFailure");
        AssertOpsTypeDeclares(
            "DesktopNodeFirewallOps",
            "Execute",
            "NativeFirewallFailure");
    }

    [Fact]
    public void TrustStoreDomainLivesInTrustStoreOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeTrustStoreActionForOps",
            "NativeTrustStoreFailure");
        AssertOpsTypeDeclares(
            "DesktopNodeTrustStoreOps",
            "Execute",
            "NativeTrustStoreFailure");
    }

    [Fact]
    public void EventLogDomainLivesInEventLogOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeEventLogActionForOps",
            "ExecuteEventLogDefaultTransitionWithTimeout",
            "ExecuteEventLogDefaultTransitionCore",
            "WriteEventLogDefaultTransitionEvidence",
            "NativeEventLogFailure");
        AssertOpsTypeDeclares(
            "DesktopNodeEventLogOps",
            "Execute",
            "ExecuteEventLogDefaultTransitionCore",
            "NativeEventLogFailure");
    }

    [Fact]
    public void ConfigMigrationDomainLivesInConfigMigrationOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeConfigMigrationActionForOps",
            "ExecuteNativeConfigMigrationAction",
            "ApplyNativeConfigMigration",
            "NativeConfigMigrationFailure",
            "TryReadProductManifest");
        AssertOpsTypeDeclares(
            "DesktopNodeConfigMigrationOps",
            "Execute",
            "ApplyNativeConfigMigration",
            "TryReadProductManifest");
    }

    [Fact]
    public void JobStoreMigrationDomainLivesInJobStoreMigrationOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeJobStoreMigrationActionForOps",
            "ExecuteNativeJobStoreMigrationAction",
            "ApplyNativeJobStoreMigration",
            "NativeJobStoreMigrationFailure",
            "TryReadJobStore");
        AssertOpsTypeDeclares(
            "DesktopNodeJobStoreMigrationOps",
            "Execute",
            "ApplyNativeJobStoreMigration",
            "TryReadJobStore");
    }

    [Fact]
    public void CredentialManagerDomainLivesInCredentialManagerOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeCredentialManagerActionForOps",
            "CredentialManagerResult",
            "ExecuteNativeCredentialManagerDefaultTransition",
            "CreateCredentialManagerTransitionDescriptor",
            "CredentialManagerTransitionResult",
            "WriteCredentialManagerTransitionEvidence",
            "WriteCredentialManagerTransitionRollbackDiagnostics",
            "FixedTimeEquals");
        AssertOpsTypeDeclares(
            "DesktopNodeCredentialManagerOps",
            "Execute",
            "ExecuteNativeCredentialManagerDefaultTransition",
            "FixedTimeEquals");
    }

    [Fact]
    public void ServiceLifecycleDomainLivesInServiceLifecycleOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeServiceActionForOps",
            "ExecuteNativeServiceAction",
            "ExecuteNativeConfigureOrRepair",
            "ExecuteNativeRemove");
        AssertOpsTypeDeclares(
            "DesktopNodeServiceLifecycleOps",
            "Execute",
            "ExecuteNativeConfigureOrRepair",
            "ExecuteNativeRemove");
    }

    [Fact]
    public void DataRootLifecycleDomainLivesInDataRootLifecycleOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeDataRootLifecycleActionForOps",
            "ExecuteNativeDataRootLifecycleAction",
            "ExecuteNativeDataRootRemove",
            "RemoveOwnedJobStoreTempFiles",
            "PrepareDirectoryForDelete",
            "PrepareFileForDelete",
            "RestoreFileDeleteAcl",
            "RestoreDirectoryDeleteAcl",
            "AllowDeleteForServiceAdministrators");
        AssertOpsTypeDeclares(
            "DesktopNodeDataRootLifecycleOps",
            "Execute",
            "ExecuteNativeDataRootRemove",
            "AllowDeleteForServiceAdministrators");
    }

    [Fact]
    public void ServiceTokenRotationLivesInServiceTokenOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeServiceTokenActionForOps",
            "ExecuteNativeServiceTokenRotationRevoke",
            "WriteServiceTokenRotationAudit");
        AssertOpsTypeDeclares(
            "DesktopNodeServiceTokenOps",
            "Execute",
            "ExecuteNativeServiceTokenRotationRevoke",
            "WriteServiceTokenRotationAudit");

        // 공개 token 표면은 그대로 남아야 한다. 이 단언이 없으면 위 이동이 공개 표면까지
        // 함께 옮겨가도 통과한다.
        AssertServiceActionDeclares(
            "EnsureProtectedTokenFile",
            "EnsureAccountAuthBootstrapFiles");
    }

    [Fact]
    public void NoOpsForwarderRemainsOnHostServiceAction()
    {
        var declared = GetDeclaredMethodNames("DesktopNode.Host", nameof(DesktopNodeHostServiceAction))
            .Where(name => name.EndsWith("ForOps", StringComparison.Ordinal))
            .ToArray();

        // ExecuteAsync -> Ops.X.Execute -> DesktopNodeHostServiceAction.*ForOps 왕복을 없애는 것이
        // 이 분해의 목적이다. ForOps 이름이 하나라도 남아 있으면 그 도메인은 아직 돌아온다.
        Assert.Empty(declared);
    }

    [Fact]
    public void HostServiceActionKeepsOnlyItsPublicSurface()
    {
        AssertServiceActionDeclares(
            "CreatePlan",
            "ExecuteAsync",
            "EnsureProtectedTokenFile",
            "EnsureAccountAuthBootstrapFiles");
    }
}
