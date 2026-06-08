namespace Parte2;

/// <summary>
/// Representa a configuração instantânea do AP em um dado passo da simulação.
/// Exibe: estado corrente, entrada restante e conteúdo da pilha.
/// </summary>
public record ConfiguracaoInstantanea(
    int Passo,
    string Estado,
    string EntradaRestante,
    string ConteudoPilha,
    string TransicaoAplicada
);

/// <summary>
/// Representa um Autômato de Pilha (AP) com aceitação por pilha vazia,
/// definido como a 7-tupla formal: M = (Q, Σ, Γ, δ, q0, Z0, ∅)
/// 
/// Referência: SIPSER, Michael. Introduction to the Theory of Computation. 3. ed. Cap. 2.
/// HOPCROFT, John E. et al. Introdução à Teoria de Autômatos, Linguagens e Computação. Cap. 6.
/// </summary>
public class AP
{
    /// <summary>Conjunto finito de estados (Q)</summary>
    public HashSet<string> Q { get; }

    /// <summary>Alfabeto finito de entrada (Σ)</summary>
    public HashSet<char> Sigma { get; }

    /// <summary>Alfabeto da pilha (Γ)</summary>
    public HashSet<char> Gama { get; }

    /// <summary>
    /// Função de transição δ: Q × (Σ ∪ {ε}) × Γ → P(Q × Γ*)
    /// A chave é (estado, símbolo_entrada, topo_pilha), onde símbolo_entrada = '\0' representa ε (λ-movimento).
    /// O valor é uma lista de (novo_estado, string_a_empilhar), onde a string é empilhada da direita
    /// para a esquerda (primeiro caractere fica no topo). String vazia = apenas desempilhar.
    /// </summary>
    public Dictionary<(string estado, char entrada, char topoPilha), List<(string novoEstado, string empilhar)>> Delta { get; }

    /// <summary>Estado inicial (q0 ∈ Q)</summary>
    public string Q0 { get; }

    /// <summary>Símbolo inicial da pilha (Z0 ∈ Γ)</summary>
    public char Z0 { get; }

    // F = ∅ — aceitação exclusivamente por pilha vazia

    /// <summary>Nome descritivo do AP (ex: "L2 = { aⁿbⁿ | n ≥ 1 }")</summary>
    public string Nome { get; }

    private const int LIMITE_PASSOS = 10000;

    public AP(HashSet<string> q, HashSet<char> sigma, HashSet<char> gama,
              Dictionary<(string estado, char entrada, char topoPilha), List<(string novoEstado, string empilhar)>> delta,
              string q0, char z0, string nome = "AP")
    {
        Q = q;
        Sigma = sigma;
        Gama = gama;
        Delta = delta;
        Q0 = q0;
        Z0 = z0;
        Nome = nome;

        Validar();
    }

    /// <summary>
    /// Simula o AP sobre a cadeia de entrada usando DFS com backtracking.
    /// Retorna true se existe pelo menos um caminho computacional que
    /// consome toda a entrada e esvazia a pilha.
    /// </summary>
    public bool Aceitar(string cadeia)
    {
        var pilhaInicial = new Stack<char>();
        pilhaInicial.Push(Z0);
        return DFS(Q0, cadeia, 0, pilhaInicial, 0);
    }

    /// <summary>
    /// Retorna a lista de configurações instantâneas do caminho que aceita a cadeia,
    /// ou do caminho mais longo explorado caso a cadeia seja rejeitada.
    /// </summary>
    public List<ConfiguracaoInstantanea> ObterCaminho(string cadeia)
    {
        var pilhaInicial = new Stack<char>();
        pilhaInicial.Push(Z0);

        var caminhoAtual = new List<ConfiguracaoInstantanea>();
        var melhorCaminho = new List<ConfiguracaoInstantanea>();

        string pilhaStr = PilhaParaString(pilhaInicial);
        string entradaRestante = cadeia.Length == 0 ? "ε" : cadeia;
        caminhoAtual.Add(new ConfiguracaoInstantanea(0, Q0, entradaRestante, pilhaStr, ""));

        DFSCaminho(Q0, cadeia, 0, pilhaInicial, caminhoAtual, melhorCaminho, 0);

        return melhorCaminho;
    }

    /// <summary>
    /// Imprime no console a tabela de transições δ do AP.
    /// </summary>
    public void ExibirDiagrama()
    {
        Console.WriteLine($"─── Tabela de Transições (δ) — {Nome} ───");
        Console.WriteLine();

        var transicoes = Delta.OrderBy(t => t.Key.estado)
                              .ThenBy(t => t.Key.entrada)
                              .ThenBy(t => t.Key.topoPilha);

        int largOrigem = "Estado".Length;
        int largEntrada = "Entrada".Length;
        int largTopo = "Topo".Length;
        int largDestino = "δ(estado, entrada, topo)".Length;

        var linhas = new List<(string origem, string entrada, string topo, string destinos)>();

        foreach (var kvp in transicoes)
        {
            string origem = kvp.Key.estado;
            string entrada = kvp.Key.entrada == '\0' ? "ε" : kvp.Key.entrada.ToString();
            string topo = kvp.Key.topoPilha.ToString();

            var destinos = string.Join(" | ",
                kvp.Value.Select(d =>
                {
                    string emp = d.empilhar.Length == 0 ? "ε" : d.empilhar;
                    return $"({d.novoEstado}, {emp})";
                }));

            largOrigem = Math.Max(largOrigem, origem.Length);
            largEntrada = Math.Max(largEntrada, entrada.Length);
            largTopo = Math.Max(largTopo, topo.Length);
            largDestino = Math.Max(largDestino, destinos.Length);

            linhas.Add((origem, entrada, topo, destinos));
        }

        string sep = "+" + new string('-', largOrigem + 2)
                   + "+" + new string('-', largEntrada + 2)
                   + "+" + new string('-', largTopo + 2)
                   + "+" + new string('-', largDestino + 2) + "+";

        Console.WriteLine(sep);
        Console.WriteLine($"| {"Estado".PadRight(largOrigem)} | {"Entrada".PadRight(largEntrada)} | {"Topo".PadRight(largTopo)} | {"Resultado δ".PadRight(largDestino)} |");
        Console.WriteLine(sep);

        foreach (var (origem, entrada, topo, destinos) in linhas)
        {
            Console.WriteLine($"| {origem.PadRight(largOrigem)} | {entrada.PadRight(largEntrada)} | {topo.PadRight(largTopo)} | {destinos.PadRight(largDestino)} |");
        }

        Console.WriteLine(sep);
    }

    /// <summary>
    /// Exibe a definição formal da 7-tupla M = (Q, Σ, Γ, δ, q0, Z0, ∅).
    /// </summary>
    public void ExibirDefinicaoFormal()
    {
        Console.WriteLine($"─── Definição Formal: M = (Q, Σ, Γ, δ, q0, Z0, ∅) — {Nome} ───");
        Console.WriteLine($"  Q  (estados):           {{ {string.Join(", ", Q.OrderBy(e => e))} }}");
        Console.WriteLine($"  Σ  (alfabeto entrada):  {{ {string.Join(", ", Sigma.OrderBy(s => s))} }}");
        Console.WriteLine($"  Γ  (alfabeto pilha):    {{ {string.Join(", ", Gama.OrderBy(g => g))} }}");
        Console.WriteLine($"  q0 (estado inicial):    {Q0}");
        Console.WriteLine($"  Z0 (símbolo inicial):   {Z0}");
        Console.WriteLine($"  F  (estados aceitação): ∅ (aceitação por pilha vazia)");
        Console.WriteLine($"  δ  (transições):        {Delta.Values.Sum(v => v.Count)} transição(ões) definida(s)");
    }

    // ──────────────────── Simulação via DFS ────────────────────

    /// <summary>
    /// DFS que verifica se existe caminho computacional que aceita.
    /// </summary>
    private bool DFS(string estado, string cadeia, int posicao, Stack<char> pilha, int passos)
    {
        if (passos > LIMITE_PASSOS) return false;

        // Aceitação: entrada consumida e pilha vazia
        if (posicao == cadeia.Length && pilha.Count == 0)
            return true;

        if (pilha.Count == 0)
            return false;

        char topo = pilha.Peek();

        // Tenta λ-movimentos (ε-transições) primeiro
        var chaveEpsilon = (estado, '\0', topo);
        if (Delta.TryGetValue(chaveEpsilon, out var transicoesEpsilon))
        {
            foreach (var (novoEstado, empilhar) in transicoesEpsilon)
            {
                var novaPilha = ClonarPilha(pilha);
                novaPilha.Pop();
                EmpilharString(novaPilha, empilhar);

                if (DFS(novoEstado, cadeia, posicao, novaPilha, passos + 1))
                    return true;
            }
        }

        // Tenta transições lendo símbolo da entrada
        if (posicao < cadeia.Length)
        {
            char simbolo = cadeia[posicao];
            var chave = (estado, simbolo, topo);
            if (Delta.TryGetValue(chave, out var transicoes))
            {
                foreach (var (novoEstado, empilhar) in transicoes)
                {
                    var novaPilha = ClonarPilha(pilha);
                    novaPilha.Pop();
                    EmpilharString(novaPilha, empilhar);

                    if (DFS(novoEstado, cadeia, posicao + 1, novaPilha, passos + 1))
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// DFS que registra o caminho de configurações instantâneas.
    /// Armazena o caminho aceito ou, se rejeitado, o caminho mais longo explorado.
    /// </summary>
    private bool DFSCaminho(string estado, string cadeia, int posicao,
                            Stack<char> pilha, List<ConfiguracaoInstantanea> caminhoAtual,
                            List<ConfiguracaoInstantanea> melhorCaminho, int passos)
    {
        if (passos > LIMITE_PASSOS) return false;

        // Aceitação: entrada consumida e pilha vazia
        if (posicao == cadeia.Length && pilha.Count == 0)
        {
            melhorCaminho.Clear();
            melhorCaminho.AddRange(caminhoAtual);
            return true;
        }

        // Atualiza melhor caminho se este é o mais longo até agora
        if (caminhoAtual.Count > melhorCaminho.Count)
        {
            melhorCaminho.Clear();
            melhorCaminho.AddRange(caminhoAtual);
        }

        if (pilha.Count == 0)
            return false;

        char topo = pilha.Peek();

        // Tenta λ-movimentos (ε-transições)
        var chaveEpsilon = (estado, '\0', topo);
        if (Delta.TryGetValue(chaveEpsilon, out var transicoesEpsilon))
        {
            foreach (var (novoEstado, empilhar) in transicoesEpsilon)
            {
                var novaPilha = ClonarPilha(pilha);
                novaPilha.Pop();
                EmpilharString(novaPilha, empilhar);

                string empExib = empilhar.Length == 0 ? "ε" : empilhar;
                string transStr = $"δ({estado}, ε, {topo}) = ({novoEstado}, {empExib})";
                string entradaRest = posicao >= cadeia.Length ? "ε" : cadeia[posicao..];
                string pilhaStr = PilhaParaString(novaPilha);

                caminhoAtual.Add(new ConfiguracaoInstantanea(
                    caminhoAtual.Count, novoEstado, entradaRest, pilhaStr, transStr));

                if (DFSCaminho(novoEstado, cadeia, posicao, novaPilha, caminhoAtual, melhorCaminho, passos + 1))
                    return true;

                caminhoAtual.RemoveAt(caminhoAtual.Count - 1);
            }
        }

        // Tenta transições lendo símbolo da entrada
        if (posicao < cadeia.Length)
        {
            char simbolo = cadeia[posicao];
            var chave = (estado, simbolo, topo);
            if (Delta.TryGetValue(chave, out var transicoes))
            {
                foreach (var (novoEstado, empilhar) in transicoes)
                {
                    var novaPilha = ClonarPilha(pilha);
                    novaPilha.Pop();
                    EmpilharString(novaPilha, empilhar);

                    string empExib = empilhar.Length == 0 ? "ε" : empilhar;
                    string transStr = $"δ({estado}, {simbolo}, {topo}) = ({novoEstado}, {empExib})";
                    string entradaRest = posicao + 1 >= cadeia.Length ? "ε" : cadeia[(posicao + 1)..];
                    string pilhaStr = PilhaParaString(novaPilha);

                    caminhoAtual.Add(new ConfiguracaoInstantanea(
                        caminhoAtual.Count, novoEstado, entradaRest, pilhaStr, transStr));

                    if (DFSCaminho(novoEstado, cadeia, posicao + 1, novaPilha, caminhoAtual, melhorCaminho, passos + 1))
                        return true;

                    caminhoAtual.RemoveAt(caminhoAtual.Count - 1);
                }
            }
        }

        return false;
    }

    // ──────────────────── Utilitários de pilha ────────────────────

    /// <summary>
    /// Empilha a string da direita para a esquerda, de modo que o primeiro caractere fique no topo.
    /// </summary>
    private static void EmpilharString(Stack<char> pilha, string s)
    {
        for (int i = s.Length - 1; i >= 0; i--)
            pilha.Push(s[i]);
    }

    /// <summary>
    /// Cria uma cópia independente da pilha (necessário para backtracking no DFS).
    /// </summary>
    private static Stack<char> ClonarPilha(Stack<char> original)
    {
        var arr = original.ToArray();
        Array.Reverse(arr);
        return new Stack<char>(arr);
    }

    /// <summary>
    /// Converte o conteúdo da pilha para string (topo à esquerda). Pilha vazia = "ε".
    /// </summary>
    private static string PilhaParaString(Stack<char> pilha)
    {
        if (pilha.Count == 0) return "ε";
        return new string(pilha.ToArray());
    }

    // ──────────────────── Validação ────────────────────

    private void Validar()
    {
        if (!Q.Contains(Q0))
            throw new InvalidOperationException(
                $"Estado inicial '{Q0}' não pertence ao conjunto de estados Q.");

        if (!Gama.Contains(Z0))
            throw new InvalidOperationException(
                $"Símbolo inicial da pilha '{Z0}' não pertence ao alfabeto da pilha Γ.");

        foreach (var kvp in Delta)
        {
            var (estado, entrada, topoPilha) = kvp.Key;

            if (!Q.Contains(estado))
                throw new InvalidOperationException(
                    $"Transição referencia estado '{estado}' que não pertence a Q.");

            if (entrada != '\0' && !Sigma.Contains(entrada))
                throw new InvalidOperationException(
                    $"Transição referencia símbolo de entrada '{entrada}' que não pertence a Σ.");

            if (!Gama.Contains(topoPilha))
                throw new InvalidOperationException(
                    $"Transição referencia símbolo de pilha '{topoPilha}' que não pertence a Γ.");

            foreach (var (novoEstado, empilhar) in kvp.Value)
            {
                if (!Q.Contains(novoEstado))
                    throw new InvalidOperationException(
                        $"Transição leva a estado '{novoEstado}' que não pertence a Q.");

                foreach (char c in empilhar)
                {
                    if (!Gama.Contains(c))
                        throw new InvalidOperationException(
                            $"Transição empilha símbolo '{c}' que não pertence a Γ.");
                }
            }
        }
    }

    // ──────────────────── Factory Methods ────────────────────

    /// <summary>
    /// Constrói o AP para L2 = { aⁿbⁿ | n ≥ 1 }.
    /// Autômato determinístico com aceitação por pilha vazia.
    /// 
    /// Estratégia:
    ///   q0 → lê 'a's e empilha 'A' sobre Z
    ///   q1 → lê 'b's e desempilha 'A'; ao encontrar Z, faz λ-movimento para esvaziar
    /// </summary>
    public static AP CriarAPL2()
    {
        var q = new HashSet<string> { "q0", "q1" };
        var sigma = new HashSet<char> { 'a', 'b' };
        var gama = new HashSet<char> { 'Z', 'A' };

        var delta = new Dictionary<(string, char, char), List<(string, string)>>
        {
            [("q0", 'a', 'Z')] = new() { ("q0", "AZ") },
            [("q0", 'a', 'A')] = new() { ("q0", "AA") },
            [("q0", 'b', 'A')] = new() { ("q1", "") },
            [("q1", 'b', 'A')] = new() { ("q1", "") },
            [("q1", '\0', 'Z')] = new() { ("q1", "") },
        };

        return new AP(q, sigma, gama, delta, "q0", 'Z', "L2 = { aⁿbⁿ | n ≥ 1 }");
    }

    /// <summary>
    /// Constrói o AP para L3 = { w ∈ {a,b}* | w = wᴿ, |w| ≥ 1 } (palíndromos).
    /// Autômato NÃO-determinístico com aceitação por pilha vazia.
    /// 
    /// Estratégia:
    ///   q0 (empilhar) → lê símbolos e empilha; não-deterministicamente adivinha o meio
    ///     - Para palíndromos de comprimento ímpar: consome o símbolo do meio sem empilhar
    ///     - Para palíndromos de comprimento par: faz λ-movimento para mudar de fase
    ///   q1 (desempilhar) → lê símbolos e compara com topo da pilha (pop se match)
    /// </summary>
    public static AP CriarAPL3()
    {
        var q = new HashSet<string> { "q0", "q1" };
        var sigma = new HashSet<char> { 'a', 'b' };
        var gama = new HashSet<char> { 'Z', 'A', 'B' };

        var delta = new Dictionary<(string, char, char), List<(string, string)>>
        {
            // Fase de empilhamento (q0): lê e empilha, ou adivinha meio (ímpar)
            [("q0", 'a', 'Z')] = new() { ("q0", "AZ"), ("q1", "Z") },
            [("q0", 'b', 'Z')] = new() { ("q0", "BZ"), ("q1", "Z") },
            [("q0", 'a', 'A')] = new() { ("q0", "AA"), ("q1", "A") },
            [("q0", 'a', 'B')] = new() { ("q0", "AB"), ("q1", "B") },
            [("q0", 'b', 'A')] = new() { ("q0", "BA"), ("q1", "A") },
            [("q0", 'b', 'B')] = new() { ("q0", "BB"), ("q1", "B") },

            // Adivinha meio (par): λ-movimento de q0 → q1
            [("q0", '\0', 'A')] = new() { ("q1", "A") },
            [("q0", '\0', 'B')] = new() { ("q1", "B") },

            // Fase de desempilhamento (q1): compara entrada com topo
            [("q1", 'a', 'A')] = new() { ("q1", "") },
            [("q1", 'b', 'B')] = new() { ("q1", "") },

            // Pop Z0 para aceitar
            [("q1", '\0', 'Z')] = new() { ("q1", "") },
        };

        return new AP(q, sigma, gama, delta, "q0", 'Z', "L3 = { w ∈ {a,b}* | w = wᴿ, |w| ≥ 1 }");
    }
}
