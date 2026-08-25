namespace DesktopNode.Delivery.Tests.Contracts;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PcvLegacyContractAttribute : FactAttribute
{
    public PcvLegacyContractAttribute(
        string contractId,
        string legacyPath,
        int legacyOrdinal,
        string legacyName)
    {
        ContractId = contractId;
        LegacyPath = legacyPath;
        LegacyOrdinal = legacyOrdinal;
        LegacyName = legacyName;
    }

    public string ContractId { get; }
    public string LegacyPath { get; }
    public int LegacyOrdinal { get; }
    public string LegacyName { get; }
}
