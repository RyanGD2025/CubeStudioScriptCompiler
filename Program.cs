using System;
using System.Collections.Generic;
using CubeStudioScriptCompiler;
using System.Linq;
using System.IO; // Necessário para operações de arquivo
using System.IO.Compression; // Necessário para criar o ZIP

class Program
{
    // =================================================================
    // FUNÇÃO AUXILIAR PARA IMPRIMIR A ÁRVORE (AST)
    // =================================================================
    static void PrintAST(AstNode node, int indent = 0)
    {
        string prefix = new string(' ', indent * 4);
        
        // Nó Literal (Valor simples: 10, "texto", true)
        if (node is LiteralNode literal)
        {
            Console.WriteLine($"{prefix}[Literal] Tipo: {literal.Tipo}, Valor: '{literal.Valor}'");
        }
        // Nó de Expressão Binária (A + B, X == Y)
        else if (node is BinaryExpressionNode binary)
        {
            Console.WriteLine($"{prefix}[Expressao Binaria] Operador: {binary.Operator}");
            PrintAST(binary.Left, indent + 1); // Lado Esquerdo
            PrintAST(binary.Right, indent + 1); // Lado Direito
        }
        // Nó de Bloco ({ ... })
        else if (node is BlockNode block)
        {
            Console.WriteLine($"{prefix}[Bloco] Inicio:");
            foreach (var statement in block.Statements)
            {
                PrintAST(statement, indent + 1);
            }
            Console.WriteLine($"{prefix}[Bloco] Fim.");
        }
        // Nó de Declaração Local (local x = ...)
        else if (node is LocalDeclarationNode localDecl)
        {
            Console.WriteLine($"{prefix}[Declaracao Local] Nome: '{localDecl.Name}'");
            if (localDecl.InitialValue != null)
            {
                Console.WriteLine($"{prefix}  Valor Inicial:");
                PrintAST(localDecl.InitialValue, indent + 2);
            }
        }
        // Nó de Estrutura Condicional (if/else)
        else if (node is IfStatementNode ifStmt)
        {
            Console.WriteLine($"{prefix}[IF] Condicao:");
            PrintAST(ifStmt.Condition, indent + 1);
            Console.WriteLine($"{prefix}[IF] Bloco (True):");
            PrintAST(ifStmt.IfBlock, indent + 1);
            if (ifStmt.ElseBlock != null)
            {
                Console.WriteLine($"{prefix}[ELSE] Bloco:");
                PrintAST(ifStmt.ElseBlock, indent + 1);
            }
        }
        // Nó de Loop (while)
        else if (node is WhileStatementNode whileStmt)
        {
            Console.WriteLine($"{prefix}[WHILE] Condicao:");
            PrintAST(whileStmt.Condition, indent + 1);
            Console.WriteLine($"{prefix}[WHILE] Bloco:");
            PrintAST(whileStmt.LoopBlock, indent + 1);
        }
        // Nó de Chamada de Função (print.log.Console)
        else if (node is CallStatementNode callStmt)
        {
            Console.WriteLine($"{prefix}[Chamada de Funcao] Caminho: {string.Join(".", callStmt.Path)}");
            Console.WriteLine($"{prefix}  Argumentos ({callStmt.Arguments.Count}):");
            foreach (var arg in callStmt.Arguments)
            {
                PrintAST(arg, indent + 2);
            }
        }
        // Outros nós (Function, Return, Class) seriam adicionados aqui
    }

    // =================================================================
    // MAIN: Ponto de Entrada do Compilador
    // =================================================================
    static void Main(string[] args)
    {
        // Dentro de static void Main(string[] args)
static void Main(string[] args)
{
    // ... (restante da inicialização)

    const string codigoCsscript =
        "function iniciar(x) {" +
        "  local vidas = 10 * x;" +
        "  if (vidas == 0) {" +
        "    print.log.Console(\"Fim de Jogo\");" +
        "    return false;" +
        "  } else {" +
        "    Sprite.Pos(vidas, 50);" +
        "  }" +
        "  return true;" +
        "}" +
        "local loop = iniciar(10);";

    // ... (Inicialização do Lexer e Parser)

    try
    {
        // 1. PARSE: Analisa o código e cria a AST
        var ast = parser.ParseProgram();
        Console.WriteLine("1. ANÁLISE SINTÁTICA CONCLUÍDA. AST GERADA.");

        // 2. SEMÂNTICA: Verifica o sentido e constrói a Tabela de Símbolos
        var semanticAnalyzer = new SemanticAnalyzer();
        semanticAnalyzer.Analyze(ast); // Certifique-se de ter o SemanticAnalyzer.cs completo
        Console.WriteLine("2. ANÁLISE SEMÂNTICA CONCLUÍDA. CÓDIGO VÁLIDO.");

        // 3. GERAÇÃO DE CÓDIGO: Cria o runtime.js
        var codeGenerator = new CodeGenerator();
        string runtimeJsContent = codeGenerator.Generate(ast);
        
        Console.WriteLine("\n3. GERAÇÃO DE CÓDIGO CONCLUÍDA.");
        Console.WriteLine("=======================================");
        Console.WriteLine("// CONTEÚDO FINAL DO ARQUIVO: runtime.js");
        Console.WriteLine("=======================================");
        Console.Write(runtimeJsContent); // Exibe o código JS gerado
        Console.WriteLine("=======================================");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO FATAL: {ex.Message}");
    }
}
        
        Console.WriteLine("--- Cube Studio Script Compiler: Teste de Expressões Binárias ---\n");

        // O TESTE: Código que força a precedência de operadores
        const string codigoCsscript =
            "local resultado = 10 + (2 * 5) == valorMaximo - 20;";
            
        Console.WriteLine("Código a Analisar:\n" + codigoCsscript + "\n");

        var lexer = new Lexer(codigoCsscript);
        var parser = new Parser(lexer);

        try
        {
            var ast = parser.ParseProgram();
            Console.WriteLine("ANÁLISE SINTÁTICA CONCLUÍDA COM SUCESSO. ÁRVORE (AST) GERADA:\n");

            foreach (var node in ast)
            {
                PrintAST(node);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRO FATAL DE PARSER: {ex.Message}");
        }
    }
}
// Dentro de static void Main(string[] args)
static void Main(string[] args)
{
    // ... (Seu código de teste CSScript e as inicializações do Lexer/Parser/SemanticAnalyzer) ...

    const string codigoCsscript = 
        // ... (Seu código de teste CSScript complexo) ...
        "function iniciar(x) {" +
        "  local vidas = 10 * x;" +
        "  if (vidas == 0) {" +
        "    print.log.Console(\"Fim de Jogo\");" +
        "    return false;" +
        "  } else {" +
        "    Sprite.Pos(vidas, 50);" +
        "  }" +
        "  return true;" +
        "}" +
        "local loop = iniciar(10);";
        
    // ... (Toda a lógica try/catch do Parser e SemanticAnalyzer) ...
    
    try
    {
        // 1. PARSE e 2. SEMÂNTICA... (Mantenha esta parte intacta)
        
        // ...
        // ...

        // 3. GERAÇÃO DE CÓDIGO: Cria o runtime.js
        var codeGenerator = new CodeGenerator();
        string runtimeJsContent = codeGenerator.Generate(ast);
        
        // ===============================================
        // 4. ETAPA DE EMPACOTAMENTO E DISTRIBUIÇÃO
        // ===============================================

        string outputDir = "CubeStudio_Build";
        string jsFileName = "runtime.js";
        string htmlFileName = "index.html";
        string zipFileName = "CubeStudio_Game_Package.zip";

        // Cria a pasta de saída
        if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
        Directory.CreateDirectory(outputDir);

        // --- A. Geração do runtime.js ---
        string jsPath = Path.Combine(outputDir, jsFileName);
        File.WriteAllText(jsPath, runtimeJsContent);
        Console.WriteLine($"\n[BUILD] Salvo {jsFileName} em: {jsPath}");

        // --- B. Geração do index.html (O container do seu jogo) ---
        string htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Cube Studio Game</title>
    <style>
        body {{ margin: 0; background-color: #333; }}
        #game-container {{ width: 800px; height: 600px; background-color: white; }}
    </style>
</head>
<body>
    <div id=""game-container"">
        <p>Carregando jogo...</p>
    </div>
    <script src=""{jsFileName}""></script>
</body>
</html>
";
        string htmlPath = Path.Combine(outputDir, htmlFileName);
        File.WriteAllText(htmlPath, htmlContent);
        Console.WriteLine($"[BUILD] Salvo {htmlFileName} em: {htmlPath}");

        // --- C. Criação do Pacote ZIP ---
        if (File.Exists(zipFileName)) File.Delete(zipFileName);
        
        // Compacta todo o conteúdo da pasta de build
        ZipFile.CreateFromDirectory(outputDir, zipFileName);
        Console.WriteLine($"\n[DISTRIBUICAO] Pacote ZIP gerado com sucesso: {zipFileName}");
        
        // NOTA: Para gerar EXE, o C# compila este próprio programa. Para APK, seria necessário um
        // framework como MAUI/Xamarin e etapas adicionais de build.

    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nERRO FATAL DURANTE A COMPILAÇÃO OU BUILD: {ex.Message}");
    }
}
