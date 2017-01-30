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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace SolutionAudit.AuditInformation
{
    public class MissingPackageDependency : Wrapper<PackageDependency>, IVisualStudioCommand
    {
        public MissingPackageDependency(PackageDependency wrapped, IEnumerable<IPackage> referencingPackages) : base(wrapped)
        {
            ReferencingPackages = referencingPackages;
        }

        private IEnumerable<IPackage> ReferencingPackages { get; set; }

        public string VsCommand()
        {
            return string.Format("Install-Package {0} -Version {1} {2}", Wrapped.Id, Wrapped.VersionSpec.MinVersion, Wrapped.VersionSpec.ToString().Contains("-") ? "-PreRelease" : string.Empty);
        }

        public override string ToString()
        {
            return string.Format("+ {0} [{1}]", Wrapped, string.Join(", ", ReferencingPackages.Select(p => p.Id)));
        }
    }
}
