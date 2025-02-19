using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SimpleCompiler.CodeGen
{
    // Return statement composite – emits code to move the return value into EAX and then RET.
    public class AsmReturnStatement : AsmComboContainer
    {
        public AsmReturnStatement(AsmEmittableCodeContainer parent)
            : base(AsmCodeBlockType.ACB_RETURN, parent, 1) { }

        public override AsmCodeContainer AssemblyCodeContainer()
        {
            AsmCodeContainer rep = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, m_parent);
            rep.AddCode("; return statement", AsmCodeContextType.ACC_RETURN);
            // Assume that the return expression was generated in ACC_RETURN context.
            rep.AddCode("mov eax, " + AssemblyContext(AsmCodeContextType.ACC_RETURN).ToString(), AsmCodeContextType.ACC_RETURN);
            rep.AddNewLine(AsmCodeContextType.ACC_RETURN);
            rep.AddCode("ret", AsmCodeContextType.ACC_RETURN);
            rep.AddNewLine(AsmCodeContextType.ACC_RETURN);
            return rep;
        }

        public override void PrintStructure(StreamWriter m_ostream)
        {
            foreach (List<AsmEmittableCodeContainer> list in m_repository)
            {
                foreach (var code in list)
                {
                    code.PrintStructure(m_ostream);
                }
            }
            if (m_parent != null)
                m_ostream.WriteLine("\"{0}\" -> \"{1}\"", m_parent.NodeName, this.NodeName);
        }
    }

    // If statement composite – generates labels for ELSE and ENDIF.
    public class AsmIfStatement : AsmComboContainer
    {
        public AsmIfStatement(AsmEmittableCodeContainer parent)
            : base(AsmCodeBlockType.ACB_IF, parent, 3) { }

        public override AsmCodeContainer AssemblyCodeContainer()
        {
            AsmCodeContainer rep = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, m_parent);
            rep.AddCode("; if statement", AsmCodeContextType.ACC_IF_CONDITION);
            rep.AddCode(AssemblyContext(AsmCodeContextType.ACC_IF_CONDITION).ToString(), AsmCodeContextType.ACC_IF_CONDITION);
            rep.AddNewLine(AsmCodeContextType.ACC_IF_CONDITION);
            string elseLabel = NodeName + "_else";
            string endLabel = NodeName + "_endif";
            rep.AddCode("cmp eax, 0", AsmCodeContextType.ACC_IF_CONDITION);
            rep.AddNewLine(AsmCodeContextType.ACC_IF_CONDITION);
            rep.AddCode("je " + elseLabel, AsmCodeContextType.ACC_IF_CONDITION);
            rep.AddNewLine(AsmCodeContextType.ACC_IF_CONDITION);
            // Then block
            rep.AddCode("; then block", AsmCodeContextType.ACC_IF_THEN);
            rep.AddCode(AssemblyContext(AsmCodeContextType.ACC_IF_THEN).ToString(), AsmCodeContextType.ACC_IF_THEN);
            rep.AddNewLine(AsmCodeContextType.ACC_IF_THEN);
            rep.AddCode("jmp " + endLabel, AsmCodeContextType.ACC_IF_THEN);
            rep.AddNewLine(AsmCodeContextType.ACC_IF_THEN);
            // Else block
            rep.AddCode(elseLabel + ":", AsmCodeContextType.ACC_IF_ELSE);
            rep.AddNewLine(AsmCodeContextType.ACC_IF_ELSE);
            rep.AddCode("; else block", AsmCodeContextType.ACC_IF_ELSE);
            rep.AddCode(AssemblyContext(AsmCodeContextType.ACC_IF_ELSE).ToString(), AsmCodeContextType.ACC_IF_ELSE);
            rep.AddNewLine(AsmCodeContextType.ACC_IF_ELSE);
            rep.AddCode(endLabel + ":", AsmCodeContextType.ACC_IF_CONDITION);
            rep.AddNewLine(AsmCodeContextType.ACC_IF_CONDITION);
            return rep;
        }

        public override void PrintStructure(StreamWriter m_ostream)
        {
            foreach (List<AsmEmittableCodeContainer> list in m_repository)
            {
                foreach (var code in list)
                {
                    code.PrintStructure(m_ostream);
                }
            }
            if (m_parent != null)
                m_ostream.WriteLine("\"{0}\" -> \"{1}\"", m_parent.NodeName, this.NodeName);
        }
    }

    // While statement composite – generates loop and exit labels.
    public class AsmWhileStatement : AsmComboContainer
    {
        public AsmWhileStatement(AsmEmittableCodeContainer parent)
            : base(AsmCodeBlockType.ACB_WHILE, parent, 2) { }

        public override AsmCodeContainer AssemblyCodeContainer()
        {
            AsmCodeContainer rep = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, m_parent);
            string loopLabel = NodeName + "_loop";
            string endLabel = NodeName + "_end";
            rep.AddCode(loopLabel + ":", AsmCodeContextType.ACC_WHILE_CONDITION);
            rep.AddNewLine(AsmCodeContextType.ACC_WHILE_CONDITION);
            rep.AddCode("; while condition", AsmCodeContextType.ACC_WHILE_CONDITION);
            rep.AddCode(AssemblyContext(AsmCodeContextType.ACC_WHILE_CONDITION).ToString(), AsmCodeContextType.ACC_WHILE_CONDITION);
            rep.AddNewLine(AsmCodeContextType.ACC_WHILE_CONDITION);
            rep.AddCode("cmp eax, 0", AsmCodeContextType.ACC_WHILE_CONDITION);
            rep.AddNewLine(AsmCodeContextType.ACC_WHILE_CONDITION);
            rep.AddCode("je " + endLabel, AsmCodeContextType.ACC_WHILE_CONDITION);
            rep.AddNewLine(AsmCodeContextType.ACC_WHILE_CONDITION);
            rep.AddCode("; while body", AsmCodeContextType.ACC_WHILE_BODY);
            rep.AddCode(AssemblyContext(AsmCodeContextType.ACC_WHILE_BODY).ToString(), AsmCodeContextType.ACC_WHILE_BODY);
            rep.AddNewLine(AsmCodeContextType.ACC_WHILE_BODY);
            rep.AddCode("jmp " + loopLabel, AsmCodeContextType.ACC_WHILE_BODY);
            rep.AddNewLine(AsmCodeContextType.ACC_WHILE_BODY);
            rep.AddCode(endLabel + ":", AsmCodeContextType.ACC_WHILE_CONDITION);
            rep.AddNewLine(AsmCodeContextType.ACC_WHILE_CONDITION);
            return rep;
        }

        public override void PrintStructure(StreamWriter m_ostream)
        {
            foreach (List<AsmEmittableCodeContainer> list in m_repository)
            {
                foreach (var code in list)
                {
                    code.PrintStructure(m_ostream);
                }
            }
            if (m_parent != null)
                m_ostream.WriteLine("\"{0}\" -> \"{1}\"", m_parent.NodeName, this.NodeName);
        }
    }

    // Compound statement (block) composite.
    public class AsmCompoundStatement : AsmComboContainer
    {
        public AsmCompoundStatement(AsmEmittableCodeContainer parent)
            : base(AsmCodeBlockType.ACB_COMPOUND, parent, 1) { }

        public override AsmCodeContainer AssemblyCodeContainer()
        {
            AsmCodeContainer rep = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, m_parent);
            rep.EnterScope();
            rep.AddCode(AssemblyContext(AsmCodeContextType.ACC_COMPOUND_BODY).ToString(), AsmCodeContextType.ACC_COMPOUND_BODY);
            rep.LeaveScope();
            return rep;
        }


        public override void PrintStructure(StreamWriter m_ostream)
        {
            foreach (List<AsmEmittableCodeContainer> list in m_repository)
            {
                foreach (var code in list)
                {
                    code.PrintStructure(m_ostream);
                }
            }
            if (m_parent != null)
                m_ostream.WriteLine("\"{0}\" -> \"{1}\"", m_parent.NodeName, this.NodeName);
        }
    }

    // Function definition composite with integrated local symbol table.
    public class AsmFunctionDefinition : AsmComboContainer
    {
        public string FunctionName { get; set; }
        public string ReturnType { get; set; } // Only 'int' supported.
        public int LocalSize { get; set; } = 0;  // Stack allocation size for local variables.

        // Local symbol table mapping variable names to their stack offsets.
        private Dictionary<string, int> m_localSymbolTable = new Dictionary<string, int>();
        public Dictionary<string, int> Parameters { get; } = new Dictionary<string, int>();

        public AsmFunctionDefinition(AsmEmittableCodeContainer parent, string functionName, string returnType)
            : base(AsmCodeBlockType.ACB_FUNCTION, parent, 2)
        {
            FunctionName = functionName;
            ReturnType = returnType;
        }
        

        public void AddParameter(string name, int offset)
        {
            if (!Parameters.ContainsKey(name))
                Parameters.Add(name, offset);
        }

        public bool TryGetParameterOffset(string name, out int offset)
        {
            return Parameters.TryGetValue(name, out offset);
        }

        
        public bool TryGetLocalVariableOffset(string varname, out int offset)
        {
            return m_localSymbolTable.TryGetValue(varname, out offset);
        }

        // Declare a local variable if not already declared.
        public void DeclareVariable(string varname)
        {
            if (!m_localSymbolTable.ContainsKey(varname))
            {
                // Calculate offset: first variable at -4, next at -8, etc.
                int offset = -(m_localSymbolTable.Count + 1) * 4;
                m_localSymbolTable[varname] = offset;
                // Increase local stack size; assume each variable occupies 4 bytes.
                LocalSize += 4;
                // Emit a comment indicating the declaration with offset.
                AsmCodeContainer decl = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, this);
                decl.AddCode("; local variable " + varname + " at offset " + offset, AsmCodeContextType.ACC_FUNCTION_BODY);
                this.AddCode(decl, AsmCodeContextType.ACC_FUNCTION_BODY);
            }
        }

        public override AsmCodeContainer AssemblyCodeContainer()
        {
            AsmCodeContainer rep = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, m_parent);
            // Function header and prologue.
            rep.AddCode(FunctionName + " PROC", AsmCodeContextType.ACC_FUNCTION_HEADER);
            rep.AddNewLine(AsmCodeContextType.ACC_FUNCTION_HEADER);
            rep.AddCode("push ebp", AsmCodeContextType.ACC_FUNCTION_HEADER);
            rep.AddNewLine(AsmCodeContextType.ACC_FUNCTION_HEADER);
            rep.AddCode("mov ebp, esp", AsmCodeContextType.ACC_FUNCTION_HEADER);
            rep.AddNewLine(AsmCodeContextType.ACC_FUNCTION_HEADER);
            if (LocalSize > 0)
            {
                rep.AddCode("sub esp, " + LocalSize, AsmCodeContextType.ACC_FUNCTION_HEADER);
                rep.AddNewLine(AsmCodeContextType.ACC_FUNCTION_HEADER);
            }
            // Function body.
            rep.AddCode(AssemblyContext(AsmCodeContextType.ACC_FUNCTION_BODY).ToString(), AsmCodeContextType.ACC_FUNCTION_BODY);
            // Epilogue.
            rep.AddCode("mov esp, ebp", AsmCodeContextType.ACC_FUNCTION_BODY);
            rep.AddNewLine(AsmCodeContextType.ACC_FUNCTION_BODY);
            rep.AddCode("pop ebp", AsmCodeContextType.ACC_FUNCTION_BODY);
            rep.AddNewLine(AsmCodeContextType.ACC_FUNCTION_BODY);
            rep.AddCode("ret", AsmCodeContextType.ACC_FUNCTION_BODY);
            rep.AddNewLine(AsmCodeContextType.ACC_FUNCTION_BODY);
            rep.AddCode(FunctionName + " ENDP", AsmCodeContextType.ACC_FUNCTION_BODY);
            rep.AddNewLine(AsmCodeContextType.ACC_FUNCTION_BODY);
            return rep;
        }

        public override void PrintStructure(StreamWriter m_ostream)
        {
            foreach (List<AsmEmittableCodeContainer> list in m_repository)
            {
                foreach (var code in list)
                {
                    code.PrintStructure(m_ostream);
                }
            }
            if (m_parent != null)
                m_ostream.WriteLine("\"{0}\" -> \"{1}\"", m_parent.NodeName, this.NodeName);
        }
    }

    // Top-level file composite with integrated global symbol table.
    public class AsmFile : AsmComboContainer
    {
        private HashSet<string> m_globalVarSymbolTable = new HashSet<string>();
        private HashSet<string> m_functionSymbolTable = new HashSet<string>();

        public AsmFile() : base(AsmCodeBlockType.ACB_FILE, null, 2) { } // Context 0: FILE_DATA, Context 1: FILE_CODE

        // Declare a global variable if not already declared.
        public void DeclareGlobalVariable(string varname, string initializer = null)
        {
            if (!m_globalVarSymbolTable.Contains(varname))
            {
                m_globalVarSymbolTable.Add(varname);
                AsmCodeContainer decl = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, this);
                if (initializer != null)
                {
                    decl.AddCode(varname + " DWORD " + initializer, AsmCodeContextType.ACC_FILE_DATA);
                }
                else
                {   
                    decl.AddCode(varname + " DWORD ?", AsmCodeContextType.ACC_FILE_DATA);
                }
                this.AddCode(decl, AsmCodeContextType.ACC_FILE_DATA);
            }
        }

        // Declare a function prototype if not already declared.
        public void DeclareFunction(string funname, string funheader)
        {
            if (!m_functionSymbolTable.Contains(funname))
            {
                m_functionSymbolTable.Add(funname);
                AsmCodeContainer decl = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, this);
                decl.AddCode(funheader + ";", AsmCodeContextType.ACC_FILE_DATA);
                this.AddCode(decl, AsmCodeContextType.ACC_FILE_DATA);
            }
        }

        public override AsmCodeContainer AssemblyCodeContainer()
        {
            AsmCodeContainer rep = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, null);
            rep.AddCode(".386");
            rep.AddCode(".model flat, stdcall");
            rep.AddCode(".stack 4096");

            rep.AddCode(".data", AsmCodeContextType.ACC_FILE_DATA);
            rep.AddNewLine(AsmCodeContextType.ACC_FILE_DATA);
            rep.AddCode(AssemblyContext(AsmCodeContextType.ACC_FILE_DATA).ToString(), AsmCodeContextType.ACC_FILE_DATA);
            rep.AddNewLine(AsmCodeContextType.ACC_FILE_DATA);
            rep.AddCode(".code", AsmCodeContextType.ACC_FILE_CODE);
            rep.AddNewLine(AsmCodeContextType.ACC_FILE_CODE);
            rep.AddCode(AssemblyContext(AsmCodeContextType.ACC_FILE_CODE).ToString(), AsmCodeContextType.ACC_FILE_CODE);
            
            rep.AddCode("END main");
            return rep;
        }

        public override void PrintStructure(StreamWriter m_ostream)
        {
            foreach (List<AsmEmittableCodeContainer> list in m_repository)
            {
                foreach (var code in list)
                {
                    code.PrintStructure(m_ostream);
                }
            }
        }
    }
}
