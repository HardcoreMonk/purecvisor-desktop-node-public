using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

// wave 1 은 diagnostics/auth/ops 소유자를 DesktopNodeApiRequestProcessor 에서 떼어내고
// ApiArchitectureOwnershipTests 로 그 형태를 잠갔다. 이 파일은 그 다음 물결이 떼어낸 소유자를
// 같은 방식으로 잠근다. 떠난 것과 도착한 것을 함께 확인해야 "옮긴 척하고 원본을 남겨둔" 상태가
// 통과하지 않는다.
//
// BindingFlags 대신 metadata 를 읽는 이유: csharp-architecture-test-migration.json 이 테스트 코드의
// private_reflection.current_occurrence_count 를 0 으로 고정하고 있고, ApiArchitectureOwnershipTests
// 가 PEReader 를 그 정책에 맞는 패턴으로 이미 세워 뒀다.
public sealed class ApiRequestProcessorDecompositionOwnershipTests
{
    private const string ApiNamespace = "DesktopNode.Api";
    private const string ProcessorTypeName = "DesktopNode.Api.DesktopNodeApiRequestProcessor";

    [Fact]
    public void ResponseEnvelopeConstructionLeavesTheProcessor()
    {
        var moved = new[]
        {
            "GetStatusForOperationResult",
            "Failure",
            "Body",
            "JobData",
            "EmptyObject",
            "JsonFromObject",
            "Json",
            "AttachRequestId",
            "SerializeResponsePayload",
            "OperationResponse",
            "JobCreated"
        };

        AssertProcessorDoesNotDeclare(moved);
        AssertTypeDeclares("DesktopNodeApiResponseFactory", moved);
    }

    [Fact]
    public void JsonElementReadingLeavesTheProcessor()
    {
        var moved = new[]
        {
            "FindVm",
            "ReadNestedElement",
            "EnumerateVmList",
            "EnumerateCheckpointList",
            "MatchesVmId",
            "GetStringProperty",
            "ReadString",
            "ReadInt",
            "ReadBool",
            "ReadStringList",
            "ReadStringDictionary",
            "ReadElement",
            "ReadOperationResult",
            "ReadApiError"
        };

        AssertProcessorDoesNotDeclare(moved);
        AssertTypeDeclares("DesktopNodeApiJsonReader", moved);
    }

    [Fact]
    public void RequestParsingLeavesTheProcessor()
    {
        var moved = new[]
        {
            "TryParseBody",
            "DecodeRouteId",
            "TryMatch",
            "ParseJobListPage",
            "TryParseNonNegativeInt",
            "QueryValue",
            "NormalizePath",
            "NormalizeRequestId"
        };

        AssertProcessorDoesNotDeclare(moved);
        AssertTypeDeclares("DesktopNodeApiRequestParsing", moved);
    }

    [Fact]
    public void Wave1OwnersDoNotCarryTheirOwnResponseHelperCopies()
    {
        // wave 1 은 소유자를 processor 에서 떼어내면서 응답/파싱 helper 를 소유자마다 복사해 갔다.
        // processor 가 그것을 선언하지 않는지는 위 두 [Fact] 가 이미 잠갔지만, 사본 쪽은 잠기지
        // 않아 정규 버전과 나란히 살아 있었다. 사본이 남으면 정규 버전을 고쳐도 소유자는 옛 동작을
        // 유지하므로, "떼어냈다"는 단언이 실제로는 성립하지 않는다.
        var canonicalHelpers = new[]
        {
            "Json",
            "Body",
            "Failure",
            "SerializeResponsePayload",
            "TryParseBody",
            "EmptyObject",
            "JobData",
            "JsonFromObject"
        };

        foreach (var owner in new[]
        {
            "DesktopNodeApiAuthSessionHandler",
            "DesktopNodeApiDiagnosticsHandler",
            "DesktopNodeApiOpsSummaryHandler",
            "DesktopNodeApiOpsSummaryQuery"
        })
        {
            AssertTypeDoesNotDeclare(owner, canonicalHelpers);
        }

        // ParsedJson 은 메서드가 아니라 중첩 레코드로 복사돼 있었다. 이름만 같은 두 번째 타입이
        // 다시 생기면 위 메서드 단언은 통과하므로 여기서 따로 막는다.
        AssertTypeDeclaresNoNestedType("DesktopNodeApiAuthSessionHandler", "ParsedJson");

        // 떠난 자리에 도착지가 있는지도 함께 확인한다. 도착지가 비면 사본 제거가 아니라 기능 삭제다.
        AssertTypeDeclares(
            "DesktopNodeApiResponseFactory",
            "Json",
            "Body",
            "Failure",
            "SerializeResponsePayload",
            "EmptyObject",
            "JobData",
            "JsonFromObject");
        AssertTypeDeclares("DesktopNodeApiRequestParsing", "TryParseBody");
    }

    [Fact]
    public void ErrorMappingAndNativeInvocationLeaveTheProcessor()
    {
        AssertProcessorDoesNotDeclare(
            "ToRuntimeError",
            "ToApiError",
            "JobStoreCommitError",
            "ToJobExecutionOutcome",
            "InvokeHyperVOperation",
            "IsNativeOperationCandidate");
        AssertTypeDeclares(
            "DesktopNodeApiErrorMapping",
            "ToRuntimeError",
            "ToApiError",
            "JobStoreCommitError",
            "ToJobExecutionOutcome");
        AssertTypeDeclares(
            "DesktopNodeApiHyperVOperationInvoker",
            "Invoke",
            "IsNativeOperationCandidate");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiHyperVOperationInvoker");
    }

    [Fact]
    public void ConsoleRoutesUseACallbackFreeOwner()
    {
        AssertProcessorDoesNotDeclare(
            "CreateConsoleRuntimePolicy",
            "HandleConsoleCapabilities",
            "HandleVmConsoleSession",
            "BuildConsoleCapabilities",
            "BuildVmConsoleSession",
            "FormatNoVncWebSocketPath");
        AssertTypeDeclares("DesktopNodeApiConsoleRouteHandler", "TryHandle", "CreateRuntimePolicy");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiConsoleRouteHandler");
        AssertApiAssemblyDoesNotDefine("DesktopNodeApiConsoleHandler");
    }

    [Fact]
    public void GuestExecutionRoutesUseACallbackFreeOwner()
    {
        AssertProcessorDoesNotDeclare(
            "HandleGuestExecPreviewRoute",
            "HandleGuestChannelPreviewRoute",
            "HandleGuestExecutionBoundaryRoute",
            "GuestExecutionOperationFor");
        AssertTypeDeclares("DesktopNodeApiGuestExecutionRouteHandler", "TryHandle");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiGuestExecutionRouteHandler");
    }

    [Fact]
    public void JobQueryAndControlRoutesUseACallbackFreeOwner()
    {
        AssertProcessorDoesNotDeclare(
            "BuildJobListResponse",
            "HandleJobGet",
            "HandleJobCancel",
            "HandleJobRetry",
            "HandleVmDeleteStatus");
        AssertTypeDeclares("DesktopNodeApiJobRouteHandler", "TryHandle", "HandleVmDeleteStatus");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiJobRouteHandler");
        AssertApiAssemblyDoesNotDefine("DesktopNodeApiJobRuntimeHandler");
    }

    [Fact]
    public void ReconciliationUsesACallbackFreeOwner()
    {
        AssertProcessorDoesNotDeclare(
            "HandleJobReconcile",
            "ReconcileVmDeleteJob",
            "ReconcileCheckpointCreateJob",
            "ReconcileCheckpointRestoreJob",
            "RenderReconciliationResult",
            "ReconciliationRequiredError",
            "BuildVmRenameParameters",
            "BuildVmDeleteParameters",
            "BuildCheckpointCreateParameters",
            "BuildCheckpointRestoreParameters",
            "CaptureVmRenameBaseline",
            "CaptureVmDeleteBaseline",
            "CaptureCheckpointCreateBaseline",
            "CaptureCheckpointRestoreBaseline",
            "BuildVmRenameFingerprint",
            "BuildVmDeleteFingerprint",
            "IsManagedVm",
            "RenameFingerprintMatches",
            "TryReadCapturedRenameBaseline",
            "TryReadCapturedDeleteBaseline",
            "TryReadCapturedCheckpointCreateBaseline",
            "TryReadCapturedCheckpointRestoreBaseline");
        AssertTypeDeclares(
            "DesktopNodeApiJobReconciliationHandler",
            "TryHandle",
            "BuildVmRenameParameters",
            "BuildVmDeleteParameters",
            "BuildCheckpointCreateParameters",
            "BuildCheckpointRestoreParameters");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiJobReconciliationHandler");
    }

    [Fact]
    public void QueuedMutationRoutesUseACallbackFreeOwner()
    {
        AssertProcessorDoesNotDeclare(
            "HandleQueuedMutationRoute",
            "QueueVmLimit",
            "HandleQosPreviewRoute",
            "QueueVmQosMutation",
            "ValidateQosRange",
            "BuildQosParameters",
            "QueueVmResourceMutation",
            "QueueVmGuestExec",
            "QueueVmGuestChannelVerify",
            "QueueVmGuestChannelEnsure",
            "CreateJob");
        AssertTypeDeclares(
            "DesktopNodeApiVmMutationRouteHandler",
            "HandleQueuedMutationRoute",
            "TryHandleQosPreview");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiVmMutationRouteHandler");
    }

    [Fact]
    public void WorkerTickExecutionLeavesTheProcessorButKeepsItsPublicSurface()
    {
        AssertProcessorDoesNotDeclare("ProcessOneQueuedJobAsync", "CreateJob");
        AssertTypeDeclares("DesktopNodeApiJobWorker", "ProcessOneQueuedJobAsync");

        // 공개 표면 3종은 processor 에 남아 worker 로 위임한다. 이것이 사라지면 호출자 계약이 깨진다.
        var processorMethods = GetDeclaredMethodNames(nameof(DesktopNodeApiRequestProcessor));
        Assert.Contains("ProcessOneQueuedJob", processorMethods);
        Assert.Contains("ProcessWorkerPool", processorMethods);
        Assert.Contains("RunWorkerLoopAsync", processorMethods);
    }

    [Fact]
    public void WorkerOwnerCarriesExactlyOneDeclaredCallbackSeam()
    {
        // BeforeJobFinalization 은 provider 결과와 직렬화된 finalization 경계를 결정적으로 만드는
        // 테스트 seam 이다. 도메인 협력자가 아니므로 callback-free 규칙의 유일한 예외로 남기되,
        // 예외를 조용히 두면 다음 사람이 두 번째 callback 을 추가하므로 크기를 여기서 고정한다.
        AssertDeclaredCallbackFieldsAre("DesktopNodeApiJobWorker", "BeforeJobFinalization");
    }

    [Fact]
    public void VmReadRoutesUseACallbackFreeOwner()
    {
        // HandleCore 가 dispatcher 로 남으려면 GET 조회 분기도 떠나야 한다. 남겨 두면 HandleCore 가
        // 라우팅과 provider 호출을 동시에 갖는 상태로 되돌아간다.
        AssertTypeDeclares("DesktopNodeApiVmReadRouteHandler", "Handle");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiVmReadRouteHandler");
        AssertProcessorCallsOwner("HandleCore", "DesktopNodeApiVmReadRouteHandler", "Handle");
    }

    [Fact]
    public void HandleCoreIsTheOnlyDispatcherForEveryRouteOwner()
    {
        // 이 물결의 핵심 불변식이다. 소유자를 만들어 놓고 다른 곳에서 호출하기 시작하면 라우팅이
        // 다시 흩어진다. wave 1 의 ApiArchitectureOwnershipTests 가 diagnostics/auth/ops 에 대해
        // 같은 것을 확인하고, 여기서 나머지 소유자를 같은 기준으로 잠근다.
        AssertProcessorCallsOwner("HandleCore", "DesktopNodeApiJobRouteHandler", "TryHandle");
        AssertProcessorCallsOwner("HandleCore", "DesktopNodeApiJobReconciliationHandler", "TryHandle");
        AssertProcessorCallsOwner("HandleCore", "DesktopNodeApiConsoleRouteHandler", "TryHandle");
        AssertProcessorCallsOwner("HandleCore", "DesktopNodeApiGuestExecutionRouteHandler", "TryHandle");
        AssertProcessorCallsOwner("HandleCore", "DesktopNodeApiVmMutationRouteHandler", "TryHandleQosPreview");
        AssertProcessorCallsOwner("HandleCore", "DesktopNodeApiVmMutationRouteHandler", "HandleQueuedMutationRoute");
    }

    [Fact]
    public void RequestThrottlingLeavesTheProcessor()
    {
        AssertProcessorDoesNotDeclare(
            "EnforceRequestRateLimit",
            "RateLimitExceededResponse",
            "RouteTimeoutResponse");
        AssertTypeDeclares("DesktopNodeApiRequestThrottle", "Enforce", "RouteTimeoutResponse");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiRequestThrottle");
    }

    internal static string[] GetDeclaredMethodNames(string typeName)
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();

        return metadata.GetTypeDefinition(FindTypeDefinition(metadata, typeName))
            .GetMethods()
            .Select(handle => metadata.GetString(metadata.GetMethodDefinition(handle).Name))
            .ToArray();
    }

    internal static void AssertProcessorDoesNotDeclare(params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(nameof(DesktopNodeApiRequestProcessor));
        foreach (var methodName in methodNames)
        {
            Assert.DoesNotContain(methodName, declared);
        }
    }

    internal static void AssertTypeDeclares(string typeName, params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(typeName);
        foreach (var methodName in methodNames)
        {
            Assert.Contains(methodName, declared);
        }
    }

    internal static void AssertTypeDoesNotDeclare(string typeName, params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(typeName);
        foreach (var methodName in methodNames)
        {
            Assert.DoesNotContain(methodName, declared);
        }
    }

    internal static void AssertTypeDeclaresNoNestedType(string typeName, string nestedTypeName)
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var nestedNames = metadata.GetTypeDefinition(FindTypeDefinition(metadata, typeName))
            .GetNestedTypes()
            .Select(handle => metadata.GetString(metadata.GetTypeDefinition(handle).Name))
            .ToArray();

        Assert.DoesNotContain(nestedTypeName, nestedNames);
    }

    // wave 1B~1D 가 세운 규칙을 이름 하나로 재사용할 수 있게 일반화한 것이다. 소유자가 Func/Action
    // 을 들고 있으면 그것은 곧 processor 로 되돌아가는 통로이고, processor 타입을 직접 참조하면
    // 왕복이 된다. 둘 다 배제해야 "소유"가 성립한다.
    internal static void AssertTypeIsCallbackFreeOwner(string typeName)
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var typeProvider = new MetadataTypeNameProvider();
        var typeHandle = FindTypeDefinition(metadata, typeName);
        var definition = metadata.GetTypeDefinition(typeHandle);

        Assert.True(definition.Attributes.HasFlag(TypeAttributes.Sealed), $"{typeName} must be sealed.");
        Assert.False(definition.Attributes.HasFlag(TypeAttributes.Abstract), $"{typeName} must not be abstract.");

        var fieldTypes = definition.GetFields()
            .Select(handle => metadata.GetFieldDefinition(handle)
                .DecodeSignature(typeProvider, genericContext: null))
            .ToArray();
        Assert.DoesNotContain(fieldTypes, IsCallbackType);
        Assert.DoesNotContain(fieldTypes, fieldType => fieldType == ProcessorTypeName);

        var signatureTypes = definition.GetMethods()
            .Select(handle => metadata.GetMethodDefinition(handle)
                .DecodeSignature(typeProvider, genericContext: null))
            .SelectMany(signature => signature.ParameterTypes.Append(signature.ReturnType))
            .ToArray();
        Assert.DoesNotContain(signatureTypes, IsCallbackType);
        Assert.DoesNotContain(signatureTypes, signatureType => signatureType == ProcessorTypeName);
    }

    // IL 디코더는 ApiArchitectureOwnershipTests 에 하나만 두고 빌려 쓴다. 두 벌을 두면 서로 다르게
    // 틀릴 수 있고, 그 경우 어느 쪽이 맞는지 판단할 근거가 없어진다.
    internal static void AssertProcessorCallsOwner(
        string callerMethodName,
        string ownerTypeName,
        string ownerMethodName)
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var processorHandle = ApiArchitectureOwnershipTests.FindTypeDefinition(
            metadata,
            ApiNamespace,
            nameof(DesktopNodeApiRequestProcessor));
        var callerHandles = metadata.GetTypeDefinition(processorHandle)
            .GetMethods()
            .Where(handle => metadata.GetString(metadata.GetMethodDefinition(handle).Name) == callerMethodName)
            .ToArray();
        Assert.NotEmpty(callerHandles);

        var calls = callerHandles
            .SelectMany(handle => ApiArchitectureOwnershipTests.ReadCalledMethods(peReader, metadata, handle))
            .ToArray();
        Assert.Contains(
            calls,
            call => call.DeclaringType == ApiNamespace + "." + ownerTypeName &&
                call.Name == ownerMethodName);
    }

    internal static void AssertApiAssemblyDoesNotDefine(string typeName)
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var apiTypeNames = metadata.TypeDefinitions
            .Select(handle => metadata.GetTypeDefinition(handle))
            .Where(definition => metadata.GetString(definition.Namespace) == ApiNamespace)
            .Select(definition => metadata.GetString(definition.Name))
            .ToArray();

        Assert.DoesNotContain(typeName, apiTypeNames);
    }

    // 예외를 조용히 두면 다음 사람이 두 번째 callback 을 추가한다. 예외가 필요한 타입은
    // "정확히 이 이름들만" 을 단언해 예외의 크기를 고정한다.
    internal static void AssertDeclaredCallbackFieldsAre(string typeName, params string[] expectedFieldNames)
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var typeProvider = new MetadataTypeNameProvider();
        var definition = metadata.GetTypeDefinition(FindTypeDefinition(metadata, typeName));
        var callbackFieldNames = definition.GetFields()
            .Select(handle => metadata.GetFieldDefinition(handle))
            .Where(field => IsCallbackType(field.DecodeSignature(typeProvider, genericContext: null)))
            // 자동 구현 속성의 backing field 는 '<Name>k__BackingField' 로 방출된다.
            .Select(field => StripBackingFieldDecoration(metadata.GetString(field.Name)))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedFieldNames.OrderBy(name => name, StringComparer.Ordinal).ToArray(),
            callbackFieldNames);
    }

    private static string StripBackingFieldDecoration(string fieldName)
    {
        const string suffix = ">k__BackingField";
        if (!fieldName.StartsWith('<') || !fieldName.EndsWith(suffix, StringComparison.Ordinal))
        {
            return fieldName;
        }

        return fieldName[1..^suffix.Length];
    }

    private static bool IsCallbackType(string typeName)
    {
        return typeName == "System.Delegate" ||
            typeName.StartsWith("System.Func`", StringComparison.Ordinal) ||
            typeName.StartsWith("System.Action`", StringComparison.Ordinal) ||
            typeName == "System.Action";
    }

    private static TypeDefinitionHandle FindTypeDefinition(MetadataReader metadata, string typeName)
    {
        return metadata.TypeDefinitions.Single(handle =>
        {
            var definition = metadata.GetTypeDefinition(handle);
            return metadata.GetString(definition.Namespace) == ApiNamespace &&
                metadata.GetString(definition.Name) == typeName;
        });
    }

    private sealed class MetadataTypeNameProvider : ISignatureTypeProvider<string, object?>
    {
        public string GetArrayType(string elementType, ArrayShape shape)
        {
            return elementType + "[" + new string(',', Math.Max(0, shape.Rank - 1)) + "]";
        }

        public string GetByReferenceType(string elementType) => elementType + "&";

        public string GetFunctionPointerType(MethodSignature<string> signature) => "methodptr";

        public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments)
        {
            return genericType + "<" + string.Join(",", typeArguments) + ">";
        }

        public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;

        public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;

        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;

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

        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var definition = reader.GetTypeDefinition(handle);
            return FullTypeName(reader.GetString(definition.Namespace), reader.GetString(definition.Name));
        }

        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var reference = reader.GetTypeReference(handle);
            return FullTypeName(reader.GetString(reference.Namespace), reader.GetString(reference.Name));
        }

        public string GetTypeFromSpecification(
            MetadataReader reader,
            object? genericContext,
            TypeSpecificationHandle handle,
            byte rawTypeKind)
        {
            return reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
        }

        private static string FullTypeName(string typeNamespace, string typeName)
        {
            return string.IsNullOrEmpty(typeNamespace) ? typeName : typeNamespace + "." + typeName;
        }
    }
}
