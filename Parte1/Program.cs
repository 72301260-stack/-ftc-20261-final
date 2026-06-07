namespace Parte1;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║       Simulador de AFD - Parte 1         ║");
        Console.WriteLine("║  Fundamentos Teóricos da Computação      ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();

        // Carrega o AFD automaticamente a partir do arquivo de configuração
        string caminhoJson = "afd.json";
        string caminhoEntradas = "entradas.txt";

        if (args.Length >= 1) caminhoJson = args[0];
        if (args.Length >= 2) caminhoEntradas = args[1];

        AFD afd;
        try
        {
            afd = AFD.CarregarDeJson(caminhoJson);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"ERRO: {ex.Message}");
            return;
        }
        catch (System.Text.Json.JsonException ex)
        {
            Console.WriteLine($"ERRO: {ex.Message}");
            return;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"ERRO DE VALIDAÇÃO: {ex.Message}");
            return;
        }

        Console.WriteLine($"AFD carregado com sucesso de: {caminhoJson}");
        Console.WriteLine();
        ExibirDefinicaoFormal(afd);

        // Loop principal do menu interativo
        bool executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("┌──────────────────────────────────┐");
            Console.WriteLine("│          MENU PRINCIPAL           │");
            Console.WriteLine("├──────────────────────────────────┤");
            Console.WriteLine("│  [1] Exibir tabela de transições  │");
            Console.WriteLine("│  [2] Processar entradas.txt       │");
            Console.WriteLine("│  [3] Testar cadeia manualmente    │");
            Console.WriteLine("│  [4] Exibir definição formal      │");
            Console.WriteLine("│  [0] Sair                         │");
            Console.WriteLine("└──────────────────────────────────┘");
            Console.Write("Opção: ");

            string? opcao = Console.ReadLine()?.Trim();
            Console.WriteLine();

            if (opcao == null)
            {
                Console.WriteLine("Encerrando simulador. Até logo!");
                break;
            }

            switch (opcao)
            {
                case "1":
                    ExibirDiagrama(afd);
                    break;
                case "2":
                    ProcessarEntradas(afd, caminhoEntradas);
                    break;
                case "3":
                    TestarCadeiaManual(afd);
                    break;
                case "4":
                    ExibirDefinicaoFormal(afd);
                    break;
                case "0":
                    Console.WriteLine("Encerrando simulador. Até logo!");
                    executando = false;
                    break;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
        }
    }

    /// <summary>
    /// Exibe a tabela de transições (diagrama) do AFD no console.
    /// Atende ao requisito (e): void ExibirDiagrama().
    /// </summary>
    static void ExibirDiagrama(AFD afd)
    {
        Console.WriteLine("─── Tabela de Transições (δ) ───");
        Console.WriteLine();
        afd.ExibirDiagrama();
    }

    /// <summary>
    /// Lê múltiplas cadeias do arquivo entradas.txt e exibe para cada uma:
    /// a cadeia, o rastro de estados percorridos e o resultado (ACEITA/REJEITA).
    /// Atende ao requisito (d).
    /// </summary>
    static void ProcessarEntradas(AFD afd, string caminhoEntradas)
    {
        if (!File.Exists(caminhoEntradas))
        {
            Console.WriteLine($"ERRO: Arquivo não encontrado: {caminhoEntradas}");
            return;
        }

        string[] linhas = File.ReadAllLines(caminhoEntradas);
        Console.WriteLine($"─── Processando {linhas.Length} cadeia(s) de {caminhoEntradas} ───");
        Console.WriteLine();

        int aceitas = 0;
        int rejeitadas = 0;

        foreach (string linha in linhas)
        {
            string cadeia = linha;
            string exibicaoCadeia = cadeia.Length == 0 ? "ε (vazia)" : $"\"{cadeia}\"";

            List<string> rastro = afd.ObterRastro(cadeia);
            bool aceita = afd.Aceitar(cadeia);

            string rastroStr = string.Join(" → ", rastro);
            string resultado = aceita ? "ACEITA" : "REJEITA";

            Console.WriteLine($"  Cadeia: {exibicaoCadeia,-14} | Rastro: {rastroStr,-35} | {resultado}");

            if (aceita) aceitas++;
            else rejeitadas++;
        }

        Console.WriteLine();
        Console.WriteLine($"  Resumo: {aceitas} aceita(s), {rejeitadas} rejeitada(s)");
    }

    /// <summary>
    /// Permite ao usuário digitar uma cadeia e ver o resultado da simulação
    /// com o rastro completo de estados.
    /// </summary>
    static void TestarCadeiaManual(AFD afd)
    {
        Console.Write("Digite a cadeia (ou ENTER para cadeia vazia): ");
        string? entrada = Console.ReadLine();
        string cadeia = entrada ?? "";

        string exibicaoCadeia = cadeia.Length == 0 ? "ε (vazia)" : $"\"{cadeia}\"";

        List<string> rastro = afd.ObterRastro(cadeia);
        bool aceita = afd.Aceitar(cadeia);

        string rastroStr = string.Join(" → ", rastro);
        string resultado = aceita ? "ACEITA" : "REJEITA";

        Console.WriteLine();
        Console.WriteLine($"  Cadeia:    {exibicaoCadeia}");
        Console.WriteLine($"  Rastro:    {rastroStr}");
        Console.WriteLine($"  Resultado: {resultado}");
    }

    /// <summary>
    /// Exibe a definição formal da 5-tupla M = (Q, Σ, δ, q0, F) do AFD carregado.
    /// </summary>
    static void ExibirDefinicaoFormal(AFD afd)
    {
        Console.WriteLine("─── Definição Formal: M = (Q, Σ, δ, q0, F) ───");
        Console.WriteLine($"  Q  (estados):      {{ {string.Join(", ", afd.Q.OrderBy(e => e))} }}");
        Console.WriteLine($"  Σ  (alfabeto):     {{ {string.Join(", ", afd.Sigma.OrderBy(s => s))} }}");
        Console.WriteLine($"  q0 (estado inicial): {afd.Q0}");
        Console.WriteLine($"  F  (aceitação):    {{ {string.Join(", ", afd.F.OrderBy(f => f))} }}");
        Console.WriteLine($"  δ  (transições):   {afd.Delta.Count} transição(ões) definida(s)");
    }
}
