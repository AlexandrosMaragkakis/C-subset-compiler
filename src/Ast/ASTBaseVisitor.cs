namespace SimpleCompiler.Ast;

public abstract class ASTBaseVisitor<T>
{
    public virtual T Visit(ASTElement node)
    {
        return node.Accept(this);
    }
    
    public virtual T VisitChildren(ASTElement node)
    {
        T result = default(T);
        foreach (var child in node.GetChildren())
        {
            result = AggregateResult(result, child.Accept(this));
        }

        return result;
    }

    public virtual T AggregateResult(T oldResult, T value)
    {
        return value;
    }
}