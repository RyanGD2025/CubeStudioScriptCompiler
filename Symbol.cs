using System;
using System.Collections.Generic;

namespace CubeStudioScriptCompiler
{
    // Tipos de dados básicos suportados pelo CSScript (para verificação futura)
    public enum TipoDado
    {
        NULO,
        NUMERO,
        STRING,
        BOOLEANO,
        FUNCAO,
        CLASSE
    }
    
    // A classe base para qualquer coisa que pode ser declarada (variável, função, classe)
    public abstract class Symbol
    {
        public string Name { get; }
        public TipoDado Type { get; protected set; }
        
        protected Symbol(string name, TipoDado type)
        {
            Name = name;
            Type = type;
        }
    }

    // Representa uma variável local (ex: local vidas = 10;)
    public class VarSymbol : Symbol
    {
        public VarSymbol(string name, TipoDado type = TipoDado.NULO) : base(name, type) { }
    }

    // Representa uma função (ex: function iniciar(param) { ... })
    public class FunctionSymbol : Symbol
    {
        public List<string> Parameters { get; } = new List<string>();
        
        public FunctionSymbol(string name, List<string> parameters) : base(name, TipoDado.FUNCAO)
        {
            Parameters = parameters;
        }
    }
    
    // Representa uma classe
    public class ClassSymbol : Symbol
    {
        public ClassSymbol(string name) : base(name, TipoDado.CLASSE) { }
    }
}
