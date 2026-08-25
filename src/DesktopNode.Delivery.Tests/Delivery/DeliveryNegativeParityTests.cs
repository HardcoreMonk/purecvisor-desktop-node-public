using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery;

[Trait("Category", "VerificationInfrastructure")]
public sealed class DeliveryNegativeParityTests
{
    private const string Owner = "fixtures/delivery-contract.txt";

    [Fact]
    public void HelpersAcceptContainedWellFormedFixtures()
    {
        using var tree = new TemporaryContractTree();
        tree.WriteUtf8("valid.txt", "first\nsecond\n");
        Assert.Equal("first\nsecond\n", tree.ReadUtf8("valid.txt", Owner));

        using var json = JsonContract.Parse(Owner, """{"contract":"fixture","enabled":true}""");
        json.RequireExactProperties(json.Root, "contract", "enabled");
        Assert.Equal("fixture", json.RequireString(json.Root, "contract"));
        Assert.True(json.RequireBoolean(json.Root, "enabled"));

        var xml = XmlContract.Parse(
            Owner,
            "<Root xmlns=\"urn:expected\"><Item /></Root>",
            "urn:expected");
        Assert.Equal("Item", xml.RequireSingle(xml.Root, "Item").Name.LocalName);

        var markdown = MarkdownContract.Parse(
            Owner,
            "# First\n\n## Second\n\nstatus: pass\n\n| State | Value |\n| --- | --- |\n| current | pass |\n| historical | pass |\n");
        markdown.RequireHeadingOrder("First", "Second");
        markdown.RequireTableRowOrder("current", "historical");
        Assert.Equal("pass", markdown.RequireSingleKeyValue("status"));

        SourceContract.RequireExecutableToken(Owner, "Invoke-Safe -Input $value\n", "Invoke-Safe");
        SourceContract.RequireNoExecutableToken(Owner, "# Invoke-Danger\n", "Invoke-Danger");
    }

    [Fact]
    public void JsonRejectsDuplicateObjectKeys()
    {
        AssertInvalid(() =>
        {
            using var _ = JsonContract.Parse(Owner, """{"value":1,"value":2}""");
        }, "duplicate-json-key");
    }

    [Fact]
    public void JsonRejectsWrongPropertyType()
    {
        using var json = JsonContract.Parse(Owner, """{"value":[]}""");

        AssertInvalid(() => json.RequireString(json.Root, "value"), "json-type");
    }

    [Fact]
    public void XmlRejectsNamespaceDrift()
    {
        AssertInvalid(() => XmlContract.Parse(
            Owner,
            "<Root xmlns=\"urn:wrong\"><Item /></Root>",
            "urn:expected"), "xml-namespace");
    }

    [Fact]
    public void XmlRejectsDuplicateElementCardinality()
    {
        var xml = XmlContract.Parse(
            Owner,
            "<Root xmlns=\"urn:expected\"><Item /><Item /></Root>",
            "urn:expected");

        AssertInvalid(() => xml.RequireSingle(xml.Root, "Item"), "xml-cardinality");
    }

    [Fact]
    public void MarkdownRejectsHeadingReorder()
    {
        var markdown = MarkdownContract.Parse(Owner, "# Second\n\n## First\n");

        AssertInvalid(() => markdown.RequireHeadingOrder("First", "Second"), "markdown-order");
    }

    [Fact]
    public void MarkdownRejectsTableRowReorder()
    {
        var markdown = MarkdownContract.Parse(
            Owner,
            "| State | Value |\n| --- | --- |\n| historical | pass |\n| current | pass |\n");

        AssertInvalid(() => markdown.RequireTableRowOrder("current", "historical"), "markdown-order");
    }

    [Fact]
    public void SourceRejectsTokenFoundOnlyInCommentOrString()
    {
        const string source = "# Invoke-Danger\n$value = 'Invoke-Danger'\n";

        AssertInvalid(
            () => SourceContract.RequireExecutableToken(Owner, source, "Invoke-Danger"),
            "source-token-context");
    }

    [Fact]
    public void TemporaryTreeRejectsEscapingPath()
    {
        using var tree = new TemporaryContractTree();

        AssertInvalid(() => tree.ReadUtf8("../outside.txt", Owner), "path-containment");
    }

    [Fact]
    public void TemporaryTreeRejectsSymbolicLink()
    {
        using var tree = new TemporaryContractTree();
        var target = tree.WriteUtf8("target.txt", "safe\n");
        File.CreateSymbolicLink(Path.Combine(tree.RootPath, "linked.txt"), target);

        AssertInvalid(() => tree.ReadUtf8("linked.txt", Owner), "symlink");
    }

    [Fact]
    public void TemporaryTreeRejectsMalformedUtf8()
    {
        using var tree = new TemporaryContractTree();
        tree.WriteBytes("invalid.txt", [0xc3, 0x28]);

        AssertInvalid(() => tree.ReadUtf8("invalid.txt", Owner), "utf8");
    }

    [Fact]
    public void TemporaryTreeRejectsMixedNewlinePolicy()
    {
        using var tree = new TemporaryContractTree();
        tree.WriteUtf8("mixed.txt", "first\r\nsecond\n");

        AssertInvalid(() => tree.ReadUtf8("mixed.txt", Owner), "newline-policy");
    }

    private static void AssertInvalid(Action action, string detail)
    {
        var error = Assert.Throws<InvalidDataException>(action);
        Assert.Equal($"PCV_DELIVERY_CONTRACT_INVALID|{Owner}|{detail}", error.Message);
        Assert.DoesNotContain(Path.GetTempPath(), error.Message, StringComparison.OrdinalIgnoreCase);
    }
}
