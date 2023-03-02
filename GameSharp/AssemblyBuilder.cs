using Microsoft.NET.HostModel.AppHost;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Reflection;
using System.Runtime.Versioning;

namespace GameSharp;

internal static class AssemblyBuilder
{
    private const string ApplicationName = "GameSharpApplication";

    private static readonly Guid s_guid = new("87D4DBE1-1143-4FAD-AAB3-1001F92068E6");
    private static readonly BlobContentId s_contentId = new(s_guid, 0x04030201);

    private static MethodDefinitionHandle GenerateMetadata(MetadataBuilder metadata, BlobBuilder ilBuilder, List<string> postfix)
    {
        // Create module and assembly for a console application
        metadata.AddModule(0, metadata.GetOrAddString($"{ApplicationName}.exe"), metadata.GetOrAddGuid(s_guid), default, default);

        AssemblyDefinitionHandle assembly = metadata.AddAssembly(metadata.GetOrAddString(ApplicationName), new Version(1, 0, 0, 0), default, default, 0, AssemblyHashAlgorithm.None);

        byte[] microsoftPublicKeyToken = { 0xB0, 0x3F, 0x5F, 0x7F, 0x11, 0xD5, 0x0A, 0x3A };

        // Create references to System.Object, System.Console and System.Versioning.TargetFrameworkAttribute types.
        AssemblyReferenceHandle systemRuntimeAssemblyRef = metadata.AddAssemblyReference(metadata.GetOrAddString("System.Runtime"), new Version(8, 0, 0, 0), default, metadata.GetOrAddBlob(microsoftPublicKeyToken), default, default);
        AssemblyReferenceHandle systemConsoleAssemblyRef = metadata.AddAssemblyReference(metadata.GetOrAddString(typeof(Console).Assembly.GetName().Name!), new Version(8, 0, 0, 0), default, metadata.GetOrAddBlob(microsoftPublicKeyToken), default, default);

        TypeReferenceHandle systemObjectTypeRef = metadata.AddTypeReference(
            systemRuntimeAssemblyRef,
            metadata.GetOrAddString(typeof(object).Namespace!),
            metadata.GetOrAddString(nameof(Object)));

        TypeReferenceHandle systemConsoleTypeRefHandle = metadata.AddTypeReference(
            systemConsoleAssemblyRef,
            metadata.GetOrAddString(typeof(Console).Namespace!),
            metadata.GetOrAddString(nameof(Console)));

        TypeReferenceHandle targetFrameworkAttHandle = metadata.AddTypeReference(
            systemRuntimeAssemblyRef,
            metadata.GetOrAddString(typeof(TargetFrameworkAttribute).Namespace!),
            metadata.GetOrAddString(nameof(TargetFrameworkAttribute)));

        BlobBuilder targetFrameworkStringCtorSignature = new();

        new BlobEncoder(targetFrameworkStringCtorSignature)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(1, static returnType => returnType.Void(), static parameters => parameters.AddParameter().Type().String());

        BlobHandle targetFrameworkStringCtorHandle = metadata.GetOrAddBlob(targetFrameworkStringCtorSignature);

        MemberReferenceHandle attCtorMemberRef = metadata.AddMemberReference(
            targetFrameworkAttHandle,
            metadata.GetOrAddString(".ctor"),
            targetFrameworkStringCtorHandle);

        BlobBuilder targetFrameworkAttributeBlob = new();

        new BlobEncoder(targetFrameworkAttributeBlob).CustomAttributeSignature(
            static fixedArgumentsEncoder => fixedArgumentsEncoder.AddArgument().Scalar().Constant(new FrameworkName(".NETCoreApp", new Version(8, 0)).FullName),
            static customAttributeNamedArgumentsEncoder => customAttributeNamedArgumentsEncoder.Count(1).AddArgument(
                false,
                static namedArgumentTypeEncoder => namedArgumentTypeEncoder.ScalarType().String(),
                static nameEncoder => nameEncoder.Name(nameof(TargetFrameworkAttribute.FrameworkDisplayName)),
                static literalEncoder => literalEncoder.Scalar().Constant(".NET 8.0")));

        metadata.AddCustomAttribute(assembly, attCtorMemberRef, metadata.GetOrAddBlob(targetFrameworkAttributeBlob));






        // Get reference to Console.WriteLine(string) method.
        BlobBuilder consoleWriteLineSignature = new();

        new BlobEncoder(consoleWriteLineSignature)
            .MethodSignature()
            .Parameters(1,
                static returnType => returnType.Void(),
                static parameters => parameters.AddParameter().Type().String());

        MemberReferenceHandle consoleWriteLineMemberRef = metadata.AddMemberReference(
            systemConsoleTypeRefHandle,
            metadata.GetOrAddString(nameof(Console.WriteLine)),
            metadata.GetOrAddBlob(consoleWriteLineSignature));

        // Get reference to Object's constructor.
        BlobBuilder parameterlessCtorSignature = new();

        new BlobEncoder(parameterlessCtorSignature)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(0, static returnType => returnType.Void(), static _ => { });

        BlobHandle parameterlessCtorBlobIndex = metadata.GetOrAddBlob(parameterlessCtorSignature);

        MemberReferenceHandle objectCtorMemberRef = metadata.AddMemberReference(
            systemObjectTypeRef,
            metadata.GetOrAddString(".ctor"),
            parameterlessCtorBlobIndex);

        // Create signature for "void Main()" method.
        BlobBuilder mainSignature = new();

        new BlobEncoder(mainSignature)
            .MethodSignature()
            .Parameters(0, static returnType => returnType.Void(), static _ => { });

        MethodBodyStreamEncoder methodBodyStream = new(ilBuilder);

        BlobBuilder codeBuilder = new();
        InstructionEncoder il;

        // Emit IL for Program::.ctor
        il = new InstructionEncoder(codeBuilder);

        il.LoadArgument(0);

        // call instance void [System.Runtime]System.Object::.ctor()
        il.Call(objectCtorMemberRef);

        // ret
        il.OpCode(ILOpCode.Ret);

        int ctorBodyOffset = methodBodyStream.AddMethodBody(il);
        codeBuilder.Clear();

        // Emit IL for Program::Main
        ControlFlowBuilder flowBuilder = new();
        il = new InstructionEncoder(codeBuilder, flowBuilder);

        //il.LoadString(metadata.GetOrAddUserString("Hello, world"));

        BlobBuilder consoleWriteLineIntBlob = new();

        new BlobEncoder(consoleWriteLineIntBlob)
            .MethodSignature()
            .Parameters(1,
                static returnType => returnType.Void(),
                static parameters => parameters.AddParameter().Type().Int32());

        MemberReferenceHandle consoleWriteLineIntRef = metadata.AddMemberReference(
            systemConsoleTypeRefHandle,
            metadata.GetOrAddString(nameof(Console.WriteLine)),
            metadata.GetOrAddBlob(consoleWriteLineIntBlob));






        BlobBuilder mathPowSignatureBlob = new();

        new BlobEncoder(mathPowSignatureBlob)
            .MethodSignature()
            .Parameters(
                2,
                static returnTypeEncoder => returnTypeEncoder.Type().Double(),
                static parametersEncoder =>
                {
                    parametersEncoder.AddParameter().Type().Double();
                    parametersEncoder.AddParameter().Type().Double();
                });

        BlobHandle mathPowSignatureHandle = metadata.GetOrAddBlob(mathPowSignatureBlob);

        TypeReferenceHandle systemMathHandle = metadata.AddTypeReference(
            systemRuntimeAssemblyRef,
            metadata.GetOrAddString(typeof(Math).Namespace!),
            metadata.GetOrAddString(nameof(Math)));

        MemberReferenceHandle mathPowRef = metadata.AddMemberReference(systemMathHandle, metadata.GetOrAddString(nameof(Math.Pow)), mathPowSignatureHandle);

        foreach (string token in postfix)
        {
            if (int.TryParse(token, out int val))
            {
                il.LoadConstantI4(val);
            }
            else if (GameSharpCompiler.IsOperator(token))
            {
                switch (token)
                {
                    case "+":
                        il.OpCode(ILOpCode.Add);
                        break;
                    case "-":
                        il.OpCode(ILOpCode.Sub);
                        break;
                    case "*":
                        il.OpCode(ILOpCode.Mul);
                        break;
                    case "/":
                    case "÷":
                        il.OpCode(ILOpCode.Div);
                        break;
                    case "%":
                        il.OpCode(ILOpCode.Rem);
                        break;
                    case "^":
                        // convert top two values to float64, since that's
                        // what Math.Pow expects

                        il.StoreLocal(0); // pop into temp variable
                        il.OpCode(ILOpCode.Conv_r8); // convert remaining value
                        il.LoadLocal(0); // push temp variable to stack
                        il.OpCode(ILOpCode.Conv_r8); // convert pushed value

                        // call Math.Pow
                        il.Call(mathPowRef);

                        // convert the float64 return value to int32
                        il.OpCode(ILOpCode.Conv_i4);
                        break;
                    default:
                        throw new /*Compile*/Exception($"Unknown operator: '{token}'.");
                }
            }
            else
            {
                // if the token's not an integer or an operator, barf appropriately
                if ("()".Contains(token))
                {
                    throw new /*Compile*/Exception("Unbalanced parentheses.");
                }
                else
                {
                    throw new /*Compile*/Exception($"Unable to compile expression; unknown token '{token}'.");
                }
            }
        }

        il.Call(consoleWriteLineIntRef);

        // call void System.Console::WriteLine(string)
        //il.Call(consoleWriteLineMemberRef);

        // ret
        il.OpCode(ILOpCode.Ret);



        // if a method requires local variables, this has to be defined like this
        BlobBuilder mainLocalVariables = new();
        new BlobEncoder(mainLocalVariables).LocalVariableSignature(1).AddVariable().Type().Int32();
        StandaloneSignatureHandle mainLocalVariablesSignature = metadata.AddStandaloneSignature(metadata.GetOrAddBlob(mainLocalVariables));



        int mainBodyOffset = methodBodyStream.AddMethodBody(il, localVariablesSignature: mainLocalVariablesSignature);
        codeBuilder.Clear();

        // Create method definition for Program::Main
        MethodDefinitionHandle mainMethodDef = metadata.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            MethodImplAttributes.IL,
            metadata.GetOrAddString("Main"),
            metadata.GetOrAddBlob(mainSignature),
            mainBodyOffset,
            parameterList: default);

        // Create method definition for Program::.ctor
        MethodDefinitionHandle ctorDef = metadata.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            MethodImplAttributes.IL,
            metadata.GetOrAddString(".ctor"),
            parameterlessCtorBlobIndex,
            ctorBodyOffset,
            parameterList: default);

        // Create type definition for the special <Module> type that holds global functions
        metadata.AddTypeDefinition(
            default,
            default,
            metadata.GetOrAddString("<Module>"),
            baseType: default,
            fieldList: MetadataTokens.FieldDefinitionHandle(1),
            methodList: mainMethodDef);

        // Create type definition for ConsoleApplication.Program
        metadata.AddTypeDefinition(
            TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit,
            metadata.GetOrAddString(ApplicationName),
            metadata.GetOrAddString(nameof(Program)),
            baseType: systemObjectTypeRef,
            fieldList: MetadataTokens.FieldDefinitionHandle(1),
            methodList: mainMethodDef);

        return mainMethodDef;
    }

    private static void WritePEImage(Stream peStream, MetadataBuilder metadataBuilder, BlobBuilder ilBuilder, MethodDefinitionHandle entryPointHandle)
    {
        PEHeaderBuilder peHeaderBuilder = new(imageCharacteristics: Characteristics.ExecutableImage | Characteristics.LargeAddressAware);

        ManagedPEBuilder peBuilder = new(peHeaderBuilder, new MetadataRootBuilder(metadataBuilder), ilBuilder, entryPoint: entryPointHandle, flags: CorFlags.ILOnly, deterministicIdProvider: static _ => s_contentId);

        BlobBuilder peBlob = new();
        BlobContentId contentId = peBuilder.Serialize(peBlob);
        peBlob.WriteContentTo(peStream);
    }

    internal static void BuildApplication(List<string> postfix)
    {
        string desktopApplicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ApplicationName);

        Directory.CreateDirectory(desktopApplicationPath);

        using (FileStream peStream = new(Path.Combine(desktopApplicationPath, $"{ApplicationName}.dll"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            BlobBuilder ilBuilder = new();
            MetadataBuilder metadataBuilder = new();

            MethodDefinitionHandle entryPoint = GenerateMetadata(metadataBuilder, ilBuilder, postfix);
            WritePEImage(peStream, metadataBuilder, ilBuilder, entryPoint);
        }

        HostWriter.CreateAppHost(@"..\..\..\apphost.exe", Path.Combine(desktopApplicationPath, $"{ApplicationName}.exe"), @$".\{ApplicationName}.dll");
        
        File.WriteAllText(Path.Combine(desktopApplicationPath, $"{ApplicationName}.runtimeconfig.json"), """
            {
                "runtimeOptions":
                {
                    "tfm": "net8.0",
                    "rollForward": "latestMajor",
                    "framework":
                    {
                        "name": "Microsoft.NETCore.App",
                        "version": "8.0.0-preview.1.23110.8"
                    }
                },
                "configProperties":
                {
                    "System.Runtime.TieredPGO": true
                }
            }
            """);
    }
}
