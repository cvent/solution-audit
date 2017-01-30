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
using System.Threading.Tasks;
using log4net;
using NuGet;
using Roslyn.Services;
using SolutionAudit.AuditInformation;

namespace SolutionAudit
{
    public class AuditSolution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _solutionPath;
        private Options _options;

        public IEnumerable<AuditProject> Projects { get; private set; }
        public IEnumerable<IllegalFile> IllegalSolutionFiles { get; private set; }

        public AuditSolution(String solutionPath, Options options)
        {
            _solutionPath = solutionPath;
            _options = options;

            Log.Debug("Auditing projects");

            Projects = Solution.Load(solutionPath)
                .Projects
                .AsParallel()
                .Select(p => new AuditProject(p, new FrameworkName(".NETFramework, Version=4.5"), options))
                .ToArray();

            Log.Debug("Adding version inconsistencies");

            // We have to check inconsistent packages separately since it is based on packages in a solution,
            // rather than just in a project.
            if (options.Inconsistent) AuditProject.FillInconsistentPackageReferences(Projects, options.AllInconsistent);

            IllegalSolutionFiles = options.NugetTargets
                ? TestForNugetFolder()
                : Enumerable.Empty<IllegalFile>();

            Log.Debug("Auditing complete");
        }

        private IEnumerable<IllegalFile> TestForNugetFolder()
        {
            var nugetPath = Path.Combine(Path.GetDirectoryName(_solutionPath), ".nuget");
            return Directory.Exists(nugetPath)
                ? new IllegalFile[] { new IllegalFile(nugetPath) }
                : Enumerable.Empty<IllegalFile>();
        }

        public bool PassAudit
        {
            get
            {
                return IllegalSolutionFiles.IsEmpty()
                    && Projects.All(p => p.PassAudit());
            }
        }

        public override string ToString()
        {
            return PassAudit
                ? "Solution passed audit."
                : GetErrors();
        }

        public string GetErrors()
        {
            var errors = new StringBuilder();

            foreach (var file in IllegalSolutionFiles)
            {
                errors.AppendLine(file.ToString());
            }

            foreach (var auditProject in Projects.Where(auditProject => !auditProject.PassAudit()))
            {
                errors.Append(_options.NugetCommands ? auditProject.VsCommand() : auditProject.ToString());
            }

            return errors.ToString();
        }
    }
}
