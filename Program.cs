using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Diagnostics;

namespace SimpleCompiler
{
    class Program
    {
        public static void Main(string[] args)
        {
            // deprecated (but still works)
            //    var inputStream = new StreamReader(fileName);
            //    var antlrStream = new AntlrInputStream(inputStream);

            // fileName = args[0]
            const string fileName = "tests/files/test.minic";
            Test(fileName);
            
        }
    
        private static void Test(string fileName)
        {
            //var stack = new Stack<string>();
            
            var antlrStream = CharStreams.fromPath(fileName);
            var lexer = new MINICLexer(antlrStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new MINICParser(tokens);
            
            // CompileUnitContext or IParseTree, works with both.
            var tree = parser.compileUnit();
            
            // Console.WriteLine(tree.ToStringTree(parser));

            var stPrinter = new STPrinterVisitor();
            stPrinter.Visit(tree);
            RunCommand("dot -Tgif test.dot -o ST.gif");
        }
        
        static void RunCommand(string command)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe"; // For Windows
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
