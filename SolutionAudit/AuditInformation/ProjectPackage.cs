using NuGet;

namespace SolutionAudit.AuditInformation
{
    public class ProjectPackage : Wrapper<IPackage>
    {
        public ProjectPackage(IPackage wrapped) : base(wrapped)
        {
        }

        public override string ToString()
        {
            return string.Format("- {0} => Project Reference", Wrapped.Id);
        }
    }
}