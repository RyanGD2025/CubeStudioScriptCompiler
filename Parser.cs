using System;
using System.Collections.Generic;

namespace CubeStudioScriptCompiler
{
    public class Parser
    {
        private readonly Lexer _lexer;
        private Token _currentToken;

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            // Carrega o primeiro token para começar
            _currentToken = _lexer.GetNextToken(); 
        }

        // --- Funções de Ajuda do Parser ---
        
        /// <summary>
        /// Verifica se o token atual é o esperado e avança para o próximo.
        /// Lança um erro se a expectativa falhar.
        /// </summary>
        private void Consume(TipoToken expectedType)
        {
            if (_currentToken.Tipo == expectedType)
            {
                _currentToken = _lexer.GetNextToken();
            }
            else
            {
                throw new Exception($"ERRO DE SINTAXE: Esperado {expectedType}, mas encontrado {_currentToken.Tipo} ('{_currentToken.Lexema}') na linha.");
            }
        }

        /// <summary>
        /// Apenas verifica se o token atual é o esperado, sem consumir.
        /// </summary>
        private void Expect(TipoToken expectedType)
        {
            if (_currentToken.Tipo != expectedType)
            {
                throw new Exception($"ERRO DE SINTAXE: Esperado {expectedType}, mas encontrado {_currentToken.Tipo} ('{_currentToken.Lexema}')");
            }
        }

        // --- Funções para Analisar Componentes ---
        
        /// <summary>
        /// Analisa um bloco de instruções entre chaves { ... }
        /// </summary>
        private BlockNode ParseBlock()
        {
            var block = new BlockNode();
            Consume(TipoToken.CHAVE_ABRE); // Consome '{'

            // Analisa as instruções dentro do bloco até encontrar o fechamento ou EOF
            while (_currentToken.Tipo != TipoToken.CHAVE_FECHA && _currentToken.Tipo != TipoToken.EOF)
            {
                block.Statements.Add(ParseStatement());
            }

            Consume(TipoToken.CHAVE_FECHA); // Consome '}'
            return block;
        }

        /// <summary>
        /// Analisa um valor simples (Número, String, True, False, Identificador).
        /// NOTA: Esta função precisa ser expandida para expressões complexas (Ex: x == 10).
        /// </summary>
        private AstNode ParseSimpleExpression()
        {
            // Trata valores literais (NUMERO, STRING, TRUE, FALSE)
            if (_currentToken.Tipo == TipoToken.NUMERO || _currentToken.Tipo == TipoToken.STRING ||
                _currentToken.Tipo == TipoToken.TRUE || _currentToken.Tipo == TipoToken.FALSE)
            {
                var node = new LiteralNode(_currentToken.Tipo, _currentToken.Lexema);
                Consume(_currentToken.Tipo);
                return node;
            }
            
            // Trata Identificadores (que podem ser variáveis)
            if (_currentToken.Tipo == TipoToken.IDENTIFICADOR)
            {
                // Por enquanto, trata o identificador como um literal de variável.
                var node = new LiteralNode(_currentToken.Tipo, _currentToken.Lexema); 
                Consume(TipoToken.IDENTIFICADOR);
                return node;
            }

            throw new Exception($"ERRO DE SINTAXE: Expressao Invalida ('{_currentToken.Lexema}')");
        }

        // --- Funções para Analisar Declarações (Statements) ---

        /// <summary>
        /// Analisa: local nome = valor;
        /// </summary>
        private LocalDeclarationNode ParseLocalDeclaration()
        {
            Consume(TipoToken.LOCAL); // Consome 'local'
            
            Expect(TipoToken.IDENTIFICADOR);
            var name = _currentToken.Lexema;
            Consume(TipoToken.IDENTIFICADOR);

            AstNode initialValue = null;
            if (_currentToken.Tipo == TipoToken.IGUAL)
            {
                Consume(TipoToken.IGUAL); // Consome '='
                initialValue = ParseSimpleExpression(); 
            }
            
            Consume(TipoToken.PONTO_E_VIRGULA); // Consome ';'
            return new LocalDeclarationNode(name, initialValue);
        }

        /// <summary>
        /// Analisa: print.log.Console("Texto"); OU add("Sprite");
        /// </summary>
        private CallStatementNode ParseCallStatement()
        {
            var node = new CallStatementNode();

            // 1. Analisa a cadeia de chamadas (print.log.Console)
            Expect(TipoToken.IDENTIFICADOR);
            node.Path.Add(_currentToken.Lexema);
            Consume(TipoToken.IDENTIFICADOR);

            // Continua consumindo identificadores se houver operador de ponto (Ex: .log.Console)
            while (_currentToken.Tipo == TipoToken.PONTO)
            {
                Consume(TipoToken.PONTO);
                Expect(TipoToken.IDENTIFICADOR);
                node.Path.Add(_currentToken.Lexema);
                Consume(TipoToken.IDENTIFICADOR);
            }

            // 2. Analisa os Argumentos (("Texto", 10))
            if (_currentToken.Tipo == TipoToken.PARENTESES_ABRE)
            {
                Consume(TipoToken.PARENTESES_ABRE);

                if (_currentToken.Tipo != TipoToken.PARENTESES_FECHA)
                {
                    // Assume que há pelo menos 1 argumento
                    node.Arguments.Add(ParseSimpleExpression());
                    
                    // Trata argumentos separados por vírgula
                    while (_currentToken.Tipo == TipoToken.VIRGULA)
                    {
                        Consume(TipoToken.VIRGULA);
                        node.Arguments.Add(ParseSimpleExpression());
                    }
                }
                Consume(TipoToken.PARENTESES_FECHA);
            }
            
            // Requer ponto e vírgula para finalizar a instrução
            Consume(TipoToken.PONTO_E_VIRGULA); 
            return node;
        }

        /// <summary>
        /// Analisa: if (condicao) { ... } else { ... }
        /// </summary>
        private IfStatementNode ParseIfStatement()
        {
            Consume(TipoToken.IF); // Consome 'if'

            Consume(TipoToken.PARENTESES_ABRE); // Consome '('
            var condition = ParseSimpleExpression(); // Analisa a condição
            Consume(TipoToken.PARENTESES_FECHA); // Consome ')'

            // Analisa o bloco do IF
            var ifBlock = ParseBlock();

            BlockNode elseBlock = null;
            if (_currentToken.Tipo == TipoToken.ELSE)
            {
                Consume(TipoToken.ELSE); // Consome 'else'
                elseBlock = ParseBlock(); // Analisa o bloco do ELSE
            }

            return new IfStatementNode(condition, ifBlock, elseBlock);
        }

        /// <summary>
        /// Analisa: while (condicao) do { ... }
        /// </summary>
        private WhileStatementNode ParseWhileStatement()
        {
            Consume(TipoToken.WHILE); // Consome 'while'

            Consume(TipoToken.PARENTESES_ABRE); // Consome '('
            var condition = ParseSimpleExpression(); // Analisa a condição
            Consume(TipoToken.PARENTESES_FECHA); // Consome ')'
            
            // O 'do' é opcional no seu Grammar
            if (_currentToken.Tipo == TipoToken.DO)
            {
                Consume(TipoToken.DO); 
            }

            // Analisa o bloco do loop { ... }
            var loopBlock = ParseBlock();

            return new WhileStatementNode(condition, loopBlock);
        }

        /// <summary>
        /// Analisa uma única instrução (o coração do Parser)
        /// </summary>
        public StatementNode ParseStatement()
        {
            if (_currentToken.Tipo == TipoToken.LOCAL)
            {
                return ParseLocalDeclaration();
            }
            else if (_currentToken.Tipo == TipoToken.IF)
            {
                return ParseIfStatement();
            }
            else if (_currentToken.Tipo == TipoToken.WHILE)
            {
                return ParseWhileStatement();
            }
            
            // Se for um identificador, assumimos que é uma chamada encadeada ou simples
            if (_currentToken.Tipo == TipoToken.IDENTIFICADOR)
            {
                return ParseCallStatement();
            }

            // Se for uma palavra-chave não implementada como instrução, lança um erro
            throw new Exception($"ERRO DE SINTAXE: Instrucao nao reconhecida: {_currentToken.Lexema} (Tipo: {_currentToken.Tipo})");
        }

        /// <summary>
        /// Função principal que analisa todo o código fonte e constrói a AST
        /// </summary>
        public List<StatementNode> ParseProgram()
        {
            var program = new List<StatementNode>();
            while (_currentToken.Tipo != TipoToken.EOF)
            {
                program.Add(ParseStatement());
            }
            return program;
        }
    }
}
// Adicionar em Parser.cs
/// <summary>
/// Analisa: function nome(arg1, arg2) { ... }
/// </summary>
private FunctionDeclarationNode ParseFunctionDeclaration()
{
    Consume(TipoToken.FUNCTION); // Consome 'function'

    Expect(TipoToken.IDENTIFICADOR);
    var name = _currentToken.Lexema;
    Consume(TipoToken.IDENTIFICADOR); // Nome da função

    Consume(TipoToken.PARENTESES_ABRE); // Consome '('
    
    var parameters = new List<string>();

    // Analisa os Parâmetros
    if (_currentToken.Tipo == TipoToken.IDENTIFICADOR)
    {
        parameters.Add(_currentToken.Lexema);
        Consume(TipoToken.IDENTIFICADOR);
        
        while (_currentToken.Tipo == TipoToken.VIRGULA)
        {
            Consume(TipoToken.VIRGULA); // Consome ','
            Expect(TipoToken.IDENTIFICADOR);
            parameters.Add(_currentToken.Lexema);
            Consume(TipoToken.IDENTIFICADOR);
        }
    }

    Consume(TipoToken.PARENTESES_FECHA); // Consome ')'

    // O corpo da função é um bloco
    var body = ParseBlock();
    
    return new FunctionDeclarationNode(name, parameters, body);
}
// Adicionar em Parser.cs
/// <summary>
/// Analisa: return expressao; OU return;
/// </summary>
private ReturnStatementNode ParseReturnStatement()
{
    Consume(TipoToken.RETURN); // Consome 'return'

    ExpressionNode returnedValue = null;

    // Se o próximo token NÃO for um ponto e vírgula, significa que há um valor a ser retornado
    if (_currentToken.Tipo != TipoToken.PONTO_E_VIRGULA)
    {
        returnedValue = ParseSimpleExpression(); // Analisa o valor (ex: 10, variavel, true)
    }

    Consume(TipoToken.PONTO_E_VIRGULA); // Consome ';'
    
    return new ReturnStatementNode(returnedValue);
}
// Adicionar em Parser.cs
/// <summary>
/// Analisa o nó mais fundamental: um Literal, um Identificador ou uma Expressão entre parênteses.
/// </summary>
private ExpressionNode ParsePrimary()
{
    // Se for (expressao), resolve a expressao primeiro
    if (_currentToken.Tipo == TipoToken.PARENTESES_ABRE)
    {
        Consume(TipoToken.PARENTESES_ABRE);
        var expr = ParseExpression(); // Chama ParseExpression recursivamente
        Consume(TipoToken.PARENTESES_FECHA);
        return expr;
    }

    // Se for um literal (número, string, true, false) ou identificador (variável)
    if (_currentToken.Tipo == TipoToken.NUMERO || _currentToken.Tipo == TipoToken.STRING ||
        _currentToken.Tipo == TipoToken.TRUE || _currentToken.Tipo == TipoToken.FALSE ||
        _currentToken.Tipo == TipoToken.IDENTIFICADOR)
    {
        var node = new LiteralNode(_currentToken.Tipo, _currentToken.Lexema);
        Consume(_currentToken.Tipo);
        return node;
    }
    
    throw new Exception($"ERRO DE SINTAXE: Expressao Invalida ('{_currentToken.Lexema}')");
}

/// <summary>
/// Analisa todas as expressões binárias (A + B, C * D, X == Y) respeitando a precedência.
/// </summary>
private ExpressionNode ParseExpression(int parentPrecedence = 0)
{
    // Começa analisando o lado esquerdo (um literal, variável, ou expressão entre parênteses)
    ExpressionNode left = ParsePrimary();

    while (IsBinaryOperator(_currentToken.Tipo) || _currentToken.Tipo == TipoToken.IGUAL)
    {
        // Pega a precedência do operador atual
        TipoToken op = _currentToken.Tipo;
        if (!Precedence.TryGetValue(op, out int currentPrecedence))
        {
            // Se o operador não tiver precedência definida, sai.
            break; 
        }

        // Se a precedência atual for MENOR ou IGUAL à precedência do pai, sai do loop.
        // Ex: Se o pai é * (50) e o atual é + (40), o * deve ser feito primeiro.
        if (currentPrecedence <= parentPrecedence)
        {
            break;
        }

        // Consome o operador
        Consume(op);

        // Analisa o lado direito recursivamente com a precedência MAIS ALTA que a atual.
        // Isso garante que (a * b) seja analisado antes que (a * b) + c.
        ExpressionNode right = ParseExpression(currentPrecedence); 

        // Cria o nó de expressão binária
        left = new BinaryExpressionNode(left, op, right);
    }

    return left;
}
// Adicionar em Parser.cs
/// <summary>
/// Analisa: class NomeDaClasse { ... }
/// </summary>
private ClassDeclarationNode ParseClassDeclaration()
{
    Consume(TipoToken.CLASS); // Consome 'class'

    Expect(TipoToken.IDENTIFICADOR);
    var name = _currentToken.Lexema;
    Consume(TipoToken.IDENTIFICADOR); // Nome da classe

    // O corpo da classe é analisado como um bloco de instruções
    // NOTA: No futuro, este bloco só poderá conter 'local' (propriedades) e 'function' (métodos).
    var body = ParseBlock();
    
    // NOTA: Não há ponto-e-vírgula após a declaração de classe
    
    return new ClassDeclarationNode(name, body);
}
// Adicionar em Parser.cs
/// <summary>
/// Analisa atribuições ou chamadas de função (Ex: x = 10; OU print.log.Console();)
/// </summary>
private StatementNode ParseAssignmentOrCall()
{
    // 1. O alvo (pode ser 'x' ou 'Sprite.Pos')
    var target = ParsePrimary(); // ParsePrimary lida com identificadores e literais

    // Se o próximo token for IGUAL, é uma atribuição.
    if (_currentToken.Tipo == TipoToken.IGUAL)
    {
        Consume(TipoToken.IGUAL); // Consome '='
        var value = ParseExpression(); // Analisa a expressão do lado direito
        
        Consume(TipoToken.PONTO_E_VIRGULA);
        return new AssignmentStatementNode(target, value);
    }
    
    // Se não for atribuição, assumimos que é uma Chamada de Função ou Propriedade.
    // O nó 'target' precisa ser reempacotado em CallStatementNode aqui, mas isso torna o parser complicado.
    
    // Pelo bem da simplicidade e arquitetura atual, vamos assumir que o Parser continua a chamar o CallStatement.
    // NOTA: Para um compilador real, isso exigiria uma reestruturação do parser para disambiguar.

    // Pelo seu código atual, se não for '=', o parser continua como CallStatement.
    // Vamos manter a chamada atual, mas você deve saber que a atribuição deve ser resolvida antes.
    
    // Voltando um passo para evitar grandes reestruturas
    
    // Se o target for apenas um LiteralNode (Identificador), tentamos resolver a chamada de função
    if (target is LiteralNode && target.Tipo == TipoToken.IDENTIFICADOR)
    {
        // ... (Seria aqui a lógica para transformar o identificador em um caminho de chamada)
    }

    // POR ENQUANTO, DEIXAMOS A LÓGICA ANTIGA DO PARSER PARA CHAMADAS DE FUNÇÃO PARA NÃO QUEBRAR
    // O PARSER JÁ FUNCIONAL, MAS MANTEMOS O NÓ DE ATRIBUIÇÃO PARA FUTURAS IMPLEMENTAÇÕES SIMPLES.
    
    // Retorna ao estado anterior por segurança, priorizando o CallStatement
    return ParseCallStatement();
}
// Dentro da função StatementNode ParseStatement()
public StatementNode ParseStatement()
{
    // ... (Manter ifs para LOCAL, IF, WHILE, FUNCTION, CLASS, RETURN)
    
    // Se a instrução começa com um nome (variável, propriedade ou função)
    else if (_currentToken.Tipo == TipoToken.IDENTIFICADOR)
    {
        return ParseAssignmentOrCall(); // <-- NOVO PONTO DE ENTRADA
    }
    
    // ... (o resto do switch/lógica de erro)
}
// Adicionar em Parser.cs
/// <summary>
/// Analisa um caminho de acesso encadeado (Ex: Sprite.Pos, print.log.Console).
/// O caminho é retornado como uma lista de strings.
/// </summary>
private List<string> ParseAccessPath()
{
    List<string> path = new List<string>();
    
    // O caminho deve SEMPRE começar com um IDENTIFICADOR.
    Expect(TipoToken.IDENTIFICADOR);
    path.Add(_currentToken.Lexema);
    Consume(TipoToken.IDENTIFICADOR);

    // Continua consumindo DOTs e IDENTIFICADORes subsequentes
    while (_currentToken.Tipo == TipoToken.PONTO)
    {
        Consume(TipoToken.PONTO);
        Expect(TipoToken.IDENTIFICADOR);
        path.Add(_currentToken.Lexema);
        Consume(TipoToken.IDENTIFICADOR);
    }
    
    return path;
}
// Adicionar em Parser.cs
/// <summary>
/// Analisa se a instrução é uma Atribuição (x = 10;) ou uma Chamada de Função (print.log.Console();).
/// </summary>
private StatementNode ParseAssignmentOrCall()
{
    // 1. Analisa o caminho de acesso (Ex: 'Sprite.Pos' ou 'iniciar')
    List<string> accessPath = ParseAccessPath();
    
    // 2. Verifica o que vem depois do caminho:
    
    // --- CASO 1: ATRIBUIÇÃO (x = 10; ou Sprite.Pos = vector(x, y);) ---
    if (_currentToken.Tipo == TipoToken.IGUAL)
    {
        Consume(TipoToken.IGUAL); // Consome '='
        
        // O lado esquerdo (Target) de uma atribuição é uma expressão.
        // Neste caso, o 'target' é o próprio AccessPath, que precisa ser
        // representado como uma ExpressionNode.
        
        // Criamos um LiteralNode para representar o alvo (não é ideal, mas simplifica o AST)
        // NOTA: Para um compilador real, o 'target' seria um nó especial de "Propriedade/Variável".
        
        // Vamos usar um nó auxiliar simples para representar o lado esquerdo
        LiteralNode targetLiteral;
        if (accessPath.Count == 1)
        {
            // Atribuição simples de variável (x = 10;)
            targetLiteral = new LiteralNode(TipoToken.IDENTIFICADOR, accessPath[0]);
        }
        else
        {
            // Atribuição a Propriedade (Sprite.Pos = x;)
            targetLiteral = new LiteralNode(TipoToken.IDENTIFICADOR, string.Join(".", accessPath));
        }

        ExpressionNode value = ParseExpression(); // Analisa o valor a ser atribuído

        Consume(TipoToken.PONTO_E_VIRGULA);
        return new AssignmentStatementNode(targetLiteral, value);
    }
    
    // --- CASO 2: CHAMADA DE FUNÇÃO (iniciar(x); ou print.log.Console(msg);) ---
    else if (_currentToken.Tipo == TipoToken.PARENTESES_ABRE)
    {
        // Se a chamada de função for um caminho longo, a lista 'accessPath' já o contém.
        
        Consume(TipoToken.PARENTESES_ABRE); // Consome '('
        
        List<ExpressionNode> args = new List<ExpressionNode>();
        if (_currentToken.Tipo != TipoToken.PARENTESES_FECHA)
        {
            // Loop para analisar os argumentos
            do
            {
                args.Add(ParseExpression()); // Analisa o argumento como uma expressão
                if (_currentToken.Tipo == TipoToken.VIRGULA)
                {
                    Consume(TipoToken.VIRGULA);
                }
            } while (_currentToken.Tipo != TipoToken.PARENTESES_FECHA);
        }
        
        Consume(TipoToken.PARENTESES_FECHA); // Consome ')'
        Consume(TipoToken.PONTO_E_VIRGULA);
        
        return new CallStatementNode(accessPath, args);
    }
    
    // --- ERRO ---
    else
    {
        throw new Exception($"ERRO DE SINTAXE: Esperado '=' ou '(' após o identificador '{accessPath.First()}' na linha {lexer.CurrentToken.Linha}.");
    }
}
