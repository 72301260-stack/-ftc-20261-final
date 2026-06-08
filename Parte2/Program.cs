namespace Parte2;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║   Simulador de Autômato de Pilha - Parte 2   ║");
        Console.WriteLine("║   Fundamentos Teóricos da Computação         ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
        Console.WriteLine();

        AP apL2 = AP.CriarAPL2();
        AP apL3 = AP.CriarAPL3();

        Console.WriteLine("APs carregados com sucesso:");
        Console.WriteLine($"  • {apL2.Nome}");
        Console.WriteLine($"  • {apL3.Nome}");

        bool executando = true;
        while (executando)
        {
            Console.WriteLine();
            Console.WriteLine("┌────────────────────────────────────────┐");
            Console.WriteLine("│           MENU PRINCIPAL                │");
            Console.WriteLine("├────────────────────────────────────────┤");
            Console.WriteLine("│  [1] AP para L2 (aⁿbⁿ)                 │");
            Console.WriteLine("│  [2] AP para L3 (palíndromos)           │");
            Console.WriteLine("│  [0] Sair                               │");
            Console.WriteLine("└────────────────────────────────────────┘");
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
                    MenuAP(apL2, "entradas_ap.txt");
                    break;
                case "2":
                    MenuAP(apL3, "entradas_palindromos.txt");
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
    /// Submenu de operações para um AP específico.
    /// </summary>
    static void MenuAP(AP ap, string caminhoEntradas)
    {
        bool noSubmenu = true;
        while (noSubmenu)
        {
            Console.WriteLine();
            Console.WriteLine($"┌────────────────────────────────────────┐");
            Console.WriteLine($"│  {ap.Nome,-38}│");
            Console.WriteLine($"├────────────────────────────────────────┤");
            Console.WriteLine($"│  [1] Exibir definição formal (7-tupla)  │");
            Console.WriteLine($"│  [2] Exibir tabela de transições        │");
            Console.WriteLine($"│  [3] Processar arquivo de entradas      │");
            Console.WriteLine($"│  [4] Testar cadeia manualmente          │");
            Console.WriteLine($"│  [0] Voltar ao menu principal           │");
            Console.WriteLine($"└────────────────────────────────────────┘");
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
                    ap.ExibirDefinicaoFormal();
                    break;
                case "2":
                    ap.ExibirDiagrama();
                    break;
                case "3":
                    ProcessarEntradas(ap, caminhoEntradas);
                    break;
                case "4":
                    TestarCadeiaManual(ap);
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
    /// Lê cadeias do arquivo de entradas e exibe para cada uma:
    /// a cadeia, a configuração instantânea passo a passo e o resultado.
    /// </summary>
    static void ProcessarEntradas(AP ap, string caminhoEntradas)
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
            ProcessarCadeia(ap, cadeia, ref aceitas, ref rejeitadas);
        }

        Console.WriteLine();
        Console.WriteLine($"  Resumo: {aceitas} aceita(s), {rejeitadas} rejeitada(s)");
    }

    /// <summary>
    /// Processa uma única cadeia: exibe configurações instantâneas e resultado.
    /// </summary>
    static void ProcessarCadeia(AP ap, string cadeia, ref int aceitas, ref int rejeitadas)
    {
        string exibicaoCadeia = cadeia.Length == 0 ? "ε (vazia)" : $"\"{cadeia}\"";
        bool aceita = ap.Aceitar(cadeia);
        string resultado = aceita ? "ACEITA" : "REJEITA";

        Console.WriteLine();
        Console.WriteLine($"  ┌─ Cadeia: {exibicaoCadeia}");

        var caminho = ap.ObterCaminho(cadeia);
        foreach (var config in caminho)
        {
            string transInfo = config.TransicaoAplicada.Length > 0
                ? $"  ← {config.TransicaoAplicada}"
                : "";
            Console.WriteLine($"  │  Passo {config.Passo}: ({config.Estado}, {config.EntradaRestante}, {config.ConteudoPilha}){transInfo}");
        }

        string marcador = aceita ? "✓" : "✗";
        Console.WriteLine($"  └─ Resultado: {resultado} {marcador}");

        if (aceita) aceitas++;
        else rejeitadas++;
    }

    /// <summary>
    /// Permite ao usuário digitar uma cadeia e ver o resultado da simulação
    /// com as configurações instantâneas completas.
    /// </summary>
    static void TestarCadeiaManual(AP ap)
    {
        Console.Write("Digite a cadeia (ou ENTER para cadeia vazia): ");
        string? entrada = Console.ReadLine();
        string cadeia = entrada ?? "";

        int aceitas = 0, rejeitadas = 0;
        ProcessarCadeia(ap, cadeia, ref aceitas, ref rejeitadas);
    }
}
