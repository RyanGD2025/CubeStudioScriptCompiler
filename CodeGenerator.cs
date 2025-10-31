using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CubeStudioScriptCompiler
{
    // O Gerador de Código que traduz AST (CSScript) para JS (runtime.js)
    public class CodeGenerator
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int _indentationLevel = 0;

        // Propriedades do motor de jogo que serão mapeadas para funções JS/Engine
        private static readonly Dictionary<string, string> GamePropertyMap = new Dictionary<string, string>
        {
            // Propriedades do CSScript : Mapeamento em JS
            {"Pos", "setPos"}, 
            {"Size", "setSize"},
            {"Image", "setImage"},
            // Adicionar mais mapeamentos aqui (ex: Collide, Anchored)
        };

        private void Indent()
        {
            _sb.Append(new string(' ', _indentationLevel * 4));
        }

        private void AppendLine(string code)
        {
            Indent();
            _sb.AppendLine(code);
        }

        // =================================================================
        // 1. O MÉTODO PRINCIPAL
        // =================================================================
        public string Generate(List<StatementNode> ast)
        {
            // --- HEADER do runtime.js (Setup inicial do ambiente JS/Motor) ---
            _sb.AppendLine("// Cube Studio Script Runtime - Gerado em " + DateTime.Now);
            _sb.AppendLine("const Engine = window.CubeStudioEngine; // Assumindo que o motor JS global existe\n");
            
            // Gerar o código de cada instrução principal
            foreach (var statement in ast)
            {
                VisitStatement(statement);
            }

            return _sb.ToString();
        }

        // =================================================================
        // 2. MÉTODOS DE VISITA (TRADUÇÃO)
        // =================================================================

        private void VisitStatement(StatementNode node)
        {
            switch (node)
            {
                case LocalDeclarationNode n: VisitLocalDeclaration(n); break;
                case CallStatementNode n: VisitCallStatement(n); break;
                case IfStatementNode n: VisitIfStatement(n); break;
                case WhileStatementNode n: VisitWhileStatement(n); break;
                case ReturnStatementNode n: VisitReturnStatement(n); break;
                case FunctionDeclarationNode n: VisitFunctionDeclaration(n); break;
                case ClassDeclarationNode n: VisitClassDeclaration(n); break;
                case BlockNode n: VisitBlock(n); break;
                // Outras instruções...
            }
        }

        private void VisitBlock(BlockNode node)
        {
            AppendLine("{");
            _indentationLevel++;
            foreach (var statement in node.Statements)
            {
                VisitStatement(statement);
            }
            _indentationLevel--;
            AppendLine("}");
        }

        // --- EXPRESSÕES ---

        private string VisitExpression(ExpressionNode node)
        {
            if (node is LiteralNode literal)
            {
                if (literal.Tipo == TipoToken.STRING)
                    return $"\"{literal.Valor}\""; // Strings precisam de aspas em JS
                if (literal.Tipo == TipoToken.TRUE || literal.Tipo == TipoToken.FALSE)
                    return literal.Valor.ToLower(); // Booleanos em JS são minúsculos (true/false)
                if (literal.Tipo == TipoToken.IDENTIFICADOR || literal.Tipo == TipoToken.NUMERO)
                    return literal.Valor; // Variáveis e números são diretos
            }
            else if (node is BinaryExpressionNode binary)
            {
                // Traduz a expressão binária respeitando a precedência (o Parser já garantiu a ordem)
                string op = GetJSOperator(binary.Operator);
                string left = VisitExpression(binary.Left);
                string right = VisitExpression(binary.Right);
                
                // Envolve a expressão binária em parênteses para garantir a ordem de execução no JS
                return $"({left} {op} {right})";
            }
            
            // Se for um tipo de expressão complexa não mapeada, usa seu nome
            return "/* EXPRESSAO NAO MAPEADA */";
        }
        
        private string GetJSOperator(TipoToken csscriptOp)
        {
            // Mapeamento de Operadores CSScript para JavaScript
            switch (csscriptOp)
            {
                case TipoToken.IGUAL: return "=";
                case TipoToken.OP_ADICAO: return "+";
                case TipoToken.OP_SUBTRACAO: return "-";
                case TipoToken.OP_MULTIPLICACAO: return "*";
                case TipoToken.OP_DIVISAO: return "/";
                case TipoToken.OP_IGUALDADE: return "=="; 
                case TipoToken.OP_DIFERENCA: return "!=";
                case TipoToken.OP_MAIOR_QUE: return ">";
                case TipoToken.OP_MENOR_QUE: return "<";
                default: return "/* OPR INV */";
            }
        }

        // --- INSTRUÇÕES ---

        private void VisitLocalDeclaration(LocalDeclarationNode node)
        {
            string value = node.InitialValue != null ? VisitExpression(node.InitialValue) : "null";
            AppendLine($"let {node.Name} = {value};");
        }

        private void VisitCallStatement(CallStatementNode node)
        {
            string arguments = string.Join(", ", node.Arguments.Select(VisitExpression));
            
            // O PATH é mapeado para chamadas encadeadas em JS (Ex: print.log.Console)
            string callPath = string.Join(".", node.Path); 
            
            // Se o último item do Path for uma Propriedade (Ex: Sprite.Pos = vector(x,y))
            if (node.Path.Count > 1 && GamePropertyMap.ContainsKey(node.Path.Last()))
            {
                string propertyName = node.Path.Last();
                string objectPath = string.Join(".", node.Path.Take(node.Path.Count - 1));
                string jsMethod = GamePropertyMap[propertyName];
                
                // Traduz Sprite.Pos(...) para Sprite.setPos(...)
                AppendLine($"{objectPath}.{jsMethod}({arguments});");
            }
            else
            {
                // Traduz chamadas normais (Ex: add(sprite) ou print.log.Console(msg))
                AppendLine($"{callPath}({arguments});");
            }
        }
        
        private void VisitIfStatement(IfStatementNode node)
        {
            AppendLine($"if ({VisitExpression(node.Condition)})");
            VisitBlock(node.IfBlock);
            
            if (node.ElseBlock != null)
            {
                AppendLine("else");
                VisitBlock(node.ElseBlock);
            }
        }

        private void VisitWhileStatement(WhileStatementNode node)
        {
            AppendLine($"while ({VisitExpression(node.Condition)})");
            VisitBlock(node.LoopBlock);
        }

        private void VisitReturnStatement(ReturnStatementNode node)
        {
            string value = node.Value != null ? VisitExpression(node.Value) : "";
            AppendLine($"return {value};");
        }

        private void VisitFunctionDeclaration(FunctionDeclarationNode node)
        {
            string parameters = string.Join(", ", node.Parameters);
            AppendLine($"function {node.Name}({parameters})");
            VisitBlock(node.Body);
        }

        private void VisitClassDeclaration(ClassDeclarationNode node)
        {
            // Mapeia classes do CSScript para classes JavaScript
            AppendLine($"class {node.Name}");
            VisitBlock(node.Body);
            // NOTA: O corpo da classe precisa ser ajustado para métodos/propriedades JS
        }
    }
}
// Adicione em CodeGenerator.cs o método de visita
private void VisitAssignmentStatement(AssignmentStatementNode node)
{
    string target = VisitExpression(node.Target);
    string value = VisitExpression(node.Value);

    // No futuro, se Target for um caminho (Sprite.Opacity), traduzir para Sprite.setOpacity(value);
    // Por enquanto, apenas traduz atribuições simples de JS:
    AppendLine($"{target} = {value};");
}

// Atualize VisitStatement para incluir o novo nó:
private void VisitStatement(StatementNode node)
{
    // ...
    case AssignmentStatementNode n: VisitAssignmentStatement(n); break; // <-- NOVO
    // ...
}
// Dentro da classe CodeGenerator
// ...

private void VisitClassDeclaration(ClassDeclarationNode node)
{
    // Inicia a declaração da classe: "class Nome"
    StringBuilder classHeader = new StringBuilder();
    classHeader.Append($"class {node.Name}");

    // Adiciona a herança, se existir: " extends Pai"
    if (!string.IsNullOrEmpty(node.ParentClassName))
    {
        classHeader.Append($" extends {node.ParentClassName}");
    }
    
    // Escreve o cabeçalho completo
    AppendLine(classHeader.ToString());
    
    // O corpo da classe (métodos e propriedades) é o bloco
    // NOTA: Para um compilador real, o bloco precisaria de uma visita mais granular
    // para tratar construtores e métodos separadamente das propriedades.
    VisitBlock(node.Body); 
}

// ...
// Exemplo em CodeGenerator.cs

public void GenerateCode(StatementNode node)
{
    // ... casos para outros nós

    if (node is TryCatchNode tryCatchNode)
    {
        GenerateTryCatch(tryCatchNode);
        return;
    }

    // ...
}
public void GenerateTryCatch(TryCatchNode node)
{
    // 1. Cria os Rótulos (Labels) necessários
    var catchLabel = NewLabel();       // Onde o fluxo salta em caso de erro
    var finallyLabel = NewLabel();     // Opcional: Onde o fluxo salta para o 'finally'
    var endLabel = NewLabel();         // Onde o fluxo vai após tudo ser executado
    
    // --- Início do Bloco TRY ---
    
    // 2. Emite o início do bloco de proteção
    // Esta instrução (ou metadado) informa ao runtime que o código a seguir
    // deve ser monitorado.
    EmitInstruction(OpCode.BEGIN_TRY, catchLabel); 

    // 3. Gera o código do bloco TRY
    GenerateCode(node.TryBlock);
    
    // 4. Se o TRY for bem-sucedido, pule para o FINALLY ou FIM
    if (node.FinallyBlock != null)
    {
        EmitInstruction(OpCode.JUMP, finallyLabel);
    }
    else
    {
        EmitInstruction(OpCode.JUMP, endLabel);
    }
    
    // --- Fim do Bloco TRY ---
    
    
    // --- Início do Bloco CATCH ---
    
    // 5. Marca o ponto de salto do CATCH
    MarkLabel(catchLabel); 
    
    // 6. Manipula o Objeto de Exceção
    if (!string.IsNullOrEmpty(node.ExceptionIdentifier))
    {
        // No .NET/CIL, a exceção é implicitamente colocada na pilha.
        // Se você usa uma VM própria, precisa de uma instrução para:
        // a) Pegar o objeto de exceção do sistema de runtime (OpCode.GET_EXCEPTION).
        // b) Armazená-lo na variável local de CATCH que você definiu na análise semântica.
        EmitInstruction(OpCode.STORE_LOCAL, node.ExceptionIdentifier); 
    }
    
    // 7. Gera o código do bloco CATCH
    GenerateCode(node.CatchBlock);

    // 8. Após o CATCH, pule para o FINALLY ou FIM
    if (node.FinallyBlock != null)
    {
        EmitInstruction(OpCode.JUMP, finallyLabel);
    }
    else
    {
        EmitInstruction(OpCode.JUMP, endLabel);
    }
    
    // --- Fim do Bloco CATCH ---
    
    
    // --- Início do Bloco FINALLY (Opcional) ---
    
    if (node.FinallyBlock != null)
    {
        // 9. Marca o ponto de salto do FINALLY
        MarkLabel(finallyLabel);
        
        // 10. Gera o código do bloco FINALLY
        GenerateCode(node.FinallyBlock);
    }
    
    // 11. Marca o fim de toda a estrutura
    MarkLabel(endLabel);
    
    // 12. Encerra o Bloco de Proteção
    EmitInstruction(OpCode.END_TRY_CATCH);
}
// Exemplo em CodeGenerator.cs, dentro de GenerateCode(StatementNode node)

// ...
if (node is ThrowNode throwNode)
{
    GenerateThrow(throwNode);
    return;
}
// ...
public void GenerateThrow(ThrowNode node)
{
    // 1. Gera o código da expressão de exceção
    // Isso garante que o objeto de exceção (ou a string/valor) seja colocado na pilha.
    GenerateExpression(node.ExceptionExpression); 
    
    // 2. Emite o OpCode para lançar a exceção
    // Esta instrução faz duas coisas cruciais:
    // a) Pega o valor/objeto no topo da pilha.
    // b) Interrompe a execução normal e desvia o controle para o bloco CATCH apropriado.
    EmitInstruction(OpCode.THROW); 
    
    // Se estiver a gerar para CIL/.NET, o OpCode seria 'throw'.
    // Se for uma VM própria, use o OpCode que define o seu mecanismo de exceção.
}
