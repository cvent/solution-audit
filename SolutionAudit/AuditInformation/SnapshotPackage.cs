using NuGet;

namespace SolutionAudit.AuditInformation
{
    public class SnapshotPackage : Wrapper<IPackage>
    {
        public SnapshotPackage(IPackage wrapped) : base(wrapped)
        {
        }

        public override string ToString()
        {
            return string.Format("$ {0}", Wrapped);
        }
    }
}