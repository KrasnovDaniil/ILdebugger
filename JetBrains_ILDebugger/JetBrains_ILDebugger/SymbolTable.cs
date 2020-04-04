using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetBrains_ILDebugger
{
    class SymbolTable
    {
        private Dictionary<string, VarNode> symbolTable;
        public SymbolTable()
        {
            symbolTable = new Dictionary<string, VarNode>();
        }

        public void Add(string name)
        {
            try
            {
                symbolTable.Add(name, null);
            }catch
            {
                Console.WriteLine("variable already declared");
            }
        }

        public void Add(VarNode var)
        {
            try
            {
                symbolTable.Add(var.name, var);
            }
            catch
            {
                Console.WriteLine("variable " + var.name + " already declarated in this scope");
            }
        }

        public long getValue(VarNode var)
        {
            VarNode varnode;
            if (symbolTable.TryGetValue(var.name, out varnode))
                return varnode.val;
            else
            {
                string msg = ("variable " + var.name + " wasn't found in this scope");
                throw new Exception(msg);
            }
        }
    }
}
