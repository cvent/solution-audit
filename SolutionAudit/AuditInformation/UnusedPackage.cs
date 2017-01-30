using NuGet;

namespace SolutionAudit.AuditInformation
{
    public class UnusedPackage : Wrapper<IPackage>, IVisualStudioCommand
    {
        public UnusedPackage(IPackage wrapped) : base(wrapped)
        {
        }

        public string VsCommand()
        {
            return string.Format("Uninstall-Package {0}", Wrapped.Id);
        }

        public override string ToString()
        {
            return string.Format("- {0}", Wrapped);
        }
    }
}