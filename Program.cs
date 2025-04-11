using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Diagnostics;
using System.IO;
using SimpleCompiler.Analysis;
using SimpleCompiler.Ast;
using SimpleCompiler.CodeGen;

namespace SimpleCompiler
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Use your test file.
            const string fileName = "tests/files/test.minic";
            Test(fileName);
        }
    
        private static void Test(string fileName)
        {
            var antlrStream = CharStreams.fromPath(fileName);
            var lexer = new MINICLexer(antlrStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new MINICParser(tokens);
            var tree = parser.compileUnit();

            // Print parse tree structure.
            var stPrinter = new STPrinterVisitor();
            stPrinter.Visit(tree);
            RunCommand("dot -Tgif test.dot -o ST.gif");

            // Build the AST.
            var astVisitor = new ASTGenerator();
            astVisitor.Visit(tree);
            CCompileUnit root = astVisitor.GetRoot();
            var astPrinter = new ASTPrinterVisitor();
            astPrinter.PrintAst(root);
            RunCommand("dot -Tgif test_ast.dot -o AST.gif");
            
            // === New Logical Analysis Pass ===
            // Analyze the AST for logical errors using the local LLM.
            //var analyzer = new LogicalAnalyzer();
            // Since AnalyzeAsync is asynchronous, call it synchronously here.
            //analyzer.AnalyzeAsync(root).GetAwaiter().GetResult();
            // ===================================
            var bugAnalyzer = new BugDetectionAnalyzer();
            bugAnalyzer.AnalyzeSourceAsync("tests/files/test.minic").GetAwaiter().GetResult();
            
            // Code generation
            var codeGenVisitor = new CodeGenVisitor();
            root.Accept(codeGenVisitor);
            AsmFile asmFile = codeGenVisitor.GetAsmFile();

            // Emit assembly code to a file.
            using (StreamWriter sw = new StreamWriter("output.asm"))
            {
                asmFile.EmitToFile(sw);
            }
            Console.WriteLine("Assembly code generated in output.asm");

            
        }
        
        static void RunCommand(string command)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {command}";
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
                Console.WriteLine($"Command '{command}' executed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command '{command}': {ex.Message}");
            }
        }
    }
}
