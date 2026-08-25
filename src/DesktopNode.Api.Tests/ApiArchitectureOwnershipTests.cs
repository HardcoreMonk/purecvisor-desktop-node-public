using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class ApiArchitectureOwnershipTests
{
    private static readonly IReadOnlyDictionary<short, OpCode> IlOpCodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(OpCode))
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(opCode => opCode.Value);

    private static readonly string[] ForbiddenJobRuntimeFieldNames =
    [
        "jobStore",
        "jobClock",
        "jobs",
        "runningJobCancellations",
        "queue",
        "jobStoreLoadBlock",
        "jobStoreSchemaVersion",
        "prunedTerminalJobs",
        "MaxRetainedTerminalJobs"
    ];

    private static readonly string[] ForbiddenJobRuntimeNestedTypeNames =
    [
        "DesktopNodeApiJob",
        "StartedJobSnapshot",
        "RuntimeJobAdapterResult"
    ];

    private static readonly string[] RetiredApiJobSeamTypeNames =
    [
        "IDesktopNodeApiJobStore",
        "JsonFileDesktopNodeApiJobStore",
        "IDesktopNodeApiClock",
        "SystemDesktopNodeApiClock"
    ];

    [Fact]
    public void RequestProcessorDoesNotCarryAmbientRequestIdState()
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var processorHandle = FindTypeDefinition(
            metadata,
            "DesktopNode.Api",
            nameof(DesktopNodeApiRequestProcessor));
        var processor = metadata.GetTypeDefinition(processorHandle);
        var fieldNames = processor.GetFields()
            .Select(handle => metadata.GetString(metadata.GetFieldDefinition(handle).Name))
            .ToArray();
        var ambientRequestContextTypes = metadata.TypeReferences
            .Select(handle => metadata.GetTypeReference(handle))
            .Where(reference =>
                metadata.GetString(reference.Namespace) == "System.Threading" &&
                metadata.GetString(reference.Name).StartsWith("AsyncLocal`", StringComparison.Ordinal))
            .Select(reference => metadata.GetString(reference.Name))
            .ToArray();

        Assert.DoesNotContain("CurrentRequestId", fieldNames);
        Assert.Empty(ambientRequestContextTypes);
    }

    [Fact]
    public void RequestProcessorDelegatesConcreteJobStateToRuntimeOwner()
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var processorHandle = FindTypeDefinition(
            metadata,
            "DesktopNode.Api",
            nameof(DesktopNodeApiRequestProcessor));
        var processor = metadata.GetTypeDefinition(processorHandle);
        var fields = processor.GetFields()
            .Select(handle => new
            {
                Handle = handle,
                Name = metadata.GetString(metadata.GetFieldDefinition(handle).Name)
            })
            .ToArray();
        var fieldNames = fields.Select(field => field.Name).ToArray();

        foreach (var forbiddenFieldName in ForbiddenJobRuntimeFieldNames)
        {
            Assert.DoesNotContain(forbiddenFieldName, fieldNames);
        }

        var jobRuntimeField = Assert.Single(fields, field => field.Name == "jobRuntime");
        var jobRuntimeFieldType = ReadDirectFieldType(metadata, jobRuntimeField.Handle);
        Assert.Equal("DesktopNode.Runtime.DesktopNodeJobRuntime", jobRuntimeFieldType.FullName);
        Assert.Equal("DesktopNode.Runtime", jobRuntimeFieldType.AssemblyName);

        var nestedTypeNames = metadata.TypeDefinitions
            .Where(handle => metadata.GetTypeDefinition(handle).GetDeclaringType() == processorHandle)
            .Select(handle => metadata.GetString(metadata.GetTypeDefinition(handle).Name))
            .ToArray();
        foreach (var forbiddenNestedTypeName in ForbiddenJobRuntimeNestedTypeNames)
        {
            Assert.DoesNotContain(forbiddenNestedTypeName, nestedTypeNames);
        }
    }

    [Fact]
    public void ApiAssemblyDoesNotDefineRetiredJobStoreOrClockSeams()
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var apiTypeNames = metadata.TypeDefinitions
            .Select(handle => metadata.GetTypeDefinition(handle))
            .Where(definition => metadata.GetString(definition.Namespace) == "DesktopNode.Api")
            .Select(definition => metadata.GetString(definition.Name))
            .ToArray();

        foreach (var retiredTypeName in RetiredApiJobSeamTypeNames)
        {
            Assert.DoesNotContain(retiredTypeName, apiTypeNames);
        }
    }

    [Fact]
    public void RequestProcessorDelegatesDiagnosticsBehaviorToCallbackFreeOwner()
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var typeProvider = new MetadataTypeNameProvider();
        var processorHandle = FindTypeDefinition(
            metadata,
            "DesktopNode.Api",
            nameof(DesktopNodeApiRequestProcessor));
        var processor = metadata.GetTypeDefinition(processorHandle);
        var processorFields = processor.GetFields()
            .Select(handle =>
            {
                var field = metadata.GetFieldDefinition(handle);
                return new MetadataField(
                    metadata.GetString(field.Name),
                    field.DecodeSignature(typeProvider, genericContext: null),
                    field.Attributes);
            })
            .ToArray();
        var diagnosticsField = Assert.Single(
            processorFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeApiDiagnosticsHandler");
        Assert.Equal("diagnosticsHandler", diagnosticsField.Name);
        Assert.DoesNotContain(
            processorFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeDiagnosticBundleOptions");
        Assert.DoesNotContain(
            processorFields,
            field => IsCallbackType(field.TypeName));

        var retiredProcessorMethods = new[]
        {
            "CreateDiagnosticBundle",
            "BuildDiagnosticBundleListResponse",
            "DownloadDiagnosticBundle",
            "ApplyDiagnosticBundleRetention",
            "ParseDiagnosticBundleListPage",
            "BuildDiagnosticBundleListRow",
            "ParseDiagnosticBundleCreatedAt",
            "RedactRequestBody",
            "RedactJsonValue",
            "IsSensitiveDiagnosticProperty",
            "RedactDiagnosticString"
        };
        var processorMethods = processor.GetMethods()
            .Select(handle => new MetadataMethod(
                Handle: handle,
                Name: metadata.GetString(metadata.GetMethodDefinition(handle).Name),
                Signature: metadata.GetMethodDefinition(handle).DecodeSignature(typeProvider, genericContext: null)))
            .ToArray();
        var processorMethodNames = processorMethods
            .Select(method => method.Name)
            .ToArray();
        foreach (var retiredMethod in retiredProcessorMethods)
        {
            Assert.DoesNotContain(retiredMethod, processorMethodNames);
        }

        var processorNestedTypeNames = metadata.TypeDefinitions
            .Where(handle => metadata.GetTypeDefinition(handle).GetDeclaringType() == processorHandle)
            .Select(handle => metadata.GetString(metadata.GetTypeDefinition(handle).Name))
            .ToArray();
        Assert.DoesNotContain("DiagnosticBundleListPage", processorNestedTypeNames);
        Assert.DoesNotContain("DiagnosticBundleListRow", processorNestedTypeNames);

        var handlerHandle = FindTypeDefinition(
            metadata,
            "DesktopNode.Api",
            "DesktopNodeApiDiagnosticsHandler");
        var handler = metadata.GetTypeDefinition(handlerHandle);
        Assert.False(
            handler.Attributes.HasFlag(TypeAttributes.Abstract) &&
            handler.Attributes.HasFlag(TypeAttributes.Sealed));
        var handlerFields = handler.GetFields()
            .Select(handle =>
            {
                var field = metadata.GetFieldDefinition(handle);
                return new MetadataField(
                    metadata.GetString(field.Name),
                    field.DecodeSignature(typeProvider, genericContext: null),
                    field.Attributes);
            })
            .ToArray();
        var handlerStateFields = handlerFields
            .Where(field => !field.Attributes.HasFlag(FieldAttributes.Literal))
            .ToArray();
        Assert.Single(
            handlerStateFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeDiagnosticBundleOptions");
        Assert.DoesNotContain(
            handlerFields,
            field => IsCallbackType(field.TypeName));
        Assert.DoesNotContain(
            handlerFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeApiRequestProcessor");

        var handlerMethods = handler.GetMethods()
            .Select(handle => new MetadataMethod(
                Handle: handle,
                Name: metadata.GetString(metadata.GetMethodDefinition(handle).Name),
                Signature: metadata.GetMethodDefinition(handle).DecodeSignature(typeProvider, genericContext: null)))
            .ToArray();
        var handlerConstructors = handlerMethods
            .Where(method => method.Name == ".ctor")
            .ToArray();
        var handlerConstructor = Assert.Single(handlerConstructors);
        Assert.Equal(
            [
                "DesktopNode.Api.DesktopNodeDiagnosticBundleOptions",
                "DesktopNode.Runtime.DesktopNodeJobRuntime"
            ],
            handlerConstructor.Signature.ParameterTypes.ToArray());
        Assert.DoesNotContain(
            handlerMethods.SelectMany(method => method.Signature.ParameterTypes),
            IsCallbackType);
        Assert.DoesNotContain(
            handlerMethods.SelectMany(method => method.Signature.ParameterTypes),
            parameterType => parameterType == "DesktopNode.Api.DesktopNodeApiRequestProcessor");
        var tryHandle = Assert.Single(
            handlerMethods,
            method => method.Name == "TryHandle");
        Assert.Equal("DesktopNode.Api.DesktopNodeApiResponse", tryHandle.Signature.ReturnType);
        Assert.Equal(
            ["DesktopNode.Api.DesktopNodeApiRequest", "System.String", "System.String"],
            tryHandle.Signature.ParameterTypes.ToArray());
        Assert.DoesNotContain(
            tryHandle.Signature.ParameterTypes,
            IsCallbackType);
        Assert.DoesNotContain(
            tryHandle.Signature.ParameterTypes,
            parameterType => parameterType == "DesktopNode.Api.DesktopNodeApiRequestProcessor");

        var processorCalls = processorMethods
            .SelectMany(caller => ReadCalledMethods(peReader, metadata, caller.Handle)
                .Select(called => new CalledMethod(caller.Name, called.DeclaringType, called.Name)))
            .ToArray();
        Assert.DoesNotContain(
            processorCalls,
            call => call.DeclaringType.StartsWith("System.IO.", StringComparison.Ordinal));

        var diagnosticsDispatch = Assert.Single(
            processorCalls,
            call => call.DeclaringType == "DesktopNode.Api.DesktopNodeApiDiagnosticsHandler" &&
                call.Name == "TryHandle");
        Assert.Equal("HandleCore", diagnosticsDispatch.CallerName);
    }

    [Fact]
    public void RequestProcessorDelegatesAuthSessionBehaviorToCallbackFreeOwner()
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var typeProvider = new MetadataTypeNameProvider();
        var processorHandle = FindTypeDefinition(
            metadata,
            "DesktopNode.Api",
            nameof(DesktopNodeApiRequestProcessor));
        var processor = metadata.GetTypeDefinition(processorHandle);
        var processorFields = processor.GetFields()
            .Select(handle =>
            {
                var field = metadata.GetFieldDefinition(handle);
                return new MetadataField(
                    metadata.GetString(field.Name),
                    field.DecodeSignature(typeProvider, genericContext: null),
                    field.Attributes);
            })
            .ToArray();
        var authOwnerField = Assert.Single(
            processorFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeApiAuthSessionHandler");
        Assert.Equal("authSessionHandler", authOwnerField.Name);
        Assert.True(authOwnerField.Attributes.HasFlag(FieldAttributes.Private));
        Assert.True(authOwnerField.Attributes.HasFlag(FieldAttributes.InitOnly));
        Assert.False(authOwnerField.Attributes.HasFlag(FieldAttributes.Static));
        Assert.DoesNotContain(
            processorFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeAccountAuthService");
        Assert.DoesNotContain(
            processorFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeAccountAuthOptions");
        Assert.DoesNotContain(processorFields, field => IsCallbackType(field.TypeName));

        var processorMethods = processor.GetMethods()
            .Select(handle => new MetadataMethod(
                Handle: handle,
                Name: metadata.GetString(metadata.GetMethodDefinition(handle).Name),
                Signature: metadata.GetMethodDefinition(handle).DecodeSignature(typeProvider, genericContext: null)))
            .ToArray();
        var retiredProcessorMethods = new[]
        {
            "HandleAccountAuthRoute",
            "EnforceAccountAuthorization",
            "RequiredPermissionForRoute",
            "AuthResult",
            "AuthValidationFailure",
            "ResolveActor"
        };
        foreach (var retiredMethod in retiredProcessorMethods)
        {
            Assert.DoesNotContain(retiredMethod, processorMethods.Select(method => method.Name));
        }

        var handlerHandle = FindTypeDefinition(
            metadata,
            "DesktopNode.Api",
            "DesktopNodeApiAuthSessionHandler");
        var handler = metadata.GetTypeDefinition(handlerHandle);
        Assert.False(handler.Attributes.HasFlag(TypeAttributes.Abstract));
        Assert.True(handler.Attributes.HasFlag(TypeAttributes.Sealed));
        var handlerFields = handler.GetFields()
            .Select(handle =>
            {
                var field = metadata.GetFieldDefinition(handle);
                return new MetadataField(
                    metadata.GetString(field.Name),
                    field.DecodeSignature(typeProvider, genericContext: null),
                    field.Attributes);
            })
            .ToArray();
        var handlerStateFields = handlerFields
            .Where(field => !field.Attributes.HasFlag(FieldAttributes.Literal) &&
                !field.Attributes.HasFlag(FieldAttributes.Static))
            .ToArray();
        var accountAuthField = Assert.Single(handlerStateFields);
        Assert.Equal("accountAuth", accountAuthField.Name);
        Assert.Equal("DesktopNode.Api.DesktopNodeAccountAuthService", accountAuthField.TypeName);
        Assert.True(accountAuthField.Attributes.HasFlag(FieldAttributes.Private));
        Assert.True(accountAuthField.Attributes.HasFlag(FieldAttributes.InitOnly));
        Assert.False(accountAuthField.Attributes.HasFlag(FieldAttributes.Static));
        Assert.DoesNotContain(handlerFields, field => IsCallbackType(field.TypeName));
        Assert.DoesNotContain(
            handlerFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeApiRequestProcessor");

        var handlerMethods = handler.GetMethods()
            .Select(handle => new MetadataMethod(
                Handle: handle,
                Name: metadata.GetString(metadata.GetMethodDefinition(handle).Name),
                Signature: metadata.GetMethodDefinition(handle).DecodeSignature(typeProvider, genericContext: null)))
            .ToArray();
        var handlerConstructor = Assert.Single(handlerMethods, method => method.Name == ".ctor");
        Assert.Equal(
            ["DesktopNode.Api.DesktopNodeAccountAuthOptions"],
            handlerConstructor.Signature.ParameterTypes.ToArray());
        Assert.DoesNotContain(
            handlerMethods.SelectMany(method => method.Signature.ParameterTypes),
            IsCallbackType);
        Assert.DoesNotContain(
            handlerMethods.SelectMany(method => method.Signature.ParameterTypes),
            parameterType => parameterType == "DesktopNode.Api.DesktopNodeApiRequestProcessor");

        AssertHandlerMethod(
            handlerMethods,
            "TryHandle",
            "DesktopNode.Api.DesktopNodeApiResponse",
            "DesktopNode.Api.DesktopNodeApiRequest",
            "System.String",
            "System.String");
        AssertHandlerMethod(
            handlerMethods,
            "Authorize",
            "DesktopNode.Api.DesktopNodeApiResponse",
            "DesktopNode.Api.DesktopNodeApiRequest",
            "System.String",
            "System.String");
        AssertHandlerMethod(
            handlerMethods,
            "ResolveActor",
            "System.String",
            "DesktopNode.Api.DesktopNodeApiRequest");
        AssertHandlerMethod(
            handlerMethods,
            "CreateRuntimePolicy",
            "DesktopNode.Contracts.RuntimePolicyAuthPolicy",
            "System.String");

        var nestedTypeNames = metadata.TypeDefinitions
            .Where(handle => metadata.GetTypeDefinition(handle).GetDeclaringType() == handlerHandle)
            .Select(handle => metadata.GetString(metadata.GetTypeDefinition(handle).Name))
            .ToArray();
        Assert.DoesNotContain("AuthRouteHandler", nestedTypeNames);

        var processorCalls = processorMethods
            .SelectMany(caller => ReadCalledMethods(peReader, metadata, caller.Handle)
                .Select(called => new CalledMethod(caller.Name, called.DeclaringType, called.Name)))
            .ToArray();
        Assert.DoesNotContain(
            processorCalls,
            call => call.DeclaringType == "DesktopNode.Api.DesktopNodeAccountAuthService");
        var handleCoreOwnerCalls = processorCalls
            .Where(call => call.CallerName == "HandleCore" &&
                call.DeclaringType == "DesktopNode.Api.DesktopNodeApiAuthSessionHandler")
            .Select(call => call.Name)
            .ToArray();
        Assert.Equal(["TryHandle", "Authorize"], handleCoreOwnerCalls.Take(2).ToArray());

        // ResolveActor 의 호출자가 옮겨졌다. guest 실행 preview 와 queued VM 변경이
        // DesktopNodeApiRequestProcessor 를 떠나 각자 소유자를 갖게 되면서, actor 를 해석하는 쪽도
        // 그 소유자들이 됐다. 이 단언의 목적 - actor 해석은 auth 소유자에 남고 누구도 다시 구현하지
        // 않는다 - 은 바뀌지 않았으므로, processor 로 고정하는 대신 실제 호출자를 확인한다.
        foreach (var ownerTypeName in new[]
        {
            "DesktopNodeApiGuestExecutionRouteHandler",
            "DesktopNodeApiVmMutationRouteHandler"
        })
        {
            var ownerHandle = FindTypeDefinition(metadata, "DesktopNode.Api", ownerTypeName);
            var ownerCalls = metadata.GetTypeDefinition(ownerHandle)
                .GetMethods()
                .SelectMany(handle => ReadCalledMethods(peReader, metadata, handle))
                .ToArray();
            Assert.Contains(
                ownerCalls,
                call => call.DeclaringType == "DesktopNode.Api.DesktopNodeApiAuthSessionHandler" &&
                    call.Name == "ResolveActor");
        }

        Assert.Contains(
            processorCalls,
            call => call.DeclaringType == "DesktopNode.Api.DesktopNodeApiAuthSessionHandler" &&
                call.Name == "CreateRuntimePolicy");
    }

    [Fact]
    public void OpsSummaryProjectionUsesDedicatedOwner()
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var typeProvider = new MetadataTypeNameProvider();
        var processorHandle = FindTypeDefinition(
            metadata,
            "DesktopNode.Api",
            nameof(DesktopNodeApiRequestProcessor));
        var processor = metadata.GetTypeDefinition(processorHandle);
        var processorFields = processor.GetFields()
            .Select(handle =>
            {
                var field = metadata.GetFieldDefinition(handle);
                return new MetadataField(
                    metadata.GetString(field.Name),
                    field.DecodeSignature(typeProvider, genericContext: null),
                    field.Attributes);
            })
            .ToArray();
        var opsOwnerField = Assert.Single(
            processorFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeApiOpsSummaryHandler");
        Assert.Equal("opsSummaryHandler", opsOwnerField.Name);
        Assert.True(opsOwnerField.Attributes.HasFlag(FieldAttributes.Private));
        Assert.True(opsOwnerField.Attributes.HasFlag(FieldAttributes.InitOnly));
        Assert.DoesNotContain(
            processorFields,
            field => field.TypeName == "DesktopNode.Api.BatchEvidenceSummaryReader");
        Assert.DoesNotContain(processorFields, field => IsCallbackType(field.TypeName));

        var processorMethods = processor.GetMethods()
            .Select(handle => new MetadataMethod(
                Handle: handle,
                Name: metadata.GetString(metadata.GetMethodDefinition(handle).Name),
                Signature: metadata.GetMethodDefinition(handle).DecodeSignature(typeProvider, genericContext: null)))
            .ToArray();
        Assert.DoesNotContain("BuildOpsSummary", processorMethods.Select(method => method.Name));
        var processorCalls = processorMethods
            .SelectMany(caller => ReadCalledMethods(peReader, metadata, caller.Handle)
                .Select(called => new CalledMethod(caller.Name, called.DeclaringType, called.Name)))
            .ToArray();
        Assert.DoesNotContain(
            processorCalls,
            call => call.DeclaringType == "DesktopNode.Api.DesktopNodeApiOpsSummaryBuilder");
        Assert.DoesNotContain(
            processorCalls,
            call => call.DeclaringType == "DesktopNode.Api.BatchEvidenceSummaryReader");
        Assert.Contains(
            processorCalls,
            call => call.CallerName == "HandleCore" &&
                call.DeclaringType == "DesktopNode.Api.DesktopNodeApiOpsSummaryHandler" &&
                call.Name == "TryHandle");

        var handlerHandle = FindTypeDefinition(
            metadata,
            "DesktopNode.Api",
            "DesktopNodeApiOpsSummaryHandler");
        var handler = metadata.GetTypeDefinition(handlerHandle);
        Assert.False(handler.Attributes.HasFlag(TypeAttributes.Abstract));
        Assert.True(handler.Attributes.HasFlag(TypeAttributes.Sealed));
        var handlerFields = handler.GetFields()
            .Select(handle =>
            {
                var field = metadata.GetFieldDefinition(handle);
                return new MetadataField(
                    metadata.GetString(field.Name),
                    field.DecodeSignature(typeProvider, genericContext: null),
                    field.Attributes);
            })
            .ToArray();
        var queryField = Assert.Single(handlerFields);
        Assert.Equal("query", queryField.Name);
        Assert.Equal("DesktopNode.Api.IDesktopNodeApiOpsSummaryQuery", queryField.TypeName);
        Assert.True(queryField.Attributes.HasFlag(FieldAttributes.Private));
        Assert.True(queryField.Attributes.HasFlag(FieldAttributes.InitOnly));
        Assert.DoesNotContain(handlerFields, field => IsCallbackType(field.TypeName));
        Assert.DoesNotContain(
            handlerFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeApiRequestProcessor");
        var handlerMethods = handler.GetMethods()
            .Select(handle => new MetadataMethod(
                Handle: handle,
                Name: metadata.GetString(metadata.GetMethodDefinition(handle).Name),
                Signature: metadata.GetMethodDefinition(handle).DecodeSignature(typeProvider, genericContext: null)))
            .ToArray();
        AssertHandlerMethod(
            handlerMethods,
            "TryHandle",
            "DesktopNode.Api.DesktopNodeApiResponse",
            "System.String",
            "System.String",
            "System.Threading.CancellationToken");
        Assert.DoesNotContain(
            handlerMethods.SelectMany(method => method.Signature.ParameterTypes),
            IsCallbackType);
        var handlerCalls = handlerMethods
            .SelectMany(caller => ReadCalledMethods(peReader, metadata, caller.Handle)
                .Select(called => new CalledMethod(caller.Name, called.DeclaringType, called.Name)))
            .ToArray();
        Assert.Single(
            handlerCalls,
            call => call.CallerName == "TryHandle" &&
                call.DeclaringType == "DesktopNode.Api.IDesktopNodeApiOpsSummaryQuery" &&
                call.Name == "Read");
        Assert.Single(
            handlerCalls,
            call => call.CallerName == "TryHandle" &&
                call.DeclaringType == "DesktopNode.Api.DesktopNodeApiOpsSummaryBuilder" &&
                call.Name == "Build");

        var queryHandle = FindTypeDefinition(
            metadata,
            "DesktopNode.Api",
            "DesktopNodeApiOpsSummaryQuery");
        var query = metadata.GetTypeDefinition(queryHandle);
        Assert.False(query.Attributes.HasFlag(TypeAttributes.Abstract));
        Assert.True(query.Attributes.HasFlag(TypeAttributes.Sealed));
        var queryFields = query.GetFields()
            .Select(handle =>
            {
                var field = metadata.GetFieldDefinition(handle);
                return new MetadataField(
                    metadata.GetString(field.Name),
                    field.DecodeSignature(typeProvider, genericContext: null),
                    field.Attributes);
            })
            .ToArray();
        Assert.Contains(
            queryFields,
            field => field.TypeName == "DesktopNode.HyperV.IDesktopNodeHyperVNativeAdapter");
        Assert.Contains(
            queryFields,
            field => field.TypeName == "DesktopNode.Runtime.DesktopNodeJobRuntime");
        Assert.Contains(
            queryFields,
            field => field.TypeName == "DesktopNode.Api.BatchEvidenceSummaryReader");
        Assert.DoesNotContain(queryFields, field => IsCallbackType(field.TypeName));
        Assert.DoesNotContain(
            queryFields,
            field => field.TypeName == "DesktopNode.Api.DesktopNodeApiRequestProcessor");
        var queryMethods = query.GetMethods()
            .Select(handle => new MetadataMethod(
                Handle: handle,
                Name: metadata.GetString(metadata.GetMethodDefinition(handle).Name),
                Signature: metadata.GetMethodDefinition(handle).DecodeSignature(typeProvider, genericContext: null)))
            .ToArray();
        AssertHandlerMethod(
            queryMethods,
            "Read",
            "DesktopNode.Api.DesktopNodeApiOpsSummarySnapshot",
            "System.Threading.CancellationToken");
        Assert.DoesNotContain(
            queryMethods.SelectMany(method => method.Signature.ParameterTypes),
            IsCallbackType);

        var apiTypeNames = metadata.TypeDefinitions
            .Select(handle => metadata.GetTypeDefinition(handle))
            .Where(definition => metadata.GetString(definition.Namespace) == "DesktopNode.Api")
            .Select(definition => metadata.GetString(definition.Name))
            .ToArray();
        Assert.DoesNotContain("DesktopNodeApiOpsSummaryDataBuilder", apiTypeNames);
        Assert.DoesNotContain("DesktopNodeApiOpsSummaryOperationInvoker", apiTypeNames);
    }

    private static void AssertHandlerMethod(
        IReadOnlyList<MetadataMethod> methods,
        string name,
        string returnType,
        params string[] parameterTypes)
    {
        var method = Assert.Single(methods, candidate => candidate.Name == name);
        Assert.Equal(returnType, method.Signature.ReturnType);
        Assert.Equal(parameterTypes, method.Signature.ParameterTypes.ToArray());
    }

    // wave 2 의 ApiRequestProcessorDecompositionOwnershipTests 가 같은 IL 워커를 쓴다.
    // 디코더를 복제하면 두 벌이 서로 다르게 틀릴 수 있어 여기 하나만 둔다.
    internal static IReadOnlyList<CalledTarget> ReadCalledMethods(
        PEReader peReader,
        MetadataReader metadata,
        MethodDefinitionHandle methodHandle)
    {
        var method = metadata.GetMethodDefinition(methodHandle);
        if (method.RelativeVirtualAddress == 0)
        {
            return [];
        }

        var il = peReader.GetMethodBody(method.RelativeVirtualAddress).GetILBytes();
        if (il is null || il.Length == 0)
        {
            return [];
        }

        var calledMethods = new List<CalledTarget>();
        var offset = 0;
        while (offset < il.Length)
        {
            var firstByte = il[offset++];
            var opCodeValue = firstByte == 0xfe
                ? unchecked((short)(0xfe00 | il[offset++]))
                : (short)firstByte;
            if (!IlOpCodes.TryGetValue(opCodeValue, out var opCode))
            {
                throw new InvalidOperationException(
                    $"Unknown IL opcode '0x{opCodeValue:x4}' in '{metadata.GetString(method.Name)}'.");
            }

            var operandOffset = offset;
            var operandSize = ReadOperandSize(opCode.OperandType, il, operandOffset);
            if (opCode.OperandType == OperandType.InlineMethod)
            {
                var metadataToken = BitConverter.ToInt32(il, operandOffset);
                var called = ReadCalledTarget(metadata, MetadataTokens.EntityHandle(metadataToken));
                if (called is not null)
                {
                    calledMethods.Add(called);
                }
            }

            offset += operandSize;
        }

        return calledMethods;
    }

    private static CalledTarget? ReadCalledTarget(MetadataReader metadata, EntityHandle handle)
    {
        return handle.Kind switch
        {
            HandleKind.MethodDefinition => ReadMethodDefinitionTarget(
                metadata,
                (MethodDefinitionHandle)handle),
            HandleKind.MemberReference => ReadMemberReferenceTarget(
                metadata,
                (MemberReferenceHandle)handle),
            HandleKind.MethodSpecification => ReadCalledTarget(
                metadata,
                metadata.GetMethodSpecification((MethodSpecificationHandle)handle).Method),
            _ => null
        };
    }

    private static CalledTarget ReadMethodDefinitionTarget(
        MetadataReader metadata,
        MethodDefinitionHandle handle)
    {
        var method = metadata.GetMethodDefinition(handle);
        return new CalledTarget(
            ReadTypeName(metadata, method.GetDeclaringType()),
            metadata.GetString(method.Name));
    }

    private static CalledTarget ReadMemberReferenceTarget(
        MetadataReader metadata,
        MemberReferenceHandle handle)
    {
        var member = metadata.GetMemberReference(handle);
        return new CalledTarget(
            ReadParentTypeName(metadata, member.Parent),
            metadata.GetString(member.Name));
    }

    private static string ReadParentTypeName(MetadataReader metadata, EntityHandle handle)
    {
        return handle.Kind switch
        {
            HandleKind.TypeDefinition => ReadTypeName(metadata, (TypeDefinitionHandle)handle),
            HandleKind.TypeReference => ReadTypeName(metadata, (TypeReferenceHandle)handle),
            HandleKind.TypeSpecification => metadata.GetTypeSpecification((TypeSpecificationHandle)handle)
                .DecodeSignature(new MetadataTypeNameProvider(), genericContext: null),
            HandleKind.MethodDefinition => ReadTypeName(
                metadata,
                metadata.GetMethodDefinition((MethodDefinitionHandle)handle).GetDeclaringType()),
            _ => handle.Kind.ToString()
        };
    }

    private static string ReadTypeName(MetadataReader metadata, TypeDefinitionHandle handle)
    {
        var definition = metadata.GetTypeDefinition(handle);
        return FullTypeName(
            metadata.GetString(definition.Namespace),
            metadata.GetString(definition.Name));
    }

    private static string ReadTypeName(MetadataReader metadata, TypeReferenceHandle handle)
    {
        var reference = metadata.GetTypeReference(handle);
        return FullTypeName(
            metadata.GetString(reference.Namespace),
            metadata.GetString(reference.Name));
    }

    private static bool IsCallbackType(string typeName)
    {
        return typeName == "System.Delegate" ||
            typeName.StartsWith("System.Func`", StringComparison.Ordinal) ||
            typeName.StartsWith("System.Action`", StringComparison.Ordinal);
    }

    private static int ReadOperandSize(OperandType operandType, byte[] il, int operandOffset)
    {
        return operandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or
            OperandType.ShortInlineI or
            OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineBrTarget or
            OperandType.InlineField or
            OperandType.InlineI or
            OperandType.InlineMethod or
            OperandType.InlineSig or
            OperandType.InlineString or
            OperandType.InlineTok or
            OperandType.InlineType or
            OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + (BitConverter.ToInt32(il, operandOffset) * 4),
            _ => throw new InvalidOperationException($"Unsupported IL operand type '{operandType}'.")
        };
    }

    private sealed record MetadataField(
        string Name,
        string TypeName,
        FieldAttributes Attributes);

    private sealed record MetadataMethod(
        MethodDefinitionHandle Handle,
        string Name,
        MethodSignature<string> Signature);

    private sealed record CalledMethod(
        string CallerName,
        string DeclaringType,
        string Name);

    internal sealed record CalledTarget(
        string DeclaringType,
        string Name);

    private sealed class MetadataTypeNameProvider : ISignatureTypeProvider<string, object?>
    {
        public string GetArrayType(string elementType, ArrayShape shape)
        {
            return elementType + "[" + new string(',', Math.Max(0, shape.Rank - 1)) + "]";
        }

        public string GetByReferenceType(string elementType) => elementType + "&";

        public string GetFunctionPointerType(MethodSignature<string> signature) => "methodptr";

        public string GetGenericInstantiation(
            string genericType,
            ImmutableArray<string> typeArguments)
        {
            return genericType + "<" + string.Join(",", typeArguments) + ">";
        }

        public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;

        public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;

        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired)
        {
            return unmodifiedType;
        }

        public string GetPinnedType(string elementType) => elementType;

        public string GetPointerType(string elementType) => elementType + "*";

        public string GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            return typeCode switch
            {
                PrimitiveTypeCode.Boolean => "System.Boolean",
                PrimitiveTypeCode.Byte => "System.Byte",
                PrimitiveTypeCode.Char => "System.Char",
                PrimitiveTypeCode.Double => "System.Double",
                PrimitiveTypeCode.Int16 => "System.Int16",
                PrimitiveTypeCode.Int32 => "System.Int32",
                PrimitiveTypeCode.Int64 => "System.Int64",
                PrimitiveTypeCode.IntPtr => "System.IntPtr",
                PrimitiveTypeCode.Object => "System.Object",
                PrimitiveTypeCode.SByte => "System.SByte",
                PrimitiveTypeCode.Single => "System.Single",
                PrimitiveTypeCode.String => "System.String",
                PrimitiveTypeCode.TypedReference => "System.TypedReference",
                PrimitiveTypeCode.UInt16 => "System.UInt16",
                PrimitiveTypeCode.UInt32 => "System.UInt32",
                PrimitiveTypeCode.UInt64 => "System.UInt64",
                PrimitiveTypeCode.UIntPtr => "System.UIntPtr",
                PrimitiveTypeCode.Void => "System.Void",
                _ => typeCode.ToString()
            };
        }

        public string GetSZArrayType(string elementType) => elementType + "[]";

        public string GetTypeFromDefinition(
            MetadataReader reader,
            TypeDefinitionHandle handle,
            byte rawTypeKind)
        {
            return ReadTypeName(reader, handle);
        }

        public string GetTypeFromReference(
            MetadataReader reader,
            TypeReferenceHandle handle,
            byte rawTypeKind)
        {
            return ReadTypeName(reader, handle);
        }

        public string GetTypeFromSpecification(
            MetadataReader reader,
            object? genericContext,
            TypeSpecificationHandle handle,
            byte rawTypeKind)
        {
            return reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
        }
    }

    internal static TypeDefinitionHandle FindTypeDefinition(
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

    private static (string FullName, string? AssemblyName) ReadDirectFieldType(
        MetadataReader metadata,
        FieldDefinitionHandle fieldHandle)
    {
        var field = metadata.GetFieldDefinition(fieldHandle);
        var signature = metadata.GetBlobReader(field.Signature);
        var header = signature.ReadSignatureHeader();
        Assert.Equal(SignatureKind.Field, header.Kind);
        Assert.Equal(SignatureTypeCode.TypeHandle, signature.ReadSignatureTypeCode());

        var typeHandle = signature.ReadTypeHandle();
        return typeHandle.Kind switch
        {
            HandleKind.TypeReference => ReadTypeReference(metadata, (TypeReferenceHandle)typeHandle),
            HandleKind.TypeDefinition => ReadTypeDefinition(metadata, (TypeDefinitionHandle)typeHandle),
            _ => throw new InvalidOperationException(
                $"Expected a direct type reference for the job runtime field, but found '{typeHandle.Kind}'.")
        };
    }

    private static (string FullName, string? AssemblyName) ReadTypeReference(
        MetadataReader metadata,
        TypeReferenceHandle handle)
    {
        var reference = metadata.GetTypeReference(handle);
        var assemblyName = reference.ResolutionScope.Kind == HandleKind.AssemblyReference
            ? metadata.GetString(metadata.GetAssemblyReference(
                (AssemblyReferenceHandle)reference.ResolutionScope).Name)
            : null;
        return (FullTypeName(
            metadata.GetString(reference.Namespace),
            metadata.GetString(reference.Name)), assemblyName);
    }

    private static (string FullName, string? AssemblyName) ReadTypeDefinition(
        MetadataReader metadata,
        TypeDefinitionHandle handle)
    {
        var definition = metadata.GetTypeDefinition(handle);
        return (FullTypeName(
            metadata.GetString(definition.Namespace),
            metadata.GetString(definition.Name)), null);
    }

    private static string FullTypeName(string typeNamespace, string typeName)
    {
        return string.IsNullOrEmpty(typeNamespace)
            ? typeName
            : typeNamespace + "." + typeName;
    }
}
