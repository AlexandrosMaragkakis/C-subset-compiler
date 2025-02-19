namespace SimpleCompiler.Ast;

public abstract class ASTElement // should it be abstract
{
    // node of the AST - Abstract Syntax Tree
    
    // we don't include a nodeType to keep the code reusable
    
    // m_children is an array of lists with ASTElements
    // index is the context, corresponding to the node type of the children
    private List<ASTElement>[]? m_children = null; 
    
    // examine use of private Dictionary<int, List<ASTElement>> _children; instead of the list
    
    // reference to parent
    private ASTElement m_parent;
    
    // node serial number
    private int m_serial;
    
    // node name
    private string m_name;
    
    // serial number generator
    private static int m_serialCounter = 0;

    // properties
    public ASTElement MParent => m_parent;
    public string MName => m_name;
    
    /// <summary>
    /// How many child contexts this node supports. i.e., how many
    /// separate 'slots' for children we have.
    /// </summary>
    public int ContextCount
    {
        get { return m_children == null ? 0 : m_children.Length; }
    }

    protected ASTElement(int context)
    {
        m_serial = ++m_serialCounter;
        m_name = GenerateNodeName();
        // context == 0 means no children 
        if (context != 0)
        {
            m_children = new List<ASTElement>[context];
            for (int i = 0; i < context; i++)
            {
                m_children[i] = new List<ASTElement>();
            }
        }
    }

    public void AddChild(ASTElement child, int contextIndex)
    {
        if (m_children == null)
        {
            throw new InvalidOperationException("Cannot add child to ASTElement");
        }

        if (contextIndex < 0 || contextIndex >= m_children.Length)
        {
            throw new InvalidOperationException("This node had no children");
        }
        m_children[contextIndex].Add(child);
        
        child.SetParent(this); //
    }

    public ASTElement GetChild(int context, int index)
    {
        // which context, which child in it
        return m_children[context][index];
    }
    
    
    private void SetParent(ASTElement parent) //
    {
        m_parent = parent;
        
    }
    
    public virtual string GenerateNodeName() => "_" + m_serial;
    
    public abstract T Accept<T>(ASTBaseVisitor<T> visitor);

    public IEnumerable<ASTElement> GetChildren()
    {
        for (int i = 0; i < m_children?.Length; i++)
        {
            foreach (var child in m_children[i])
            {
                yield return child;
            }
        }
    }
    
    public IEnumerable<ASTElement> GetChildren(int context)
    {
        if (m_children == null || context < 0 || context >= m_children.Length)
            yield break;
        foreach (var child in m_children[context])
            yield return child;
    }


}