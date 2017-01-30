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

using System.Collections.Generic;
using NuGet;

namespace SolutionAudit.AuditInformation
{
    public class DuplicatePackageReference : Wrapper<PackageReference>
    {
        public DuplicatePackageReference(PackageReference wrapped, string fileName, string dllName = null) : base(wrapped)
        {
            FileName = fileName;
            DllName = dllName;
        }

        private string FileName { get; set; }
        private string DllName { get; set; }

        public override string ToString()
        {
            var dllName = DllName == null ? string.Empty : "/" + DllName;
            return string.Format("& {0}{2} (More than 1 entry in {1})", Wrapped, FileName, dllName);
        }
    }
}
