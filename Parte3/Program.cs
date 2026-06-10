namespace Parte3;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║   Simulador de Máquina de Turing - Parte 3       ║");
        Console.WriteLine("║   Fundamentos Teóricos da Computação             ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.WriteLine();

        MT mtL4 = MT.CriarMTL4();
        MT mtUnario = MT.CriarMTUnario();

        Console.WriteLine("MTs carregadas com sucesso:");
        Console.WriteLine($"  • {mtL4.Nome}");
        Console.WriteLine($"  • {mtUnario.Nome}");

        bool executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("┌──────────────────────────────────────────┐");
            Console.WriteLine("│            MENU PRINCIPAL                 │");
            Console.WriteLine("├──────────────────────────────────────────┤");
            Console.WriteLine("│  [1] MT para L4 (aⁿbⁿcⁿ)                 │");
            Console.WriteLine("│  [2] MT para f(n) = n + 1 (unário)        │");
            Console.WriteLine("│  [0] Sair                                 │");
            Console.WriteLine("└──────────────────────────────────────────┘");
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
                    MenuMT(mtL4, "entradas_mt.txt");
                    break;
                case "2":
                    MenuMT(mtUnario, "entradas_unario.txt");
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
    /// Submenu de operações para uma MT específica.
    /// </summary>
    static void MenuMT(MT mt, string caminhoEntradas)
    {
        bool noSubmenu = true;
        while (noSubmenu)
        {
            Console.WriteLine();
            Console.WriteLine($"┌──────────────────────────────────────────┐");
            Console.WriteLine($"│  {mt.Nome,-40}│");
            Console.WriteLine($"├──────────────────────────────────────────┤");
            Console.WriteLine($"│  [1] Exibir definição formal (7-tupla)    │");
            Console.WriteLine($"│  [2] Exibir tabela de transições          │");
            Console.WriteLine($"│  [3] Processar arquivo de entradas        │");
            Console.WriteLine($"│  [4] Testar cadeia manualmente            │");
            Console.WriteLine($"│  [5] Configurar limite de passos ({mt.LimitePassos,5})   │");
            Console.WriteLine($"│  [0] Voltar ao menu principal             │");
            Console.WriteLine($"└──────────────────────────────────────────┘");
            Console.Write("Opção: ");

            string? opcao = Console.ReadLine()?.Trim();
            Console.WriteLine();

            if (opcao == null)
            {
                noSubmenu = false;
                break;
            }

            switch (opcao)
            {
                case "1":
                    mt.ExibirDefinicaoFormal();
                    break;
                case "2":
                    mt.ExibirDiagrama();
                    break;
                case "3":
                    ProcessarEntradas(mt, caminhoEntradas);
                    break;
                case "4":
                    TestarCadeiaManual(mt);
                    break;
                case "5":
                    ConfigurarLimite(mt);
                    break;
                case "0":
                    noSubmenu = false;
                    break;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
        }
    }

    /// <summary>
    /// Lê cadeias do arquivo de entradas e exibe para cada uma o rastreio
    /// passo a passo e o resultado.
    /// </summary>
    static void ProcessarEntradas(MT mt, string caminhoEntradas)
    {
        if (!File.Exists(caminhoEntradas))
        {
            Console.WriteLine($"ERRO: Arquivo não encontrado: {caminhoEntradas}");
            return;
        }

        string[] linhas = File.ReadAllLines(caminhoEntradas);
        Console.WriteLine($"─── Processando {linhas.Length} cadeia(s) de {caminhoEntradas} ───");

        int aceitas = 0;
        int rejeitadas = 0;

        foreach (string linha in linhas)
        {
            string cadeia = linha;
            ProcessarCadeia(mt, cadeia, ref aceitas, ref rejeitadas);
        }

        Console.WriteLine();
        if (mt.EhComputadora)
            Console.WriteLine($"  Resumo: {linhas.Length} computação(ões) executada(s)");
        else
            Console.WriteLine($"  Resumo: {aceitas} aceita(s), {rejeitadas} rejeitada(s)");
    }

    /// <summary>
    /// Processa uma única cadeia: exibe configurações passo a passo e resultado.
    /// Para MTs computadoras, exibe a fita de saída em vez de ACEITA/REJEITA.
    /// </summary>
    static void ProcessarCadeia(MT mt, string cadeia, ref int aceitas, ref int rejeitadas)
    {
        string exibicaoCadeia = cadeia.Length == 0 ? "ε (vazia)" : $"\"{cadeia}\"";
        var resultado = mt.Executar(cadeia);

        Console.WriteLine();
        Console.WriteLine($"  ┌─ Cadeia: {exibicaoCadeia}");

        foreach (var config in resultado.Configuracoes)
        {
            string transInfo = config.TransicaoAplicada.Length > 0
                ? $"  ← {config.TransicaoAplicada}"
                : "";
            Console.WriteLine($"  │  Passo {config.Passo,3}: Estado: {config.Estado,-10} | Fita: {config.FitaFormatada}{transInfo}");
        }

        Console.WriteLine($"  │  Total de passos: {resultado.TotalPassos}");

        if (mt.EhComputadora)
        {
            Console.WriteLine($"  └─ Fita de saída: \"{resultado.FitaSaida}\"");
        }
        else
        {
            string marcador;
            string textoResultado;

            if (resultado.LimiteExcedido)
            {
                marcador = "⚠";
                textoResultado = $"LIMITE EXCEDIDO ({mt.LimitePassos} passos)";
            }
            else if (resultado.Aceita)
            {
                marcador = "✓";
                textoResultado = "ACEITA";
                aceitas++;
            }
            else
            {
                marcador = "✗";
                textoResultado = "REJEITA";
                rejeitadas++;
            }

            Console.WriteLine($"  └─ Resultado: {textoResultado} {marcador}");
        }
    }

    /// <summary>
    /// Permite ao usuário digitar uma cadeia e ver o resultado da simulação.
    /// </summary>
    static void TestarCadeiaManual(MT mt)
    {
        Console.Write("Digite a cadeia (ou ENTER para cadeia vazia): ");
        string? entrada = Console.ReadLine();
        string cadeia = entrada ?? "";

        int aceitas = 0, rejeitadas = 0;
        ProcessarCadeia(mt, cadeia, ref aceitas, ref rejeitadas);
    }

    /// <summary>
    /// Permite configurar o limite máximo de passos da MT.
    /// </summary>
    static void ConfigurarLimite(MT mt)
    {
        Console.WriteLine($"Limite atual: {mt.LimitePassos} passos");
        Console.Write("Novo limite (ou ENTER para manter): ");
        string? entrada = Console.ReadLine()?.Trim();

        if (!string.IsNullOrEmpty(entrada) && int.TryParse(entrada, out int novoLimite) && novoLimite > 0)
        {
            mt.LimitePassos = novoLimite;
            Console.WriteLine($"Limite atualizado para {novoLimite} passos.");
        }
        else if (!string.IsNullOrEmpty(entrada))
        {
            Console.WriteLine("Valor inválido. Mantendo limite atual.");
        }
    }
}
