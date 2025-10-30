using System;
using System.Collections.Generic;
using System.Text;

namespace CubeStudioScriptCompiler
{
    // =================================================================
    // 1. Definição de Tipos de Token (Baseado no seu CSScript)
    // =================================================================
    public enum TipoToken
    {
        EOF,
        ERRO,

        // Valores
        NUMERO,
        STRING,
        TRUE,
        FALSE,

        // Keywords
        IF, ELSE, RETURN, CLASS,
        LOCAL, FUNCTION, WHILE, DO,

        // Identificador
        IDENTIFICADOR,

        // Propriedades (Para futura referência no Parser)
        PROP_IMAGE, PROP_PARENT, PROP_NAME,
        PROP_POS, PROP_SIZE, PROP_ANCHORED,
        PROP_COLLIDE,

        // Símbolos e Operadores
        PONTO,          // .
        VIRGULA,        // ,
        PONTO_E_VIRGULA, // ;
        PARENTESES_ABRE, // (
        PARENTESES_FECHA, // )
        CHAVE_ABRE,     // {
        CHAVE_FECHA,    // }
        
        IGUAL,           // = (Atribuição)
        OP_IGUALDADE,    // == (Comparação)
        OP_DIFERENCA,    // != (Diferença)
        // Adicionar outros operadores (+, -, *, /) aqui no futuro, se necessário.
    }

    // =================================================================
    // 2. Estrutura do Token
    // =================================================================
    public struct Token
    {
        public TipoToken Tipo;
        public string Lexema; // O texto original (ex: "if" ou "100")

        public override string ToString()
        {
            return $"TIPO: {Tipo,-25} | LEXEMA: '{Lexema}'";
        }
    }

    // =================================================================
    // 3. O Analisador Lexical (Lexer)
    // =================================================================
    public class Lexer
    {
        private readonly string _codigoFonte;
        private int _posicao; // Posição atual no código

        // Dicionário para mapear strings para Keywords
        private static readonly Dictionary<string, TipoToken> Keywords = new Dictionary<string, TipoToken>
        {
            {"if", TipoToken.IF}, {"else", TipoToken.ELSE}, {"return", TipoToken.RETURN},
            {"class", TipoToken.CLASS}, {"local", TipoToken.LOCAL}, {"function", TipoToken.FUNCTION},
            {"while", TipoToken.WHILE}, {"do", TipoToken.DO},
            {"true", TipoToken.TRUE}, {"false", TipoToken.FALSE},
            
            // Propriedades (Para que o Lexer as reconheça como tokens específicos)
            {"Image", TipoToken.PROP_IMAGE}, {"Parent", TipoToken.PROP_PARENT}, {"Name", TipoToken.PROP_NAME},
            {"Pos", TipoToken.PROP_POS}, {"Size", TipoToken.PROP_SIZE}, {"Anchored", TipoToken.PROP_ANCHORED},
            {"Collide", TipoToken.PROP_COLLIDE},
        };

        public Lexer(string codigoFonte)
        {
            _codigoFonte = codigoFonte;
            _posicao = 0;
        }

        // Obtém o caractere atual
        private char Peek() => _posicao < _codigoFonte.Length ? _codigoFonte[_posicao] : '\0';

        // Avança a posição
        private void Avancar() => _posicao++;

        // --- Função Principal: Retorna o Próximo Token ---
        public Token GetNextToken()
            // Dentro do método GetNextToken(), na seção 6. Verifica SÍMBOLOS
switch (currentChar)
{
    // ... casos existentes
    case '+': tipoSimbolo = TipoToken.OP_ADICAO; break;
    case '-': tipoSimbolo = TipoToken.OP_SUBTRACAO; break;
    case '*': tipoSimbolo = TipoToken.OP_MULTIPLICACAO; break;
    case '/': tipoSimbolo = TipoToken.OP_DIVISAO; break; // Cuidado com comentários (// ou /*) já tratados!

    case '>':
        // Por simplicidade, assumimos que > não é seguido por = (>=), apenas >
        tipoSimbolo = TipoToken.OP_MAIOR_QUE; break;
    case '<':
        // Por simplicidade, assumimos que < não é seguido por = (<=), apenas <
        tipoSimbolo = TipoToken.OP_MENOR_QUE; break;
        
    // ... (Mantenha o resto dos casos IGUAL e !)
}
        
        {
            // 1. Ignora Espaços em Branco e Comentários
            while (_posicao < _codigoFonte.Length)
            {
                if (char.IsWhiteSpace(Peek()))
                {
                    Avancar();
                }
                // Comentário de Linha (//)
                else if (Peek() == '/' && _posicao + 1 < _codigoFonte.Length && _codigoFonte[_posicao + 1] == '/')
                {
                    while (Peek() != '\n' && Peek() != '\0') Avancar();
                }
                // Comentário de Bloco (/* ... */)
                else if (Peek() == '/' && _posicao + 1 < _codigoFonte.Length && _codigoFonte[_posicao + 1] == '*')
                {
                    Avancar(); Avancar(); // Pula /*
                    while (! (Peek() == '*' && _posicao + 1 < _codigoFonte.Length && _codigoFonte[_posicao + 1] == '/'))
                    {
                        if (Peek() == '\0') return new Token { Tipo = TipoToken.ERRO, Lexema = "Comentario de bloco nao fechado" };
                        Avancar();
                    }
                    Avancar(); Avancar(); // Pula */
                }
                else
                {
                    break; 
                }
            }

            // 2. Verifica FIM DO ARQUIVO
            if (Peek() == '\0')
            {
                return new Token { Tipo = TipoToken.EOF, Lexema = "FIM" };
            }

            // 3. Verifica STRING
            if (Peek() == '"')
            {
                Avancar(); // Pula "
                var sb = new StringBuilder();
                while (Peek() != '"' && Peek() != '\0')
                {
                    sb.Append(Peek());
                    Avancar();
                }

                if (Peek() == '"')
                {
                    Avancar(); // Pula "
                    return new Token { Tipo = TipoToken.STRING, Lexema = sb.ToString() };
                }
                return new Token { Tipo = TipoToken.ERRO, Lexema = "String nao fechada" };
            }

            // 4. Verifica NÚMERO
            if (char.IsDigit(Peek()))
            {
                var sb = new StringBuilder();
                while (char.IsDigit(Peek()))
                {
                    sb.Append(Peek());
                    Avancar();
                }
                return new Token { Tipo = TipoToken.NUMERO, Lexema = sb.ToString() };
            }

            // 5. Verifica IDENTIFICADOR/KEYWORDS
            if (char.IsLetter(Peek()) || Peek() == '_')
            {
                var sb = new StringBuilder();
                while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
                {
                    sb.Append(Peek());
                    Avancar();
                }
                var lexema = sb.ToString();

                // Verifica se é uma Keyword/Propriedade
                if (Keywords.TryGetValue(lexema, out TipoToken tipo))
                {
                    return new Token { Tipo = tipo, Lexema = lexema };
                }

                // Se não for Keyword, é um Identificador
                return new Token { Tipo = TipoToken.IDENTIFICADOR, Lexema = lexema };
            }

            // 6. Verifica SÍMBOLOS ÚNICOS E COMBINADOS (Operadores)
            var currentChar = Peek();
            Avancar(); // Consome o caractere
            TipoToken tipoSimbolo = TipoToken.ERRO;
            string lexema = currentChar.ToString();

            switch (currentChar)
            {
                case '.': tipoSimbolo = TipoToken.PONTO; break;
                case ';': tipoSimbolo = TipoToken.PONTO_E_VIRGULA; break;
                case '(': tipoSimbolo = TipoToken.PARENTESES_ABRE; break;
                case ')': tipoSimbolo = TipoToken.PARENTESES_FECHA; break;
                case '{': tipoSimbolo = TipoToken.CHAVE_ABRE; break;
                case '}': tipoSimbolo = TipoToken.CHAVE_FECHA; break;
                case ',': tipoSimbolo = TipoToken.VIRGULA; break;
                
                case '=':
                    if (Peek() == '=')
                    {
                        Avancar(); 
                        tipoSimbolo = TipoToken.OP_IGUALDADE; // ==
                        lexema = "==";
                    }
                    else
                    {
                        tipoSimbolo = TipoToken.IGUAL; // =
                    }
                    break;

                case '!': 
                    if (Peek() == '=')
                    {
                        Avancar(); 
                        tipoSimbolo = TipoToken.OP_DIFERENCA; // !=
                        lexema = "!=";
                    }
                    else
                    {
                        // Por simplicidade, assume erro se '!' não for seguido por '='
                        return new Token { Tipo = TipoToken.ERRO, Lexema = $"Caractere desconhecido ou operador incompleto: {currentChar}" };
                    }
                    break;

                default:
                    return new Token { Tipo = TipoToken.ERRO, Lexema = $"Caractere desconhecido: {currentChar}" };
            }

            return new Token { Tipo = tipoSimbolo, Lexema = lexema };
        }
    }
}
// Dentro de public enum TipoToken
// ...
OP_IGUALDADE,    // == 
OP_DIFERENCA,    // != 
OP_MAIOR_QUE,    // >
OP_MENOR_QUE,    // <

OP_ADICAO,       // +
OP_SUBTRACAO,    // -
OP_MULTIPLICACAO, // *
OP_DIVISAO,      // /
// ...
