using System.Text;

namespace SimpleCompiler.CodeGen;

public enum AsmCodeBlockType
    {
        ACB_NA = -1,
        ACB_FILE = 0,
        ACB_FUNCTION = 1,
        ACB_IF = 2,
        ACB_WHILE = 3,
        ACB_COMPOUND = 4,
        ACB_EXPRESSION = 5,
        ACB_ASSIGNMENT = 6,
        ACB_RETURN = 7
    };

    public enum AsmCodeContextType
    {
        ACC_NA = -1,
        // File-level contexts
        ACC_FILE_DATA,        // Global declarations, string literals, etc.
        ACC_FILE_CODE,        // Function definitions go here.
        // Function-level contexts
        ACC_FUNCTION_HEADER,  // Function prologue (label, push ebp, etc.)
        ACC_FUNCTION_BODY,    // Function body instructions.
        // If statement contexts
        ACC_IF_CONDITION,     // Evaluate condition.
        ACC_IF_THEN,          // Then-block.
        ACC_IF_ELSE,          // Else-block.
        // While statement contexts
        ACC_WHILE_CONDITION,  // Evaluate condition.
        ACC_WHILE_BODY,       // Loop body.
        // Compound statement (block)
        ACC_COMPOUND_BODY,    // Code inside a block.
        // Expression contexts (if needed)
        ACC_EXPRESSION,
        // Assignment contexts
        ACC_ASSIGNMENT,
        // Return statement
        ACC_RETURN
    };

    public abstract class AsmEmittableCodeContainer
    {
        protected AsmCodeBlockType m_nodeType;
        protected int m_serialNumber;
        protected static int m_serialNumberCounter = 0;
        protected string m_nodeName;
        protected AsmEmittableCodeContainer m_parent;
        protected int m_nestingLevel = 0;

        public AsmEmittableCodeContainer Parent => m_parent;
        public int SerialNumber => m_serialNumber;
        public AsmCodeBlockType NodeType => m_nodeType;
        public string NodeName => m_nodeName;
        public int NestingLevel { get => m_nestingLevel; set => m_nestingLevel = value; }

        protected AsmEmittableCodeContainer(AsmCodeBlockType nodeType, AsmEmittableCodeContainer parent)
        {
            m_nodeType = nodeType;
            m_serialNumber = m_serialNumberCounter++;
            m_nodeName = m_nodeType.ToString() + "_" + m_serialNumber;
            m_parent = parent;
            m_nestingLevel = parent?.NestingLevel ?? 0;
        }

        public abstract AsmCodeContainer AssemblyCodeContainer();
        public abstract void AddCode(string code, AsmCodeContextType context = AsmCodeContextType.ACC_NA);
        public abstract void AddCode(AsmEmittableCodeContainer code, AsmCodeContextType context = AsmCodeContextType.ACC_NA);
        public abstract void PrintStructure(StreamWriter m_ostream);
        public abstract string EmitStdout();
        public abstract void EmitToFile(StreamWriter f);
        public virtual void EnterScope() { m_nestingLevel++; }
        public virtual void LeaveScope() { if (m_nestingLevel > 0) m_nestingLevel--; else throw new Exception("Non-matched nesting"); }
        public abstract void AddNewLine(AsmCodeContextType context = AsmCodeContextType.ACC_NA);
    }

    public abstract class AsmComboContainer : AsmEmittableCodeContainer
    {
        // m_repository holds a separate list of child containers for each context.
        protected List<AsmEmittableCodeContainer>[] m_repository;

        protected AsmComboContainer(AsmCodeBlockType nodeType, AsmEmittableCodeContainer parent, int numContexts)
            : base(nodeType, parent)
        {
            m_repository = new List<AsmEmittableCodeContainer>[numContexts];
            for (int i = 0; i < numContexts; i++)
                m_repository[i] = new List<AsmEmittableCodeContainer>();
        }

        /// <summary>
        /// Collects and assembles the code from a given context.
        /// </summary>
        protected virtual AsmCodeContainer AssemblyContext(AsmCodeContextType ct)
        {
            int index = GetContextIndex(ct);
            AsmCodeContainer rep = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, this);
            for (int i = 0; i < m_repository[index].Count; i++)
            {
                rep.AddCode(m_repository[index][i].AssemblyCodeContainer());
            }
            return rep;
        }

        public override void AddCode(string code, AsmCodeContextType context)
        {
            AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, this);
            container.AddCode(code, AsmCodeContextType.ACC_NA);
            m_repository[GetContextIndex(context)].Add(container);
        }

        public override void AddCode(AsmEmittableCodeContainer code, AsmCodeContextType context)
        {
            m_repository[GetContextIndex(context)].Add(code);
        }

        public override void AddNewLine(AsmCodeContextType context)
        {
            AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, this);
            container.AddNewLine();
            m_repository[GetContextIndex(context)].Add(container);
        }

        public override string EmitStdout()
        {
            string s = AssemblyCodeContainer().ToString();
            Console.WriteLine(s);
            return s;
        }
        public override void EmitToFile(StreamWriter f)
        {
            f.WriteLine(AssemblyCodeContainer().ToString());
        }
        public override string ToString()
        {
            return AssemblyCodeContainer().ToString();
        }
        public override void PrintStructure(StreamWriter m_ostream)
        {
            foreach (List<AsmEmittableCodeContainer> contextList in m_repository)
            {
                foreach (AsmEmittableCodeContainer container in contextList)
                {
                    container.PrintStructure(m_ostream);
                }
            }
            if (m_parent != null)
                m_ostream.WriteLine("\"{0}\" -> \"{1}\"", m_parent.NodeName, m_nodeName);
        }

        internal int GetContextIndex(AsmCodeContextType ct)
        {
            // For this example we use a simple mapping.
            // For each composite node type, we assume its valid contexts start at a fixed offset.
            int baseOffset = (int)ContextBase(m_nodeType);
            return (int)ct - baseOffset;
        }

        private AsmCodeContextType ContextBase(AsmCodeBlockType blockType)
        {
            // Define a base context for each block type.
            switch (blockType)
            {
                case AsmCodeBlockType.ACB_FILE: return AsmCodeContextType.ACC_FILE_DATA;
                case AsmCodeBlockType.ACB_FUNCTION: return AsmCodeContextType.ACC_FUNCTION_HEADER;
                case AsmCodeBlockType.ACB_IF: return AsmCodeContextType.ACC_IF_CONDITION;
                case AsmCodeBlockType.ACB_WHILE: return AsmCodeContextType.ACC_WHILE_CONDITION;
                case AsmCodeBlockType.ACB_COMPOUND: return AsmCodeContextType.ACC_COMPOUND_BODY;
                default: return AsmCodeContextType.ACC_NA;
            }
        }
    }

    public class AsmCodeContainer : AsmEmittableCodeContainer
    {
        private StringBuilder m_repository = new StringBuilder();

        public AsmCodeContainer(AsmCodeBlockType nodeType, AsmEmittableCodeContainer parent)
            : base(nodeType, parent)
        {
        }

        public override void AddCode(string code, AsmCodeContextType context = AsmCodeContextType.ACC_NA)
        {
            string[] lines = code.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                m_repository.Append(line);
                m_repository.Append("\r\n");
                m_repository.Append(new string('\t', m_nestingLevel));
            }
        }

        public override void AddCode(AsmEmittableCodeContainer code, AsmCodeContextType context = AsmCodeContextType.ACC_NA)
        {
            AddCode(code.AssemblyCodeContainer().ToString(), context);
        }

        public override void AddNewLine(AsmCodeContextType context = AsmCodeContextType.ACC_NA)
        {
            m_repository.Append("\r\n");
            m_repository.Append(new string('\t', m_nestingLevel));
        }

        public override string EmitStdout()
        {
            string s = m_repository.ToString();
            Console.WriteLine(s);
            return s;
        }

        public override void EmitToFile(StreamWriter f)
        {
            f.WriteLine(m_repository.ToString());
        }

        public override AsmCodeContainer AssemblyCodeContainer()
        {
            return this;
        }

        public override void PrintStructure(StreamWriter m_ostream)
        {
            if (m_parent != null)
                m_ostream.WriteLine("\"{0}\" -> \"{1}\"", m_parent.NodeName, m_nodeName);
        }

        public override string ToString()
        {
            return m_repository.ToString();
        }
    }