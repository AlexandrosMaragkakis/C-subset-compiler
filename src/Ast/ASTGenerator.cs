using Antlr4.Runtime.Tree;

namespace SimpleCompiler.Ast;

public class ASTGenerator : MINICBaseVisitor<int> // <ASTElement> ???
{
    private CCompileUnit m_root;
    
    // The stack holds the current parent node and the context index where new children should be added
    Stack<(MINIC_ASTElement ParentNode,int ChildContext)> m_parents = new();
    
    public override int VisitCompileUnit(MINICParser.CompileUnitContext context)
    {
        m_root = new CCompileUnit();;
        m_parents.Push((m_root, CCompileUnit.CT_BODY));
        
        // Visit all children
        foreach (var child in context.children)
        {
            
            Visit(child);
        }

        // Done with compileUnit, pop the stack.
        m_parents.Pop();
        return 0;
        
    }
    
    /// <summary>
    /// functionDefinition : typeSpecifier IDENTIFIER LP fargs? RP block;
    /// </summary>
    public override int VisitFunctionDefinition(MINICParser.FunctionDefinitionContext context)
    {
        var returnType = context.typeSpecifier().GetText();
        var functionName = context.IDENTIFIER().GetText();
        var line = context.Start.Line;
        var column = context.Start.Column;
        
        var funcNode = new CFunctionDefinition(functionName, returnType, line, column);
    
        // Add to parent's context
        m_parents.Peek().Item1.AddChild(funcNode, m_parents.Peek().Item2);
    
        // Process parameters
        if (context.fargs() != null)
        {
            m_parents.Push((funcNode, CFunctionDefinition.CT_FARGS));
            Visit(context.fargs());
            m_parents.Pop();
        }

        // Process body
        m_parents.Push((funcNode, CFunctionDefinition.CT_BODY));
        Visit(context.block());
        m_parents.Pop();
    
        return 0;
    }

    /// <summary>
    /// functionDeclaration : typeSpecifier IDENTIFIER LP fargs? RP SEMICOLON;
    /// </summary>
    public override int VisitFunctionDeclaration(MINICParser.FunctionDeclarationContext context)
    {
        var returnType = context.typeSpecifier().GetText();
        var functionName = context.IDENTIFIER().GetText();
        var line = context.Start.Line;
        var column = context.Start.Column;

        var funcDecl = new CFunctionDeclaration(functionName, returnType, line, column);

        // Attach to the current parent.
        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(funcDecl, childContext);

        // fargs if present.
        if (context.fargs() != null)
        {
            m_parents.Push((funcDecl, CFunctionDeclaration.CT_PARAMETERS));
            Visit(context.fargs());
            m_parents.Pop();
        }

        return 0;
    }

    /// <summary>
    /// declaration : typeSpecifier IDENTIFIER initializer? SEMICOLON
    ///             | functionDeclaration;
    /// </summary>
    public override int VisitDeclaration(MINICParser.DeclarationContext context)
    {
        // If it's a function declaration, we handle that directly.
        if (context.functionDeclaration() != null)
        {
            // Just visit that subrule.
            return Visit(context.functionDeclaration());
        }
        else
        {
            // It's a global variable declaration: typeSpecifier IDENTIFIER initializer? SEMICOLON
            var line = context.Start.Line;
            var column = context.Start.Column;

            // Create the global declaration node.
            var globalDecl = new CGlobalDeclaration(line, column);

            // Attach to parent's child context.
            var (parentNode, childContext) = m_parents.Peek();
            parentNode.AddChild(globalDecl, childContext);

            // Push so we can visit the sub-parts.
            m_parents.Push((globalDecl, 0));

            // type specifier
            Visit(context.typeSpecifier());

            // identifier
            Visit(context.IDENTIFIER());

            // initializer if any
            if (context.initializer() != null)
            {
                Visit(context.initializer());
            }

            m_parents.Pop();
        }
        return 0;
    }

    /// <summary>
    /// initializer : ASSIGN expression;
    /// </summary>
    public override int VisitInitializer(MINICParser.InitializerContext context)
    {
        // We just visit the expression so it ends up as a child node.
        return Visit(context.expression());
    }

    /// <summary>
    /// typeSpecifier : INTEGER | DOUBLE;
    /// We could optionally create a specialized node to represent the type.
    /// </summary>
    public override int VisitTypeSpecifier(MINICParser.TypeSpecifierContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;

        var typeNode = new CTypeSpecifier(context.GetText(), line, column);

        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(typeNode, childContext);

        return 0;
    }

    /// <summary>
    /// statement : expression SEMICOLON
    ///           | ifStatement
    ///           | whileStatement
    ///           | block
    ///           | RETURN expression SEMICOLON
    ///           | BREAK SEMICOLON
    ///           | localDeclaration;
    /// We'll let base do its work, or handle each subrule.
    /// </summary>
    public override int VisitStatement(MINICParser.StatementContext context)
    {
        // The base visitor will dispatch to the correct specialized method.
        return base.VisitStatement(context);
    }

    /// <summary>
    /// localDeclaration : typeSpecifier IDENTIFIER (ASSIGN expression)? SEMICOLON;
    /// </summary>
    public override int VisitLocalDeclaration(MINICParser.LocalDeclarationContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var localDecl = new CLocalDeclaration(line, column);

        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(localDecl, childContext);

        // Push so we can store type, ident, and expression if present.
        m_parents.Push((localDecl, 0));
    
        // type specifier
        Visit(context.typeSpecifier());

        // identifier
        Visit(context.IDENTIFIER());

        // optional expression
        if (context.expression() != null)
        {
            Visit(context.expression());
        }
    
        m_parents.Pop();
        return 0;
    }

    /// <summary>
    /// ifStatement : IF LP expression RP block (ELSE block)?;
    /// </summary>
    public override int VisitIfStatement(MINICParser.IfStatementContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;

        var ifNode = new CIfStatement(line, column);
        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(ifNode, childContext);

        // Condition
        m_parents.Push((ifNode, CIfStatement.CT_CONDITION));
        Visit(context.expression());
        m_parents.Pop();

        // If body
        m_parents.Push((ifNode, CIfStatement.CT_BODY));
        Visit(context.block(0));
        m_parents.Pop();

        // Else body if present
        if (context.block().Length > 1)
        {
            m_parents.Push((ifNode, CIfStatement.CT_ELSE));
            Visit(context.block(1));
            m_parents.Pop();
        }

        return 0;
    }

    /// <summary>
    /// whileStatement : WHILE LP expression RP statement;
    /// </summary>
    public override int VisitWhileStatement(MINICParser.WhileStatementContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;

        var whileNode = new CWhileStatement(line, column);
        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(whileNode, childContext);

        // Condition
        m_parents.Push((whileNode, CWhileStatement.CT_CONDITION));
        Visit(context.expression());
        m_parents.Pop();

        // Body
        m_parents.Push((whileNode, CWhileStatement.CT_BODY));
        Visit(context.statement());
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// block : LCB statementList RCB;
    /// </summary>
    public override int VisitBlock(MINICParser.BlockContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;

        var blockNode = new CBlockStatement(line, column);
        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(blockNode, childContext);

        // The statementList is the subchild of block.
        m_parents.Push((blockNode, CBlockStatement.CT_BODY));
        Visit(context.statementList());
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// statementList : (statement)+
    /// </summary>
    public override int VisitStatementList(MINICParser.StatementListContext context)
    {
        foreach (var stmt in context.statement())
        {
            Visit(stmt);
        }
        return 0;
    }

    /// <summary>
    /// expression : expression op=(PLUS|MINUS) expression  # expr_PLUSMINUS
    /// </summary>
    public override int VisitExpr_PLUSMINUS(MINICParser.Expr_PLUSMINUSContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;

        var opText = context.op.Text; // '+' or '-'
        var op = (opText == "+") ? MINICType.BinaryOperator.Plus : MINICType.BinaryOperator.Minus;

        var binExpr = new CBinaryExpression(op, line, column);
        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(binExpr, childContext);

        // Left operand
        m_parents.Push((binExpr, CBinaryExpression.CT_LEFT));
        Visit(context.expression(0));
        m_parents.Pop();

        // Right operand
        m_parents.Push((binExpr, CBinaryExpression.CT_RIGHT));
        Visit(context.expression(1));
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// expression : expression LTE expression # expr_LTE
    /// </summary>
    public override int VisitExpr_LTE(MINICParser.Expr_LTEContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var binExpr = new CBinaryExpression(MINICType.BinaryOperator.LessThanOrEqual, line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(binExpr, childCtx);

        m_parents.Push((binExpr, CBinaryExpression.CT_LEFT));
        Visit(context.expression(0));
        m_parents.Pop();

        m_parents.Push((binExpr, CBinaryExpression.CT_RIGHT));
        Visit(context.expression(1));
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// expression : IDENTIFIER ASSIGN expression  # expr_ASSIGN
    /// </summary>
    public override int VisitExpr_ASSIGN(MINICParser.Expr_ASSIGNContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;

        var assignExpr = new CAssignmentExpression(line, column);
        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(assignExpr, childContext);

        // Left side (IDENTIFIER)
        m_parents.Push((assignExpr, CAssignmentExpression.CT_LEFT));
        Visit(context.IDENTIFIER());
        m_parents.Pop();

        // Right side (expression)
        m_parents.Push((assignExpr, CAssignmentExpression.CT_RIGHT));
        Visit(context.expression());
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// expression : expression LT expression # expr_LT
    /// </summary>
    public override int VisitExpr_LT(MINICParser.Expr_LTContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var binExpr = new CBinaryExpression(MINICType.BinaryOperator.LessThan, line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(binExpr, childCtx);

        // left
        m_parents.Push((binExpr, CBinaryExpression.CT_LEFT));
        Visit(context.expression(0));
        m_parents.Pop();

        // right
        m_parents.Push((binExpr, CBinaryExpression.CT_RIGHT));
        Visit(context.expression(1));
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// expression : expression NEQUAL expression # expr_NEQUAL
    /// </summary>
    public override int VisitExpr_NEQUAL(MINICParser.Expr_NEQUALContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var binExpr = new CBinaryExpression(MINICType.BinaryOperator.NotEqual, line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(binExpr, childCtx);

        m_parents.Push((binExpr, CBinaryExpression.CT_LEFT));
        Visit(context.expression(0));
        m_parents.Pop();

        m_parents.Push((binExpr, CBinaryExpression.CT_RIGHT));
        Visit(context.expression(1));
        m_parents.Pop();
        return 0;
    }

    /// <summary>
    /// expression : PLUS expression # expr_PLUS
    /// unary plus
    /// </summary>
    public override int VisitExpr_PLUS(MINICParser.Expr_PLUSContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;

        var unExpr = new CUnaryExpression(MINICType.UnaryOperator.Plus, line, column);
        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(unExpr, childContext);

        m_parents.Push((unExpr, CUnaryExpression.CT_OPERAND));
        Visit(context.expression());
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// expression : expression EQUAL expression # expr_EQUAL
    /// </summary>
    public override int VisitExpr_EQUAL(MINICParser.Expr_EQUALContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var binExpr = new CBinaryExpression(MINICType.BinaryOperator.EqualEqual, line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(binExpr, childCtx);

        m_parents.Push((binExpr, CBinaryExpression.CT_LEFT));
        Visit(context.expression(0));
        m_parents.Pop();

        m_parents.Push((binExpr, CBinaryExpression.CT_RIGHT));
        Visit(context.expression(1));
        m_parents.Pop();
        return 0;
    }

    /// <summary>
    /// expression : expression GT expression # expr_GT
    /// </summary>
    public override int VisitExpr_GT(MINICParser.Expr_GTContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var binExpr = new CBinaryExpression(MINICType.BinaryOperator.GreaterThan, line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(binExpr, childCtx);

        m_parents.Push((binExpr, CBinaryExpression.CT_LEFT));
        Visit(context.expression(0));
        m_parents.Pop();

        m_parents.Push((binExpr, CBinaryExpression.CT_RIGHT));
        Visit(context.expression(1));
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// expression : MINUS expression # expr_MINUS
    /// unary minus
    /// </summary>
    public override int VisitExpr_MINUS(MINICParser.Expr_MINUSContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var unExpr = new CUnaryExpression(MINICType.UnaryOperator.Minus, line, column);
        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(unExpr, childContext);

        m_parents.Push((unExpr, CUnaryExpression.CT_OPERAND));
        Visit(context.expression());
        m_parents.Pop();
        return 0;
    }

    /// <summary>
    /// expression : IDENTIFIER LP args RP # expr_FCALL
    /// </summary>
    public override int VisitExpr_FCALL(MINICParser.Expr_FCALLContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var functionName = context.IDENTIFIER().GetText();

        var callExpr = new CFunctionCallExpression(functionName, line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(callExpr, childCtx);

        // If there's an args rule, push.
        m_parents.Push((callExpr, CFunctionCallExpression.CT_ARGS));
        if (context.args() != null)
        {
            Visit(context.args());
        }
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// expression : expression OR expression # expr_OR
    /// </summary>
    public override int VisitExpr_OR(MINICParser.Expr_ORContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var binExpr = new CBinaryExpression(MINICType.BinaryOperator.Or, line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(binExpr, childCtx);

        m_parents.Push((binExpr, CBinaryExpression.CT_LEFT));
        Visit(context.expression(0));
        m_parents.Pop();

        m_parents.Push((binExpr, CBinaryExpression.CT_RIGHT));
        Visit(context.expression(1));
        m_parents.Pop();
        return 0;
    }

    /// <summary>
    /// expression : expression (DIV|MULT) expression # expr_DIVMULT
    /// </summary>
    public override int VisitExpr_DIVMULT(MINICParser.Expr_DIVMULTContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var opText = context.op.Text; // '/' or '*'
        var op = (opText == "/") ? MINICType.BinaryOperator.Divide : MINICType.BinaryOperator.Multiply;

        var binExpr = new CBinaryExpression(op, line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(binExpr, childCtx);

        // Left
        m_parents.Push((binExpr, CBinaryExpression.CT_LEFT));
        Visit(context.expression(0));
        m_parents.Pop();

        // Right
        m_parents.Push((binExpr, CBinaryExpression.CT_RIGHT));
        Visit(context.expression(1));
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// expression : NOT expression # expr_NOT
    /// </summary>
    public override int VisitExpr_NOT(MINICParser.Expr_NOTContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var unExpr = new CUnaryExpression(MINICType.UnaryOperator.Not, line, column);
        var (parentNode, childContext) = m_parents.Peek();
        parentNode.AddChild(unExpr, childContext);

        m_parents.Push((unExpr, CUnaryExpression.CT_OPERAND));
        Visit(context.expression());
        m_parents.Pop();
        return 0;
    }

    /// <summary>
    /// expression : expression GTE expression # expr_GTE
    /// </summary>
    public override int VisitExpr_GTE(MINICParser.Expr_GTEContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var binExpr = new CBinaryExpression(MINICType.BinaryOperator.GreaterThanOrEqual, line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(binExpr, childCtx);

        m_parents.Push((binExpr, CBinaryExpression.CT_LEFT));
        Visit(context.expression(0));
        m_parents.Pop();

        m_parents.Push((binExpr, CBinaryExpression.CT_RIGHT));
        Visit(context.expression(1));
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// expression : expression AND expression # expr_AND
    /// </summary>
    public override int VisitExpr_AND(MINICParser.Expr_ANDContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var binExpr = new CBinaryExpression(MINICType.BinaryOperator.And, line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(binExpr, childCtx);

        // left
        m_parents.Push((binExpr, CBinaryExpression.CT_LEFT));
        Visit(context.expression(0));
        m_parents.Pop();

        // right
        m_parents.Push((binExpr, CBinaryExpression.CT_RIGHT));
        Visit(context.expression(1));
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// args : (expression (COMMA expression)*)?;
    /// We'll create an NT_ARGS node to store them.
    /// </summary>
    public override int VisitArgs(MINICParser.ArgsContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var argsNode = new CArgs(line, column);
        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(argsNode, childCtx);

        // Now push so that all expressions become children.
        m_parents.Push((argsNode, 0));
        foreach (var expr in context.expression())
        {
            Visit(expr);
        }
        m_parents.Pop();

        return 0;
    }

    /// <summary>
    /// fargs : (typeSpecifier IDENTIFIER (COMMA typeSpecifier IDENTIFIER)*)?
    /// We'll build a CFargs node that holds children.
    /// For each parameter we create a CParameter.
    /// </summary>
    public override int VisitFargs(MINICParser.FargsContext context)
    {
        var line = context.Start.Line;
        var column = context.Start.Column;
        var fargsNode = new CFargs(line, column);

        var (parentNode, childCtx) = m_parents.Peek();
        parentNode.AddChild(fargsNode, childCtx);

        // If there are parameters, we create them.
        m_parents.Push((fargsNode, CFargs.CT_PARAMETERS));

        int paramCount = context.typeSpecifier().Length;
        for (int i = 0; i < paramCount; i++)
        {
            var ts = context.typeSpecifier()[i];
            var id = context.IDENTIFIER()[i];

            var pLine = id.Symbol.Line;
            var pCol = id.Symbol.Column;

            var param = new CParameter(id.GetText(), ts.GetText(), pLine, pCol);
            fargsNode.AddChild(param, CFargs.CT_PARAMETERS);
        }

        m_parents.Pop();
        return 0;
    }

    /// <summary>
    /// Called for any terminal node. We only specifically handle IDENTIFIER and NUMBER here.
    /// Others remain as-is.
    /// </summary>
    public override int VisitTerminal(ITerminalNode node)
    {
        switch (node.Symbol.Type)
        {
            case MINICLexer.NUMBER:
            {
                var numberNode = new CNumberLiteral(node.GetText(), node.Symbol.Line, node.Symbol.Column);
                var (parentNode, childCtx) = m_parents.Peek();
                parentNode.AddChild(numberNode, childCtx);
                break;
            }
            case MINICLexer.IDENTIFIER:
            {
                var idNode = new CIdentifier(node.GetText(), node.Symbol.Line, node.Symbol.Column);
                var (parentNode, childCtx) = m_parents.Peek();
                parentNode.AddChild(idNode, childCtx);
                break;
            }
        }
        return 0;
    }

    /// <summary>
    /// Access the final AST.
    /// </summary>
    public CCompileUnit GetRoot()
    {
        return m_root;
    }
}