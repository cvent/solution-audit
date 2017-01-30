using NuGet;

namespace SolutionAudit.AuditInformation
{
    public class InconsistentVersionPackageReference : Wrapper<PackageReference>, IVisualStudioCommand
    {
        public InconsistentVersionPackageReference(PackageReference wrapped) : base(wrapped)
        {
        }

        public string VsCommand()
        {
            return string.Format("Update-Package {0} -Version {1} {2}", Wrapped.Id, Wrapped.Version, Wrapped.Version.ToString().Contains("-") ? "-PreRelease" : string.Empty);
        }

        public override string ToString()
        {
            return string.Format("= {0}", Wrapped);
        }
    }
}