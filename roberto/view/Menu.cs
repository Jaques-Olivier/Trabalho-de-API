using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDeskSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var sistema = new SistemaHelpDesk();
            await sistema.IniciarAsync();
        }
    }

    #region Modelos
    public enum TipoUsuario
    {
        Usuario,
        Tecnico,
        Admin
    }

    public enum StatusChamado
    {
        Aberto,
        EmAndamento,
        Aguardando,
        Resolvido,
        Fechado
    }

    public enum PrioridadeChamado
    {
        Baixa,
        Normal,
        Alta,
        Urgente
    }

    public enum CategoriaChamado
    {
        Hardware,
        Software,
        Rede,
        Email,
        Impressora,
        Sistema,
        Outro
    }

    public enum DepartamentoChamado
    {
        TI,
        RH,
        Financeiro,
        Comercial,
        Producao,
        Geral
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public TipoUsuario Tipo { get; set; }
        public DepartamentoChamado Departamento { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }

    public class Chamado
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public int? TecnicoId { get; set; }
        public StatusChamado Status { get; set; } = StatusChamado.Aberto;
        public PrioridadeChamado Prioridade { get; set; } = PrioridadeChamado.Normal;
        public CategoriaChamado Categoria { get; set; }
        public DepartamentoChamado Departamento { get; set; }
        public bool EhUrgente { get; set; }
        public bool EhRemoto { get; set; }
        public DateTime DataAbertura { get; set; } = DateTime.Now;
        public DateTime? DataResolucao { get; set; }
        public List<NotaAtendimento> Notas { get; set; } = new List<NotaAtendimento>();
        public int? TempoResolucaoMinutos { get; set; }
    }

    public class NotaAtendimento
    {
        public int Id { get; set; }
        public int ChamadoId { get; set; }
        public int UsuarioId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public bool EhTecnico { get; set; }
    }

    public class ArtigoConhecimento
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Conteudo { get; set; } = string.Empty;
        public CategoriaChamado Categoria { get; set; }
        public List<string> PalavrasChave { get; set; } = new List<string>();
    }
    #endregion

    #region Serviços
    public class SistemaHelpDesk
    {
        private readonly UsuarioService _usuarioService;
        private readonly ChamadoService _chamadoService;
        private readonly ArtigoService _artigoService;
        private Usuario? _usuarioLogado;

        public SistemaHelpDesk()
        {
            _usuarioService = new UsuarioService();
            _chamadoService = new ChamadoService();
            _artigoService = new ArtigoService();
            InicializarDadosExemplo();
        }

        public async Task IniciarAsync()
        {
            bool continuar = true;

            while (continuar)
            {
                if (_usuarioLogado == null)
                {
                    await MenuLogin();
                }
                else
                {
                    switch (_usuarioLogado.Tipo)
                    {
                        case TipoUsuario.Usuario:
                            await MenuUsuario();
                            break;
                        case TipoUsuario.Tecnico:
                            await MenuTecnico();
                            break;
                        case TipoUsuario.Admin:
                            await MenuAdmin();
                            break;
                    }
                }
            }
        }

        private async Task MenuLogin()
        {
            Console.Clear();
            Console.WriteLine("=== SISTEMA HELP DESK - LOGIN ===");
            Console.WriteLine("1 - Login");
            Console.WriteLine("2 - Registrar Usuário");
            Console.WriteLine("0 - Sair");
            Console.Write("Escolha uma opção: ");

            var opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    await RealizarLogin();
                    break;
                case "2":
                    await RegistrarUsuario();
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    Console.ReadKey();
                    break;
            }
        }

        private async Task RealizarLogin()
        {
            Console.Clear();
            Console.WriteLine("=== LOGIN ===");
            Console.Write("Email: ");
            var email = Console.ReadLine();

            var usuario = await _usuarioService.BuscarPorEmailAsync(email);
            if (usuario != null)
            {
                _usuarioLogado = usuario;
                Console.WriteLine($"Login realizado com sucesso! Bem-vindo, {usuario.Nome}");
            }
            else
            {
                Console.WriteLine("Usuário não encontrado!");
            }
            Console.ReadKey();
        }

        private async Task RegistrarUsuario()
        {
            Console.Clear();
            Console.WriteLine("=== REGISTRAR USUÁRIO ===");
            
            Console.Write("Nome: ");
            var nome = Console.ReadLine();
            
            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.WriteLine("Tipo de usuário:");
            Console.WriteLine("1 - Usuário");
            Console.WriteLine("2 - Técnico");
            Console.Write("Escolha: ");
            
            var tipoInput = Console.ReadLine();
            var tipo = tipoInput == "2" ? TipoUsuario.Tecnico : TipoUsuario.Usuario;

            Console.WriteLine("Departamento:");
            var departamentos = Enum.GetValues<DepartamentoChamado>();
            for (int i = 0; i < departamentos.Length; i++)
            {
                Console.WriteLine($"{i + 1} - {departamentos[i]}");
            }
            Console.Write("Escolha: ");
            
            if (int.TryParse(Console.ReadLine(), out int depIndex) && depIndex > 0 && depIndex <= departamentos.Length)
            {
                var departamento = departamentos[depIndex - 1];
                
                var novoUsuario = new Usuario
                {
                    Nome = nome,
                    Email = email,
                    Tipo = tipo,
                    Departamento = departamento
                };

                var usuario = await _usuarioService.CriarAsync(novoUsuario);
                if (usuario != null)
                {
                    Console.WriteLine("Usuário registrado com sucesso!");
                    _usuarioLogado = usuario;
                }
            }
            else
            {
                Console.WriteLine("Departamento inválido!");
            }
            Console.ReadKey();
        }

        private async Task MenuUsuario()
        {
            Console.Clear();
            Console.WriteLine($"=== MENU USUÁRIO - {_usuarioLogado?.Nome} ===");
            Console.WriteLine("1 - Abrir Chamado");
            Console.WriteLine("2 - Meus Chamados");
            Console.WriteLine("3 - Acompanhar Status");
            Console.WriteLine("4 - Adicionar Nota ao Chamado");
            Console.WriteLine("5 - Buscar Artigos de Conhecimento");
            Console.WriteLine("9 - Logout");
            Console.WriteLine("0 - Sair");
            Console.Write("Escolha uma opção: ");

            var opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    await AbrirChamado();
                    break;
                case "2":
                    await ListarMeusChamados();
                    break;
                case "3":
                    await AcompanharStatus();
                    break;
                case "4":
                    await AdicionarNotaChamado();
                    break;
                case "5":
                    await BuscarArtigos();
                    break;
                case "9":
                    _usuarioLogado = null;
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    Console.ReadKey();
                    break;
            }
        }

        private async Task MenuTecnico()
        {
            Console.Clear();
            Console.WriteLine($"=== MENU TÉCNICO - {_usuarioLogado?.Nome} ===");
            Console.WriteLine("1 - Visualizar Chamados");
            Console.WriteLine("2 - Responder Chamado");
            Console.WriteLine("3 - Alterar Status do Chamado");
            Console.WriteLine("4 - Finalizar Chamado");
            Console.WriteLine("5 - Chamados por Categoria");
            Console.WriteLine("6 - Relatório de Atendimentos");
            Console.WriteLine("9 - Logout");
            Console.WriteLine("0 - Sair");
            Console.Write("Escolha uma opção: ");

            var opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    await VisualizarChamados();
                    break;
                case "2":
                    await ResponderChamado();
                    break;
                case "3":
                    await AlterarStatusChamado();
                    break;
                case "4":
                    await FinalizarChamado();
                    break;
                case "5":
                    await ChamadosPorCategoria();
                    break;
                case "6":
                    await RelatorioAtendimentos();
                    break;
                case "9":
                    _usuarioLogado = null;
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    Console.ReadKey();
                    break;
            }
        }

        private async Task MenuAdmin()
        {
            Console.Clear();
            Console.WriteLine($"=== MENU ADMINISTRADOR - {_usuarioLogado?.Nome} ===");
            Console.WriteLine("1 - Gerenciar Usuários");
            Console.WriteLine("2 - Relatório Geral");
            Console.WriteLine("3 - Gerenciar Artigos");
            Console.WriteLine("4 - Estatísticas do Sistema");
            Console.WriteLine("9 - Logout");
            Console.WriteLine("0 - Sair");
            Console.Write("Escolha uma opção: ");

            var opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    await GerenciarUsuarios();
                    break;
                case "2":
                    await RelatorioGeral();
                    break;
                case "3":
                    await GerenciarArtigos();
                    break;
                case "4":
                    await EstatisticasSistema();
                    break;
                case "9":
                    _usuarioLogado = null;
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    Console.ReadKey();
                    break;
            }
        }

        #region Métodos Usuário
        private async Task AbrirChamado()
        {
            Console.Clear();
            Console.WriteLine("=== ABRIR CHAMADO ===");
            
            Console.Write("Título: ");
            var titulo = Console.ReadLine();
            
            Console.Write("Descrição: ");
            var descricao = Console.ReadLine();

            Console.WriteLine("É urgente? (s/n): ");
            var urgente = Console.ReadLine()?.ToLower() == "s";

            Console.WriteLine("É remoto? (s/n): ");
            var remoto = Console.ReadLine()?.ToLower() == "s";

            Console.WriteLine("Categoria:");
            var categorias = Enum.GetValues<CategoriaChamado>();
            for (int i = 0; i < categorias.Length; i++)
            {
                Console.WriteLine($"{i + 1} - {categorias[i]}");
            }
            Console.Write("Escolha: ");

            if (int.TryParse(Console.ReadLine(), out int catIndex) && catIndex > 0 && catIndex <= categorias.Length)
            {
                var categoria = categorias[catIndex - 1];
                var prioridade = urgente ? PrioridadeChamado.Urgente : PrioridadeChamado.Normal;

                var chamado = new Chamado
                {
                    Titulo = titulo,
                    Descricao = descricao,
                    UsuarioId = _usuarioLogado.Id,
                    EhUrgente = urgente,
                    EhRemoto = remoto,
                    Categoria = categoria,
                    Prioridade = prioridade,
                    Departamento = _usuarioLogado.Departamento
                };

                var novoChamado = await _chamadoService.CriarAsync(chamado);
                if (novoChamado != null)
                {
                    Console.WriteLine($"Chamado aberto com sucesso! ID: {novoChamado.Id}");
                }
            }
            else
            {
                Console.WriteLine("Categoria inválida!");
            }
            Console.ReadKey();
        }

        private async Task ListarMeusChamados()
        {
            Console.Clear();
            Console.WriteLine("=== MEUS CHAMADOS ===");
            
            var chamados = await _chamadoService.BuscarPorUsuarioAsync(_usuarioLogado.Id);
            
            if (chamados.Any())
            {
                foreach (var chamado in chamados)
                {
                    Console.WriteLine($"ID: {chamado.Id} | {chamado.Titulo}");
                    Console.WriteLine($"Status: {chamado.Status} | Prioridade: {chamado.Prioridade}");
                    Console.WriteLine($"Data: {chamado.DataAbertura:dd/MM/yyyy HH:mm}");
                    Console.WriteLine("---");
                }
            }
            else
            {
                Console.WriteLine("Nenhum chamado encontrado.");
            }
            Console.ReadKey();
        }

        private async Task AcompanharStatus()
        {
            Console.Clear();
            Console.WriteLine("=== ACOMPANHAR STATUS ===");
            Console.Write("ID do Chamado: ");
            
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var chamado = await _chamadoService.BuscarPorIdAsync(id);
                if (chamado != null && chamado.UsuarioId == _usuarioLogado.Id)
                {
                    Console.WriteLine($"\nChamado: {chamado.Titulo}");
                    Console.WriteLine($"Status: {chamado.Status}");
                    Console.WriteLine($"Prioridade: {chamado.Prioridade}");
                    Console.WriteLine($"Categoria: {chamado.Categoria}");
                    Console.WriteLine($"Data Abertura: {chamado.DataAbertura:dd/MM/yyyy HH:mm}");
                    
                    if (chamado.TecnicoId.HasValue)
                    {
                        var tecnico = await _usuarioService.BuscarPorIdAsync(chamado.TecnicoId.Value);
                        Console.WriteLine($"Técnico: {tecnico?.Nome}");
                    }

                    Console.WriteLine("\n=== HISTÓRICO ===");
                    foreach (var nota in chamado.Notas.OrderBy(n => n.DataCriacao))
                    {
                        var autor = await _usuarioService.BuscarPorIdAsync(nota.UsuarioId);
                        var tipo = nota.EhTecnico ? "[TÉCNICO]" : "[USUÁRIO]";
                        Console.WriteLine($"{nota.DataCriacao:dd/MM HH:mm} - {tipo} {autor?.Nome}: {nota.Descricao}");
                    }
                }
                else
                {
                    Console.WriteLine("Chamado não encontrado ou você não tem permissão.");
                }
            }
            Console.ReadKey();
        }

        private async Task AdicionarNotaChamado()
        {
            Console.Clear();
            Console.WriteLine("=== ADICIONAR NOTA ===");
            Console.Write("ID do Chamado: ");
            
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var chamado = await _chamadoService.BuscarPorIdAsync(id);
                if (chamado != null && chamado.UsuarioId == _usuarioLogado.Id)
                {
                    Console.Write("Nota: ");
                    var descricao = Console.ReadLine();

                    var nota = new NotaAtendimento
                    {
                        ChamadoId = id,
                        UsuarioId = _usuarioLogado.Id,
                        Descricao = descricao,
                        EhTecnico = false
                    };

                    await _chamadoService.AdicionarNotaAsync(nota);
                    Console.WriteLine("Nota adicionada com sucesso!");
                }
                else
                {
                    Console.WriteLine("Chamado não encontrado ou você não tem permissão.");
                }
            }
            Console.ReadKey();
        }
        #endregion

        #region Métodos Técnico
        private async Task VisualizarChamados()
        {
            Console.Clear();
            Console.WriteLine("=== CHAMADOS DISPONÍVEIS ===");
            
            var chamados = await _chamadoService.ListarTodosAsync();
            var chamadosAbertos = chamados.Where(c => c.Status != StatusChamado.Fechado).OrderByDescending(c => c.Prioridade).ThenBy(c => c.DataAbertura);
            
            foreach (var chamado in chamadosAbertos)
            {
                var usuario = await _usuarioService.BuscarPorIdAsync(chamado.UsuarioId);
                Console.WriteLine($"ID: {chamado.Id} | {chamado.Titulo}");
                Console.WriteLine($"Usuário: {usuario?.Nome} | Depto: {chamado.Departamento}");
                Console.WriteLine($"Status: {chamado.Status} | Prioridade: {chamado.Prioridade}");
                Console.WriteLine($"Categoria: {chamado.Categoria} | Remoto: {(chamado.EhRemoto ? "Sim" : "Não")}");
                Console.WriteLine($"Data: {chamado.DataAbertura:dd/MM/yyyy HH:mm}");
                Console.WriteLine("---");
            }
            Console.ReadKey();
        }

        private async Task ResponderChamado()
        {
            Console.Clear();
            Console.WriteLine("=== RESPONDER CHAMADO ===");
            Console.Write("ID do Chamado: ");
            
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var chamado = await _chamadoService.BuscarPorIdAsync(id);
                if (chamado != null)
                {
                    Console.WriteLine($"Chamado: {chamado.Titulo}");
                    Console.WriteLine($"Descrição: {chamado.Descricao}");
                    Console.Write("\nSua resposta: ");
                    var resposta = Console.ReadLine();

                    // Assumir o chamado se ainda não tiver técnico
                    if (!chamado.TecnicoId.HasValue)
                    {
                        chamado.TecnicoId = _usuarioLogado.Id;
                        chamado.Status = StatusChamado.EmAndamento;
                        await _chamadoService.AtualizarAsync(chamado);
                    }

                    var nota = new NotaAtendimento
                    {
                        ChamadoId = id,
                        UsuarioId = _usuarioLogado.Id,
                        Descricao = resposta,
                        EhTecnico = true
                    };

                    await _chamadoService.AdicionarNotaAsync(nota);
                    Console.WriteLine("Resposta adicionada com sucesso!");
                }
                else
                {
                    Console.WriteLine("Chamado não encontrado.");
                }
            }
            Console.ReadKey();
        }

        private async Task AlterarStatusChamado()
        {
            Console.Clear();
            Console.WriteLine("=== ALTERAR STATUS ===");
            Console.Write("ID do Chamado: ");
            
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var chamado = await _chamadoService.BuscarPorIdAsync(id);
                if (chamado != null)
                {
                    Console.WriteLine($"Chamado: {chamado.Titulo}");
                    Console.WriteLine($"Status atual: {chamado.Status}");
                    
                    Console.WriteLine("\nNovo status:");
                    var statuses = Enum.GetValues<StatusChamado>();
                    for (int i = 0; i < statuses.Length; i++)
                    {
                        Console.WriteLine($"{i + 1} - {statuses[i]}");
                    }
                    Console.Write("Escolha: ");

                    if (int.TryParse(Console.ReadLine(), out int statusIndex) && statusIndex > 0 && statusIndex <= statuses.Length)
                    {
                        chamado.Status = statuses[statusIndex - 1];
                        await _chamadoService.AtualizarAsync(chamado);
                        Console.WriteLine("Status alterado com sucesso!");
                    }
                }
                else
                {
                    Console.WriteLine("Chamado não encontrado.");
                }
            }
            Console.ReadKey();
        }

        private async Task FinalizarChamado()
        {
            Console.Clear();
            Console.WriteLine("=== FINALIZAR CHAMADO ===");
            Console.Write("ID do Chamado: ");
            
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var chamado = await _chamadoService.BuscarPorIdAsync(id);
                if (chamado != null && chamado.TecnicoId == _usuarioLogado.Id)
                {
                    Console.Write("Descreva o que foi feito: ");
                    var solucao = Console.ReadLine();
                    
                    Console.Write("Tempo gasto (em minutos): ");
                    if (int.TryParse(Console.ReadLine(), out int tempo))
                    {
                        chamado.Status = StatusChamado.Resolvido;
                        chamado.DataResolucao = DateTime.Now;
                        chamado.TempoResolucaoMinutos = tempo;

                        var nota = new NotaAtendimento
                        {
                            ChamadoId = id,
                            UsuarioId = _usuarioLogado.Id,
                            Descricao = $"SOLUÇÃO: {solucao}",
                            EhTecnico = true
                        };

                        await _chamadoService.AdicionarNotaAsync(nota);
                        await _chamadoService.AtualizarAsync(chamado);
                        
                        Console.WriteLine("Chamado finalizado com sucesso!");
                    }
                }
                else
                {
                    Console.WriteLine("Chamado não encontrado ou você não é o técnico responsável.");
                }
            }
            Console.ReadKey();
        }

        private async Task ChamadosPorCategoria()
        {
            Console.Clear();
            Console.WriteLine("=== CHAMADOS POR CATEGORIA ===");
            
            var chamados = await _chamadoService.ListarTodosAsync();
            var grupos = chamados.GroupBy(c => c.Categoria);

            foreach (var grupo in grupos.OrderBy(g => g.Key))
            {
                Console.WriteLine($"\n{grupo.Key}: {grupo.Count()} chamados");
                foreach (var chamado in grupo.Take(5))
                {
                    Console.WriteLine($"  - ID: {chamado.Id} | {chamado.Titulo} | {chamado.Status}");
                }
                if (grupo.Count() > 5)
                {
                    Console.WriteLine($"  ... e mais {grupo.Count() - 5} chamados");
                }
            }
            Console.ReadKey();
        }

        private async Task RelatorioAtendimentos()
        {
            Console.Clear();
            Console.WriteLine("=== RELATÓRIO DE ATENDIMENTOS ===");
            
            var chamados = await _chamadoService.BuscarPorTecnicoAsync(_usuarioLogado.Id);
            
            Console.WriteLine($"Total de chamados atendidos: {chamados.Count()}");
            Console.WriteLine($"Chamados resolvidos: {chamados.Count(c => c.Status == StatusChamado.Resolvido)}");
            Console.WriteLine($"Chamados em andamento: {chamados.Count(c => c.Status == StatusChamado.EmAndamento)}");
            
            var tempoMedio = chamados.Where(c => c.TempoResolucaoMinutos.HasValue)
                                   .Average(c => c.TempoResolucaoMinutos.Value);
            
            if (tempoMedio > 0)
            {
                Console.WriteLine($"Tempo médio de resolução: {tempoMedio:F1} minutos");
            }

            Console.ReadKey();
        }
        #endregion

        #region Métodos Genéricos
        private async Task BuscarArtigos()
        {
            Console.Clear();
            Console.WriteLine("=== BUSCAR ARTIGOS ===");
            Console.Write("Palavra-chave: ");
            var palavra = Console.ReadLine();

            var artigos = await _artigoService.BuscarAsync(palavra);
            
            if (artigos.Any())
            {
                foreach (var artigo in artigos)
                {
                    Console.WriteLine($"\n=== {artigo.Titulo} ===");
                    Console.WriteLine($"Categoria: {artigo.Categoria}");
                    Console.WriteLine(artigo.Conteudo);
                    Console.WriteLine("---");
                }
            }
            else
            {
                Console.WriteLine("Nenhum artigo encontrado.");
            }
            Console.ReadKey();
        }

        private async Task GerenciarUsuarios()
        {
            Console.Clear();
            Console.WriteLine("=== GERENCIAR USUÁRIOS ===");
            
            var usuarios = await _usuarioService.ListarTodosAsync();
            foreach (var user in usuarios)
            {
                Console.WriteLine($"ID: {user.Id} | {user.Nome} | {user.Email} | {user.Tipo} | {user.Departamento}");
            }
            Console.ReadKey();
        }

        private async Task RelatorioGeral()
        {
            Console.Clear();
            Console.WriteLine("=== RELATÓRIO GERAL ===");
            
            var chamados = await _chamadoService.ListarTodosAsync();
            var usuarios = await _usuarioService.ListarTodosAsync();

            Console.WriteLine($"Total de usuários: {usuarios.Count()}");
            Console.WriteLine($"Total de técnicos: {usuarios.Count(u => u.Tipo == TipoUsuario.Tecnico)}");
            Console.WriteLine($"Total de chamados: {chamados.Count()}");
            Console.WriteLine($"Chamados abertos: {chamados.Count(c => c.Status == StatusChamado.Aberto)}");
            Console.WriteLine($"Chamados resolvidos: {chamados.Count(c => c.Status == StatusChamado.Resolvido)}");
            Console.WriteLine($"Chamados urgentes: {chamados.Count(c => c.EhUrgente)}");

            Console.WriteLine("\nChamados por departamento:");
            var porDepartamento = chamados.GroupBy(c => c.Departamento);
            foreach (var grupo in porDepartamento)
            {
                Console.WriteLine($"  {grupo.Key}: {grupo.Count()}");
            }

            Console.ReadKey();
        }

        private async Task GerenciarArtigos()
        {
            Console.Clear();
            Console.WriteLine("=== GERENCIAR ARTIGOS ===");
            
            var artigos = await _artigoService.ListarTodosAsync();
            Console.WriteLine($"Total de artigos: {artigos.Count()}");
            
            foreach (var artigo in artigos)
            {
                Console.WriteLine($"ID: {artigo.Id} | {artigo.Titulo} | {artigo.Categoria}");
            }
            Console.ReadKey();
        }

        private async Task EstatisticasSistema()
        {
            Console.Clear();
            Console.WriteLine("=== ESTATÍSTICAS DO SISTEMA ===");
            
            var chamados = await _chamadoService.ListarTodosAsync();
            
            Console.WriteLine("Chamados por categoria:");
            var porCategoria = chamados.GroupBy(c => c.Categoria);
            foreach (var grupo in porCategoria.OrderByDescending(g => g.Count()))
            {
                Console.WriteLine($"  {grupo.Key}: {grupo.Count()}");
            }

            Console.WriteLine("\nChamados por prioridade:");
            var porPrioridade = chamados.GroupBy(c => c.Prioridade);
            foreach (var grupo in porPrioridade.OrderByDescending(g => g.Count()))
            {
                Console.WriteLine($"  {grupo.Key}: {grupo.Count()}");
            }

            var tempoMedioGeral = chamados.Where(c => c.TempoResolucaoMinutos.HasValue)
                                        .Average(c => c.TempoResolucaoMinutos.Value);
            
            if (tempoMedioGeral > 0)
            {
                Console.WriteLine($"\nTempo médio de resolução geral: {tempoMedioGeral:F1} minutos");
            }

            Console.ReadKey();
        }
        #endregion

        private void InicializarDadosExemplo()
        {
            // Criar usuários exemplo
            var admin = new Usuario
            {
                Id = 1,
                Nome = "Administrador",
                Email = "admin@empresa.com",
                Tipo = TipoUsuario.Admin,
                Departamento = DepartamentoChamado.TI
            };

            var tecnico1 = new Usuario
            {
                Id = 2,
                Nome = "João Técnico",
                Email = "joao.tecnico@empresa.com",
                Tipo = TipoUsuario.Tecnico,
                Departamento = DepartamentoChamado.TI
            };

            var tecnico2 = new Usuario
            {
                Id = 3,
                Nome = "Maria Suporte",
                Email = "maria.suporte@empresa.com",
                Tipo = TipoUsuario.Tecnico,
                Departamento = DepartamentoChamado.TI
            };

            var usuario1 = new Usuario
            {
                Id = 4,
                Nome = "Carlos Silva",
                Email = "carlos@empresa.com",
                Tipo = TipoUsuario.Usuario,
                Departamento = DepartamentoChamado.Comercial
            };

            var usuario2 = new Usuario
            {
                Id = 5,
                Nome = "Ana Santos",
                Email = "ana@empresa.com",
                Tipo = TipoUsuario.Usuario,
                Departamento = DepartamentoChamado.RH
            };

            _usuarioService.AdicionarExemplo(admin);
            _usuarioService.AdicionarExemplo(tecnico1);
            _usuarioService.AdicionarExemplo(tecnico2);
            _usuarioService.AdicionarExemplo(usuario1);
            _usuarioService.AdicionarExemplo(usuario2);

            // Criar artigos exemplo
            var artigos = new List<ArtigoConhecimento>
            {
                new ArtigoConhecimento
                {
                    Id = 1,
                    Titulo = "Como resolver problemas de impressão",
                    Categoria = CategoriaChamado.Impressora,
                    Conteudo = "1. Verificar se a impressora está ligada\n2. Verificar cabos de conexão\n3. Verificar fila de impressão\n4. Reiniciar spooler de impressão",
                    PalavrasChave = new List<string> { "impressora", "imprimir", "papel", "toner" }
                },
                new ArtigoConhecimento
                {
                    Id = 2,
                    Titulo = "Problemas de conexão com a internet",
                    Categoria = CategoriaChamado.Rede,
                    Conteudo = "1. Verificar cabo de rede\n2. Testar ping para o gateway\n3. Verificar configurações de IP\n4. Reiniciar adaptador de rede",
                    PalavrasChave = new List<string> { "internet", "rede", "conexão", "ip" }
                },
                new ArtigoConhecimento
                {
                    Id = 3,
                    Titulo = "Recuperação de senha do email",
                    Categoria = CategoriaChamado.Email,
                    Conteudo = "1. Acessar portal de recuperação\n2. Informar email corporativo\n3. Verificar email de recuperação\n4. Criar nova senha",
                    PalavrasChave = new List<string> { "email", "senha", "outlook", "recuperar" }
                }
            };

            foreach (var artigo in artigos)
            {
                _artigoService.AdicionarExemplo(artigo);
            }

            // Criar chamados exemplo
            var chamados = new List<Chamado>
            {
                new Chamado
                {
                    Id = 1,
                    Titulo = "Computador não liga",
                    Descricao = "Meu computador não está ligando desde ontem",
                    UsuarioId = 4,
                    Categoria = CategoriaChamado.Hardware,
                    Departamento = DepartamentoChamado.Comercial,
                    Prioridade = PrioridadeChamado.Alta,
                    EhUrgente = true,
                    EhRemoto = false,
                    Status = StatusChamado.Aberto
                },
                new Chamado
                {
                    Id = 2,
                    Titulo = "Não consigo acessar o email",
                    Descricao = "Esqueci minha senha do Outlook",
                    UsuarioId = 5,
                    TecnicoId = 2,
                    Categoria = CategoriaChamado.Email,
                    Departamento = DepartamentoChamado.RH,
                    Prioridade = PrioridadeChamado.Normal,
                    EhUrgente = false,
                    EhRemoto = true,
                    Status = StatusChamado.EmAndamento
                }
            };

            foreach (var chamado in chamados)
            {
                _chamadoService.AdicionarExemplo(chamado);
            }
        }
    }

    public class UsuarioService
    {
        private List<Usuario> _usuarios = new List<Usuario>();
        private int _proximoId = 1;

        public async Task<List<Usuario>> ListarTodosAsync()
        {
            await Task.Delay(100);
            return _usuarios.ToList();
        }

        public async Task<Usuario?> BuscarPorIdAsync(int id)
        {
            await Task.Delay(50);
            return _usuarios.FirstOrDefault(u => u.Id == id);
        }

        public async Task<Usuario?> BuscarPorEmailAsync(string email)
        {
            await Task.Delay(50);
            return _usuarios.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Usuario?> CriarAsync(Usuario usuario)
        {
            await Task.Delay(100);
            usuario.Id = _proximoId++;
            _usuarios.Add(usuario);
            return usuario;
        }

        public void AdicionarExemplo(Usuario usuario)
        {
            _usuarios.Add(usuario);
            if (usuario.Id >= _proximoId)
                _proximoId = usuario.Id + 1;
        }
    }

    public class ChamadoService
    {
        private List<Chamado> _chamados = new List<Chamado>();
        private List<NotaAtendimento> _notas = new List<NotaAtendimento>();
        private int _proximoId = 1;
        private int _proximoIdNota = 1;

        public async Task<List<Chamado>> ListarTodosAsync()
        {
            await Task.Delay(100);
            var chamados = _chamados.ToList();
            foreach (var chamado in chamados)
            {
                chamado.Notas = _notas.Where(n => n.ChamadoId == chamado.Id).ToList();
            }
            return chamados;
        }

        public async Task<Chamado?> BuscarPorIdAsync(int id)
        {
            await Task.Delay(50);
            var chamado = _chamados.FirstOrDefault(c => c.Id == id);
            if (chamado != null)
            {
                chamado.Notas = _notas.Where(n => n.ChamadoId == id).ToList();
            }
            return chamado;
        }

        public async Task<List<Chamado>> BuscarPorUsuarioAsync(int usuarioId)
        {
            await Task.Delay(100);
            var chamados = _chamados.Where(c => c.UsuarioId == usuarioId).ToList();
            foreach (var chamado in chamados)
            {
                chamado.Notas = _notas.Where(n => n.ChamadoId == chamado.Id).ToList();
            }
            return chamados;
        }

        public async Task<List<Chamado>> BuscarPorTecnicoAsync(int tecnicoId)
        {
            await Task.Delay(100);
            var chamados = _chamados.Where(c => c.TecnicoId == tecnicoId).ToList();
            foreach (var chamado in chamados)
            {
                chamado.Notas = _notas.Where(n => n.ChamadoId == chamado.Id).ToList();
            }
            return chamados;
        }

        public async Task<Chamado?> CriarAsync(Chamado chamado)
        {
            await Task.Delay(100);
            chamado.Id = _proximoId++;
            _chamados.Add(chamado);
            return chamado;
        }

        public async Task<bool> AtualizarAsync(Chamado chamado)
        {
            await Task.Delay(100);
            var existente = _chamados.FirstOrDefault(c => c.Id == chamado.Id);
            if (existente != null)
            {
                existente.Status = chamado.Status;
                existente.TecnicoId = chamado.TecnicoId;
                existente.DataResolucao = chamado.DataResolucao;
                existente.TempoResolucaoMinutos = chamado.TempoResolucaoMinutos;
                return true;
            }
            return false;
        }

        public async Task<NotaAtendimento?> AdicionarNotaAsync(NotaAtendimento nota)
        {
            await Task.Delay(50);
            nota.Id = _proximoIdNota++;
            _notas.Add(nota);
            return nota;
        }

        public void AdicionarExemplo(Chamado chamado)
        {
            _chamados.Add(chamado);
            if (chamado.Id >= _proximoId)
                _proximoId = chamado.Id + 1;
        }
    }

    public class ArtigoService
    {
        private List<ArtigoConhecimento> _artigos = new List<ArtigoConhecimento>();
        private int _proximoId = 1;

        public async Task<List<ArtigoConhecimento>> ListarTodosAsync()
        {
            await Task.Delay(50);
            return _artigos.ToList();
        }

        public async Task<List<ArtigoConhecimento>> BuscarAsync(string palavraChave)
        {
            await Task.Delay(100);
            if (string.IsNullOrWhiteSpace(palavraChave))
                return _artigos.ToList();

            var palavra = palavraChave.ToLower();
            return _artigos.Where(a => 
                a.Titulo.ToLower().Contains(palavra) ||
                a.Conteudo.ToLower().Contains(palavra) ||
                a.PalavrasChave.Any(p => p.ToLower().Contains(palavra))
            ).ToList();
        }

        public async Task<ArtigoConhecimento?> CriarAsync(ArtigoConhecimento artigo)
        {
            await Task.Delay(100);
            artigo.Id = _proximoId++;
            _artigos.Add(artigo);
            return artigo;
        }

        public void AdicionarExemplo(ArtigoConhecimento artigo)
        {
            _artigos.Add(artigo);
            if (artigo.Id >= _proximoId)
                _proximoId = artigo.Id + 1;
        }
    }
    #endregion
}