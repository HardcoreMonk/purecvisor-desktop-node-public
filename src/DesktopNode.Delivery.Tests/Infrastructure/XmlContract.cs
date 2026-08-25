using System.Xml;
using System.Xml.Linq;

namespace DesktopNode.Delivery.Tests.Infrastructure;

internal sealed class XmlContract
{
    private readonly string owner;
    private readonly XNamespace expectedNamespace;

    private XmlContract(string owner, XDocument document, XNamespace expectedNamespace)
    {
        this.owner = owner;
        Document = document;
        this.expectedNamespace = expectedNamespace;
    }

    internal XDocument Document { get; }
    internal XElement Root => Document.Root
        ?? throw DeliveryContractError.Invalid(owner, "xml-cardinality");

    internal static XmlContract Parse(string owner, string source, string expectedNamespace)
    {
        var normalizedOwner = DeliveryContractError.RequireOwner(owner);
        if (string.IsNullOrWhiteSpace(expectedNamespace))
        {
            throw DeliveryContractError.Invalid(normalizedOwner, "xml-namespace");
        }

        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
            };
            using var text = new StringReader(source);
            using var reader = XmlReader.Create(text, settings);
            var document = XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            if (document.Root?.Name.NamespaceName != expectedNamespace)
            {
                throw DeliveryContractError.Invalid(normalizedOwner, "xml-namespace");
            }

            return new XmlContract(normalizedOwner, document, expectedNamespace);
        }
        catch (XmlException error)
        {
            throw DeliveryContractError.Invalid(normalizedOwner, "xml-cardinality", error);
        }
    }

    internal XElement RequireSingle(XElement parent, string localName)
    {
        var matches = parent.Elements(expectedNamespace + localName).ToArray();
        return matches.Length == 1
            ? matches[0]
            : throw DeliveryContractError.Invalid(owner, "xml-cardinality");
    }

    internal XElement RequireSingleDescendant(string localName)
    {
        var matches = Document.Descendants(expectedNamespace + localName).ToArray();
        return matches.Length == 1
            ? matches[0]
            : throw DeliveryContractError.Invalid(owner, "xml-cardinality");
    }
}
