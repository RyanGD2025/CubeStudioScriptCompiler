# 🧊 Cube Studio Script Compiler

## 🚀 Visão Geral do Projeto

Este repositório contém o código-fonte do **Cube Studio Script Compiler**, um interpretador/compilador dedicado à linguagem de script proprietária **CSScript**. O objetivo principal é fornecer a lógica de back-end (Lexer, Parser e Interpretador) para o ambiente de desenvolvimento de jogos **2D** do Cube Studio.

O compilador está sendo desenvolvido em **C#** (usando o Visual Studio) para garantir modularidade e performance.

---

## 💡 A Linguagem CSScript (Cube Studio Script)

A CSScript é uma linguagem de tipagem dinâmica e sintaxe inspirada em linguagens como Lua e JavaScript, focada em simplicidade e controle de objetos de jogo 2D (Sprites, Posições, etc.).

### ✨ Palavras-Chave (Keywords)

| Uso | Keywords |
| :--- | :--- |
| **Geral** | `if`, `else`, `return`, `class` |
| **Controle** | `local`, `function`, `while`, `do` |
| **Valores** | `true`, `false` |

### 📐 Propriedades de Jogo 2D

As propriedades são acessadas via operador de ponto (`.`) em objetos:
* **Gráficos/Estrutura:** `.Image`, `.Parent`, `.Name`
* **Geometria 2D:** `.Pos`, `.Size`
* **Física:** `.Anchored`, `.Collide`

---

## 🛠️ Estrutura do Compilador em C# (Visual Studio)

O projeto segue as fases clássicas de um compilador:

1.  **Lexer.cs (Análise Lexical):**
    * Transforma o código CSScript em tokens.
    * **Status:** 100% completo, incluindo suporte para operadores de comparação (`==`, `!=`), números, strings e todas as keywords.

2.  **Ast.cs (Árvore Sintática Abstrata):**
    * Define os nós que representam a estrutura do código (`IfStatementNode`, `WhileStatementNode`, etc.).

3.  **Parser.cs (Análise Sintática):**
    * Verifica a gramática e constrói a AST.
    * **Status:** Suporte completo para:
        * Declaração de Variável (`local x = 10;`)
        * Chamada de Função Encadeada (`print.log.Console("msg");`)
        * Estrutura Condicional (`if (true) { ... } else { ... }`)
        * Loop de Repetição (`while (condicao) { ... }`)

4.  **(Próximos Passos) Semântico/Interpretador:**
    * Implementação da Tabela de Símbolos e Execução do código.

---

## 💻 Configuração

O projeto é desenvolvido no **Visual Studio** e utiliza as bibliotecas padrão do .NET. Certifique-se de configurar o repositório com o template `.gitignore` do `Visual Studio`.

---

## 📝 Status Atual do Desenvolvimento (Resumo)

* **Lexer (100%):** Concluído.
* **Parser (Em Andamento):** Estruturas principais de controle concluídas.
* 
