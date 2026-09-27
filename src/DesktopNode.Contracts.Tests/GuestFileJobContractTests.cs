using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class GuestFileJobContractTests
{
    [Fact]
    public void AcceptsAllowlistedHostToGuestFile()
    {
        var hostPath = Path.Combine(GuestFileJobContract.HostRoot, "payload.iso");
        var guestPath = Path.Combine(GuestFileJobContract.GuestPrefix, "payload.iso");

        var result = GuestFileJobContract.Evaluate(new GuestFileJobRequest(
            "host-to-guest",
            hostPath,
            guestPath,
            1024,
            "wincred:PureCVisor/guest/admin"));

        Assert.True(result.Ok);
        Assert.Null(result.ErrorCode);
        Assert.Equal(Path.GetFullPath(hostPath), result.NormalizedHostPath);
        Assert.Equal(Path.GetFullPath(guestPath), result.NormalizedGuestPath);
        Assert.Equal(1024, result.SizeBytes);
        Assert.Equal(GuestFileJobContract.DirectionHostToGuest, result.Direction);
    }

    [Theory]
    [InlineData(@"C:\Windows\notepad.exe")]
    [InlineData(@"\\server\share\payload.iso")]
    [InlineData("payload.iso")]
    [InlineData(@"C:\ProgramData\PureCVisor\desktop-node\guest-files\foo:bar.iso")]
    [InlineData(@"C:\ProgramData\PureCVisor\desktop-node\guest-files\*.iso")]
    [InlineData(@"C:\ProgramData\PureCVisor\desktop-node\guest-files\")]
    public void RejectsHostPathOutsideAllowlist(string hostPath)
    {
        var result = GuestFileJobContract.Evaluate(Valid() with { HostPath = hostPath });
        Assert.False(result.Ok);
        Assert.Equal(GuestFileJobProblemCodes.PathNotAllowed, result.ErrorCode);
    }

    [Fact]
    public void RejectsHostPathEscapeThroughParentSegments()
    {
        var escaped = Path.Combine(GuestFileJobContract.HostRoot, "..", "..", "Windows", "win.ini");
        var result = GuestFileJobContract.Evaluate(Valid() with { HostPath = escaped });
        Assert.False(result.Ok);
        Assert.Equal(GuestFileJobProblemCodes.PathNotAllowed, result.ErrorCode);
    }

    [Theory]
    [InlineData(@"C:\Windows\System32\config\SAM")]
    [InlineData(@"C:\Users\Public\payload.iso")]
    [InlineData(@"C:\Users\Public\PureCVisor\")]
    [InlineData(@"\\.\C:\Users\Public\PureCVisor\payload.iso")]
    public void RejectsGuestPathOutsideAllowlist(string guestPath)
    {
        var result = GuestFileJobContract.Evaluate(Valid() with { GuestPath = guestPath });
        Assert.False(result.Ok);
        Assert.Equal(GuestFileJobProblemCodes.PathNotAllowed, result.ErrorCode);
    }

    [Fact]
    public void RejectsGuestPathEscapeThroughParentSegments()
    {
        var escaped = Path.Combine(GuestFileJobContract.GuestPrefix, "..", "..", "Windows", "win.ini");
        var result = GuestFileJobContract.Evaluate(Valid() with { GuestPath = escaped });
        Assert.False(result.Ok);
        Assert.Equal(GuestFileJobProblemCodes.PathNotAllowed, result.ErrorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(GuestFileJobContract.MaxSizeBytes + 1)]
    public void RejectsSizeOutsideLimit(long? size)
    {
        var result = GuestFileJobContract.Evaluate(Valid() with { SizeBytes = size });
        Assert.False(result.Ok);
        Assert.Equal(GuestFileJobProblemCodes.SizeLimit, result.ErrorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("guest-to-host")]
    [InlineData("both")]
    public void RejectsNonHostToGuestDirection(string? direction)
    {
        var result = GuestFileJobContract.Evaluate(Valid() with { Direction = direction });
        Assert.False(result.Ok);
        Assert.Equal(GuestFileJobProblemCodes.DirectionInvalid, result.ErrorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("raw-password-value")]
    public void RejectsMissingOrRawCredentialRef(string? credentialRef)
    {
        var result = GuestFileJobContract.Evaluate(Valid() with { CredentialRef = credentialRef });
        Assert.False(result.Ok);
        Assert.Equal(GuestFileJobProblemCodes.CredentialRefRequired, result.ErrorCode);
    }

    [Fact]
    public void RejectsSharedFolderAsHgfs()
    {
        var result = GuestFileJobContract.Evaluate(Valid() with { SharedFolder = "hgfs-share" });
        Assert.False(result.Ok);
        Assert.Equal(GuestFileJobProblemCodes.HgfsForbidden, result.ErrorCode);
    }

    [Fact]
    public void RejectsHgfsPathToken()
    {
        var result = GuestFileJobContract.Evaluate(Valid() with
        {
            GuestPath = @"C:\Users\Public\PureCVisor\.hgfs\payload.iso"
        });
        Assert.False(result.Ok);
        Assert.Equal(GuestFileJobProblemCodes.HgfsForbidden, result.ErrorCode);
    }

    [Fact]
    public void AcceptsMaxSizeInclusive()
    {
        var result = GuestFileJobContract.Evaluate(Valid() with { SizeBytes = GuestFileJobContract.MaxSizeBytes });
        Assert.True(result.Ok);
        Assert.Equal(GuestFileJobContract.MaxSizeBytes, result.SizeBytes);
    }

    private static GuestFileJobRequest Valid()
    {
        return new GuestFileJobRequest(
            "host-to-guest",
            Path.Combine(GuestFileJobContract.HostRoot, "payload.iso"),
            Path.Combine(GuestFileJobContract.GuestPrefix, "payload.iso"),
            1024,
            "wincred:PureCVisor/guest/admin");
    }
}
