using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NUnit.Framework;
using Roslyn.Services;

namespace SolutionAudit.Tests
{
    [TestFixture]
    public class InconsistentFiles
    {
        private ISolution RoslynSolution { get; set; }
        private readonly FrameworkName _targetFramework = new FrameworkName(".NETFramework, Version=4.5");
        private Options Options { get; set; }

        [SetUp]
        public void Setup()
        {
            var solutionPath = Path.GetFullPath("../../../test/InconsistentFiles/InconsistentFiles.sln");
            RoslynSolution = Solution.Load(solutionPath);
            Options = new Options {FileDiff = true};
        }

        [Test]
        public void DuplicatePackagesEntry()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("DuplicatePackagesEntry"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.DuplicatePackageReferences.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string> {"& NuGet.Core 2.8.3 (More than 1 entry in packages.config)"}, audit);
        }

        [Test]
        public void MissingInPackagesConfig()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("MissingInPackagesConfig"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.MissingPackageReferences.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string> { "& NuGet.Core 2.8.3 (Only present in .csproj)" }, audit);
        }

        [Test]
        public void MissingInCsproj()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("MissingInCsproj"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.MissingPackageReferences.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string> { "& NuGet.Core 2.8.3 (Only present in packages.config)" }, audit);
        }

        [Test]
        public void OnlyBindingRedirect()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("OnlyBindingRedirect"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.BindingRedirectOnlyReferences.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string> { "& RabbitMQ.Client (Only present as an assembly binding)" }, audit);
        }

        [Test]
        public void BadProjectRefGuid()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("BadProjectRefGuid"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.BadProjectRefGuids.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string> { "& InconsistentFiles.OnlyBindingRedirect (Bad Project Reference GUID)" }, audit);
        }
    }
}
