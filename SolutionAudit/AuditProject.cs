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
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using CWDev.SLNTools.Core;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using NuGet;
using MoreLinq;
using Roslyn.Services;
using SolutionAudit.AuditInformation;
using SolutionAudit.Extensions;
using Project = Microsoft.Build.Evaluation.Project;
using SlnToolsProject = CWDev.SLNTools.Core.Project;

namespace SolutionAudit
{
    public class AuditProject : IVisualStudioCommand
    {
        public AuditProject(IProject roslynProject, FrameworkName targetFramework, Options options)
        {
            // Shared
            RoslynProject = roslynProject;
            MsBuildProject = RoslynProject.GetMsBuildProject();
            LocalRepository = RoslynProject.Solution.GetLocalPackageRepository();
            ConfigReferences = PackageReferenceFile.CreateFromProject(RoslynProject.FilePath).GetPackageReferences(false).ToList();
            CsprojReferences = MsBuildProject.GetDirectPackageAssemblies().Select(g => g.Key).Where(r => r != null).Distinct().ToList();
            References = ConfigReferences.Union(CsprojReferences).ToList();
            Packages = options.Unused || options.Snapshot || options.Missing || options.Project
                ? References.Select(r => r.GetPackage(LocalRepository)).Where(p => p != null).ToList()
                : Enumerable.Empty<IPackage>();
            // Unused Packages
            UnusedPackages = options.Unused
                ? GetUnusedPackages(targetFramework, options.UnusedUsings)
                : Enumerable.Empty<UnusedPackage>();

            // Snapshot References
            SnapshotPackages = options.Snapshot
                ? Packages.GetSnapshotPackages()
                : Enumerable.Empty<SnapshotPackage>();

            // Missing Packages
            MissingPackages = options.Missing
                ? GetMissingPackages(targetFramework)
                : Enumerable.Empty<MissingPackageDependency>();

            // Project Packages
            ProjectPackages = options.Project
                ? GetProjectPackages()
                : Enumerable.Empty<ProjectPackage>();

            // File Diff References
            MissingPackageReferences = options.FileDiff
                ? GetMissingPackageReferences()
                : Enumerable.Empty<InconsistentFiles>();

            // Duplicate Package References
            DuplicatePackageReferences = options.FileDiff
                ? GetDuplicatePackageReferences()
                : Enumerable.Empty<DuplicatePackageReference>();

            // Duplicate Package References
            BindingRedirectMismatchReferences = options.FileDiff
                ? GetBindingRedirectMismatchReferences()
                : Enumerable.Empty<OrphanOrMismatchAssemblyBinding>();

            BadProjectRefGuids = options.FileDiff
                ? GetBadProjectRefGuids()
                : Enumerable.Empty<MismatchedGuid>();

            IllegalNugetTargets = options.NugetTargets
                ? GetIllegalNugetTargets()
                : Enumerable.Empty<IllegalProjectFileElement>();
        }

        public bool PassAudit()
        {
            return MissingPackages.IsEmpty()
                   && ProjectPackages.IsEmpty()
                   && InconsistentPackages.IsEmpty()
                   && UnusedPackages.IsEmpty()
                   && SnapshotPackages.IsEmpty()
                   && MissingPackageReferences.IsEmpty()
                   && DuplicatePackageReferences.IsEmpty()
                   && BindingRedirectMismatchReferences.IsEmpty()
                   && BadProjectRefGuids.IsEmpty()
                   && IllegalNugetTargets.IsEmpty();
        }

        private IEnumerable<IGrouping<PackageReference, KeyValuePair<string, string>>> GetAllAssemblyReferences()
        {
            return RoslynProject.GetProjectReferences().SelectMany(r => r.GetMsBuildProject().GetDirectPackageAssemblies());
        }

        private SlnToolsProject GetSolutionProjectFromProjectReference(ProjectItem projectReference,
            IEnumerable<SlnToolsProject> solutionProjects)
        {
            try
            {
                return
                    solutionProjects.First(projectReference.EqualByPath);
            }
            catch (InvalidOperationException)
            {
                Console.Error.WriteLine("The project reference to {0} in {1} could not be found in the solution", projectReference.GetProjectName(), RoslynProject.Name);
                return null;
            }
        }

        public IEnumerable<MismatchedGuid> BadProjectRefGuids { get; set; }
        private IEnumerable<MismatchedGuid> GetBadProjectRefGuids()
        {
            var solutionProjects = SolutionFile.FromFile(RoslynProject.Solution.FilePath).Projects;
            return MsBuildProject.GetProjectReferences()
                .Where(r => !r.EqualByGuid(GetSolutionProjectFromProjectReference(r, solutionProjects)))
                .Select(r => new MismatchedGuid(r.GetProjectGuid(), r.GetProjectName(), "Project Reference"));
        }

        public IEnumerable<ProjectPackage> ProjectPackages { get; set; }
        private IEnumerable<ProjectPackage> GetProjectPackages()
        {
            var allProjects = RoslynProject.Solution.Projects.Select(p => p.Name);
            return Packages.Where(p => allProjects.Contains(p.Id)).Select(p => new ProjectPackage(p));
        }

        private IEnumerable<InconsistentVersionPackageReference> InconsistentPackages { get; set; }
        public static void FillInconsistentPackageReferences(IEnumerable<AuditProject> projects, bool allPackages)
        {
            projects
                .SelectMany(p => p.References.Where(r => allPackages ? true : r.GetPackage(p.LocalRepository).IsStronglyNamed()), (project, reference) => new { Project = project, Reference = reference })
                .GroupBy(p => p.Reference.Id)
                .Where(p => !p.Select(x => x.Reference).AllEqual())
                .SelectMany(p => p)
                .GroupBy(p => p.Project)
                .ForEach(g => g.Key.InconsistentPackages = g.Select(p => new InconsistentVersionPackageReference(p.Reference)));
        }

        private IProject RoslynProject { get; set; }
        private Project MsBuildProject { get; set; }
        private IPackageRepository LocalRepository { get; set; }
        private IEnumerable<PackageReference> ConfigReferences { get; set; }
        private IEnumerable<PackageReference> CsprojReferences { get; set; }
        private IEnumerable<PackageReference> References { get; set; }
        private IEnumerable<IPackage> Packages { get; set; }

        public IEnumerable<UnusedPackage> UnusedPackages { get; set; }
        private IEnumerable<UnusedPackage> GetUnusedPackages(FrameworkName targetFramework, bool lookAtUnusedUsings)
        {
            var usedAssemblyPaths = RoslynProject.Documents
                .SelectMany(d => d.GetUsedAssemblies(lookAtUnusedUsings))
                .Select(a => a.Identity.Location)
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct().ToList();

            return Packages
                // Packages not referenced by other packages ...
                .GetDirectPackages(targetFramework)
                // ... that have assemblies ...
                .Where(p => p.AssemblyReferences.Any())
                // ... and the namespaces in those assemblies do not exist in the project ...
                .Where(p => p.GetTypes().Select(t => t.FullName).Intersect(MsBuildProject.GetNamespaces()).IsEmpty())
                // ... and the assemblies are not used by symbols in the project
                .Where(p => !p.GetAssemblyPaths().Any(a => usedAssemblyPaths.Any(u => u.Contains(a))))
                .Where(p => !p.Id.Equals("Aspose.Words")) // Roslyn seems to have a problem with this package
                .Select(p => new UnusedPackage(p));
        }

        private IEnumerable<SnapshotPackage> SnapshotPackages { get; set; }

        public IEnumerable<InconsistentFiles> MissingPackageReferences { get; set; }
        private IEnumerable<InconsistentFiles> GetMissingPackageReferences()
        {
            var configOnlyReferences = ConfigReferences.Except(CsprojReferences)
                // References in packages.config may not have assemblies, these don't show up in .csproj
                .Where(r => r.GetPackage(LocalRepository).AssemblyReferences.Any())
                .Where(r => !r.Id.Equals("Microsoft.Net.Http")) // This package is weird, it uses the GAC assembly instead of the one it ships with.
                .Select(r => new InconsistentFiles(r, "packages.config"));

            var csprojOnlyReferences = CsprojReferences.Except(ConfigReferences)
                .Select(r => new InconsistentFiles(r, ".csproj" ));

            return configOnlyReferences.Union(csprojOnlyReferences);
        }

        public IEnumerable<MissingPackageDependency> MissingPackages { get; set; }
        private IEnumerable<MissingPackageDependency> GetMissingPackages(FrameworkName targetFramework)
        {
            // All dependencies of all packages ...
            return Packages.SelectMany(p => p.GetCompatiblePackageDependencies(targetFramework),
                (package, dependency) => new { package, dependency })
                // ... that aren't installed as a package ...
                .Where(d => !Packages.Select(p => p.Id).Contains(d.dependency.Id))
                // ... or as a project
                .Where(d => !MsBuildProject.GetProjectReferenceNames().Contains(d.dependency.Id))
                // Dependencies don't have good equality checking
                .GroupBy(d => d.dependency.ToString())
                .Select(g => new MissingPackageDependency(g.First().dependency, g.Select(d => d.package)));
        }

        public IEnumerable<DuplicatePackageReference> DuplicatePackageReferences { get; set; }
        private IEnumerable<DuplicatePackageReference> GetDuplicatePackageReferences()
        {
            var configDuplicates = ConfigReferences
                .Duplicates()
                .Select(r => new DuplicatePackageReference(r, "packages.config"));

            var csprojDuplicates = MsBuildProject.GetDirectPackageAssemblies()
                .SelectMany(p => p, (grouping, s) => new {grouping, s})
                .DuplicatesBy(a => a.s)
                .Select(r => new DuplicatePackageReference(r.grouping.Key, ".csproj", r.s.Key));

            return configDuplicates.Concat(csprojDuplicates);
        }

        public IEnumerable<OrphanOrMismatchAssemblyBinding> BindingRedirectMismatchReferences { get; set; }
        private IEnumerable<OrphanOrMismatchAssemblyBinding> GetBindingRedirectMismatchReferences()
        {
            var allAssemblyIds = GetAllAssemblyReferences().SelectMany(p => p).ToDictionary(k => k.Key, v => v.Value);
            return MsBuildProject.GetBindingRedirects()
                .Where(br => !(allAssemblyIds.ContainsKey(br.Name) && allAssemblyIds[br.Name].Equals(br.NewVersion)))
                .Select(br => new OrphanOrMismatchAssemblyBinding(br));
        }

        public IEnumerable<IllegalProjectFileElement> IllegalNugetTargets { get; set; }
        private IEnumerable<IllegalProjectFileElement> GetIllegalNugetTargets()
        {
            var nugetTargets = MsBuildProject.Targets
                .Where(t => t.Key == "EnsureNuGetPackageBuildImports")
                .SelectMany(
                    t => t.Value.Children.Where(c => c.Condition.ToLower().Contains("nuget.targets")),
                    (t, c) =>  IllegalProjectFileElement.FromElement(c));

            var nugetTargetImports = MsBuildProject.Imports
                .Where(i => i.ImportedProject.FullPath.ToLower().EndsWith("nuget.targets"))
                .Select(i => IllegalProjectFileElement.FromImport(i.ImportingElement));

            return nugetTargetImports.Union(nugetTargets);
        }

        public string VsCommand()
        {
            var sb = new StringBuilder();

            UnusedPackages.ForEach(p => sb.AppendFormat("{0} -Project {1}\n", p.VsCommand(), RoslynProject.Name));
            MissingPackages.ForEach(p => sb.AppendFormat("{0} -Project {1}\n", p.VsCommand(), RoslynProject.Name));

            if (InconsistentPackages != null)
            {
                InconsistentPackages.ForEach(p => sb.AppendLine(p.VsCommand()));
            }

            // These do not have Visual Studio Commands yet
            //SnapshotPackages.ForEach(p => sb.AppendFormat("{0} -Project {1}\n", p.VsCommand(), RoslynProject.Name));
            //CsprojOnlyReferences.ForEach(p => sb.AppendFormat("{0} -Project {1}\n", p.VsCommand(), RoslynProject.Name));
            //ConfigOnlyReferences.ForEach(p => sb.AppendFormat("{0} -Project {1}\n", p.VsCommand(), RoslynProject.Name));

            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(RoslynProject.Name);

            UnusedPackages.ForEach(p => sb.AppendFormat("\t{0}\n", p));
            ProjectPackages.ForEach(p => sb.AppendFormat("\t{0}\n", p));
            MissingPackages.ForEach(p => sb.AppendFormat("\t{0}\n", p));
            MissingPackageReferences.ForEach(p => sb.AppendFormat("\t{0}\n", p));
            BindingRedirectMismatchReferences.ForEach(p => sb.AppendFormat("\t{0}\n", p));
            SnapshotPackages.ForEach(p => sb.AppendFormat("\t{0}\n", p));
            DuplicatePackageReferences.ForEach(p => sb.AppendFormat("\t{0}\n", p));
            BadProjectRefGuids.ForEach(p => sb.AppendFormat("\t{0}\n", p));
            IllegalNugetTargets.ForEach(p => sb.AppendFormat("\t{0}\n", p));

            if (InconsistentPackages != null)
            {
                InconsistentPackages.ForEach(p => sb.AppendFormat("\t{0}\n", p));
            }

            return sb.ToString();
        }
    }
}
