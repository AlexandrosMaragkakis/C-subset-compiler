using System;
using System.Collections.Generic;
using System.IO;
using SimpleCompiler.Ast; // Adjust if your AST classes are in a different namespace.

namespace SimpleCompiler
{
    /// <summary>
    /// A visitor-like class to print the MiniC AST in DOT format.
    /// </summary>
    public class ASTPrinterVisitor
    {
        private StreamWriter _dotFileWriter;
        private readonly Stack<string> _parentLabels = new Stack<string>();

        // Counter for creating unique labels in the .dot graph.
        private static int _serialCounter;

        /// <summary>
        /// Generates a Graphviz DOT file (test_ast.dot) from the given AST root.
        /// You can visualize it with Graphviz (e.g., "dot -Tpng test_ast.dot > ast.png").
        /// </summary>
        /// <param name="root">The AST root node (e.g., a CCompileUnit).</param>
        public void PrintAst(MINIC_ASTElement root)
        {
            // Open a new .dot file
            using (_dotFileWriter = new StreamWriter("test_ast.dot"))
            {
                _dotFileWriter.WriteLine("digraph G {");

                // Create a label for the root node and define it
                string rootLabel = CreateNodeLabel(root);

                // Push the root label onto the stack but there's no parent to connect from yet.
                _parentLabels.Push(rootLabel);

                // Recursively visit all children
                VisitNode(root, rootLabel);

                // End the dot graph
                _dotFileWriter.WriteLine("}");
            }
        }

        /// <summary>
        /// Recursively visits an AST node, printing edges to its children.
        /// </summary>
        /// <param name="node">The current node to visit.</param>
        /// <param name="currentLabel">The unique label of the current node in the DOT graph.</param>
        private void VisitNode(MINIC_ASTElement node, string currentLabel)
        {
            // This enumerates *all* children across *all* contexts.
            foreach (ASTElement childElement in node.GetChildren())
            {
                // childElement is an ASTElement, cast to MINIC_ASTElement so we can access NodeType, etc.
                if (childElement is MINIC_ASTElement child)
                {
                    // 1) Create a unique label for the child
                    string childLabel = CreateNodeLabel(child);

                    // 2) Print an edge parent->child
                    _dotFileWriter.WriteLine("\"{0}\" -> \"{1}\";", currentLabel, childLabel);

                    // 3) Recurse
                    VisitNode(child, childLabel);
                }
            }
        }


        /// <summary>
        /// Creates a unique label for a given AST node, including some display text.
        /// </summary>
        private string CreateNodeLabel(MINIC_ASTElement node)
        {
            int serial = _serialCounter++;

            // E.g. "NT_FUNCTIONDEFINITION_0".
            string nodeType = node.NodeType.ToString();
            string nodeLabel = nodeType + "_" + serial;

            // Prepare the text shown inside the node in Graphviz.
            // We can show NodeType, line/column, or any other property.
            string displayText = $"{nodeType}\\n(Line={node.Line},Col={node.Column})";

            // Write a definition for this label:  label="<displayText>"
            // Escape newlines with \\n if needed.
            _dotFileWriter.WriteLine("\"{0}\" [label=\"{1}\"];", nodeLabel, displayText);

            return nodeLabel;
        }
    }
}
