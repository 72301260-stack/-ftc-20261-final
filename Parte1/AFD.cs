using System.Text.Json;

namespace Parte1;

/// <summary>
/// Representa um Autômato Finito Determinístico (AFD) como a 5-tupla formal:
/// M = (Q, Σ, δ, q0, F)
/// Referência: SIPSER, Michael. Introduction to the Theory of Computation. 3. ed. Cap. 1.
/// </summary>
public class AFD
{
    /// <summary>Conjunto finito de estados (Q)</summary>
    public HashSet<string> Q { get; }

    /// <summary>Alfabeto finito de entrada (Σ)</summary>
    public HashSet<char> Sigma { get; }

    /// <summary>Função de transição δ: Q × Σ → Q</summary>
    public Dictionary<(string estado, char simbolo), string> Delta { get; }

    /// <summary>Estado inicial (q0 ∈ Q)</summary>
    public string Q0 { get; }

    /// <summary>Conjunto de estados de aceitação (F ⊆ Q)</summary>
    public HashSet<string> F { get; }

    /// <summary>
    /// Constrói um AFD a partir dos componentes da 5-tupla.
    /// Após a construção, executa validação de consistência.
    /// </summary>
    public AFD(HashSet<string> q, HashSet<char> sigma,
               Dictionary<(string estado, char simbolo), string> delta,
               string q0, HashSet<string> f)
    {
        Q = q;
        Sigma = sigma;
        Delta = delta;
        Q0 = q0;
        F = f;

        Validar();
    }

    /// <summary>
    /// Simula a leitura da cadeia símbolo a símbolo.
    /// Retorna true se a cadeia é aceita (estado final ∈ F), false caso contrário.
    /// </summary>
    public bool Aceitar(string cadeia)
    {
        string estadoAtual = Q0;

        foreach (char simbolo in cadeia)
        {
            if (!Sigma.Contains(simbolo))
                return false;

            var chave = (estadoAtual, simbolo);
            if (!Delta.ContainsKey(chave))
                return false;

            estadoAtual = Delta[chave];
        }

        return F.Contains(estadoAtual);
    }

    /// <summary>
    /// Retorna o rastro de estados percorridos durante a simulação da cadeia.
    /// O rastro inclui o estado inicial e cada estado alcançado após ler um símbolo.
    /// </summary>
    public List<string> ObterRastro(string cadeia)
    {
        var rastro = new List<string> { Q0 };
        string estadoAtual = Q0;

        foreach (char simbolo in cadeia)
        {
            if (!Sigma.Contains(simbolo))
            {
                rastro.Add("ERRO");
                return rastro;
            }

            var chave = (estadoAtual, simbolo);
            if (!Delta.ContainsKey(chave))
            {
                rastro.Add("ERRO");
                return rastro;
            }

            estadoAtual = Delta[chave];
            rastro.Add(estadoAtual);
        }

        return rastro;
    }

    /// <summary>
    /// Imprime no console uma representação textual da tabela de transições do AFD.
    /// Convenções: '>' marca o estado inicial, '*' marca estados de aceitação.
    /// </summary>
    public void ExibirDiagrama()
    {
        var simbolosOrdenados = Sigma.OrderBy(s => s).ToList();
        var estadosOrdenados = Q.OrderBy(e => e).ToList();

        // Calcula largura das colunas para alinhamento
        int larguraEstado = Math.Max("Estado".Length,
            estadosOrdenados.Max(e => FormatarEstado(e).Length));
        int larguraSimbolo = Math.Max(3,
            estadosOrdenados.SelectMany(e => simbolosOrdenados,
                (e, s) => Delta.ContainsKey((e, s)) ? Delta[(e, s)].Length : 1)
            .Max());

        // Linha separadora
        string separador = "+" + new string('-', larguraEstado + 2);
        foreach (var _ in simbolosOrdenados)
            separador += "+" + new string('-', larguraSimbolo + 2);
        separador += "+";

        Console.WriteLine(separador);

        // Cabeçalho
        string cabecalho = "| " + "Estado".PadRight(larguraEstado) + " ";
        foreach (char s in simbolosOrdenados)
            cabecalho += "| " + s.ToString().PadRight(larguraSimbolo) + " ";
        cabecalho += "|";
        Console.WriteLine(cabecalho);
        Console.WriteLine(separador);

        // Linhas de transição
        foreach (string estado in estadosOrdenados)
        {
            string marcador = FormatarEstado(estado);
            string linha = "| " + marcador.PadRight(larguraEstado) + " ";

            foreach (char s in simbolosOrdenados)
            {
                var chave = (estado, s);
                string destino = Delta.ContainsKey(chave) ? Delta[chave] : "-";
                linha += "| " + destino.PadRight(larguraSimbolo) + " ";
            }
            linha += "|";
            Console.WriteLine(linha);
        }

        Console.WriteLine(separador);
        Console.WriteLine("Legenda: '>' = estado inicial, '*' = estado de aceitação");
    }

    /// <summary>
    /// Carrega um AFD a partir de um arquivo JSON com o esquema:
    /// { estados, alfabeto, estadoInicial, estadosAceitacao, transicoes[] }
    /// </summary>
    public static AFD CarregarDeJson(string caminho)
    {
        if (!File.Exists(caminho))
            throw new FileNotFoundException(
                $"Arquivo de configuração não encontrado: {caminho}");

        string json = File.ReadAllText(caminho);

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new JsonException(
                $"Erro ao interpretar JSON em '{caminho}': {ex.Message}");
        }

        var root = doc.RootElement;

        // Extrai estados (Q)
        var estados = new HashSet<string>();
        foreach (var e in root.GetProperty("estados").EnumerateArray())
            estados.Add(e.GetString()!);

        // Extrai alfabeto (Σ)
        var alfabeto = new HashSet<char>();
        foreach (var s in root.GetProperty("alfabeto").EnumerateArray())
            alfabeto.Add(s.GetString()![0]);

        // Extrai estado inicial (q0)
        string estadoInicial = root.GetProperty("estadoInicial").GetString()!;

        // Extrai estados de aceitação (F)
        var estadosAceitacao = new HashSet<string>();
        foreach (var f in root.GetProperty("estadosAceitacao").EnumerateArray())
            estadosAceitacao.Add(f.GetString()!);

        // Extrai transições (δ)
        var delta = new Dictionary<(string estado, char simbolo), string>();
        foreach (var t in root.GetProperty("transicoes").EnumerateArray())
        {
            string origem = t.GetProperty("origem").GetString()!;
            char simbolo = t.GetProperty("simbolo").GetString()![0];
            string destino = t.GetProperty("destino").GetString()!;

            delta[(origem, simbolo)] = destino;
        }

        return new AFD(estados, alfabeto, delta, estadoInicial, estadosAceitacao);
    }

    /// <summary>
    /// Verifica a consistência da 5-tupla:
    /// - q0 deve pertencer a Q
    /// - F deve ser subconjunto de Q
    /// - Toda transição deve referenciar estados pertencentes a Q
    /// - δ deve ser total (definida para todo par estado × símbolo)
    /// </summary>
    private void Validar()
    {
        if (!Q.Contains(Q0))
            throw new InvalidOperationException(
                $"Estado inicial '{Q0}' não pertence ao conjunto de estados Q.");

        foreach (string f in F)
        {
            if (!Q.Contains(f))
                throw new InvalidOperationException(
                    $"Estado de aceitação '{f}' não pertence ao conjunto de estados Q.");
        }

        // Verifica se δ é total e se todas as transições referenciam estados válidos
        var transicoesEsperadas = new List<string>();
        foreach (string estado in Q)
        {
            foreach (char simbolo in Sigma)
            {
                var chave = (estado, simbolo);
                if (!Delta.ContainsKey(chave))
                {
                    transicoesEsperadas.Add($"δ({estado}, {simbolo})");
                }
                else
                {
                    string destino = Delta[chave];
                    if (!Q.Contains(destino))
                        throw new InvalidOperationException(
                            $"Transição δ({estado}, {simbolo}) = '{destino}': " +
                            $"estado destino não pertence a Q.");
                }
            }
        }

        if (transicoesEsperadas.Count > 0)
            throw new InvalidOperationException(
                "Função de transição δ não é total. Transições faltantes: " +
                string.Join(", ", transicoesEsperadas));
    }

    /// <summary>
    /// Formata o nome do estado com marcadores para exibição na tabela.
    /// </summary>
    private string FormatarEstado(string estado)
    {
        string prefixo = "";
        if (estado == Q0) prefixo += ">";
        if (F.Contains(estado)) prefixo += "*";

        return prefixo + estado;
    }
}
