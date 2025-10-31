using System;
using System.Collections.Generic;

namespace CubeStudioScriptCompiler
{
    // O Analisador Semântico que constrói a Tabela de Símbolos
    public class SemanticAnalyzer
    {
        public Scope GlobalScope { get; private set; }
        private Scope _currentScope;

        public SemanticAnalyzer()
        {
            // O primeiro escopo é sempre o Global
            GlobalScope = new Scope("global");
            _currentScope = GlobalScope;
            
            // Pré-definir funções do motor de jogo (Built-ins)
            // Ex: add.Sprite(...)
            // GlobalScope.Define(new FunctionSymbol("add.Sprite", ...)); 
        }
        
        // Entra em um novo escopo (Ex: Entra em uma função ou bloco IF)
        private void EnterScope(string name)
        {
            _currentScope = new Scope(name, _currentScope);
            Console.WriteLine($"[Semantico] Entrando no escopo: {_currentScope.Name}");
        }

        // Sai do escopo atual (Ex: Sai de uma função ou bloco IF)
        private void ExitScope()
        {
            if (_currentScope.Parent == null)
            {
                throw new Exception("ERRO INTERNO: Tentativa de sair do escopo global.");
            }
            Console.WriteLine($"[Semantico] Saindo do escopo: {_currentScope.Name}");
            _currentScope = _currentScope.Parent;
        }

        // O método principal que percorre a AST gerada pelo Parser
        public void Analyze(List<StatementNode> ast)
        {
            foreach (var statement in ast)
            {
                VisitStatement(statement);
            }
            
            Console.WriteLine("ANÁLISE SEMÂNTICA CONCLUÍDA COM SUCESSO.");
        }

        // Visita (processa) cada instrução da AST
        private void VisitStatement(StatementNode node)
        {
            switch (node)
            {
                case LocalDeclarationNode n: VisitLocalDeclaration(n); break;
                case FunctionDeclarationNode n: VisitFunctionDeclaration(n); break;
                case ClassDeclarationNode n: VisitClassDeclaration(n); break;
                case BlockNode n: VisitBlock(n); break;
                // Os outros nós (IF, WHILE, CALL, RETURN) são tratados aqui no futuro
                default:
                    // Por enquanto, apenas ignora nós sem necessidade de definição
                    break;
            }
        }
        
        private void VisitLocalDeclaration(LocalDeclarationNode node)
        {
            // 1. Cria o símbolo (por enquanto, com tipo NULO)
            var symbol = new VarSymbol(node.Name);
            
            // 2. Define o símbolo no escopo atual
            _currentScope.Define(symbol);
            Console.WriteLine($"[Semantico] Variavel definida: {node.Name}");

            // NOTA FUTURA: Aqui você chamaria VisitExpression(node.InitialValue) para verificar o tipo.
        }

        private void VisitFunctionDeclaration(FunctionDeclarationNode node)
        {
            // 1. Cria e define o símbolo da função no escopo atual
            var functionSymbol = new FunctionSymbol(node.Name, node.Parameters);
            _currentScope.Define(functionSymbol);
            Console.WriteLine($"[Semantico] Funcao definida: {node.Name}");

            // 2. Cria um novo escopo para o corpo da função
            EnterScope(node.Name);
            
            // 3. Define os parâmetros como variáveis dentro do novo escopo
            foreach (var paramName in node.Parameters)
            {
                _currentScope.Define(new VarSymbol(paramName));
            }

            // 4. Visita o corpo da função (o bloco)
            VisitBlock(node.Body);

            // 5. Sai do escopo da função
            ExitScope();
        }
        
        private void VisitClassDeclaration(ClassDeclarationNode node)
        {
            // 1. Cria e define o símbolo da classe no escopo atual
            _currentScope.Define(new ClassSymbol(node.Name));
            Console.WriteLine($"[Semantico] Classe definida: {node.Name}");

            // 2. Cria um novo escopo para a classe (para seus métodos e propriedades)
            EnterScope(node.Name + "_class");
            
            // 3. Visita o corpo da classe (métodos e propriedades)
            VisitBlock(node.Body);

            // 4. Sai do escopo da classe
            ExitScope();
        }

        private void VisitBlock(BlockNode node)
        {
            // Blocos anônimos (como os de IF e WHILE) criam um novo escopo temporário
            if (_currentScope.Name != "global" && _currentScope.Name != _currentScope.Parent.Name)
            {
                // Se não estivermos no escopo de uma função ou classe, entra em um escopo de bloco
                EnterScope("bloco"); 
            }
            
            foreach (var statement in node.Statements)
            {
                VisitStatement(statement);
            }
            
            if (_currentScope.Name == "bloco")
            {
                ExitScope();
            }
        }
    }
}
// Adicionar em SemanticAnalyzer.cs
private void VisitCallStatement(CallStatementNode node)
{
    // O primeiro item do 'Path' é o nome da função/objeto raiz (Ex: 'print' ou 'add')
    string rootName = node.Path[0];
    
    // Tenta resolver o nome no escopo atual e nos pais
    Symbol symbol = _currentScope.Resolve(rootName);

    if (symbol == null)
    {
        // ERRO SEMÂNTICO: A função/variável 'rootName' não foi declarada
        throw new Exception($"ERRO SEMÂNTICO: O objeto ou função '{rootName}' não está definido.");
    }

    // NOTA: Se for uma função ou classe, o código verificaria aqui se os argumentos são válidos.
    
    // Visita recursivamente todos os argumentos para garantir que todas as variáveis usadas neles existam.
    foreach (var arg in node.Arguments)
    {
        VisitExpression(arg);
    }
}
// Adicionar em SemanticAnalyzer.cs
private void VisitExpression(ExpressionNode node)
{
    switch (node)
    {
        case LiteralNode n: VisitLiteral(n); break;
        case BinaryExpressionNode n: VisitBinaryExpression(n); break;
        // ... (Adicionar outros tipos de expressão, se houver)
    }
}

private void VisitLiteral(LiteralNode node)
{
    // Verifica se o literal é, na verdade, um identificador (variável)
    if (node.Tipo == TipoToken.IDENTIFICADOR)
    {
        Symbol symbol = _currentScope.Resolve(node.Valor);
        
        if (symbol == null)
        {
            // ERRO SEMÂNTICO: A variável está sendo usada antes de ser declarada
            throw new Exception($"ERRO SEMÂNTICO: Variavel '{node.Valor}' nao declarada (escopo: {_currentScope.Name}).");
        }
    }
}

private void VisitBinaryExpression(BinaryExpressionNode node)
{
    // Verifica o lado esquerdo e o lado direito da expressão recursivamente
    VisitExpression(node.Left);
    VisitExpression(node.Right);
    
    // NOTA: Aqui você faria a VERIFICAÇÃO DE TIPO (Ex: 10 + 5 é válido; 10 + "texto" pode ser inválido).
}
// Dentro de VisitStatement(StatementNode node)
// ...
// Adicionar esta linha no switch:
case CallStatementNode n: VisitCallStatement(n); break; 
// ...
// No seu arquivo SemanticAnalyzer.cs, dentro do método VisitStatement
private void VisitStatement(StatementNode node)
{
    switch (node)
    {
        case LocalDeclarationNode n: VisitLocalDeclaration(n); break;
        case FunctionDeclarationNode n: VisitFunctionDeclaration(n); break;
        case ClassDeclarationNode n: VisitClassDeclaration(n); break;
        case BlockNode n: VisitBlock(n); break;
        case CallStatementNode n: VisitCallStatement(n); break; 
        case IfStatementNode n: VisitIfStatement(n); break;     // <-- NOVO!
        case WhileStatementNode n: VisitWhileStatement(n); break; // <-- NOVO!
        case ReturnStatementNode n: /* Adicionar lógica de verificação de retorno, ex: se está dentro de função */ break;
        default:
            // Por enquanto, ignora nós sem necessidade de definição
            break;
    }
}
// No seu arquivo SemanticAnalyzer.cs
private void VisitClassDeclaration(ClassDeclarationNode node)
{
    // 1. Verifica se a própria classe já foi definida no escopo atual
    if (_currentScope.Resolve(node.Name) != null)
    {
        throw new Exception($"ERRO SEMÂNTICO: A classe '{node.Name}' já foi definida.");
    }
    
    // 2. Verifica a Herança (Classe Pai)
    if (node.ParentClassName != null)
    {
        Symbol parentSymbol = _currentScope.Resolve(node.ParentClassName);
        
        if (parentSymbol == null)
        {
            throw new Exception($"ERRO SEMÂNTICO: Classe pai '{node.ParentClassName}' não está definida.");
        }
        
        // Garante que o símbolo pai é realmente uma classe, e não uma função ou variável
        if (!(parentSymbol is ClassSymbol))
        {
            throw new Exception($"ERRO SEMÂNTICO: '{node.ParentClassName}' deve ser uma classe para ser herdada.");
        }
    }

    // 3. Define a nova classe no escopo atual
    _currentScope.Define(new ClassSymbol(node.Name));
    Console.WriteLine($"[Semantico] Classe definida: {node.Name}");

    // 4. Entra no escopo da classe para definir métodos/propriedades
    EnterScope(node.Name + "_class");
    
    // 5. Visita o corpo da classe (métodos e propriedades)
    VisitBlock(node.Body);

    // 6. Sai do escopo da classe
    ExitScope();
}
