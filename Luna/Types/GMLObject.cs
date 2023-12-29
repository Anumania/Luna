using Luna.Instructions;
using Luna.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Types
{
    abstract class GMLObject
    {
        public Domain Environment;
        public Dictionary<string, LValue> Variables;

        public GMLObject() 
        {
            this.Environment = new Domain(this);
        }

        public void Call()
        {
            throw new Exception("Cant throw this!");
        }
    }
}
