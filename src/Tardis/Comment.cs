using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iSynaptic.Commons;

namespace Tardis
{
    public class Comment : Annotation
    {
        public Comment(string message)
        {
            Message = Guard.NotNullOrWhiteSpace(message, "message");
        }

        public string Message { get; set; }
    }
}
