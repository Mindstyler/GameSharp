using GameSharp;

string expression = "5 ^ 3 + (8 * 2)";

GameSharpCompiler.Tokenize(expression);
List<string> postfix = GameSharpCompiler.Parse();

AssemblyBuilder.BuildApplication(postfix);