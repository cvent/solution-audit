// Copyright 2017 Cvent, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using MoreLinq;
using NuGet;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace SolutionAudit.Extensions
{
    static class Roslyn
    {
        public static IEnumerable<IProject> GetProjectReferences(this IProject project)
        {
            return Utils.Closure(project, GetDirectProjectReferences);
        }

        private static IEnumerable<IProject> GetDirectProjectReferences(this IProject project)
        {
            return project.Solution.Projects.Where(p => project.ProjectReferences.Contains(p.Id));
        }

        private static IEnumerable<ISymbol> GetHiddenSymbols(this ISymbol symbol)
        {
            return Utils.Closure(symbol,
                t => ((t != null) ? t.GetOverloadSymbols() : Enumerable.Empty<ISymbol>())
                    .Concat((t is INamedTypeSymbol) ? (t as INamedTypeSymbol).GetGenericTypeSymbols() : Enumerable.Empty<ITypeSymbol>())
                    .Concat((t is ITypeSymbol) ? (t as ITypeSymbol).GetBaseTypeSymbols() : Enumerable.Empty<INamedTypeSymbol>())
                    .Concat((t is IMethodSymbol) ? (t as IMethodSymbol).GetTypes() : Enumerable.Empty<ISymbol>()));
        }

        private static IEnumerable<ITypeSymbol> GetGenericTypeSymbols(this INamedTypeSymbol named)
        {
            return named.TypeArguments.AsEnumerable();
        }

        private static IEnumerable<ITypeSymbol> GetBaseTypeSymbols(this ITypeSymbol typeSymbol)
        {
            return typeSymbol.AllInterfaces.AsEnumerable()
                .Concat(typeSymbol.BaseType == null
                ? Enumerable.Empty<INamedTypeSymbol>()
                : new List<INamedTypeSymbol> {typeSymbol.BaseType});
        }

        private static IEnumerable<ISymbol> GetOverloadSymbols(this ISymbol symbol)
        {
            return symbol.ContainingType == null
                ? Enumerable.Empty<ISymbol>()
                : symbol.ContainingType.GetMembers(symbol.Name).AsEnumerable();
        }

        private static IEnumerable<ITypeSymbol> GetTypes(this IMethodSymbol method)
        {
            return method.Parameters.AsEnumerable().Select(p => p.Type)
                .Concat(new List<ITypeSymbol> {method.ReturnType});
        }

        public static Project GetMsBuildProject(this IProject project)
        {
            var projectPath = Path.GetFullPath(project.FilePath);

            return ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectPath).IsEmpty()
                ? new Project(projectPath)
                : ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectPath).First();
        }

        public static IPackageRepository GetLocalPackageRepository(this ISolution solution)
        {
            var solutionDir = Path.GetDirectoryName(solution.FilePath);
            if (solutionDir == null) throw new FileNotFoundException("Unable to find solution");

            var localSource = Path.GetFullPath(Path.Combine(solutionDir, "packages"));
            return PackageRepositoryFactory.Default.CreateRepository(localSource);
        }

        public static IEnumerable<IAssemblySymbol> GetUsedAssemblies(this IDocument document, bool lookAtUnusedUsings)
        {
            var semanticModel = document.GetSemanticModel();
            if (semanticModel == null) return Enumerable.Empty<IAssemblySymbol>();

            var unusedUsings = lookAtUnusedUsings
                ? ((SemanticModel) semanticModel).GetUnusedImportDirectives()
                    .SelectMany(id => id.DescendantNodes()).ToList()
                : Enumerable.Empty<SyntaxNode>().ToList();

            return semanticModel.SyntaxTree.GetRoot().DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Except(unusedUsings)
                .Select(identifier => semanticModel.GetSymbolInfo(identifier))
                .Where(symbolInfo => symbolInfo.Symbol != null)
                .Select(symbolInfo => symbolInfo.Symbol)
                .SelectMany(t => t.GetHiddenSymbols())
                .Where(s => s.ContainingAssembly != null)
                .Select(s => s.ContainingAssembly);
        }
    }
}
