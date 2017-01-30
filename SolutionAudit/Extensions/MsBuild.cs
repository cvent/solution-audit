using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CWDev.SLNTools.Core;
using Microsoft.Build.Evaluation;
using NuGet;
using NuGet.Resources;
using NuGet.Runtime;
using Project = Microsoft.Build.Evaluation.Project;
using SlnToolsProject = CWDev.SLNTools.Core.Project;

namespace SolutionAudit.Extensions
{
    static class MsBuild
    {
        public static ILookup<PackageReference, string> GetDirectPackageAssemblies(this Project project)
        {
            return project.GetItems("Reference").ToLookup(r => r.GetPackageReference(), r => r.GetAssemblyName());
        }

        private static string GetAssemblyName(this ProjectItem projectItem)
        {
            return projectItem.EvaluatedInclude.Split(',')[0];
        }

        private static PackageReference GetPackageReference(this ProjectItem projectItem)
        {
            if (!projectItem.HasMetadata("HintPath")) return null;

            var pathSplit = projectItem
                    .GetMetadata("HintPath").EvaluatedValue
                    .Split(Path.DirectorySeparatorChar);

            if (!pathSplit.Contains("packages")) return null;
            var packageComponents = pathSplit
                .SkipWhile(dir => !dir.Equals("packages"))
                .ElementAt(1)
                .Split('.');

            var packageId = String.Join(".", packageComponents.TakeWhile(s => !s.All(Char.IsDigit)));
            var versionString = String.Join(".", packageComponents.SkipWhile(s => !s.All(Char.IsDigit)));
            SemanticVersion version = null;

            if (String.IsNullOrEmpty(packageId)) return null;

            if (!String.IsNullOrEmpty(versionString) && !SemanticVersion.TryParse(versionString, out version))
            {
                throw new InvalidDataException(String.Format(CultureInfo.CurrentCulture,
                    NuGetResources.ReferenceFile_InvalidVersion, versionString, projectItem.Project.FullPath));
            }

            FrameworkName targetFramework = null;
            if (pathSplit.Contains("lib"))
            {
                var targetFrameworkString = pathSplit.SkipWhile(dir => !dir.Equals(Constants.LibDirectory)).ElementAt(1);
                targetFramework = VersionUtility.ParseFrameworkName(targetFrameworkString);
                if (targetFramework == VersionUtility.UnsupportedFrameworkName)
                {
                    targetFramework = null;
                }
            }

            return new PackageReference(packageId, version, null, targetFramework, false);
        }

        public static IEnumerable<string> GetNamespaces(this Project project)
        {
            const string namespacePattern = @"@?[a-z_A-Z]\w*(?:\.@?[a-z_A-Z]\w*)+";
            var namespaceRegex = new Regex(namespacePattern);

            return project
                .GetItems("Content")
                .Select(content => Path.Combine(project.DirectoryPath, content.EvaluatedInclude))
                .Where(File.Exists)
                .SelectMany(File.ReadLines)
                .SelectMany(line => namespaceRegex.Matches(line).Cast<Match>())
                .Select(match => match.Value);
        }

        public static IEnumerable<string> GetProjectReferenceNames(this Project project)
        {
            return project.GetProjectReferences().Select(i => i.GetProjectName());
        }

        public static IEnumerable<ProjectItem> GetProjectReferences(this Project project)
        {
            return project.GetItems("ProjectReference");
        }

        public static string GetProjectName(this ProjectItem projectReference)
        {
            return projectReference.GetMetadataValue("Name");
        }

        public static Guid GetProjectGuid(this ProjectItem projectReference)
        {
            return new Guid(projectReference.GetMetadataValue("Project"));
        }

        private static string GetProjectFullPath(this ProjectItem projectReference)
        {
            return new Uri(Path.Combine(projectReference.Project.DirectoryPath, projectReference.EvaluatedInclude)).LocalPath;
        }

        public static bool EqualByGuid(this ProjectItem projectReference, SlnToolsProject solutionProject)
        {
            if (projectReference == null || solutionProject == null) return false;
            return projectReference.GetProjectGuid().Equals(new Guid(solutionProject.ProjectGuid));
        }

        public static bool EqualByPath(this ProjectItem projectReference, SlnToolsProject solutionProject)
        {
            if (projectReference == null || solutionProject == null) return false;
            return string.Equals(projectReference.GetProjectFullPath(), (new Uri(solutionProject.FullPath).LocalPath), StringComparison.OrdinalIgnoreCase);
        }

        public static IEnumerable<AssemblyBinding> GetBindingRedirects(this Project project)
        {
            XNamespace bindingRedirectNs = "urn:schemas-microsoft-com:asm.v1";

            XDocument configFile;

            try
            {
                var configPath = project.Items.First(i => i.EvaluatedInclude.ToLower().Contains("app.config")
                                                          || i.EvaluatedInclude.ToLower().Contains("web.config"))
                    .EvaluatedInclude;
                configFile = XDocument.Load(Path.Combine(project.DirectoryPath, configPath));
            }
            catch (InvalidOperationException)
            {
                return Enumerable.Empty<AssemblyBinding>();
            }

            if (configFile.Root == null
                || configFile.Root.Element("runtime") == null
                || configFile.Root.Element("runtime").Element(bindingRedirectNs + "assemblyBinding") == null)
                return Enumerable.Empty<AssemblyBinding>();

            return configFile.Root.Element("runtime")
                    .Element(bindingRedirectNs + "assemblyBinding")
                    .Elements(bindingRedirectNs + "dependentAssembly")
                    .Select(AssemblyBinding.Parse);
        }
    }
}
