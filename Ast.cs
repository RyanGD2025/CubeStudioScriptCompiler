using System;
using System.Collections.Generic;

namespace CubeStudioScriptCompiler
{
    // =================================================================
    // 1. Classes Base da Árvore Sintática Abstrata (AST)
    // =================================================================
    
    // A classe base para todos os elementos na Árvore Sintática Abstrata (AST)
    public abstract class AstNode { }

    // Representa um valor ou referência que pode ser usado em expressões (o lado direito de um '=' ou uma condição)
    public abstract class ExpressionNode : AstNode { }

    // Representa uma instrução completa que tipicamente termina com ponto e vírgula (;) ou é um bloco
    public abstract class StatementNode : AstNode { }

    // Representa um bloco de instruções dentro de chaves { ... }
    public class BlockNode : StatementNode
    {
        public List<StatementNode> Statements { get; } = new List<StatementNode>();
    }

    // =================================================================
    // 2. Nós de Expressão (Valores)
    // =================================================================

    // Representa um valor literal (Número, String, True, False, Identificador Simples)
    public class LiteralNode : ExpressionNode
    {
        public TipoToken Tipo { get; } // Ex: NUMERO, STRING, IDENTIFICADOR
        public string Valor { get; }

        public LiteralNode(TipoToken tipo, string valor)
        {
            Tipo = tipo;
            Valor = valor;
        }
    }
    
    // NOTA: Os nós para Expressões Binárias (Ex: 1 + 2, x == y) serão adicionados na próxima etapa.

    // =================================================================
    // 3. Nós de Instrução (Statements)
    // =================================================================
    
    // Declaração de Variável (Ex: local x = 10;)
    public class LocalDeclarationNode : StatementNode
    {
        public string Name { get; }
        public ExpressionNode InitialValue { get; } // O valor inicial (pode ser nulo)

        public LocalDeclarationNode(string name, ExpressionNode initialValue)
        {
            Name = name;
            InitialValue = initialValue;
        }
    }

    // Chamada de Função Encadeada (Ex: print.log.Console("Texto");)
    public class CallStatementNode : StatementNode
    {
        // O caminho completo da chamada (Ex: [print, log, Console])
        public List<string> Path { get; } = new List<string>(); 

        // Os argumentos dentro dos parênteses (Ex: "Texto")
        public List<ExpressionNode> Arguments { get; } = new List<ExpressionNode>(); 
    }

    // Estrutura Condicional (Ex: if (cond) { ... } else { ... })
    public class IfStatementNode : StatementNode
    {
        public ExpressionNode Condition { get; }
        public BlockNode IfBlock { get; }
        public BlockNode ElseBlock { get; } // Pode ser nulo

        public IfStatementNode(ExpressionNode condition, BlockNode ifBlock, BlockNode elseBlock = null)
        {
            Condition = condition;
            IfBlock = ifBlock;
            ElseBlock = elseBlock;
        }
    }

    // Loop de Repetição (Ex: while (cond) { ... })
    public class WhileStatementNode : StatementNode
    {
        public ExpressionNode Condition { get; }
        public BlockNode LoopBlock { get; }

        public WhileStatementNode(ExpressionNode condition, BlockNode loopBlock)
        {
            Condition = condition;
            LoopBlock = loopBlock;
        }
    }

    // =================================================================
    // 4. Estruturas para as Próximas Implementações (Funções e Classes)
    // =================================================================

    // Declaração de Função (Ex: function nome(arg1, arg2) { ... })
    public class FunctionDeclarationNode : StatementNode
    {
        public string Name { get; }
        public List<string> Parameters { get; } = new List<string>();
        public BlockNode Body { get; }
        
        public FunctionDeclarationNode(string name, List<string> parameters, BlockNode body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }
    
    // Instrução de Retorno (Ex: return valor;)
    public class ReturnStatementNode : StatementNode
    {
        public ExpressionNode Value { get; } // O valor a ser retornado (pode ser nulo)

        public ReturnStatementNode(ExpressionNode value = null)
        {
            Value = value;
        }
    }
    
    // Declaração de Classe (Ex: class Nome { ... })
    public class ClassDeclarationNode : StatementNode
    {
        public string Name { get; }
        public BlockNode Body { get; }

        public ClassDeclarationNode(string name, BlockNode body)
        {
            Name = name;
            Body = body;
        }
    }
}
// Adicionar em Ast.cs
// Representa uma Expressão Binária (Ex: LadoEsquerdo + Operador + LadoDireito)
public class BinaryExpressionNode : ExpressionNode
{
    public ExpressionNode Left { get; }
    public TipoToken Operator { get; } // Ex: OP_ADICAO, OP_IGUALDADE
    public ExpressionNode Right { get; }

    public BinaryExpressionNode(ExpressionNode left, TipoToken op, ExpressionNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}
// Adicionar em Ast.cs
// Atribuição a Propriedade ou Variável (Ex: Sprite.Opacity = 0.5; OU x = 10;)
public class AssignmentStatementNode : StatementNode
{
    public ExpressionNode Target { get; } // Pode ser uma variável ou um caminho de propriedade (Ex: Sprite.Opacity)
    public ExpressionNode Value { get; }

    public AssignmentStatementNode(ExpressionNode target, ExpressionNode value)
    {
        Target = target;
        Value = value;
    }
}
// Dentro de CubeStudioScriptCompiler
// ...
// Declaração de Classe (Ex: class Nome extends Pai { ... })
public class ClassDeclarationNode : StatementNode
{
    public string Name { get; }
    // NOVO: Nome da classe que está sendo herdada (pode ser null)
    public string ParentClassName { get; } 
    public BlockNode Body { get; }

    // Construtor atualizado para incluir ParentClassName
    public ClassDeclarationNode(string name, string parentClassName, BlockNode body)
    {
        Name = name;
        ParentClassName = parentClassName;
        Body = body;
    }
}
// Exemplo em Ast.cs

public class ThrowNode : StatementNode
{
    // A expressão (argumento) que está sendo lançada (ex: uma string de erro, um objeto de exceção)
    public ExpressionNode ExceptionExpression { get; set; } 

    // Construtor, etc.
}
