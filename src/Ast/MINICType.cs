namespace SimpleCompiler.Ast;   
public class MINICType(MINICType.NodeType nodeType) : CNodeType<MINICType.NodeType>(nodeType)
{
    public NodeType Type => Value;
    public enum NodeType
    {
        
        // other
        NT_COMPILEUNIT,
        NT_FUNCTIONDEFINITION,
        NT_BLOCK,
        NT_GLOBALDECLARATION,
        NT_LOCALDECLARATION,
        NT_ASSIGNMENT,
        
        // expressions
        NT_FUNCTIONDECLARATION,
        NT_BINARYEXPRESSION,
        NT_FUNCTIONCALL,
        NT_UNARYEXPRESSION,
        NT_RETURN,
        
        // control structures
        NT_IFSTATEMENT,
        NT_WHILE_STATEMENT,
        
        NT_TYPESPECIFIER,
        NT_BREAKSTATEMENT,
        NT_PARAMETER,
        NT_FARGS, 

        // For function call arguments (the "args" rule)
        NT_ARGS,

        // For numeric or string literal nodes (if you want them distinctly)
        NT_NUMBERLITERAL,

        // For an identifier node (often helpful if you want to store the name)
        NT_IDENTIFIER,

        // For an initializer ("= expression") if you want a dedicated node
        // NT_INITIALIZER,

        // For statement lists if you prefer them separate from NT_BLOCK
        // NT_STATEMENTLIST,

        // If your grammar allows "expression;" as a statement (the typical "expressionStatement"):
        NT_EXPRESSIONSTATEMENT,
        
        // default/undefined
        NT_NA = -1
        
    }
    
    public enum BinaryOperator
    {
        Plus,
        Minus,
        Multiply,
        Divide,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        EqualEqual,
        NotEqual,
        Or,
        And,
    }
    
    public enum UnaryOperator
    {
        Plus,
        Minus,
        Not,
    }

    public override NodeType Map(int type)
    {
        // needs validation?
        return (NodeType)type;
    }

    public override int Map(NodeType type)
    {
        return (int)type;
    }

    
    public override NodeType Default()
    {
        return NodeType.NT_NA;
    }

    public override NodeType NA()
    {
        return NodeType.NT_NA;
    }
}

public abstract class MINIC_ASTElement(int context, MINICType.NodeType type, int line=0, int column=0) : ASTElement(context)
{
    private MINICType m_nodeType = new(type);
    public int Line { get; }
    public int Column { get; }
    
    public MINICType.NodeType NodeType => m_nodeType.Type;



    
}

public class CCompileUnit() : MINIC_ASTElement(1, MINICType.NodeType.NT_COMPILEUNIT)
{
    // enum for children contexts?
    public const int CT_BODY = 0;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        MINICBaseASTVisitor<T> visitor_ = visitor as MINICBaseASTVisitor<T>;
        if (visitor_ != null)
        {
            visitor_.VisitCompileUnit(this);
        }
        return default(T);
    }
}

// Primary constructor format
public class CFunctionDefinition(string functionName, string typeSpecifier, int line, int column) 
    : MINIC_ASTElement(3, MINICType.NodeType.NT_FUNCTIONDEFINITION, line, column)
{
    // Child context indices
    public const int CT_FARGS = 0;
    public const int CT_BODY = 1;
    public const int CT_RETURN = 2;
    
    // Properties
    public string FunctionName { get; } = functionName;
    public string TypeSpecifier { get; } = typeSpecifier; // might need a distinguished class
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitFunctionDefinition(this);
        }
        return default(T);
    }
    
}

public class CFunctionDeclaration(string functionName, string typeSpecifier, int line, int column)
    : MINIC_ASTElement(1, MINICType.NodeType.NT_FUNCTIONDECLARATION, line, column)
{
    // Child context indices
    public const int CT_PARAMETERS = 0;
    
    // Properties
    public string FunctionName { get; } = functionName;
    public string TypeSpecifier { get; } = typeSpecifier;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitFunctionDeclaration(this);
        }
        return default(T);
    }
    
}

public class CIfStatement(int line, int column) 
    : MINIC_ASTElement(3, MINICType.NodeType.NT_IFSTATEMENT, line, column)
{
    // Child context indices
    public const int CT_CONDITION = 0;
    public const int CT_BODY = 1;
    public const int CT_ELSE = 2;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitIfStatement(this);
        }
        return default(T);
    }
    
}

public class CWhileStatement(int line, int column) 
    : MINIC_ASTElement(2, MINICType.NodeType.NT_WHILE_STATEMENT, line, column)
{
    // Child context indices
    public const int CT_CONDITION = 0;
    public const int CT_BODY = 1;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitWhileStatement(this);
        }
        return default(T);
    }
}

public class CReturnStatement(int line, int column) 
    : MINIC_ASTElement(1, MINICType.NodeType.NT_RETURN, line, column)
{
    // Child context indices
    public const int CT_RETURNEXPRESSION = 0;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitReturn(this);
        }
        return default(T);
    }
    
}

public class CBreakStatement(int line, int column) 
    : MINIC_ASTElement(0, MINICType.NodeType.NT_BREAKSTATEMENT, line, column)
{
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitBreakStatement(this);
        }
        return default(T);
    }
}

public class CBlockStatement(int line, int column) 
    : MINIC_ASTElement(1, MINICType.NodeType.NT_BLOCK, line, column)
{
    // Child context indices
    public const int CT_BODY = 0;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitBlock(this);
        }
        return default(T);
    }
    
}

/*
public class CExpressionStatement(int line, int column) 
    : MINIC_ASTElement(1, MINICType.NodeType.NT_EXPRESSIONSTATEMENT, line, column)
{
    // Child context indices
    public const int CT_EXPRESSION = 0;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitExpressionStatement(this);
        }
        return default(T);
    }
}
*/

public class CBinaryExpression(MINICType.BinaryOperator op, int line, int column) 
    : MINIC_ASTElement(2, MINICType.NodeType.NT_BINARYEXPRESSION, line, column)
{
    // Child context indices
    public const int CT_LEFT = 0;
    public const int CT_RIGHT = 1;

    // Properties
    public MINICType.BinaryOperator Operator { get; } = op;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitBinaryExpression(this);
        }
        return default(T);
    }

}

public class CUnaryExpression(MINICType.UnaryOperator op, int line, int column) 
    : MINIC_ASTElement(1, MINICType.NodeType.NT_UNARYEXPRESSION, line, column)
{
    // Child context indices
    public const int CT_OPERAND = 0;
    
    // Properties
    public MINICType.UnaryOperator Operator { get; } = op;
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitUnaryExpression(this);
        }
        return default(T);
    }
    
}

public class CAssignmentExpression(int line, int column) 
    : MINIC_ASTElement(2, MINICType.NodeType.NT_ASSIGNMENT, line, column)
{
    // Child context indices
    public const int CT_LEFT = 0;
    public const int CT_RIGHT = 1;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitAssignment(this);
        }
        return default(T);
    }
}

public class CFunctionCallExpression(string functionName, int line, int column) 
    : MINIC_ASTElement(1, MINICType.NodeType.NT_FUNCTIONCALL, line, column)
{
    // Child context indices
    public const int CT_ARGS = 0;   
    
    // Properties
    public string FunctionName { get; } = functionName;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitFunctionCall(this);
        }
        return default(T);
    }
}

public class CNumberLiteral(string value, int line, int column) 
    : MINIC_ASTElement(0, MINICType.NodeType.NT_NUMBERLITERAL, line, column)
{
    // Properties
    public string Value { get; } = value;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitNumberLiteral(this);
        }
        return default(T);
    }
}

public class CIdentifier(string name, int line, int column) 
    : MINIC_ASTElement(0, MINICType.NodeType.NT_IDENTIFIER, line, column)
{
    // Properties
    public string Name { get; } = name;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitIdentifier(this);
        }
        return default(T);
    }
}

public class CFargs(int line, int column) 
    : MINIC_ASTElement(1, MINICType.NodeType.NT_FARGS, line, column)
{
    // Properties
    public const int CT_PARAMETERS = 0;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitFArgs(this);
        }
        return default(T);
    }
}

public class CParameter(string name, string typeSpecifier, int line, int column) 
    : MINIC_ASTElement(0, MINICType.NodeType.NT_PARAMETER, line, column)
{
    // Properties
    public string Name { get; } = name;
    public string TypeSpecifier { get; } = typeSpecifier;
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitParameter(this);
        }
        return default(T);
    }
}

public class CGlobalDeclaration(int line, int column)
    : MINIC_ASTElement(1, MINICType.NodeType.NT_GLOBALDECLARATION, line, column)
{
    public string VarName { get; }
    
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitGlobalDeclaration(this);
        }
        return default(T);
    }
}

public class CLocalDeclaration(int line, int column)
    : MINIC_ASTElement(1, MINICType.NodeType.NT_LOCALDECLARATION, line, column)
{   
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitLocalDeclaration(this);
        }
        return default(T);
    }
}

public class CTypeSpecifier(string typeStr, int line, int column)
    : MINIC_ASTElement(0, MINICType.NodeType.NT_TYPESPECIFIER, line, column)
{
    public string TypeString { get; } = typeStr;
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitTypeSpecifier(this);
        }
        return default(T);
    }
}

public class CArgs(int line, int column) : MINIC_ASTElement(1, MINICType.NodeType.NT_ARGS, line, column)
{
    public override T Accept<T>(ASTBaseVisitor<T> visitor)
    {
        if (visitor is MINICBaseASTVisitor<T> minicVisitor)
        {
            return minicVisitor.VisitArgs(this);
        }
        return default(T);
    }
}
