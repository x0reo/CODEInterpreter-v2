using System.Text;
using Antlr4.Runtime;
using Interpreter;
using Interpreter.Content;

// var path = Path.Combine(Directory.GetCurrentDirectory(), "../../..");
// Directory.SetCurrentDirectory(Path.GetFullPath(path));
// var contents = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Content/test.ci"));
var fileName = "Content/test.ci";
var contents = File.ReadAllText(fileName);

var program = contents.Trim();
if (!program.StartsWith("BEGIN CODE") || !program.EndsWith("END CODE"))
{
    throw new Exception("Must start with 'BEGIN CODE' and end with 'END CODE'");
}

var inputStream = new AntlrInputStream(contents);
var codeLexer = new CODELexer(inputStream);
var commonTokenStream = new CommonTokenStream(codeLexer);
var codeParser = new CODEParser(commonTokenStream);

var codeContext = codeParser.program();
var visitor = new CodeVisitor();

visitor.Visit(codeContext);

