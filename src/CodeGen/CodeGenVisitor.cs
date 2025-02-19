using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCompiler.Ast;
using SimpleCompiler.CodeGen;

namespace SimpleCompiler.CodeGen
{
    public class CodeGenVisitor : MINICBaseASTVisitor<AsmEmittableCodeContainer>
    {
        // The top-level assembly file container.
        private AsmFile asmFile = new AsmFile();

        // Keep track of the current function (if inside one)
        private AsmFunctionDefinition currentFunction = null;

        /// <summary>
        /// Returns the completed assembly file container.
        /// </summary>
        public AsmFile GetAsmFile() => asmFile;

        public override AsmEmittableCodeContainer VisitCompileUnit(CCompileUnit node)
        {
            // Visit each child of the compile unit.
            // Global declarations and function definitions will be added to the appropriate file context.
            foreach (var child in node.GetChildren())
            {
                var code = child.Accept(this);
                if (child is CFunctionDefinition)
                {
                    asmFile.AddCode(code, AsmCodeContextType.ACC_FILE_CODE);
                }
                else if (child is CGlobalDeclaration)
                {
                    asmFile.AddCode(code, AsmCodeContextType.ACC_FILE_DATA);
                }
                else
                {
                    // Default: add to code section.
                    asmFile.AddCode(code, AsmCodeContextType.ACC_FILE_CODE);
                }
            }
            return asmFile;
        }

        public override AsmEmittableCodeContainer VisitFunctionDefinition(CFunctionDefinition node)
        {
            // Create a function container with the function name and return type.
            AsmFunctionDefinition funcContainer = new AsmFunctionDefinition(asmFile, node.FunctionName, node.TypeSpecifier);
            // Set current function context.
            AsmFunctionDefinition prevFunction = currentFunction;
            currentFunction = funcContainer;

            // Process parameters (assuming they are contained in a CFargs node).
            foreach (var child in node.GetChildren())
            {
                if (child is CFargs)
                {
                    foreach (var param in child.GetChildren())
                    {
                        // Process each parameter so that VisitParameter registers it.
                        param.Accept(this);
                    }
                }
            }

            // Process the rest of the function body (skip parameters).
            foreach (var child in node.GetChildren())
            {
                if (!(child is CFargs))
                {
                    var code = child.Accept(this);
                    funcContainer.AddCode(code, AsmCodeContextType.ACC_FUNCTION_BODY);
                }
            }

            // Restore previous function context.
            currentFunction = prevFunction;
            return funcContainer;
        }


        public override AsmEmittableCodeContainer VisitBlock(CBlockStatement node)
        {
            // A block is compiled into a compound statement.
            AsmCompoundStatement compound = new AsmCompoundStatement(asmFile);
            foreach (var child in node.GetChildren())
            {
                var stmtCode = child.Accept(this);
                compound.AddCode(stmtCode, AsmCodeContextType.ACC_COMPOUND_BODY);
            }
            return compound;
        }

        public override AsmEmittableCodeContainer VisitGlobalDeclaration(CGlobalDeclaration node)
        {
            // For a global declaration, assume that its children include a type specifier and an identifier.
            string varName = "";
            string initVal = null;
            foreach (var child in node.GetChildren())
            {
                if (child is CIdentifier id)
                {
                    varName = id.Name;
                    //break;
                }
                else if (child is CNumberLiteral num)
                {
                    initVal = num.Value;
                    //break;
                }
            }
            if (!string.IsNullOrEmpty(varName))
            {
                asmFile.DeclareGlobalVariable(varName, initVal);
            }
            // Return an empty container.
            return new AsmCodeContainer(AsmCodeBlockType.ACB_NA, asmFile);
        }

        public override AsmEmittableCodeContainer VisitLocalDeclaration(CLocalDeclaration node)
{
    // For a local declaration, determine the variable name and an optional initializer.
    string varName = "";
    // Container to hold initializer code (if any)
    AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_ASSIGNMENT, asmFile);
    bool hasInitializer = false;

    foreach (var child in node.GetChildren())
    {
        if (child is CIdentifier id)
        {
            varName = id.Name;
        }
        else if (child is CNumberLiteral num)
        {
            // Handle numeric initializer.
            container.AddCode($"mov eax, {num.Value}", AsmCodeContextType.ACC_ASSIGNMENT);
            hasInitializer = true;
        }
        else if (child is CFunctionCallExpression fc)
        {
            // Handle function call initializer.
            var callCode = fc.Accept(this);
            container.AddCode(callCode, AsmCodeContextType.ACC_ASSIGNMENT);
            hasInitializer = true;
        }
        else if (child is CAssignmentExpression assign)
        {
            // Handle assignment expression initializer.
            var initCode = assign.Accept(this);
            container.AddCode(initCode, AsmCodeContextType.ACC_ASSIGNMENT);
            hasInitializer = true;
        }
        else if (child is CBinaryExpression binExpr)
        {
            // Handle binary expression initializer (e.g., a + b)
            var binCode = binExpr.Accept(this);
            container.AddCode(binCode, AsmCodeContextType.ACC_ASSIGNMENT);
            hasInitializer = true;
        }
    }

    // Declare the variable in the current function.
    if (currentFunction != null && !string.IsNullOrEmpty(varName))
    {
        currentFunction.DeclareVariable(varName);
        // If an initializer exists, store its result (in EAX) into the variable's stack slot.
        if (hasInitializer && currentFunction.TryGetLocalVariableOffset(varName, out int offset))
        {
            string offsetStr = offset < 0 ? $"- {Math.Abs(offset)}" : $"+ {offset}";
            container.AddCode($"mov [ebp {offsetStr}], eax", AsmCodeContextType.ACC_ASSIGNMENT);
        }
    }
    return container;
}




        public override AsmEmittableCodeContainer VisitAssignment(CAssignmentExpression node)
        {
            // For assignment, assume first child is the identifier (left-hand side)
            // and the second child is the expression (right-hand side).
            AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_ASSIGNMENT, asmFile);
            var children = node.GetChildren().ToList();

            string varName = "";
            if (children.Count > 0 && children[0] is CIdentifier id)
            {
                varName = id.Name;
            }
            if (children.Count > 1)
            {
                var rhsCode = children[1].Accept(this);
                container.AddCode(rhsCode, AsmCodeContextType.ACC_ASSIGNMENT);
            }
    
            // If inside a function and the variable is local, use a stack reference.
            if (currentFunction != null && currentFunction.TryGetLocalVariableOffset(varName, out int offset))
            {
                string offsetStr = offset < 0 ? $"- {Math.Abs(offset)}" : $"+ {offset}";
                container.AddCode($"mov [ebp {offsetStr}], eax", AsmCodeContextType.ACC_ASSIGNMENT);
            }
            else
            {
                container.AddCode("mov " + varName + ", eax", AsmCodeContextType.ACC_ASSIGNMENT);
            }
            return container;
        }


        public override AsmEmittableCodeContainer VisitBinaryExpression(CBinaryExpression node)
        {
            AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_EXPRESSION, asmFile);
            var children = node.GetChildren().ToList();
            if (children.Count >= 2)
            {
                // Evaluate left operand.
                var leftCode = children[0].Accept(this);
                container.AddCode(leftCode, AsmCodeContextType.ACC_EXPRESSION);
                container.AddCode("push eax", AsmCodeContextType.ACC_EXPRESSION);

                // Evaluate right operand.
                var rightCode = children[1].Accept(this);
                container.AddCode(rightCode, AsmCodeContextType.ACC_EXPRESSION);
                container.AddCode("pop ebx", AsmCodeContextType.ACC_EXPRESSION);  // EBX now holds left operand; EAX holds right operand

                switch (node.Operator)
                {
                    case MINICType.BinaryOperator.Plus:
                        // Addition is commutative: right + left
                        container.AddCode("add eax, ebx", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.Minus:
                        // We want left - right.
                        // Save right operand (in EAX) into ECX, restore left operand from EBX, then subtract.
                        container.AddCode("mov ecx, eax", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("mov eax, ebx", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("sub eax, ecx", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.Multiply:
                        // Multiply: left * right.
                        container.AddCode("mov ecx, eax", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("mov eax, ebx", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("imul eax, ecx", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.Divide:
                        // We want left / right.
                        // Save right operand (in EAX) into ECX, restore left operand from EBX,
                        // clear edx, and perform signed division.
                        container.AddCode("mov ecx, eax", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("mov eax, ebx", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("mov edx, 0", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("idiv ecx", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.LessThan:
                        container.AddCode("cmp ebx, eax", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("setl al", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("movzx eax, al", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.GreaterThan:
                        container.AddCode("cmp ebx, eax", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("setg al", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("movzx eax, al", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.LessThanOrEqual:
                        container.AddCode("cmp ebx, eax", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("setle al", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("movzx eax, al", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.GreaterThanOrEqual:
                        container.AddCode("cmp ebx, eax", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("setge al", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("movzx eax, al", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.EqualEqual:
                        container.AddCode("cmp ebx, eax", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("sete al", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("movzx eax, al", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.NotEqual:
                        container.AddCode("cmp ebx, eax", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("setne al", AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("movzx eax, al", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.Or:
                        container.AddCode("or eax, ebx", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    case MINICType.BinaryOperator.And:
                        container.AddCode("and eax, ebx", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                    default:
                        container.AddCode("; unknown binary operator", AsmCodeContextType.ACC_EXPRESSION);
                        break;
                }
            }
            return container;
        }






        public override AsmEmittableCodeContainer VisitUnaryExpression(CUnaryExpression node)
        {
            AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_EXPRESSION, asmFile);
            // Evaluate operand.
            var operand = node.GetChildren().FirstOrDefault();
            if (operand != null)
            {
                var operandCode = operand.Accept(this);
                container.AddCode(operandCode, AsmCodeContextType.ACC_EXPRESSION);
            }
            // Apply the unary operator.
            switch (node.Operator)
            {
                case MINICType.UnaryOperator.Plus:
                    // No additional code required.
                    break;
                case MINICType.UnaryOperator.Minus:
                    container.AddCode("neg eax", AsmCodeContextType.ACC_EXPRESSION);
                    break;
                case MINICType.UnaryOperator.Not:
                    container.AddCode("not eax", AsmCodeContextType.ACC_EXPRESSION);
                    break;
            }
            return container;
        }

        public override AsmEmittableCodeContainer VisitFunctionCall(CFunctionCallExpression node)
        {
            AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_EXPRESSION, asmFile);
            foreach (var child in node.GetChildren())
            {
                if (child is CArgs args)
                {
                    // Evaluate and push arguments in reverse order.
                    foreach (var arg in args.GetChildren().Reverse())
                    {
                        var argCode = arg.Accept(this);
                        container.AddCode(argCode, AsmCodeContextType.ACC_EXPRESSION);
                        container.AddCode("push eax", AsmCodeContextType.ACC_EXPRESSION);
                    }
                }
            }
            container.AddCode("call " + node.FunctionName, AsmCodeContextType.ACC_EXPRESSION);
            // Clean up the stack (assume 4 bytes per argument).
            int argCount = 0;
            foreach (var child in node.GetChildren())
            {
                if (child is CArgs args)
                {
                    argCount = args.GetChildren().Count();
                    break;
                }
            }
            if (argCount > 0)
            {
                container.AddCode("add esp, " + (argCount * 4), AsmCodeContextType.ACC_EXPRESSION);
            }
            return container;
        }


        public override AsmEmittableCodeContainer VisitReturn(CReturnStatement node)
        {
            // For a return statement, evaluate the return expression (if any)
            // and then generate the epilogue.
            AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_RETURN, asmFile);
            foreach (var child in node.GetChildren())
            {
                var exprCode = child.Accept(this);
                container.AddCode(exprCode, AsmCodeContextType.ACC_RETURN);
            }
            // The AsmReturnStatement composite (if used) would handle moving eax and issuing ret.
            // Here we simply add a placeholder.
            container.AddCode("ret", AsmCodeContextType.ACC_RETURN);
            return container;
        }

        public override AsmEmittableCodeContainer VisitIfStatement(CIfStatement node)
        {
            // Create an if-statement composite container.
            AsmIfStatement ifContainer = new AsmIfStatement(asmFile);
            var children = node.GetChildren().ToList();
            if (children.Count > 0)
            {
                // First child: condition.
                var condCode = children[0].Accept(this);
                ifContainer.AddCode(condCode, AsmCodeContextType.ACC_IF_CONDITION);
            }
            if (children.Count > 1)
            {
                // Second child: then-block.
                var thenCode = children[1].Accept(this);
                ifContainer.AddCode(thenCode, AsmCodeContextType.ACC_IF_THEN);
            }
            if (children.Count > 2)
            {
                // Third child: else-block.
                var elseCode = children[2].Accept(this);
                ifContainer.AddCode(elseCode, AsmCodeContextType.ACC_IF_ELSE);
            }
            return ifContainer;
        }

        public override AsmEmittableCodeContainer VisitWhileStatement(CWhileStatement node)
        {
            // Create a while-statement composite container.
            AsmWhileStatement whileContainer = new AsmWhileStatement(asmFile);
            var children = node.GetChildren().ToList();
            if (children.Count > 0)
            {
                var condCode = children[0].Accept(this);
                whileContainer.AddCode(condCode, AsmCodeContextType.ACC_WHILE_CONDITION);
            }
            if (children.Count > 1)
            {
                var bodyCode = children[1].Accept(this);
                whileContainer.AddCode(bodyCode, AsmCodeContextType.ACC_WHILE_BODY);
            }
            return whileContainer;
        }

        public override AsmEmittableCodeContainer VisitTypeSpecifier(CTypeSpecifier node)
        {
            // Type specifiers do not generate code.
            return new AsmCodeContainer(AsmCodeBlockType.ACB_NA, asmFile);
        }

        public override AsmEmittableCodeContainer VisitBreakStatement(CBreakStatement node)
        {
            // Break statements are not fully implemented in this example.
            AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_NA, asmFile);
            container.AddCode("; break statement not implemented", AsmCodeContextType.ACC_NA);
            return container;
        }

        public override AsmEmittableCodeContainer VisitParameter(CParameter node)
        {
            if (currentFunction != null)
            {
                // Calculate offset: first parameter at [ebp+8], then [ebp+12], etc.
                int offset = 8 + currentFunction.Parameters.Count * 4;
                currentFunction.AddParameter(node.Name, offset);
            }
            return new AsmCodeContainer(AsmCodeBlockType.ACB_NA, asmFile);
        }



        public override AsmEmittableCodeContainer VisitFArgs(CFargs node)
        {
            // Function argument declarations do not generate code.
            return new AsmCodeContainer(AsmCodeBlockType.ACB_NA, asmFile);
        }

        public override AsmEmittableCodeContainer VisitArgs(CArgs node)
        {
            // Function call arguments are handled during the function call.
            return new AsmCodeContainer(AsmCodeBlockType.ACB_NA, asmFile);
        }

        public override AsmEmittableCodeContainer VisitNumberLiteral(CNumberLiteral node)
        {
            AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_EXPRESSION, asmFile);
            container.AddCode("mov eax, " + node.Value, AsmCodeContextType.ACC_EXPRESSION);
            return container;
        }
        
        
        public override AsmEmittableCodeContainer VisitIdentifier(CIdentifier node)
        {
            AsmCodeContainer container = new AsmCodeContainer(AsmCodeBlockType.ACB_EXPRESSION, asmFile);
            if (currentFunction != null && currentFunction.TryGetLocalVariableOffset(node.Name, out int localOffset))
            {
                // Use local variable reference.
                string offsetStr = localOffset < 0 ? $"- {Math.Abs(localOffset)}" : $"+ {localOffset}";
                container.AddCode($"mov eax, [ebp {offsetStr}]", AsmCodeContextType.ACC_EXPRESSION);
            }
            else if (currentFunction != null && currentFunction.TryGetParameterOffset(node.Name, out int paramOffset))
            {
                // Reference the function parameter using its positive offset.
                container.AddCode($"mov eax, [ebp+{paramOffset}]", AsmCodeContextType.ACC_EXPRESSION);
            }
            else
            {
                // Otherwise, assume it's a global variable.
                container.AddCode("mov eax, " + node.Name, AsmCodeContextType.ACC_EXPRESSION);
            }
            return container;
        }


    }
}
