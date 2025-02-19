namespace SimpleCompiler.Ast;

public abstract class CNodeType<T>
{
    // T = type of element in a grammar
    private  readonly T m_nodeType;

    protected CNodeType(T nodeType)
    {
        m_nodeType = nodeType;
    }

    public T Value => m_nodeType;

    // int to type
    public abstract T Map(int type);
    
    // type to int
    public abstract int Map(T type);
    
    // default type value
    public abstract T Default();
    
    // not applicable value
    public abstract T NA();
}