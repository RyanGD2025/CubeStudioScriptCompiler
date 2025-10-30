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
