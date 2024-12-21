using System.Collections;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;


namespace SimpleCompiler;

public class STPrinterVisitor : MINICBaseVisitor<int>
{
    StreamWriter m_STSpecFile = new("test.dot");
    Stack<string> m_parentsLabel = new();
    private static int ms_serialCounter = 0;
    
    public override int VisitCompileUnit(MINICParser.CompileUnitContext context)
    {
        var serial = ms_serialCounter++;
        var s = "CompileUnit_" + serial;
        m_parentsLabel.Push(s);
        m_STSpecFile.WriteLine("digraph G{");
        base.VisitChildren(context);
        m_STSpecFile.WriteLine("}");
        m_parentsLabel.Pop();
        
        m_STSpecFile.Close();

        return 0;
    }

    public override int VisitDeclaration(MINICParser.DeclarationContext context)
    {
        var label = "Declaration_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitInitializer(MINICParser.InitializerContext context)
    {
        var label = "Initializer_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitLocalDeclaration(MINICParser.LocalDeclarationContext context)
    {
        var label = "LocalDeclaration_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitFunctionDeclaration(MINICParser.FunctionDeclarationContext context)
    {
        var label = "FunctionDeclaration_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitFunctionDefinition(MINICParser.FunctionDefinitionContext context)
    {
        var label = "FunctionDefinition_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitTypeSpecifier(MINICParser.TypeSpecifierContext context)
    {
        var label = "TypeSpecifier_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitStatement(MINICParser.StatementContext context)
    {
        var label = "Statement_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitIfStatement(MINICParser.IfStatementContext context)
    {
        var label = "IfStatement_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitWhileStatement(MINICParser.WhileStatementContext context)
    {
        var label = "WhileStatement_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitBlock(MINICParser.BlockContext context)
    {
        var label = "Block_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitStatementList(MINICParser.StatementListContext context)
    {
        var label = "StatementList_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_LTE(MINICParser.Expr_LTEContext context)
    {
        var label = "LTE_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_LT(MINICParser.Expr_LTContext context)
    {
        var label = "LT_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }
    

    public override int VisitExpr_NEQUAL(MINICParser.Expr_NEQUALContext context)
    {
        var label = "NEQUAL_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_PLUS(MINICParser.Expr_PLUSContext context)
    {
        var label = "PLUS_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_EQUAL(MINICParser.Expr_EQUALContext context)
    {
        var label = "Equal_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_GT(MINICParser.Expr_GTContext context)
    {
        var label = "GT_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_PARENTHESIS(MINICParser.Expr_PARENTHESISContext context)
    {
        var label = "Parenthesis_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_MINUS(MINICParser.Expr_MINUSContext context)
    {
        var label = "MINUS_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_FCALL(MINICParser.Expr_FCALLContext context)
    {
        var label = "FCall_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_OR(MINICParser.Expr_ORContext context)
    {
        var label = "OR_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_DIVMULT(MINICParser.Expr_DIVMULTContext context)
    {
        var label = "";
        switch (context.op.Type)
        {
            case MINICLexer.MULT:
                label = "Multiplication_" + ms_serialCounter++;
                break;
            case MINICLexer.DIV:
                label = "Division_" + ms_serialCounter++;
                break;
        }
        //m_STSpecFile.WriteLine("\"{0}\"->\"{1}\";", m_parentsLabel.Peek(), label);
        //m_parentsLabel.Push(label);
        //base.VisitExpr_PLUSMINUS(context);
        //m_parentsLabel.Pop();
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_NOT(MINICParser.Expr_NOTContext context)
    {
        var label = "NOT_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_GTE(MINICParser.Expr_GTEContext context)
    {
        var label = "GTE_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_AND(MINICParser.Expr_ANDContext context)
    {
        var label = "AND_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitArgs(MINICParser.ArgsContext context)
    {
        var label = "Args_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitFargs(MINICParser.FargsContext context)
    {
        var label = "Fargs_" + ms_serialCounter++;
        m_STSpecFile.WriteLine("\"{0}\"->\"{1}\";", m_parentsLabel.Peek(), label);
        m_parentsLabel.Push(label);
        base.VisitFargs(context);
        m_parentsLabel.Pop();
        
        return 0;
    }

    public override int VisitExpr_PLUSMINUS(MINICParser.Expr_PLUSMINUSContext context)
    {
        var label = "";
        var serial = ms_serialCounter++;
        switch (context.op.Type)
        {
            case MINICLexer.PLUS:
                label = "Addition_" + serial;
                break;
            case MINICLexer.MINUS:
                label = "Subtraction_" + serial;
                break;
        }
        //m_STSpecFile.WriteLine("\"{0}\"->\"{1}\";", m_parentsLabel.Peek(), label);
        //m_parentsLabel.Push(label);
        //base.VisitExpr_PLUSMINUS(context);
        //m_parentsLabel.Pop();
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitExpr_ASSIGN(MINICParser.Expr_ASSIGNContext context)
    {
        var label = "Assignment_" + ms_serialCounter++;
        WritePushVisitPop(m_parentsLabel.Peek(), label, context);
        return 0;
    }

    public override int VisitTerminal(ITerminalNode node)
    {
        var label = "";
        var serial = ms_serialCounter++;
        switch (node.Symbol.Type)
        {
            case MINICLexer.IDENTIFIER:
                label = "Identifier_" + serial;
                m_STSpecFile.WriteLine("\"{0}\"->\"{1}\";", m_parentsLabel.Peek(), label);
                break;
            case MINICLexer.NUMBER:
                label = "Number_" + serial;
                m_STSpecFile.WriteLine("\"{0}\"->\"{1}\";", m_parentsLabel.Peek(), label);
                break;
            default:
                break;
        }
        
        return 0;
    }

    private void WritePushVisitPop(string parentLabel, string label, RuleContext context)
    {
        m_STSpecFile.WriteLine("\"{0}\"->\"{1}\";", parentLabel, label);
        m_parentsLabel.Push(label);
        base.VisitChildren(context);
        m_parentsLabel.Pop();
    }
}