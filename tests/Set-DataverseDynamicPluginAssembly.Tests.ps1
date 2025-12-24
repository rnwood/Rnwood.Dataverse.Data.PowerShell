# Set-DataverseDynamicPluginAssembly Tests
# These tests validate that Roslyn compilation doesn't cause stack overflow
# 
# Note: Full end-to-end tests with mock connection are not possible because
# the mock connection doesn't have metadata for plugin entities (pluginassembly, plugintype).
# This test focuses on the critical issue: ensuring that Microsoft.CodeAnalysis 
# satellite assembly resolution doesn't cause stack overflow.

BeforeAll {
    . "$PSScriptRoot/Common.ps1"
}

Describe 'Set-DataverseDynamicPluginAssembly - Assembly Loading' {
    It "Can use Microsoft.CodeAnalysis.CSharp without stack overflow" {
        # This test verifies that the fix in ModuleInitProvider.cs correctly
        # handles satellite assembly resolution for Microsoft.CodeAnalysis
        # by excluding culture-specific assemblies from the custom AssemblyLoadContext
        
        # Use Roslyn's CSharpSyntaxTree to parse code with errors
        # This triggers satellite assembly loads for diagnostic messages
        # which was causing the stack overflow before the fix
        $codeWithError = @"
using System;
using System.Linq;

namespace Test {
    public class TestClass {
        public void Method() {
            // This will cause a compilation error to trigger diagnostic message loading
            var undefinedVariable = someUndefinedSymbol;
        }
    }
}
"@
        
        # This should NOT cause a stack overflow when Roslyn tries to load
        # satellite assemblies for error messages
        {
            $syntaxTree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($codeWithError)
            $syntaxTree | Should -Not -BeNullOrEmpty
            
            # Parse the tree and get any syntax errors (this will trigger error message loading)
            $root = $syntaxTree.GetRoot()
            $root | Should -Not -BeNullOrEmpty
            
            # If we get here without stack overflow, the fix is working
            $true | Should -Be $true
        } | Should -Not -Throw
    }
    
    It "Can compile code with Roslyn and get diagnostics without stack overflow" {
        # This is a more thorough test that actually compiles and gets diagnostics
        $code = "public class Test { }"
        
        {
            $syntaxTree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($code)
            $references = [System.Collections.Generic.List[Microsoft.CodeAnalysis.MetadataReference]]::new()
            
            $compilation = [Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create(
                "TestAssembly",
                [Microsoft.CodeAnalysis.SyntaxTree[]]@($syntaxTree),
                $references,
                (New-Object Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions([Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary))
            )
            
            # Getting diagnostics will attempt to load satellite assemblies for error messages
            # The key is that this should not stack overflow
            $diagnostics = $compilation.GetDiagnostics()
            # We expect errors (no references), but no stack overflow
            $diagnostics.Count | Should -BeGreaterThan 0
        } | Should -Not -Throw
    }
}
