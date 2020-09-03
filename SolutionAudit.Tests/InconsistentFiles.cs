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
            var solutionPath = Path.GetFullPath("test/InconsistentFiles/InconsistentFiles.sln");
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
            Assert.AreEqual(new List<string> { "& RabbitMQ.Client (Only present in assembly binding)" }, audit);
        }

        [Test]
        public void MismatchBindingRedirect()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("MismatchBindingRedirect"));
            var auditProject = new AuditProject(project, _targetFramework, new Options {RedirectMismatch = true});
            var audit = auditProject.BindingRedirectMismatchReferences.Select(p => p.ToString()).ToList();
            Assert.AreEqual(new List<string> { "^ Autofac 3.4.0.0 (Assembly binding redirects to 3.5.0.0)" }, audit);
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
