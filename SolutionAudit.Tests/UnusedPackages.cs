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
    public class UnusedPackages
    {
        private ISolution RoslynSolution { get; set; }
        private readonly FrameworkName _targetFramework = new FrameworkName(".NETFramework, Version=4.5");
        private Options Options { get; set; }

        [SetUp]
        public void Setup()
        {
            var solutionPath = Path.GetFullPath("../../../test/UnusedPackages/UnusedPackages.sln");
            RoslynSolution = Solution.Load(solutionPath);
            Options = new Options {Unused = true, UnusedUsings = true};
        }

        [Test]
        public void CompletelyUnused()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("CompletelyUnused"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.UnusedPackages.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string> {"- NuGet.Core 2.8.3"}, audit);
        }

        [Test]
        public void OnlyUsedInUsings()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("OnlyUsedInUsings"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.UnusedPackages.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string> { "- NuGet.Core 2.8.3" }, audit);
        }

        [Test]
        public void UsedInOverload()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("UsedInOverload"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var unused = auditProject.UnusedPackages.Select(p => p.ToString()).ToList();
            Assert.AreEqual(unused, new List<string>());
        }

        [Test]
        public void UsedInProjectReferenceOverload()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("UsedInProjectReferenceOverload"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.UnusedPackages.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string>(), audit);
        }

        [Test]
        public void UsedInGenericMethodParams()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("UsedInGenericMethodParams"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.UnusedPackages.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string>(), audit);
        }
    }
}
