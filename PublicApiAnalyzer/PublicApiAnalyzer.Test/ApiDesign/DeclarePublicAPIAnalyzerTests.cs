﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace PublicApiAnalyzer.Test.ApiDesign
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using PublicApiAnalyzer.ApiDesign;
    using Xunit;
    using Path = System.IO.Path;

    public class DeclarePublicAPIAnalyzerTests
    {
        private string shippedText;
        private string shippedFilePath = DeclarePublicAPIAnalyzer.ShippedFileName;
        private string unshippedText;
        private string unshippedFilePath = DeclarePublicAPIAnalyzer.UnshippedFileName;

        [Fact]
        public async Task SimpleMissingTypeAsync()
        {
            var source = @"
public class C
{
    private C() { }
}
";

            this.shippedText = string.Empty;
            this.unshippedText = string.Empty;
            var fixedApi = "C" + Environment.NewLine;

            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("C").WithLocation(2, 14);
            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedApi, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task SimpleMissingMemberAsync()
        {
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
    public void Method() { }
    public int ArrowExpressionProperty => 0;
}
";

            this.shippedText = string.Empty;
            this.unshippedText = string.Empty;
            var fixedApi = @"C
C.ArrowExpressionProperty.get -> int
C.C() -> void
C.Field -> int
C.Method() -> void
C.Property.get -> int
C.Property.set -> void
";

            DiagnosticResult[] expected =
            {
                // Test0.cs(2,14): error RS0016: Symbol 'C' is not part of the declared API.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("C").WithLocation(2, 14),

                // Test0.cs(2,14): warning RS0016: Symbol 'implicit constructor for C' is not part of the declared API.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("implicit constructor for C").WithLocation(2, 14),

                // Test0.cs(4,16): error RS0016: Symbol 'Field' is not part of the declared API.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("Field").WithLocation(4, 16),

                // Test0.cs(5,27): error RS0016: Symbol 'Property.get' is not part of the declared API.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("Property.get").WithLocation(5, 27),

                // Test0.cs(5,32): error RS0016: Symbol 'Property.set' is not part of the declared API.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("Property.set").WithLocation(5, 32),

                // Test0.cs(6,17): error RS0016: Symbol 'Method' is not part of the declared API.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("Method").WithLocation(6, 17),

                // Test0.cs(7,43): error RS0016: Symbol 'ArrowExpressionProperty.get' is not part of the declared API.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("ArrowExpressionProperty.get").WithLocation(7, 43),
            };

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedApi, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task ShippedTextWithImplicitConstructorAsync()
        {
            var source = @"
public class C
{
    private C() { }
}
";

            this.shippedText = @"
C
C -> void()";
            this.unshippedText = string.Empty;

            // PublicAPI.Shipped.txt(3,1): warning RS0017: Symbol 'C -> void()' is part of the declared API, but is either not public or could not be found
            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.RemoveDeletedApiRule).WithArguments("C -> void()").WithLocation(DeclarePublicAPIAnalyzer.ShippedFileName, 3, 1);

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task ShippedTextForImplicitConstructorAsync()
        {
            var source = @"
public class C
{
}
";

            this.shippedText = @"
C
C.C() -> void";
            this.unshippedText = string.Empty;

            await this.VerifyAnalyzerAsync(source).ConfigureAwait(false);
        }

        [Fact]
        public async Task UnshippedTextForImplicitConstructorAsync()
        {
            var source = @"
public class C
{
}
";

            this.shippedText = @"
C";
            this.unshippedText = @"
C.C() -> void";

            await this.VerifyAnalyzerAsync(source).ConfigureAwait(false);
        }

        [Fact]
        public async Task ShippedTextWithMissingImplicitConstructorAsync()
        {
            var source = @"
public class C
{
}
";

            this.shippedText = @"
C";
            this.unshippedText = string.Empty;

            // Test0.cs(2,14): warning RS0016: Symbol 'implicit constructor for C' is not part of the declared API.
            var arg = string.Format(RoslynDiagnosticsResources.PublicImplicitConstructorErrorMessageName, "C");
            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments(arg).WithLocation(2, 14);

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task ShippedTextWithMissingImplicitStructConstructorAsync()
        {
            var source = @"
public struct C
{
}
";

            this.shippedText = @"
C";
            this.unshippedText = string.Empty;

            // Test0.cs(2,15): warning RS0016: Symbol 'implicit constructor for C' is not part of the declared API.
            var arg = string.Format(RoslynDiagnosticsResources.PublicImplicitConstructorErrorMessageName, "C");
            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments(arg).WithLocation(2, 15);

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task ShippedTextWithMissingImplicitStructConstructorWithOtherOverloadsAsync()
        {
            var source = @"
public struct C
{
    public C(int value)
    {
    }
}
";

            this.shippedText = @"
C
C.C(int value) -> void";
            this.unshippedText = string.Empty;

            // Test0.cs(2,15): warning RS0016: Symbol 'implicit constructor for C' is not part of the declared API.
            var arg = string.Format(RoslynDiagnosticsResources.PublicImplicitConstructorErrorMessageName, "C");
            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments(arg).WithLocation(2, 15);

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task ShippedTextWithImplicitConstructorAndBreakingCodeChangeAsync()
        {
            var source = @"
public class C
{
    private C() { }
}
";

            this.shippedText = @"
C
C.C() -> void";
            this.unshippedText = string.Empty;

            // PublicAPI.Shipped.txt(3,1): warning RS0017: Symbol 'C.C() -> void' is part of the declared API, but is either not public or could not be found
            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.RemoveDeletedApiRule).WithArguments("C.C() -> void").WithLocation(DeclarePublicAPIAnalyzer.ShippedFileName, 3, 1);

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task SimpleMemberAsync()
        {
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
    public void Method() { }
}
";

            this.shippedText = @"
C
C.C() -> void
C.Field -> int
C.Property.get -> int
C.Property.set -> void
C.Method() -> void
";
            this.unshippedText = string.Empty;

            await this.VerifyAnalyzerAsync(source).ConfigureAwait(false);
        }

        [Fact]
        public async Task SplitBetweenShippedUnshippedAsync()
        {
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
    public void Method() { }
}
";

            this.shippedText = @"
C
C.C() -> void
C.Field -> int
C.Property.get -> int
C.Property.set -> void
";
            this.unshippedText = @"
C.Method() -> void
";

            await this.VerifyAnalyzerAsync(source).ConfigureAwait(false);
        }

        [Fact]
        public async Task EnumSplitBetweenFilesAsync()
        {
            var source = @"
public enum E
{
    V1 = 1,
    V2 = 2,
    V3 = 3,
}
";

            this.shippedText = @"
E
E.V1 = 1 -> E
E.V2 = 2 -> E
";

            this.unshippedText = @"
E.V3 = 3 -> E
";

            await this.VerifyAnalyzerAsync(source).ConfigureAwait(false);
        }

        [Fact]
        public async Task SimpleRemovedMemberAsync()
        {
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
}
";

            this.shippedText = @"
C
C.C() -> void
C.Field -> int
C.Property.get -> int
C.Property.set -> void
C.Method() -> void
";

            this.unshippedText = $@"
{DeclarePublicAPIAnalyzer.RemovedApiPrefix}C.Method() -> void
";

            await this.VerifyAnalyzerAsync(source).ConfigureAwait(false);
        }

        [Fact]
        public async Task ApiFileShippedWithRemovedAsync()
        {
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
}
";

            this.shippedText = $@"
C
C.Field -> int
C.Property.get -> int
C.Property.set -> void
{DeclarePublicAPIAnalyzer.RemovedApiPrefix}C.Method() -> void
";

            this.unshippedText = string.Empty;

            // error RS0024: The contents of the public API files are invalid: The shipped API file can't have removed members
            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.PublicApiFilesInvalid).WithArguments(DeclarePublicAPIAnalyzer.InvalidReasonShippedCantHaveRemoved);

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task DuplicateSymbolInSameAPIFileAsync()
        {
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
}
";

            this.shippedText = @"
C
C.Field -> int
C.Property.get -> int
C.Property.set -> void
C.Property.get -> int
";

            this.unshippedText = string.Empty;

            // Warning RS0025: The symbol 'C.Property.get -> int' appears more than once in the public API files.
            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.DuplicateSymbolInApiFiles)
                .WithArguments("C.Property.get -> int")
                .WithLocation(DeclarePublicAPIAnalyzer.ShippedFileName, 6, 1)
                .WithLocation(DeclarePublicAPIAnalyzer.ShippedFileName, 4, 1);

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task DuplicateSymbolInDifferentAPIFilesAsync()
        {
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
}
";

            this.shippedText = @"
C
C.Field -> int
C.Property.get -> int
C.Property.set -> void
";

            this.unshippedText = @"
C.Property.get -> int";

            // Warning RS0025: The symbol 'C.Property.get -> int' appears more than once in the public API files.
            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.DuplicateSymbolInApiFiles)
                .WithArguments("C.Property.get -> int")
                .WithLocation(DeclarePublicAPIAnalyzer.UnshippedFileName, 2, 1)
                .WithLocation(DeclarePublicAPIAnalyzer.ShippedFileName, 4, 1);

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task ApiFileShippedWithNonExistentMembersAsync()
        {
            // Type C has no public member "Method", but the shipped API has an entry for it.
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
    private void Method() { }
}
";

            this.shippedText = $@"
C
C.C() -> void
C.Field -> int
C.Property.get -> int
C.Property.set -> void
C.Method() -> void
";
            this.unshippedText = string.Empty;

            // PublicAPI.Shipped.txt(7,1): warning RS0017: Symbol 'C.Method() -> void' is part of the declared API, but is either not public or could not be found
            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.RemoveDeletedApiRule)
                .WithArguments("C.Method() -> void")
                .WithLocation(DeclarePublicAPIAnalyzer.ShippedFileName, 7, 1);

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task ApiFileShippedWithNonExistentMembersTestFullPathAsync()
        {
            // Type C has no public member "Method", but the shipped API has an entry for it.
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
    private void Method() { }
}
";

            var tempPath = Path.GetTempPath();
            this.shippedText = $@"
C
C.C() -> void
C.Field -> int
C.Property.get -> int
C.Property.set -> void
C.Method() -> void
";
            this.shippedFilePath = Path.Combine(tempPath, DeclarePublicAPIAnalyzer.ShippedFileName);

            this.unshippedText = $@"";
            this.unshippedFilePath = Path.Combine(tempPath, DeclarePublicAPIAnalyzer.UnshippedFileName);

            // <%TEMP_PATH%>\PublicAPI.Shipped.txt(7,1): warning RS0017: Symbol 'C.Method() -> void' is part of the declared API, but is either not public or could not be found
            var expected = new DiagnosticResult(DeclarePublicAPIAnalyzer.RemoveDeletedApiRule).WithArguments("C.Method() -> void").WithLocation(this.shippedFilePath, 7, 1);

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task TypeForwardsAreProcessed1Async()
        {
            var source = @"
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.StringComparison))]
";

            this.shippedText = $@"
System.StringComparison (forwarded, contained in mscorlib)
System.StringComparison.CurrentCulture = 0 -> System.StringComparison (forwarded, contained in mscorlib)
System.StringComparison.CurrentCultureIgnoreCase = 1 -> System.StringComparison (forwarded, contained in mscorlib)
System.StringComparison.InvariantCulture = 2 -> System.StringComparison (forwarded, contained in mscorlib)
System.StringComparison.InvariantCultureIgnoreCase = 3 -> System.StringComparison (forwarded, contained in mscorlib)
System.StringComparison.Ordinal = 4 -> System.StringComparison (forwarded, contained in mscorlib)
System.StringComparison.OrdinalIgnoreCase = 5 -> System.StringComparison (forwarded, contained in mscorlib)
";
            this.unshippedText = string.Empty;

            await this.VerifyAnalyzerAsync(source).ConfigureAwait(false);
        }

        [Fact]
        public async Task TypeForwardsAreProcessed2Async()
        {
            var source = @"
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.StringComparer))]
";
            this.shippedText = $@"
System.StringComparer (forwarded, contained in mscorlib)
static System.StringComparer.InvariantCulture.get -> System.StringComparer (forwarded, contained in mscorlib)
static System.StringComparer.InvariantCultureIgnoreCase.get -> System.StringComparer (forwarded, contained in mscorlib)
static System.StringComparer.CurrentCulture.get -> System.StringComparer (forwarded, contained in mscorlib)
static System.StringComparer.CurrentCultureIgnoreCase.get -> System.StringComparer (forwarded, contained in mscorlib)
static System.StringComparer.Ordinal.get -> System.StringComparer (forwarded, contained in mscorlib)
static System.StringComparer.OrdinalIgnoreCase.get -> System.StringComparer (forwarded, contained in mscorlib)
static System.StringComparer.Create(System.Globalization.CultureInfo culture, bool ignoreCase) -> System.StringComparer (forwarded, contained in mscorlib)
System.StringComparer.Compare(object x, object y) -> int (forwarded, contained in mscorlib)
System.StringComparer.Equals(object x, object y) -> bool (forwarded, contained in mscorlib)
System.StringComparer.GetHashCode(object obj) -> int (forwarded, contained in mscorlib)
abstract System.StringComparer.Compare(string x, string y) -> int (forwarded, contained in mscorlib)
abstract System.StringComparer.Equals(string x, string y) -> bool (forwarded, contained in mscorlib)
abstract System.StringComparer.GetHashCode(string obj) -> int (forwarded, contained in mscorlib)
System.StringComparer.StringComparer() -> void (forwarded, contained in mscorlib)
";
            this.unshippedText = $@"";

            await this.VerifyAnalyzerAsync(source).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAvoidMultipleOverloadsWithOptionalParametersAsync()
        {
            var source = @"
public class C
{
    // ok - single overload with optional params, 2 overloads have no public API entries.
    public void Method1(int p1, int p2, int p3 = 0) { }
    public void Method1() { }
    public void Method1(int p1, int p2) { }
    public void Method1(char p1, params int[] p2) { }

    // ok - multiple overloads with optional params, but only one is public.
    public void Method2(int p1 = 0) { }
    internal void Method2(char p1 = '0') { }
    private void Method2(string p1 = null) { }

    // ok - multiple overloads with optional params, but all are shipped.
    public void Method3(int p1 = 0) { }
    public void Method3(string p1 = null) { }

    // fire on unshipped (1) - multiple overloads with optional params, all but first are shipped.
    public void Method4(int p1 = 0) { }
    public void Method4(char p1 = 'a') { }
    public void Method4(string p1 = null) { }

    // fire on all unshipped (3) - multiple overloads with optional params, all are unshipped, 2 have unshipped entries.
    public void Method5(int p1 = 0) { }
    public void Method5(char p1 = 'a') { }
    public void Method5(string p1 = null) { }

    // ok - multiple overloads with optional params, but all have same params (differ only by generic vs non-generic).
    public object Method6(int p1 = 0) { return Method6<object>(p1); }
    public T Method6<T>(int p1 = 0) { return default(T); }
}
";

            this.shippedText = $@"
C.Method3(int p1 = 0) -> void
C.Method3(string p1 = null) -> void
C.Method4(char p1 = 'a') -> void
C.Method4(string p1 = null) -> void
";
            this.unshippedText = $@"
C
C.C() -> void
C.Method1() -> void
C.Method1(int p1, int p2) -> void
C.Method2(int p1 = 0) -> void
C.Method4(int p1 = 0) -> void
C.Method5(char p1 = 'a') -> void
C.Method5(string p1 = null) -> void
C.Method6(int p1 = 0) -> object
C.Method6<T>(int p1 = 0) -> T
";

            DiagnosticResult[] expected =
            {
                // Test0.cs(5,17): warning RS0016: Symbol 'Method1' is not part of the declared API.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("Method1").WithLocation(5, 17),

                // Test0.cs(8,17): warning RS0016: Symbol 'Method1' is not part of the declared API.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("Method1").WithLocation(8, 17),

                // Test0.cs(20,17): warning RS0026: Symbol 'Method4' violates the backcompat requirement: 'Do not add multiple overloads with optional parameters'. See 'https://github.com/DotNetAnalyzers/PublicApiAnalyzer/blob/master/documentation/RS0026.md' for details.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.AvoidMultipleOverloadsWithOptionalParameters).WithArguments("Method4", DeclarePublicAPIAnalyzer.AvoidMultipleOverloadsWithOptionalParameters.HelpLinkUri).WithLocation(20, 17),

                // Test0.cs(25,17): warning RS0016: Symbol 'Method5' is not part of the declared API.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.DeclareNewApiRule).WithArguments("Method5").WithLocation(25, 17),

                // Test0.cs(25,17): warning RS0026: Symbol 'Method5' violates the backcompat requirement: 'Do not add multiple overloads with optional parameters'. See 'https://github.com/DotNetAnalyzers/PublicApiAnalyzer/blob/master/documentation/RS0026.md' for details.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.AvoidMultipleOverloadsWithOptionalParameters).WithArguments("Method5", DeclarePublicAPIAnalyzer.AvoidMultipleOverloadsWithOptionalParameters.HelpLinkUri).WithLocation(25, 17),

                // Test0.cs(26,17): warning RS0026: Symbol 'Method5' violates the backcompat requirement: 'Do not add multiple overloads with optional parameters'. See 'https://github.com/DotNetAnalyzers/PublicApiAnalyzer/blob/master/documentation/RS0026.md' for details.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.AvoidMultipleOverloadsWithOptionalParameters).WithArguments("Method5", DeclarePublicAPIAnalyzer.AvoidMultipleOverloadsWithOptionalParameters.HelpLinkUri).WithLocation(26, 17),

                // Test0.cs(27,17): warning RS0026: Symbol 'Method5' violates the backcompat requirement: 'Do not add multiple overloads with optional parameters'. See 'https://github.com/DotNetAnalyzers/PublicApiAnalyzer/blob/master/documentation/RS0026.md' for details.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.AvoidMultipleOverloadsWithOptionalParameters).WithArguments("Method5", DeclarePublicAPIAnalyzer.AvoidMultipleOverloadsWithOptionalParameters.HelpLinkUri).WithLocation(27, 17),
            };

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestOverloadWithOptionalParametersShouldHaveMostParametersAsync()
        {
            var source = @"
public class C
{
    // ok - single overload with optional params has most parameters.
    public void Method1(int p1, int p2, int p3 = 0) { }
    public void Method1() { }
    public void Method1(int p1, int p2) { }
    public void Method1(char p1, params int[] p2) { }

    // ok - multiple overloads with optional params violating most params requirement, but only one is public.
    public void Method2(int p1 = 0) { }
    internal void Method2(int p1, char p2 = '0') { }
    private void Method2(string p1 = null) { }

    // ok - multiple overloads with optional params violating most params requirement, but all are shipped.
    public void Method3(int p1 = 0) { }
    public void Method3(string p1 = null) { }
    public void Method3(int p1, int p2) { }

    // fire on unshipped (1) - single overload with optional params and violating most params requirement.
    public void Method4(int p1 = 0) { }     // unshipped
    public void Method4(char p1, int p2) { }        // unshipped
    public void Method4(string p1, int p2) { }      // unshipped

    // fire on shipped (1) - single shipped overload with optional params and violating most params requirement due to a new unshipped API.
    public void Method5(int p1 = 0) { }     // shipped
    public void Method5(char p1) { }        // shipped
    public void Method5(string p1) { }      // unshipped

    // fire on multiple shipped (2) - multiple shipped overloads with optional params and violating most params requirement due to a new unshipped API
    public void Method6(int p1 = 0) { }     // shipped
    public void Method6(char p1 = 'a') { }  // shipped
    public void Method6(string p1) { }      // unshipped
}
";

            this.shippedText = $@"
C.Method3(int p1 = 0) -> void
C.Method3(int p1, int p2) -> void
C.Method3(string p1 = null) -> void
C.Method5(char p1) -> void
C.Method5(int p1 = 0) -> void
C.Method6(char p1 = 'a') -> void
C.Method6(int p1 = 0) -> void
";
            this.unshippedText = $@"
C
C.C() -> void
C.Method1() -> void
C.Method1(char p1, params int[] p2) -> void
C.Method1(int p1, int p2) -> void
C.Method1(int p1, int p2, int p3 = 0) -> void
C.Method2(int p1 = 0) -> void
C.Method4(char p1, int p2) -> void
C.Method4(int p1 = 0) -> void
C.Method4(string p1, int p2) -> void
C.Method5(string p1) -> void
C.Method6(string p1) -> void
";

            DiagnosticResult[] expected =
            {
                // Test0.cs(21,17): warning RS0027: Symbol 'Method4' violates the backcompat requirement: 'Public API with optional parameter(s) should have the most parameters amongst its public overloads'. See 'https://github.com/DotNetAnalyzers/PublicApiAnalyzer/blob/master/documentation/RS0026.md' for details.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.OverloadWithOptionalParametersShouldHaveMostParameters).WithArguments("Method4", DeclarePublicAPIAnalyzer.OverloadWithOptionalParametersShouldHaveMostParameters.HelpLinkUri).WithLocation(21, 17),

                // Test0.cs(26,17): warning RS0027: Symbol 'Method5' violates the backcompat requirement: 'Public API with optional parameter(s) should have the most parameters amongst its public overloads'. See 'https://github.com/DotNetAnalyzers/PublicApiAnalyzer/blob/master/documentation/RS0026.md' for details.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.OverloadWithOptionalParametersShouldHaveMostParameters).WithArguments("Method5", DeclarePublicAPIAnalyzer.OverloadWithOptionalParametersShouldHaveMostParameters.HelpLinkUri).WithLocation(26, 17),

                // Test0.cs(31,17): warning RS0027: Symbol 'Method6' violates the backcompat requirement: 'Public API with optional parameter(s) should have the most parameters amongst its public overloads'. See 'https://github.com/DotNetAnalyzers/PublicApiAnalyzer/blob/master/documentation/RS0026.md' for details.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.OverloadWithOptionalParametersShouldHaveMostParameters).WithArguments("Method6", DeclarePublicAPIAnalyzer.OverloadWithOptionalParametersShouldHaveMostParameters.HelpLinkUri).WithLocation(31, 17),

                // Test0.cs(32,17): warning RS0027: Symbol 'Method6' violates the backcompat requirement: 'Public API with optional parameter(s) should have the most parameters amongst its public overloads'. See 'https://github.com/DotNetAnalyzers/PublicApiAnalyzer/blob/master/documentation/RS0026.md' for details.
                new DiagnosticResult(DeclarePublicAPIAnalyzer.OverloadWithOptionalParametersShouldHaveMostParameters).WithArguments("Method6", DeclarePublicAPIAnalyzer.OverloadWithOptionalParametersShouldHaveMostParameters.HelpLinkUri).WithLocation(32, 17),
            };

            await this.VerifyAnalyzerAsync(source, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestSimpleMissingMemberFixAsync()
        {
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
    public void Method() { } 
    public int ArrowExpressionProperty => 0;
    public int {|RS0016:NewField|}; // Newly added field, not in current public API.
}
";

            this.shippedText = string.Empty;
            this.unshippedText = @"C
C.ArrowExpressionProperty.get -> int
C.C() -> void
C.Field -> int
C.Method() -> void
C.Property.get -> int
C.Property.set -> void";
            var fixedUnshippedText = @"C
C.ArrowExpressionProperty.get -> int
C.C() -> void
C.Field -> int
C.Method() -> void
C.NewField -> int
C.Property.get -> int
C.Property.set -> void";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAddAndRemoveMembersFixAsync()
        {
            // Unshipped file has a state 'ObsoleteField' entry and a missing 'NewField' entry.
            var source = @"
public class C
{
    public int Field;
    public int Property { get; set; }
    public void Method() { } 
    public int ArrowExpressionProperty => 0;
    public int {|RS0016:NewField|};
}
";
            this.shippedText = string.Empty;
            this.unshippedText = @"C
C.ArrowExpressionProperty.get -> int
C.C() -> void
C.Field -> int
C.Method() -> void
{|RS0017:C.ObsoleteField -> int|}
C.Property.get -> int
C.Property.set -> void";
            var fixedUnshippedText = @"C
C.ArrowExpressionProperty.get -> int
C.C() -> void
C.Field -> int
C.Method() -> void
C.NewField -> int
C.Property.get -> int
C.Property.set -> void";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestSimpleMissingTypeFixAsync()
        {
            var source = @"
public class {|RS0016:C|}
{
    private C() { }
}
";

            this.shippedText = string.Empty;
            this.unshippedText = string.Empty;
            var fixedUnshippedText = @"C
";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestMultipleMissingTypeAndMemberFixAsync()
        {
            var source = @"
public class {|RS0016:C|}
{
    private C() { }
    public int {|RS0016:Field|};
}
public class {|RS0016:{|RS0016:C2|}|} { }
";

            this.shippedText = string.Empty;
            this.unshippedText = string.Empty;
            var fixedUnshippedText = @"C
C.Field -> int
C2
C2.C2() -> void
";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestChangingMethodSignatureForAnUnshippedMethodFixAsync()
        {
            var source = @"
public class C
{
    private C() { }
    public void {|RS0016:Method|}(int p1){ }
}
";

            this.shippedText = @"C";

            // previously method had no params, so the fix should remove the previous overload.
            this.unshippedText = @"{|RS0017:C.Method() -> void|}";
            var fixedUnshippedText = @"C.Method(int p1) -> void";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestChangingMethodSignatureForAnUnshippedMethodWithShippedOverloadsFixAsync()
        {
            var source = @"
public class C
{
    private C() { }
    public void Method(int p1){ }
    public void Method(int p1, int p2){ }
    public void {|RS0016:Method|}(char p1){ }
}
";

            this.shippedText = @"C
C.Method(int p1) -> void
C.Method(int p1, int p2) -> void";

            // previously method had no params, so the fix should remove the previous overload.
            this.unshippedText = @"{|RS0017:C.Method() -> void|}";
            var fixedUnshippedText = @"C.Method(char p1) -> void";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAddingNewPublicOverloadFixAsync()
        {
            var source = @"
public class C
{
    private C() { }
    public void {|RS0016:Method|}(){ }
    internal void Method(int p1){ }
    internal void Method(int p1, int p2){ }
    public void Method(char p1){ }
}
";

            this.shippedText = string.Empty;
            this.unshippedText = @"C
C.Method(char p1) -> void";
            var fixedUnshippedText = @"C
C.Method() -> void
C.Method(char p1) -> void";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestMissingTypeAndMemberAndNestedMembersFixAsync()
        {
            var source = @"
public class {|RS0016:C|}
{
    private C() { }
    public int {|RS0016:Field|};
    public class CC
    {
        public int {|RS0016:Field|};
    }
}
public class {|RS0016:{|RS0016:C2|}|} { }
";

            this.shippedText = @"C.CC
C.CC.CC() -> void";
            this.unshippedText = string.Empty;
            var fixedUnshippedText = @"C
C.CC.Field -> int
C.Field -> int
C2
C2.C2() -> void
";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestMissingNestedGenericMembersAndStaleMembersFixAsync()
        {
            var source = @"
public class {|RS0016:C|}
{
    private C() { }
    public CC<int> {|RS0016:Field|};
    private C3.C4 Field2;
    private C3.C4 Method(C3.C4 p1) { throw new System.NotImplementedException(); }
    public class CC<T>
    {
        public int {|RS0016:Field|};
        public CC<int> {|RS0016:Field2|};
    }

    public class C3
    {
        public class C4 { }
    }
}
public class {|RS0016:{|RS0016:C2|}|} { }
";

            this.shippedText = string.Empty;
            this.unshippedText = @"C.C3
C.C3.C3() -> void
C.C3.C4
C.C3.C4.C4() -> void
C.CC<T>
C.CC<T>.CC() -> void
{|RS0017:C.Field2 -> C.C3.C4|}
{|RS0017:C.Method(C.C3.C4 p1) -> C.C3.C4|}
";
            var fixedUnshippedText = @"C
C.C3
C.C3.C3() -> void
C.C3.C4
C.C3.C4.C4() -> void
C.CC<T>
C.CC<T>.CC() -> void
C.CC<T>.Field -> int
C.CC<T>.Field2 -> C.CC<int>
C.Field -> C.CC<int>
C2
C2.C2() -> void
";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestWithExistingUnshippedNestedMembersFixAsync()
        {
            var source = @"
public class {|RS0016:C|}
{
    private C() { }
    public int {|RS0016:Field|};
    public class CC
    {
        public int Field;
    }
}
public class {|RS0016:{|RS0016:C2|}|} { }
";

            this.shippedText = string.Empty;
            this.unshippedText = @"C.CC
C.CC.CC() -> void
C.CC.Field -> int";
            var fixedUnshippedText = @"C
C.CC
C.CC.CC() -> void
C.CC.Field -> int
C.Field -> int
C2
C2.C2() -> void";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestWithExistingUnshippedNestedGenericMembersFixAsync()
        {
            var source = @"
public class C
{
    private C() { }
    public class {|RS0016:CC|}
    {
        public int Field;
    }
    public class CC<T>
    {
        private CC() { }
        public int Field;
    }
}
";

            this.shippedText = string.Empty;
            this.unshippedText = @"C
C.CC
C.CC.Field -> int
C.CC<T>
C.CC<T>.Field -> int";
            var fixedUnshippedText = @"C
C.CC
C.CC.CC() -> void
C.CC.Field -> int
C.CC<T>
C.CC<T>.Field -> int";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestWithExistingShippedNestedMembersFixAsync()
        {
            var source = @"
public class {|RS0016:C|}
{
    private C() { }
    public int {|RS0016:Field|};
    public class CC
    {
        public int Field;
    }
}
public class {|RS0016:{|RS0016:C2|}|} { }
";

            this.shippedText = @"C.CC
C.CC.CC() -> void
C.CC.Field -> int";
            this.unshippedText = string.Empty;
            var fixedUnshippedText = @"C
C.Field -> int
C2
C2.C2() -> void
";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestOnlyRemoveStaleSiblingEntriesFixAsync()
        {
            var source = @"
public class {|RS0016:C|}
{
    private C() { }
    public int {|RS0016:Field|};
    public class CC
    {
        private int Field; // This has a stale public API entry, but this shouldn't be removed unless we attempt to add a public API entry for a sibling.
    }
}
public class {|RS0016:{|RS0016:C2|}|} { }
";

            this.shippedText = string.Empty;
            this.unshippedText = @"
C.CC
C.CC.CC() -> void
{|RS0017:C.CC.Field -> int|}";
            var fixedUnshippedText = @"C
C.CC
C.CC.CC() -> void
{|RS0017:C.CC.Field -> int|}
C.Field -> int
C2
C2.C2() -> void";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAddTrailingNewlineByDefaultAsync()
        {
            var source = @"
public class {|RS0016:{|RS0016:C|}|}
{
}
";

            this.shippedText = string.Empty;
            this.unshippedText = string.Empty;
            var fixedUnshippedText = @"C
C.C() -> void
";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("\r\n", "\r\n")]
        [InlineData("\r\n\r\n", "\r\n")]
        public async Task TestPreserveTrailingNewlineAsync(string originalEndOfFile, string expectedEndOfFile)
        {
            var source = @"
public class C
{
    public int Property { get; }
    public int {|RS0016:NewField|}; // Newly added field, not in current public API.
}
";

            this.shippedText = string.Empty;
            this.unshippedText = $@"C
C.C() -> void
C.Property.get -> int{originalEndOfFile}";
            var fixedUnshippedText = $@"C
C.C() -> void
C.NewField -> int
C.Property.get -> int{expectedEndOfFile}";

            await this.VerifyCSharpUnshippedFileFixAsync(source, fixedUnshippedText).ConfigureAwait(false);
        }

        private async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new CSharpCodeFixTest<DeclarePublicAPIAnalyzer, DeclarePublicAPIFix, XUnitVerifier>
            {
                TestCode = source,
            };

            if (this.unshippedText != null)
            {
                test.TestState.AdditionalFiles.Add((this.unshippedFilePath, this.unshippedText));
            }

            if (this.shippedText != null)
            {
                test.TestState.AdditionalFiles.Add((this.shippedFilePath, this.shippedText));
            }

            test.Exclusions &= ~AnalysisExclusions.GeneratedCode;
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync().ConfigureAwait(false);
        }

        private async Task VerifyCSharpUnshippedFileFixAsync(string source, string fixedUnshippedText, params DiagnosticResult[] expected)
        {
            var test = new CSharpCodeFixTest<DeclarePublicAPIAnalyzer, DeclarePublicAPIFix, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { source },
                    AdditionalFiles =
                    {
                        (this.shippedFilePath, this.shippedText ?? string.Empty),
                        (this.unshippedFilePath, this.unshippedText ?? string.Empty),
                    },
                },
                FixedState =
                {
                    Sources = { source },
                    AdditionalFiles =
                    {
                        (this.shippedFilePath, this.shippedText ?? string.Empty),
                        (this.unshippedFilePath, fixedUnshippedText),
                    },
                    InheritanceMode = StateInheritanceMode.Explicit,
                },
            };

            test.Exclusions &= ~AnalysisExclusions.GeneratedCode;
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync().ConfigureAwait(false);
        }
    }
}
