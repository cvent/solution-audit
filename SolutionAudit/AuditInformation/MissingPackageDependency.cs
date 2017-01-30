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