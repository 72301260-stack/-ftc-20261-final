namespace Parte3;

/// <summary>
/// Representa uma configuração instantânea da MT em um dado passo da simulação.
/// Exibe: estado corrente, conteúdo da fita (com cabeçote marcado), posição do cabeçote.
/// </summary>
public record ConfiguracaoMT(
    int Passo,
    string Estado,
    string FitaFormatada,
    int PosicaoCabecote,
    string TransicaoAplicada
);

/// <summary>
/// Resultado completo da simulação de uma Máquina de Turing.
/// </summary>
public class ResultadoMT
{
    public bool Aceita { get; init; }
    public bool Rejeitada { get; init; }
    public bool LimiteExcedido { get; init; }
    public int TotalPassos { get; init; }
    public List<ConfiguracaoMT> Configuracoes { get; init; } = new();

    /// <summary>Conteúdo final da fita (útil para MTs computadoras).</summary>
    public string FitaSaida { get; init; } = "";
}

/// <summary>
/// Representa uma Máquina de Turing (MT) como a 7-tupla formal:
/// M = (Q, Σ, Γ, δ, q0, qaccept, qreject)
///
/// Referência: SIPSER, Michael. Introduction to the Theory of Computation. 3. ed. Cap. 3.
/// HOPCROFT, John E. et al. Introdução à Teoria de Autômatos, Linguagens e Computação. Cap. 8.
/// </summary>
public class MT
{
    /// <summary>Conjunto finito de estados (Q)</summary>
    public HashSet<string> Q { get; }

    /// <summary>Alfabeto de entrada (Σ), onde ⊔ ∉ Σ</summary>
    public HashSet<char> Sigma { get; }

    /// <summary>Alfabeto da fita (Γ ⊇ Σ), onde ⊔ ∈ Γ</summary>
    public HashSet<char> Gama { get; }

    /// <summary>
    /// Função de transição δ: Q × Γ → Q × Γ × {L, R}
    /// A chave é (estado, símbolo_na_fita).
    /// O valor é (novo_estado, símbolo_a_escrever, direção).
    /// Direção: 'L' = esquerda, 'R' = direita.
    /// </summary>
    public Dictionary<(string estado, char simbolo), (string novoEstado, char novoSimbolo, char direcao)> Delta { get; }

    /// <summary>Estado inicial (q0 ∈ Q)</summary>
    public string Q0 { get; }

    /// <summary>Estado de aceitação (qaccept ∈ Q)</summary>
    public string QAccept { get; }

    /// <summary>Estado de rejeição (qreject ∈ Q, qreject ≠ qaccept)</summary>
    public string QReject { get; }

    /// <summary>Símbolo de branco na fita (⊔)</summary>
    public const char BRANCO = '_';

    /// <summary>Limite configurável de passos para evitar loops infinitos.</summary>
    public int LimitePassos { get; set; } = 1000;

    /// <summary>Nome descritivo da MT.</summary>
    public string Nome { get; }

    /// <summary>
    /// Indica se a MT é computadora (exibe fita de saída) ou reconhecedora (exibe ACEITA/REJEITA).
    /// </summary>
    public bool EhComputadora { get; }

    public MT(HashSet<string> q, HashSet<char> sigma, HashSet<char> gama,
              Dictionary<(string estado, char simbolo), (string novoEstado, char novoSimbolo, char direcao)> delta,
              string q0, string qAccept, string qReject,
              string nome = "MT", bool ehComputadora = false)
    {
        Q = q;
        Sigma = sigma;
        Gama = gama;
        Delta = delta;
        Q0 = q0;
        QAccept = qAccept;
        QReject = qReject;
        Nome = nome;
        EhComputadora = ehComputadora;

        Validar();
    }

    /// <summary>
    /// Simula a Máquina de Turing sobre a entrada fornecida.
    /// Retorna o resultado completo com configurações instantâneas de cada passo.
    /// </summary>
    public ResultadoMT Executar(string entrada)
    {
        var fita = new Dictionary<int, char>();
        for (int i = 0; i < entrada.Length; i++)
            fita[i] = entrada[i];

        int cabecote = 0;
        string estado = Q0;
        int passos = 0;
        var configuracoes = new List<ConfiguracaoMT>();

        configuracoes.Add(new ConfiguracaoMT(
            0, estado, FormatarFita(fita, cabecote), cabecote, ""));

        while (estado != QAccept && estado != QReject)
        {
            if (passos >= LimitePassos)
            {
                return new ResultadoMT
                {
                    Aceita = false,
                    Rejeitada = false,
                    LimiteExcedido = true,
                    TotalPassos = passos,
                    Configuracoes = configuracoes,
                    FitaSaida = ExtrairConteudoFita(fita)
                };
            }

            char simboloAtual = LerFita(fita, cabecote);
            var chave = (estado, simboloAtual);

            if (!Delta.TryGetValue(chave, out var transicao))
            {
                // Transição não definida → rejeita implicitamente
                estado = QReject;
                passos++;
                configuracoes.Add(new ConfiguracaoMT(
                    passos, estado, FormatarFita(fita, cabecote), cabecote,
                    $"δ({chave.estado}, {simboloAtual}) = indefinida → {QReject}"));
                break;
            }

            var (novoEstado, novoSimbolo, direcao) = transicao;

            string transStr = $"δ({estado}, {simboloAtual}) = ({novoEstado}, {novoSimbolo}, {direcao})";

            fita[cabecote] = novoSimbolo;
            estado = novoEstado;
            cabecote += direcao == 'R' ? 1 : -1;
            passos++;

            configuracoes.Add(new ConfiguracaoMT(
                passos, estado, FormatarFita(fita, cabecote), cabecote, transStr));
        }

        return new ResultadoMT
        {
            Aceita = estado == QAccept,
            Rejeitada = estado == QReject,
            LimiteExcedido = false,
            TotalPassos = passos,
            Configuracoes = configuracoes,
            FitaSaida = ExtrairConteudoFita(fita)
        };
    }

    /// <summary>
    /// Imprime no console a tabela de transições δ da MT.
    /// </summary>
    public void ExibirDiagrama()
    {
        Console.WriteLine($"─── Tabela de Transições (δ) — {Nome} ───");
        Console.WriteLine();

        var transicoes = Delta.OrderBy(t => t.Key.estado)
                              .ThenBy(t => t.Key.simbolo);

        int largEstado = "Estado".Length;
        int largLeitura = "Lê".Length;
        int largDestino = "Resultado δ(estado, símbolo)".Length;

        var linhas = new List<(string estado, string leitura, string resultado)>();

        foreach (var kvp in transicoes)
        {
            string est = kvp.Key.estado;
            string leit = kvp.Key.simbolo == BRANCO ? "⊔" : kvp.Key.simbolo.ToString();
            string dir = kvp.Value.direcao == 'L' ? "L" : "R";
            string novoSimb = kvp.Value.novoSimbolo == BRANCO ? "⊔" : kvp.Value.novoSimbolo.ToString();
            string resultado = $"({kvp.Value.novoEstado}, {novoSimb}, {dir})";

            largEstado = Math.Max(largEstado, est.Length);
            largLeitura = Math.Max(largLeitura, leit.Length);
            largDestino = Math.Max(largDestino, resultado.Length);

            linhas.Add((est, leit, resultado));
        }

        string sep = "+" + new string('-', largEstado + 2)
                   + "+" + new string('-', largLeitura + 2)
                   + "+" + new string('-', largDestino + 2) + "+";

        Console.WriteLine(sep);
        Console.WriteLine($"| {"Estado".PadRight(largEstado)} | {"Lê".PadRight(largLeitura)} | {"Resultado δ(estado, símbolo)".PadRight(largDestino)} |");
        Console.WriteLine(sep);

        foreach (var (estado, leitura, resultado) in linhas)
        {
            Console.WriteLine($"| {estado.PadRight(largEstado)} | {leitura.PadRight(largLeitura)} | {resultado.PadRight(largDestino)} |");
        }

        Console.WriteLine(sep);
    }

    /// <summary>
    /// Exibe a definição formal da 7-tupla M = (Q, Σ, Γ, δ, q0, qaccept, qreject).
    /// </summary>
    public void ExibirDefinicaoFormal()
    {
        Console.WriteLine($"─── Definição Formal: M = (Q, Σ, Γ, δ, q0, qaccept, qreject) — {Nome} ───");
        Console.WriteLine($"  Q       (estados):        {{ {string.Join(", ", Q.OrderBy(e => e))} }}");
        Console.WriteLine($"  Σ       (alf. entrada):   {{ {string.Join(", ", Sigma.OrderBy(s => s))} }}");
        Console.WriteLine($"  Γ       (alf. fita):      {{ {string.Join(", ", Gama.OrderBy(g => g))} }}");
        Console.WriteLine($"  q0      (estado inicial): {Q0}");
        Console.WriteLine($"  qaccept (aceitação):      {QAccept}");
        Console.WriteLine($"  qreject (rejeição):       {QReject}");
        Console.WriteLine($"  δ       (transições):     {Delta.Count} transição(ões) definida(s)");
        if (EhComputadora)
            Console.WriteLine($"  Tipo:   MT computadora (exibe fita de saída)");
    }

    // ──────────────────── Utilitários de fita ────────────────────

    /// <summary>
    /// Lê o símbolo na posição do cabeçote. Retorna BRANCO se a posição não foi inicializada.
    /// </summary>
    private static char LerFita(Dictionary<int, char> fita, int posicao)
    {
        return fita.TryGetValue(posicao, out char valor) ? valor : BRANCO;
    }

    /// <summary>
    /// Formata a fita para exibição, com [ ] ao redor do símbolo sob o cabeçote.
    /// Mostra desde a menor posição escrita até a maior, incluindo o cabeçote.
    /// </summary>
    private static string FormatarFita(Dictionary<int, char> fita, int cabecote)
    {
        int min = fita.Count > 0 ? Math.Min(fita.Keys.Min(), cabecote) : cabecote;
        int max = fita.Count > 0 ? Math.Max(fita.Keys.Max(), cabecote) : cabecote;

        // Expande 1 posição de branco em cada lado para contexto
        min = Math.Min(min, cabecote - 1);
        max = Math.Max(max, cabecote + 1);

        var partes = new List<string>();
        for (int i = min; i <= max; i++)
        {
            char c = fita.TryGetValue(i, out char v) ? v : BRANCO;
            if (i == cabecote)
                partes.Add($"[{c}]");
            else
                partes.Add(c.ToString());
        }

        return string.Join(" ", partes);
    }

    /// <summary>
    /// Extrai o conteúdo significativo da fita (sem brancos nas extremidades).
    /// </summary>
    private static string ExtrairConteudoFita(Dictionary<int, char> fita)
    {
        if (fita.Count == 0) return "";

        int min = fita.Keys.Min();
        int max = fita.Keys.Max();

        var chars = new List<char>();
        for (int i = min; i <= max; i++)
        {
            char c = fita.TryGetValue(i, out char v) ? v : BRANCO;
            chars.Add(c);
        }

        return new string(chars.ToArray()).Trim(BRANCO);
    }

    // ──────────────────── Validação ────────────────────

    private void Validar()
    {
        if (!Q.Contains(Q0))
            throw new InvalidOperationException(
                $"Estado inicial '{Q0}' não pertence ao conjunto de estados Q.");

        if (!Q.Contains(QAccept))
            throw new InvalidOperationException(
                $"Estado de aceitação '{QAccept}' não pertence ao conjunto de estados Q.");

        if (!Q.Contains(QReject))
            throw new InvalidOperationException(
                $"Estado de rejeição '{QReject}' não pertence ao conjunto de estados Q.");

        if (QAccept == QReject)
            throw new InvalidOperationException(
                "Estado de aceitação e rejeição não podem ser iguais (qaccept ≠ qreject).");

        if (Sigma.Contains(BRANCO))
            throw new InvalidOperationException(
                "O símbolo de branco '⊔' não pode pertencer ao alfabeto de entrada Σ.");

        if (!Gama.Contains(BRANCO))
            throw new InvalidOperationException(
                "O símbolo de branco '⊔' deve pertencer ao alfabeto da fita Γ.");

        if (!Gama.IsSupersetOf(Sigma))
            throw new InvalidOperationException(
                "O alfabeto da fita Γ deve conter o alfabeto de entrada Σ (Γ ⊇ Σ).");

        foreach (var kvp in Delta)
        {
            var (estado, simbolo) = kvp.Key;
            var (novoEstado, novoSimbolo, direcao) = kvp.Value;

            if (!Q.Contains(estado))
                throw new InvalidOperationException(
                    $"Transição referencia estado '{estado}' que não pertence a Q.");

            if (!Gama.Contains(simbolo))
                throw new InvalidOperationException(
                    $"Transição referencia símbolo '{simbolo}' que não pertence a Γ.");

            if (!Q.Contains(novoEstado))
                throw new InvalidOperationException(
                    $"Transição leva a estado '{novoEstado}' que não pertence a Q.");

            if (!Gama.Contains(novoSimbolo))
                throw new InvalidOperationException(
                    $"Transição escreve símbolo '{novoSimbolo}' que não pertence a Γ.");

            if (direcao != 'L' && direcao != 'R')
                throw new InvalidOperationException(
                    $"Direção '{direcao}' inválida. Deve ser 'L' ou 'R'.");
        }
    }

    // ──────────────────── Factory Methods ────────────────────

    /// <summary>
    /// Constrói a MT para L4 = { aⁿbⁿcⁿ | n ≥ 1 }.
    /// Utiliza estratégia de marcação: a cada iteração marca um 'a' como 'X',
    /// um 'b' como 'Y' e um 'c' como 'Z', depois retorna ao início.
    ///
    /// Estados:
    ///   q0 — procura próximo 'a' não marcado
    ///   q1 — avança à direita buscando 'b' (pula a's e Y's)
    ///   q2 — avança à direita buscando 'c' (pula b's e Z's)
    ///   q3 — volta à esquerda até o início da fita
    ///   q4 — verifica se todos os símbolos foram marcados
    /// </summary>
    public static MT CriarMTL4()
    {
        var q = new HashSet<string> { "q0", "q1", "q2", "q3", "q4", "qaccept", "qreject" };
        var sigma = new HashSet<char> { 'a', 'b', 'c' };
        var gama = new HashSet<char> { 'a', 'b', 'c', 'X', 'Y', 'Z', BRANCO };

        var delta = new Dictionary<(string, char), (string, char, char)>
        {
            // q0: procura próximo 'a' não marcado
            [("q0", 'a')] = ("q1", 'X', 'R'),
            [("q0", 'X')] = ("q0", 'X', 'R'),
            [("q0", 'Y')] = ("q4", 'Y', 'R'),
            [("q0", BRANCO)] = ("qreject", BRANCO, 'R'),

            // q1: avança buscando 'b' (pula a's e Y's)
            [("q1", 'a')] = ("q1", 'a', 'R'),
            [("q1", 'Y')] = ("q1", 'Y', 'R'),
            [("q1", 'b')] = ("q2", 'Y', 'R'),
            [("q1", BRANCO)] = ("qreject", BRANCO, 'R'),
            [("q1", 'Z')] = ("qreject", 'Z', 'R'),

            // q2: avança buscando 'c' (pula b's e Z's)
            [("q2", 'b')] = ("q2", 'b', 'R'),
            [("q2", 'Z')] = ("q2", 'Z', 'R'),
            [("q2", 'c')] = ("q3", 'Z', 'L'),
            [("q2", BRANCO)] = ("qreject", BRANCO, 'R'),

            // q3: volta à esquerda até o início da fita
            [("q3", 'a')] = ("q3", 'a', 'L'),
            [("q3", 'b')] = ("q3", 'b', 'L'),
            [("q3", 'X')] = ("q3", 'X', 'L'),
            [("q3", 'Y')] = ("q3", 'Y', 'L'),
            [("q3", 'Z')] = ("q3", 'Z', 'L'),
            [("q3", BRANCO)] = ("q0", BRANCO, 'R'),

            // q4: verifica se tudo foi marcado
            [("q4", 'Y')] = ("q4", 'Y', 'R'),
            [("q4", 'Z')] = ("q4", 'Z', 'R'),
            [("q4", BRANCO)] = ("qaccept", BRANCO, 'R'),
            [("q4", 'b')] = ("qreject", 'b', 'R'),
            [("q4", 'c')] = ("qreject", 'c', 'R'),
        };

        return new MT(q, sigma, gama, delta, "q0", "qaccept", "qreject",
                      "L4 = { aⁿbⁿcⁿ | n ≥ 1 }");
    }

    /// <summary>
    /// Constrói a MT computadora para f(n) = n + 1 em representação unária.
    /// Entrada: n ocorrências de '1'. Saída: n + 1 ocorrências de '1'.
    ///
    /// Estratégia: avança à direita sobre todos os '1's existentes;
    /// ao encontrar o branco, escreve '1' e aceita.
    /// </summary>
    public static MT CriarMTUnario()
    {
        var q = new HashSet<string> { "q0", "qaccept", "qreject" };
        var sigma = new HashSet<char> { '1' };
        var gama = new HashSet<char> { '1', BRANCO };

        var delta = new Dictionary<(string, char), (string, char, char)>
        {
            [("q0", '1')] = ("q0", '1', 'R'),
            [("q0", BRANCO)] = ("qaccept", '1', 'R'),
        };

        return new MT(q, sigma, gama, delta, "q0", "qaccept", "qreject",
                      "f(n) = n + 1 (unário)", ehComputadora: true);
    }
}
