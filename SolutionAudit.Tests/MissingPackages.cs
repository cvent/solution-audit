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
    public class MissingPackages
    {
        private ISolution RoslynSolution { get; set; }
        private readonly FrameworkName _targetFramework = new FrameworkName(".NETFramework, Version=4.5");
        private Options Options { get; set; }

        [SetUp]
        public void Setup()
        {
            var solutionPath = Path.GetFullPath("../../../test/MissingPackages/MissingPackages.sln");
            RoslynSolution = Solution.Load(solutionPath);
            Options = new Options {Missing = true};
        }

        [Test]
        public void MissingPackage()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("MissingPackage"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.MissingPackages.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string> { "+ Microsoft.Web.Xdt (≥ 2.1.0) [NuGet.Core]" }, audit);
        }

        [Test]
        public void ProjectReference()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("ProjectReference"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var audit = auditProject.MissingPackages.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string>(), audit);
        }
    }
}
