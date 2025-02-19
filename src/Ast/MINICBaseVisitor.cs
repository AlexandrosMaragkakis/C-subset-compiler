namespace SimpleCompiler.Ast;

public abstract class MINICBaseASTVisitor<T> : ASTBaseVisitor<T>
{
    public virtual T VisitCompileUnit(CCompileUnit node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitFunctionDefinition(CFunctionDefinition node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitBlock(CBlockStatement node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitGlobalDeclaration(CGlobalDeclaration node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitLocalDeclaration(CLocalDeclaration node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitAssignment(CAssignmentExpression node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitFunctionDeclaration(CFunctionDeclaration node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitBinaryExpression(CBinaryExpression node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitFunctionCall(CFunctionCallExpression node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitUnaryExpression(CUnaryExpression node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitReturn(CReturnStatement node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitIfStatement(CIfStatement node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitWhileStatement(CWhileStatement node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitTypeSpecifier(CTypeSpecifier node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitBreakStatement(CBreakStatement node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitParameter(CParameter node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitFArgs(CFargs node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitArgs(CArgs node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitNumberLiteral(CNumberLiteral node)
    {
        return VisitChildren(node);
    }

    public virtual T VisitIdentifier(CIdentifier node)
    {
        return VisitChildren(node);
    }
    
}