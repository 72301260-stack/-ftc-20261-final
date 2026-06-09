# FTC 2026/1 - Trabalho Final

Implementação de Máquinas Abstratas: AFD, Autômato de Pilha e Máquina de Turing.

## Integrantes

Amanda Pimentel - 72301260

Emilly Luiza V. Cordeiro - 72301279

## Descrição das Partes

### Parte 1 — Autômato Finito Determinístico (AFD)

Simulador genérico de AFD capaz de reconhecer linguagens regulares. Implementa a linguagem L1 = { w ∈ {a,b}* | w termina com "ab" } e permite carregar qualquer AFD a partir de um arquivo de configuração JSON.

### Parte 2 — Autômato de Pilha (AP)

Simulador de Autômato de Pilha com reconhecimento por pilha vazia. Implementa a linguagem L2 = { aⁿbⁿ | n ≥ 1 } e o desafio L3 = { w ∈ {a,b}* | w = wᴿ, |w| ≥ 1 } (palíndromos).

### Parte 3 — Máquina de Turing (MT)

Simulador de Máquina de Turing com fita dinâmica e exibição passo a passo. Implementa a linguagem L4 = { aⁿbⁿcⁿ | n ≥ 1 } e a função computável f(n) = n + 1 em representação unária.

## Como Compilar e Executar

Requisitos: .NET 8 SDK instalado.

### Parte 1 — AFD

```bash
cd Parte1
dotnet build
dotnet run
```

### Parte 2 — Autômato de Pilha

```bash
cd Parte2
dotnet build
dotnet run
```

### Parte 3 — Máquina de Turing

```bash
cd Parte3
dotnet build
dotnet run
```

## Vídeo de Defesa

[Link para o vídeo no YouTube] <!-- Substituir pelo link real antes da entrega -->
