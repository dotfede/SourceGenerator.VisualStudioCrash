using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SourceGen
{
    [Generator]
    public class ClassGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var value = 
                context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (x, _) => x is VariableDeclarationSyntax, 
                transform: (ctx, _) => ((VariableDeclarationSyntax)ctx.Node).Variables[0].Initializer.Value.ToString());

            var numbers = context.CompilationProvider.Combine(value.Collect());

            context.RegisterSourceOutput(numbers, (spc, data) =>
            {
                if (!data.Right.Any())
                    return;

                var numberOfClasses =
                    int.TryParse(data.Right.First(), out var n)
                    ? n
                    : 0;

                if (numberOfClasses <= 0)
                    return;

                var classes =
                    Enumerable.Range(1, numberOfClasses)
                              .Select(x => $"    public class Class{x} {{ }}")
                              .ToArray();

                var classesCode = string.Join(Environment.NewLine, classes);

                var code = $@"
namespace SourceGen
{{
{classesCode}
}}
";

                spc.AddSource("generated.cs", SourceText.From(code, Encoding.UTF8));
            });
        }
    }
}

