namespace CustomCode.Core.Composition.SourceGenerator.Tests
{
    using Microsoft.CodeAnalysis;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="CompositionRootGenerator"/> type.
    /// </summary>
    public sealed class CompositionRootGeneratorTests : SourceGeneratorTest
    {
        [Fact]
        public void GenerateCompositionRoot()
        {
            // Given
            var generator = new CompositionRootGenerator();

            var source = @"
                namespace Test.Namespace
                {
                    using CustomCode.Core.Composition;

                    public interface IFoo
                    { }

                    //[Export(typeof(IFoo), Lifetime.Singleton, ServiceName = ""MyService"")]
                    [Export(Lifetime.Transient, ServiceName = ""Id"")]
                    public sealed class Foo : IFoo
                    { }
                }";
            var compilation = CreateCompilation(source);

            // When
            var generated = ExecuteSourceGenerator(compilation, generator);

            // Then
            Assert.Equal(0, compilation.GetDiagnostics().Count(d => d.Severity == DiagnosticSeverity.Error));
            Assert.Equal(0, generated.compilation.GetDiagnostics().Count(d => d.Severity == DiagnosticSeverity.Error));
            Assert.Equal(0, generated.diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error));
        }
    }
}