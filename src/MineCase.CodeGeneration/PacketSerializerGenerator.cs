using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MineCase.CodeGeneration
{
    [Generator(LanguageNames.CSharp)]
    public class PacketSerializerGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 1. Find class declarations that have attributes
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => IsClassWithSerializerAttribute(node),
                    transform: static (ctx, _) => GetClassAndFieldInfo(ctx))
                .Where(static info => info.HasValue)
                .Collect();

            // 2. Register the source output for the generator
            context.RegisterSourceOutput(classDeclarations, (ctx, classInfos) =>
            {
                foreach (var classInfo in classInfos!)
                {
                    var info = classInfo.Value;
                    var codeWriter = new StringBuilder();

                    var result = GeneratePacketCode(info.parentClassSymbol, info.fields);
                    ctx.AddSource(result.FileName,
                        SourceText.From(result.SourceCode, Encoding.UTF8));
                }
            });
        }

        private static bool IsClassWithSerializerAttribute(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax cls && cls.AttributeLists.Count > 0;
        }

        private static (INamedTypeSymbol parentClassSymbol, ImmutableArray<(IFieldSymbol fieldSymbol, AttributeData attribute)> fields, bool HasValue)? GetClassAndFieldInfo(GeneratorSyntaxContext context)
        {
            var cls = (ClassDeclarationSyntax)context.Node;
            var clsSymbol = context.SemanticModel.GetDeclaredSymbol(cls) as INamedTypeSymbol;
            if (clsSymbol == null) return null;

            var clsAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("MineCase.Serialization.GenerateSerializerAttribute");
            if (clsAttributeSymbol == null) return null;

            var clsAttribute = clsSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Equals(clsAttributeSymbol, SymbolEqualityComparer.Default) ?? false);

            if (clsAttribute == null) return null;

            var fieldAttributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName("MineCase.Serialization.SerializeAsAttribute");
            if (fieldAttributeSymbol == null) return null;

            // Get all members and filter for fields with the SerializeAsAttribute
            var fields = clsSymbol.GetMembers().OfType<IFieldSymbol>()
                .Select(field => (field, attribute: field.GetAttributes()
                    .FirstOrDefault(attr => attr.AttributeClass?.Equals(fieldAttributeSymbol, SymbolEqualityComparer.Default) ?? false)))
                .Where(tuple => tuple.attribute is not null)
                .ToImmutableArray();

            return (clsSymbol, fields, true);
        }

        private static GenerationResult GeneratePacketCode(INamedTypeSymbol parentClass,
            ImmutableArray<(IFieldSymbol fieldSymbol, AttributeData attribute)> fields)
        {
            var packetInfo = parentClass.GetAttributes()
                .First(attr => attr.AttributeClass?.Name == "GenerateSerializerAttribute");

            // Get the 'Methods' property from the attribute
            var methods = packetInfo.NamedArguments.FirstOrDefault(x => x.Key == "Methods").Value.Value;
            var mdValue = methods == null ? GenerateSerializerMethods.Both : (GenerateSerializerMethods)(int)methods;

            var sb = new StringBuilder();
            sb.AppendLine($"using System.IO;");
            sb.AppendLine($"using MineCase.Serialization;");
            sb.AppendLine($"");
            sb.AppendLine($"namespace {parentClass.ContainingNamespace.ToDisplayString()}");
            sb.AppendLine("{");

            // Correctly get the full type name, including generic parameters and constraints
            var typeParameterList = ((ClassDeclarationSyntax)parentClass.DeclaringSyntaxReferences[0].GetSyntax()).TypeParameterList;
            var whereClause = typeParameterList?.ToString();

            // Use the full name including generic parameters
            sb.AppendLine($"    public partial class {parentClass.Name}{typeParameterList?.ToString()}");
            sb.AppendLine("    {");

            try
            {
                // 1. Serializer
                if (mdValue == GenerateSerializerMethods.Serialize || mdValue == GenerateSerializerMethods.Both)
                {
                    sb.AppendLine("        public void Serialize(BinaryWriter bw)");
                    sb.AppendLine("        {");

                    foreach (var field in fields)
                    {
                        var code = field.attribute.ConstructorArguments[0].Value switch
                        {
                            (int)DataType.Angle => $"WriteAsAngle({field.fieldSymbol.Name})",
                            (int)DataType.Array => $"WriteAsArray({field.fieldSymbol.Name})",
                            (int)DataType.Boolean => $"WriteAsBoolean({field.fieldSymbol.Name})",
                            (int)DataType.Byte => $"WriteAsByte({OptionalEnumCastFrom(field.fieldSymbol.Type, "byte")}{field.fieldSymbol.Name})",
                            (int)DataType.ByteArray => $"WriteAsByteArray({field.fieldSymbol.Name})",
                            (int)DataType.Chat => $"WriteAsChat({field.fieldSymbol.Name})",
                            (int)DataType.Double => $"WriteAsDouble({field.fieldSymbol.Name})",
                            (int)DataType.EntityMetadata => throw new NotSupportedException(),
                            (int)DataType.Float => $"WriteAsFloat({field.fieldSymbol.Name})",
                            (int)DataType.Int => $"WriteAsInt({OptionalEnumCastFrom(field.fieldSymbol.Type, "int")}{field.fieldSymbol.Name})",
                            (int)DataType.IntArray => $"WriteAsIntArray({field.fieldSymbol.Name})",
                            (int)DataType.Long => $"WriteAsLong({OptionalEnumCastFrom(field.fieldSymbol.Type, "long")}{field.fieldSymbol.Name})",
                            (int)DataType.NbtArray => $"WriteAsNbtArray({field.fieldSymbol.Name})",
                            (int)DataType.NBTTag => $"WriteAsNBTTag({field.fieldSymbol.Name})",
                            (int)DataType.Position => $"WriteAsPosition({field.fieldSymbol.Name})",
                            (int)DataType.Short => $"WriteAsShort({OptionalEnumCastFrom(field.fieldSymbol.Type, "short")}{field.fieldSymbol.Name})",
                            (int)DataType.Slot => $"WriteAsSlot({field.fieldSymbol.Name})",
                            (int)DataType.SlotArray => $"WriteAsSlotArray({field.fieldSymbol.Name})",
                            (int)DataType.String => $"WriteAsString({field.fieldSymbol.Name})",
                            (int)DataType.UnsignedByte => $"WriteAsUnsignedByte({OptionalEnumCastFrom(field.fieldSymbol.Type, "byte")}{field.fieldSymbol.Name})",
                            (int)DataType.UnsignedShort => $"WriteAsUnsignedShort({OptionalEnumCastFrom(field.fieldSymbol.Type, "ushort")}{field.fieldSymbol.Name})",
                            (int)DataType.UUID => $"WriteAsUUID({field.fieldSymbol.Name})",
                            (int)DataType.VarInt => $"WriteAsVarInt({OptionalEnumCastFrom(field.fieldSymbol.Type, "int")}{field.fieldSymbol.Name}, out _)",
                            (int)DataType.VarIntArray => $"WriteAsVarIntArray({field.fieldSymbol.Name})",
                            (int)DataType.VarLong => $"WriteAsVarLong({OptionalEnumCastFrom(field.fieldSymbol.Type, "ulong")}{field.fieldSymbol.Name})",
                            _ => ""
                        };
                        sb.AppendLine($"            bw.{code};");
                    }
                    sb.AppendLine("        }");
                }

                if (mdValue == GenerateSerializerMethods.Both)
                    sb.AppendLine();

                // 2. Deserializer
                if (mdValue == GenerateSerializerMethods.Deserialize || mdValue == GenerateSerializerMethods.Both)
                {
                    sb.AppendLine("        public void Deserialize(ref SpanReader br)");
                    sb.AppendLine("        {");

                    foreach (var field in fields)
                    {
                        var code = field.attribute.ConstructorArguments[0].Value switch
                        {
                            (int)DataType.Angle => $"ReadAsAngle()",
                            (int)DataType.Array => $"ReadAsArray<{((IArrayTypeSymbol)field.fieldSymbol.Type).ElementType.ToDisplayString()}>({GetArrayLengthMember(field.attribute.NamedArguments)})",
                            (int)DataType.Boolean => $"ReadAsBoolean()",
                            (int)DataType.Byte => $"ReadAsByte()",
                            (int)DataType.ByteArray => $"ReadAsByteArray({GetArrayLengthMember(field.attribute.NamedArguments)})",
                            (int)DataType.Chat => $"ReadAsChat()",
                            (int)DataType.Double => $"ReadAsDouble()",
                            (int)DataType.EntityMetadata => throw new NotSupportedException(),
                            (int)DataType.Float => $"ReadAsFloat()",
                            (int)DataType.Int => $"ReadAsInt()",
                            (int)DataType.IntArray => $"ReadAsIntArray({GetArrayLengthMember(field.attribute.NamedArguments)})",
                            (int)DataType.Long => $"ReadAsLong()",
                            (int)DataType.NbtArray => $"ReadAsNbtArray({GetArrayLengthMember(field.attribute.NamedArguments)})",
                            (int)DataType.NBTTag => $"ReadAsNBTTag()",
                            (int)DataType.Position => $"ReadAsPosition()",
                            (int)DataType.Short => $"ReadAsShort()",
                            (int)DataType.Slot => $"ReadAsSlot()",
                            (int)DataType.SlotArray => $"ReadAsSlotArray({GetArrayLengthMember(field.attribute.NamedArguments)})",
                            (int)DataType.String => $"ReadAsString()",
                            (int)DataType.UnsignedByte => $"ReadAsUnsignedByte()",
                            (int)DataType.UnsignedShort => $"ReadAsUnsignedShort()",
                            (int)DataType.UUID => $"ReadAsUUID()",
                            (int)DataType.VarInt => $"ReadAsVarInt(out _)",
                            (int)DataType.VarIntArray => $"ReadAsVarIntArray({GetArrayLengthMember(field.attribute.NamedArguments)})",
                            (int)DataType.VarLong => $"ReadAsVarLong()",
                            _ => ""
                        };
                        sb.AppendLine($"            {field.fieldSymbol.Name} = {OptionalEnumCastTo(field.fieldSymbol.Type)}br.{code};");
                    }
                    sb.AppendLine("        }");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("// Error generating code: " + ex.ToString());
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return new GenerationResult()
            {
                SourceCode = sb.ToString(),
                FileName = $"{parentClass.ContainingNamespace.ToDisplayString()}_{parentClass.Name}_Serializer.g.cs"
            };
        }

        private static string GetArrayLengthMember(ImmutableArray<KeyValuePair<string, TypedConstant>> args)
        {
            var arg = args.FirstOrDefault(x => x.Key == "ArrayLengthMember").Value;
            if (arg.IsNull) return string.Empty;
            return $"(int){arg.Value}";
        }

        private static string OptionalEnumCastFrom(ITypeSymbol type, string desiredType)
        {
            if (type.TypeKind == TypeKind.Enum || type.ToDisplayString() != desiredType)
                return $"({desiredType})";
            return string.Empty;
        }

        private static string OptionalEnumCastTo(ITypeSymbol type)
        {
            if (type.TypeKind == TypeKind.Enum || type.IsUnmanagedType)
                return $"({type.ToDisplayString()})";
            return string.Empty;
        }

        /// <summary>
        /// Order matters here, since it also has to be mapped from the consuming project.
        /// TODO: Create a common MineCase.CodeGeneration.Attributes project
        /// </summary>
        private enum GenerateSerializerMethods
        {
            Serialize = 0,
            Deserialize = 1,
            Both = 2
        }

        /// <summary>
        /// Order matters here, since it also has to be mapped from the consuming project.
        /// TODO: Create a common MineCase.CodeGeneration.Attributes project
        /// </summary>
        private enum DataType
        {
            Boolean,
            Byte,
            UnsignedByte,
            Short,
            UnsignedShort,
            Int,
            Long,
            Float,
            Double,
            String,
            Chat,
            VarInt,
            VarLong,
            EntityMetadata,
            Slot,
            NBTTag,
            Position,
            Angle,
            UUID,
            ByteArray,
            IntArray,
            NbtArray,
            VarIntArray,
            SlotArray,
            Array
        }

        class GenerationResult
        {
            public string SourceCode { get; set; }
            public string FileName { get; set; }
        }
    }
}