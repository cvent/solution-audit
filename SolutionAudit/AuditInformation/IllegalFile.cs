using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionAudit.AuditInformation
{
    public class IllegalFile
    {
        private string _filePath;

        public IllegalFile(string filePath)
        {
            _filePath = filePath;
        }

        public override string ToString()
        {
            return String.Format("Illegal solution file/folder must be removed: {0}", _filePath);
        }
    }
}
