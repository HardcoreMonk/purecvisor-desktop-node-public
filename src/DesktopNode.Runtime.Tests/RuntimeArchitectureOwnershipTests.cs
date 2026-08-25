using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using DesktopNode.Runtime;

namespace DesktopNode.Runtime.Tests;

public sealed class RuntimeArchitectureOwnershipTests
{
    private static readonly string[] RequiredJobRuntimeFieldNames =
    [
        "stateSync",
        "store",
        "clock",
        "jobs",
        "queue",
        "runningCancellations",
        "loadBlock",
        "schemaVersion",
        "prunedTerminalJobs",
        "MaxRetainedTerminalJobs"
    ];

    private static readonly string[] AllowedProductAssemblyReferences =
    [
        "DesktopNode.Contracts"
    ];

    [Fact]
    public void JobRuntimeOwnsConcreteStateContainers()
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeJobRuntime).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var runtimeHandle = FindTypeDefinition(
            metadata,
            "DesktopNode.Runtime",
            nameof(DesktopNodeJobRuntime));
        var runtime = metadata.GetTypeDefinition(runtimeHandle);
        var fieldNames = runtime.GetFields()
            .Select(handle => metadata.GetString(metadata.GetFieldDefinition(handle).Name))
            .ToArray();

        foreach (var requiredFieldName in RequiredJobRuntimeFieldNames)
        {
            Assert.Contains(requiredFieldName, fieldNames);
        }

        var nestedTypeNames = metadata.TypeDefinitions
            .Where(handle => metadata.GetTypeDefinition(handle).GetDeclaringType() == runtimeHandle)
            .Select(handle => metadata.GetString(metadata.GetTypeDefinition(handle).Name))
            .ToArray();
        Assert.Contains("MutableJob", nestedTypeNames);
    }

    [Fact]
    public void RuntimeAssemblyReferencesOnlyApprovedProductContracts()
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeJobRuntime).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var productAssemblyReferences = metadata.AssemblyReferences
            .Select(handle => metadata.GetString(metadata.GetAssemblyReference(handle).Name))
            .Where(name => name.StartsWith("DesktopNode.", StringComparison.Ordinal))
            .ToArray();

        var unexpectedReferences = productAssemblyReferences
            .Except(AllowedProductAssemblyReferences, StringComparer.Ordinal)
            .ToArray();
        Assert.Empty(unexpectedReferences);
        Assert.Equal(
            AllowedProductAssemblyReferences.OrderBy(name => name, StringComparer.Ordinal),
            productAssemblyReferences.Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal));
    }

    private static TypeDefinitionHandle FindTypeDefinition(
        MetadataReader metadata,
        string typeNamespace,
        string typeName)
    {
        return metadata.TypeDefinitions.Single(handle =>
        {
            var definition = metadata.GetTypeDefinition(handle);
            return metadata.GetString(definition.Namespace) == typeNamespace &&
                metadata.GetString(definition.Name) == typeName;
        });
    }
}
