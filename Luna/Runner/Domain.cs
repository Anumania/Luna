using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Runner;
using Luna.Assets;
using Luna.Types;

namespace Luna.Runner {
    class Domain {
        public LCode Current;
        public GMLObject Scope = null;
        public GMLObject Instance = null;
        public Dictionary<string, LValue> Locals;
        public Stack<LValue> Stack = new Stack<LValue>();
        public Int32 ProgramCounter = 0;
        public bool ArrayNext = false;

        public Domain(GMLObject _inst) {
            this.Scope = _inst;
            this.Instance = _inst;
            this.Locals = new Dictionary<string, LValue>();
        }

        public Domain(GMLObject _inst, Domain _other) {
            this.Scope = _inst;
            this.Instance = _inst;
            this.Locals = _other.Locals;
        }

        public void ExecuteCode(Game _assets, LCode _code, Tuple<int, int> _range=null) {
            this.Current = _code;
            if (_range == null) _range = new Tuple<int, int>(0, this.Current.Instructions.Count);
            for (this.ProgramCounter = _range.Item1; this.ProgramCounter < _range.Item2; this.ProgramCounter++) {
                _code.Instructions[this.ProgramCounter].Perform(_assets, this, _code, this.Stack);
            }

#if (DEBUG)
            if (_assets.Headless == true) {
                if (this.Instance is LInstance)
                {
                    LInstance inst = (LInstance)this.Instance;
                    Console.WriteLine("Instance Variables (Object: {0}, Count: {1})", (inst.Object != null ? inst.Object.Name : "N/A"), this.Instance.Variables.Count);
                    foreach (KeyValuePair<string, LValue> _instVar in this.Instance.Variables)
                    {
                        Console.WriteLine("{0}.{1} = {2}", inst.ID, _instVar.Key, _instVar.Value.Value);
                    }
                }
            }
#endif
        }
    }
}