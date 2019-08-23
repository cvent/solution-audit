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
    class NugetTargets
    {
        private ISolution RoslynSolution { get; set; }
        private readonly FrameworkName _targetFramework = new FrameworkName(".NETFramework, Version=4.5");
        private Options Options { get; set; }

        [SetUp]
        public void Setup()
        {
            var solutionPath = Path.GetFullPath("test/NugetTargets/NugetTargets.sln");
            RoslynSolution = Solution.Load(solutionPath);
            Options = new Options { NugetTargets = true };
        }

        [Test]
        public void WithoutTargets()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("WithoutTargets"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            Assert.IsEmpty(auditProject.IllegalNugetTargets);
        }

        [Test]
        public void WithTargets()
        {
            var project = RoslynSolution.Projects.First(p => p.Name.EndsWith("WithTargets"));
            var auditProject = new AuditProject(project, _targetFramework, Options);
            var issues = auditProject.IllegalNugetTargets.Select(a => a.ToString());

            var importIssues = issues.Where(a => a.Contains("<Import"));
            Assert.That(importIssues.Count(), Is.EqualTo(1));
            StringAssert.Contains(@"<Import Project=""$(SolutionDir)\.nuget\NuGet.targets"" ...>", importIssues.First());
            StringAssert.Contains(@"\test\NugetTargets\WithTargets\WithTargets.csproj (60,3)", importIssues.First());

            var targetIssues = issues.Where(a => a.Contains("<Error"));
            Assert.That(targetIssues.Count(), Is.EqualTo(1));
            StringAssert.Contains(@"<Error Condition=""!Exists('$(SolutionDir)\.nuget\NuGet.targets')"" ...>", targetIssues.First());
            StringAssert.Contains(@"\test\NugetTargets\WithTargets\WithTargets.csproj (65,5)", targetIssues.First());
        }
    }
}
