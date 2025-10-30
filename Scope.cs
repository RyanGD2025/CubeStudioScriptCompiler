using System;
using System.Collections.Generic;

namespace CubeStudioScriptCompiler
{
    // Representa um único escopo (como o corpo de uma função ou um bloco IF)
    public class Scope
    {
        private readonly Dictionary<string, Symbol> _symbols = new Dictionary<string, Symbol>();
        public Scope Parent { get; } // Escopo pai (para acessar variáveis de fora)
        public string Name { get; }

        public Scope(string name, Scope parent = null)
        {
            Name = name;
            Parent = parent;
        }

        /// <summary>
        /// Adiciona um símbolo (variável, função) ao escopo atual.
        /// </summary>
        public void Define(Symbol symbol)
        {
            if (_symbols.ContainsKey(symbol.Name))
            {
                throw new Exception($"ERRO SEMÂNTICO: O nome '{symbol.Name}' já está definido neste escopo.");
            }
            _symbols.Add(symbol.Name, symbol);
        }

        /// <summary>
        /// Procura um símbolo (variável, função, classe) começando pelo escopo atual e subindo até o global.
        /// </summary>
        public Symbol Resolve(string name)
        {
            if (_symbols.TryGetValue(name, out Symbol symbol))
            {
                return symbol; // Encontrado no escopo atual
            }

            // Não encontrado, tenta o escopo pai (recursão)
            if (Parent != null)
            {
                return Parent.Resolve(name);
            }

            // Não encontrado em lugar nenhum
            return null;
        }
    }
}
