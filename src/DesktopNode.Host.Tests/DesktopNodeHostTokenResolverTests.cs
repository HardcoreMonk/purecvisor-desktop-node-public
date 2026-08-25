using DesktopNode.Host;

namespace DesktopNode.Host.Tests;

public sealed class DesktopNodeHostTokenResolverTests
{
    [Fact]
    public void ResolveReadsBearerTokenFromWindowsCredentialManagerTarget()
    {
        var controller = new FakeWindowsCredentialManagerController("credential-secret");

        var token = DesktopNodeHostTokenResolver.Resolve(new DesktopNodeHostOptions
        {
            ApiTokenCredentialTarget = "PureCVisor/PureCVisorDesktopNode/api-token"
        }, controller);

        Assert.Equal("credential-secret", token.Value);
        Assert.Equal("credential_manager", token.Source);
        Assert.Equal("windows-credential-manager", token.Storage);
        Assert.Null(token.Path);
        Assert.Equal("PureCVisor/PureCVisorDesktopNode/api-token", token.CredentialTarget);
        Assert.Equal(["read-token"], controller.Calls);
    }

    [Fact]
    public void ResolveRejectsCredentialManagerTargetWithOtherTokenSources()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeHostTokenResolver.Resolve(new DesktopNodeHostOptions
            {
                ApiTokenProtectedFile = "api-token.dpapi.json",
                ApiTokenCredentialTarget = "PureCVisor/PureCVisorDesktopNode/api-token"
            }, new FakeWindowsCredentialManagerController("credential-secret")));

        Assert.Contains("PCV_API_TOKEN_CONFLICT", error.Message);
    }

    private sealed class FakeWindowsCredentialManagerController(string token) : IDesktopNodeWindowsCredentialManagerController
    {
        public List<string> Calls { get; } = [];

        public DesktopNodeWindowsCredentialManagerProofSnapshot WriteReadDeleteProof(string credentialTarget)
        {
            Calls.Add("write-read-delete-proof");
            return new DesktopNodeWindowsCredentialManagerProofSnapshot(
                Identity: "NT AUTHORITY\\SYSTEM",
                CredentialTarget: credentialTarget,
                CredentialWriteStatus: "pass",
                CredentialReadStatus: "pass",
                CredentialDeleteStatus: "pass",
                TokenValueObserved: false,
                NewTokenValueCreated: true);
        }

        public void WriteToken(string credentialTarget, string value)
        {
            Calls.Add("write-token");
        }

        public string ReadToken(string credentialTarget)
        {
            Calls.Add("read-token");
            return token;
        }

        public void DeleteToken(string credentialTarget)
        {
            Calls.Add("delete-token");
        }
    }
}
