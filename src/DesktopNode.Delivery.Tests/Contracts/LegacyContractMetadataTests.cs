using System.Reflection;

namespace DesktopNode.Delivery.Tests.Contracts;

[Trait("Category", "VerificationInfrastructure")]
public sealed class LegacyContractMetadataTests
{
    [Fact]
    public void AttributeHasTheCompleteImmutablePublicShape()
    {
        var attributeType = typeof(PcvLegacyContractAttribute);
        Assert.True(typeof(FactAttribute).IsAssignableFrom(attributeType));
        var constructor = Assert.Single(attributeType.GetConstructors());
        Assert.Equal(
            [typeof(string), typeof(string), typeof(int), typeof(string)],
            constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Equal(
            ["ContractId", "LegacyName", "LegacyOrdinal", "LegacyPath"],
            attributeType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public void EveryMigratedFactHasUniqueExactLegacyMetadataAndOneDomainTrait()
    {
        var methods = typeof(PcvLegacyContractAttribute).Assembly
            .GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            .Select(method => new
            {
                Method = method,
                Attribute = method.GetCustomAttribute<PcvLegacyContractAttribute>(),
            })
            .Where(item => item.Attribute is not null)
            .ToArray();
        var contractIds = new HashSet<string>(StringComparer.Ordinal);
        var legacyKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in methods)
        {
            var attribute = Assert.IsType<PcvLegacyContractAttribute>(item.Attribute);
            var traits = item.Method.DeclaringType?
                .GetCustomAttributesData()
                .Where(data => data.AttributeType == typeof(TraitAttribute))
                .Select(data => new
                {
                    Name = Assert.IsType<string>(data.ConstructorArguments[0].Value),
                    Value = Assert.IsType<string>(data.ConstructorArguments[1].Value),
                })
                .Where(trait => trait.Name == "Category")
                .ToArray() ?? [];
            var trait = Assert.Single(traits);
            Assert.Contains(trait.Value, new[] { "Installer", "Delivery" });
            Assert.True(contractIds.Add(attribute.ContractId), $"duplicate contract ID: {attribute.ContractId}");
            Assert.True(
                legacyKeys.Add($"{attribute.LegacyPath}\0{attribute.LegacyOrdinal}"),
                $"duplicate legacy key: {attribute.LegacyPath}:{attribute.LegacyOrdinal}");

            var repository = Infrastructure.RepositoryContractContext.Find();
            var parsed = LegacyPesterContractParser.Parse(
                attribute.LegacyPath,
                repository.ReadUtf8Text(attribute.LegacyPath));
            var legacy = Assert.Single(parsed, contract => contract.Ordinal == attribute.LegacyOrdinal);
            Assert.Equal(legacy.Name, attribute.LegacyName);
            Assert.Equal(
                LegacyContractId.Create(
                    trait.Value == "Installer" ? "installer" : "delivery",
                    attribute.LegacyPath,
                    attribute.LegacyOrdinal),
                attribute.ContractId);
        }
    }
}
