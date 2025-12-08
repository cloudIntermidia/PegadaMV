using Unity.ServiceLocation;
using Prism.Unity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CommonServiceLocator;
using Prism;
using Prism.Ioc;
using DLToolkit.Forms.Controls;
using MobiliVendas.Core.ViewModels;
using MobiliVendas.Core.Views.Tablet.Acesso;
using MobiliVendas.Core.Views.Tablet.Catalogo;
using MobiliVendas.Core.Views.Tablet.Comunicacao;
using MobiliVendas.Core.Views.Tablet.Gerenciamento;
using MobiliVendas.Core.Views.Tablet.Negocio;
using MobiliVendas.Core.Views.Tablet.Sincronizacao;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Infra.Repositories;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Utils;
using MobiliVendas.Core.Views.Tablet.CatalogoDetalhe;
using MobiliVendas.Core;
using MobiliVendas.Core.Contracts;
using MobiliVendas.Core.Views.Tablet.Shared;
using System.Collections.Generic;
using MobiliVendas.Core.Shared.Enums;
using System.Linq;
using MobiliVendas.Core.Views.Tablet.Carrinho;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Pegada.Core
{
    public partial class App : PrismApplication
    {
        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App() : this(null)
        {
        }

        public App(IPlatformInitializer initializer) : base(initializer)
        {
        }

        protected override async void OnInitialized()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTU4NjUxQDMxMzcyZTMzMmUzMGNpeUdhZ0FrS3lwU0FlL1VxTEtWMVNJVXpwclVYaW1hdDg0UCtTR0MxMG89;MTU4NjUyQDMxMzcyZTMzMmUzMGtQdnNqWGVPV0czNUJBaGJ0TmxDQVFNNEhYWFJZNG1yQlUxdHpwN1hLdXM9;MTU4NjUzQDMxMzcyZTMzMmUzMG1BSi9mT3FiZ2xoUmZLdDdsN2RiL2F2NUxqN3c3Sm9zaFRNWE81a0I4Y2s9;MTU4NjU0QDMxMzcyZTMzMmUzMFVQaTg2RHBtLzM5SWp6NCtqYjFJaHJudStRVDhpUjJ0QnJxak9id1JqMk09;MTU4NjU1QDMxMzcyZTMzMmUzMGl2eDFMYVVXMGYzYVgrcHVKU3paeHlSQkY3WUhvZ1Jpb2I0ZjVXNW1aL1E9;MTU4NjU2QDMxMzcyZTMzMmUzMGVhRkNSSlhYaTVONmRUV294SlRwaXJmZVNlSGhVWDdwbElNOThWeTFMRFU9;MTU4NjU3QDMxMzcyZTMzMmUzMERRdmZ1bTBrSzh6MVQ4MFZveERISFdFWW1xY0w2eXJCZFhhYkMxeVpzaTA9;MTU4NjU4QDMxMzcyZTMzMmUzMGFrWUVaaDBLdmtsN2djQSs4eGRCSWpvdkovZXNvT21vNkxVNWZteFdNSUk9;MTU4NjU5QDMxMzcyZTMzMmUzMGhWWGc3QUdPalpGV3h4eGFKcXNsTzdyUGFvNzhRaFhFTi9zUGZHNjl3Rjg9;MTU4NjYwQDMxMzcyZTMzMmUzMGl2eDFMYVVXMGYzYVgrcHVKU3paeHlSQkY3WUhvZ1Jpb2I0ZjVXNW1aL1E9;NT8mJyc2IWhiZH1gfWN9YmdoYmF8YGJ8ampqanNiYmlmamlmanMDHmghPDQ2ITo8Ezo9JzYhPjo3OjJ9MDw+fTEh");

            InitializeComponent();
            FlowListView.Init();

            Current.Properties["NavigationService"] = NavigationService;
            Current.Properties["AppName"] = "Pegada";

            //var app = new MobiliVendas.Core.App();
            MobiliVendas.Core.Session.USUARIO_LOGADO = new UsuarioCommandResult();
            MobiliVendas.Core.Session.ATENDIMENTO_ATUAL = null;
            MobiliVendas.Core.Session.URI_BUNDLE = "http://br.com.intermidia.MobiliVendas";
            MobiliVendas.Core.Session.TELA_INICIAL = "CatalogoPage";
            MobiliVendas.Core.Session.UsaMetas = false;
            MobiliVendas.Core.Session.MultiMarca = true;
            MobiliVendas.Core.ConfiguracaoVisual.IsCaixaUnica = true;
            MobiliVendas.Core.Session.DATATEMPLATES_CONFIG = new List<MobiliVendas.Core.Models.DataTemplateConfiguracao>()
            {
                new MobiliVendas.Core.Models.DataTemplateConfiguracao() { FlowColumnCount = 4, TipoExibicao = TipoExibicao.Catalogo }
                ,new MobiliVendas.Core.Models.DataTemplateConfiguracao() { FlowColumnCount = 1, TipoExibicao = TipoExibicao.Lista }
            };

            await NavigationService.NavigateAsync(new System.Uri($"{MobiliVendas.Core.Session.URI_BUNDLE}/LoginPage", System.UriKind.Absolute));
        }



        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<CatalogoPage, CatalogoPageViewModel>();
            containerRegistry.RegisterForNavigation<SelecaoMarcaPage, SelecaoMarcaPageViewModel>();
            containerRegistry.RegisterForNavigation<CatalogoDetalheTipo1Page, CatalogoDetalheTipo1PageViewModel>();
            containerRegistry.RegisterForNavigation<CatalogoDetalheTipo2Page, CatalogoDetalheTipo2PageViewModel>();
            containerRegistry.RegisterForNavigation<CatalogoDetalheTipo3Page, CatalogoDetalheTipo3PageViewModel>();
            containerRegistry.RegisterForNavigation<CatalogoDetalheTipo4Page, CatalogoDetalheTipo4PageViewModel>();
            containerRegistry.RegisterForNavigation<CatalogoDetalheZoomPage, CatalogoDetalheZoomPageViewModel>();
            containerRegistry.RegisterForNavigation<Pegada.Core.Views.Carrinho.CarrinhoPage, Pegada.Core.ViewModels.CarrinhoPageViewModel>();
            containerRegistry.RegisterForNavigation<ComunicacaoPage, ComunicacaoPageViewModel>();
            containerRegistry.RegisterForNavigation<GerenciamentoPage, GerenciamentoPageViewModel>();
            containerRegistry.RegisterForNavigation<NegociosPage>();
            containerRegistry.RegisterForNavigation<MetasPage, MetasPageViewModel>();
            containerRegistry.RegisterForNavigation<Pegada.Core.Views.Negocio.PedidoPage, PedidoPageViewModel>();
            containerRegistry.RegisterForNavigation<AlocacaoProdutoPage, AlocacaoProdutoPageViewModel>();
            containerRegistry.RegisterForNavigation<TitulosPage, TitulosPageViewModel>();
            containerRegistry.RegisterForNavigation<PreVenda, PreVendaViewModel>();
            containerRegistry.RegisterForNavigation<ConsultaEstoquePage, ConsultaEstoqueViewModel>();
            containerRegistry.RegisterForNavigation<Pegada.Core.Views.Negocio.CadastroClientePage, Pegada.Core.ViewModels.CadastroClientePageViewModel>();
            containerRegistry.RegisterForNavigation<SincronizacaoPage, SincronizacaoPageViewModel>();
            containerRegistry.RegisterForNavigation<GerenciamentoMapPage, GerenciamentoMapPageViewModel>();
            containerRegistry.Register<AtendimentoUtility>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IFormCadastroClienteView, FormCadastroClienteView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IFormCadastroClienteViewModel, FormCadastroClienteViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.ICadastroClienteNovoViewModel, CadastroClienteNovoViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IEdicaoItemViewModel, EdicaoItemViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IEdicaoItemView, EdicaoGradeView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IEdicaoItemViewModel, EdicaoItemViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.INovoAtendimentoViewModel, Pegada.Core.ViewModels.NovoAtendimentoViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.INovoAtendimentoView, Pegada.Core.Views.Shared.NovoAtendimentoView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IAlterarSenhaViewModel, AlterarSenhaViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IAlterarSenhaView, AlterarSenhaView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.ICarrinhoFechamentoView, Pegada.Core.Views.Carrinho.CarrinhoFechamentoView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.ICarrinhoFechamentoViewModel, Pegada.Core.ViewModels.CarrinhoFechamentoViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.ICopiaPedidoView, CopiarPedidoView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.ICopiaPedidoViewModel, Pegada.Core.ViewModels.CopiarPedidoViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IDistribuirPedidoView, DistribuirPedidoView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IDistribuirPedidoViewModel, DistribuirPedidoViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.ICatalogoZoomView, CatalogoZoomView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.ICatalogoZoomViewModel, CatalogoZoomViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IPedidoPage, Pegada.Core.Views.Negocio.PedidoPage>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IAlocacaoProdutoPage, AlocacaoProdutoPage>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IAlocacaoProdutoViewModel, AlocacaoProdutoPageViewModel>();

            containerRegistry.Register<MobiliVendas.Core.Contracts.ICadastroClientePage, Pegada.Core.Views.Negocio.CadastroClientePage>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IMetaView, MetaView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IMetaViewModel, MetaViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IMetaPage, MetasPage>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IPreVendaPage, PreVenda>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IConsultaEstoqueView, ConsultaEstoquePage>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IMetaPageViewModel, MetasPageViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.ICarrinhoImpressaoView,CarrinhoImpressaoView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IRelatorioExcelView, RelatorioExcelView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.ICarrinhoImpressaoViewModel, CarrinhoImpressaoViewModel>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IRelatorioExcelViewModel, RelatorioExcelViewModel>();

            containerRegistry.Register<MobiliVendas.Core.Contracts.IImportaPlanilhaView, ImportaPlanilhaView>();
            containerRegistry.Register<MobiliVendas.Core.Contracts.IImportaPlanilhaViewModel, ImportaPlanilhaViewModel>();

            containerRegistry.Register<CatalogoEdicaoAutomaticaViewModel>();
            containerRegistry.Register(typeof(IParametroRepository), typeof(ParametroRepository));
            containerRegistry.Register(typeof(IParametroSincronizacaoRepository), typeof(ParametroSincronizacaoRepository));
            containerRegistry.Register(typeof(ISincronizacaoRepository), typeof(SincronizacaoRepository));
            containerRegistry.Register(typeof(IUsuarioRepository), typeof(UsuarioRepository));
            containerRegistry.Register(typeof(INivelRepository), typeof(NivelRepository));
            containerRegistry.Register(typeof(IModeloRepository), typeof(Pegada.Core.Repositories.ModeloRepositoryExtends));
            containerRegistry.Register(typeof(IProdutoRepository), typeof(Pegada.Core.Repositories.ProdutoRepository));
            containerRegistry.Register(typeof(ITabelaPrecoRepository), typeof(TabelaPrecoRepository));
            containerRegistry.Register(typeof(IDerivacaoGradeRepository), typeof(DerivacaoGradeRepository));
            containerRegistry.Register(typeof(IClienteRepository), typeof(Pegada.Core.Repositories.ClienteRepository));
            containerRegistry.Register(typeof(IAtendimentoRepository), typeof(AtendimentoRepository));
            containerRegistry.Register(typeof(ICarrinhoRepository), typeof(Pegada.Core.Repositories.CarrinhoRepository));
            containerRegistry.Register(typeof(IFotoRepository), typeof(FotoRepository));
            containerRegistry.Register(typeof(IPedidoRepository), typeof(Pegada.Core.Repositories.PedidoRepository));
            containerRegistry.Register(typeof(ITituloRepository), typeof(TituloRepository));
            containerRegistry.Register(typeof(ICondicaoPagamentoRepository), typeof(CondicaoPagamentoRepository));
            containerRegistry.Register(typeof(IItemAtendimentoRepository), typeof(ItemAtendimentoRepository));
            containerRegistry.Register(typeof(IMarketingRepository), typeof(MarketingRepository));
            containerRegistry.Register(typeof(ISelecaoClienteView), typeof(SelecaoClienteView));
            containerRegistry.Register(typeof(ISelecaoClienteViewModel), typeof(SelecaoClienteViewModel));
            containerRegistry.Register(typeof(IInicializacaoBanco), typeof(InicializaBancoExtends));
            containerRegistry.Register(typeof(IPoliticaComercialRepository), typeof(PoliticaComercialRepository));
            containerRegistry.Register(typeof(ICarrinhoExtendsRepository), typeof(CarrinhoExtendsRepository));
            containerRegistry.Register(typeof(IKitRepository), typeof(KitRepository));
            containerRegistry.Register(typeof(ITransportadoraRepository), typeof(TransportadoraRepository));
            containerRegistry.Register(typeof(IMetaRepository), typeof(MetaRepository));
            containerRegistry.Register(typeof(IListaGenericaRepository), typeof(ListaGenericaRepository));
            containerRegistry.Register(typeof(IMarcaRepository), typeof(MarcaRepository));
            containerRegistry.Register(typeof(IIdiomaRepository), typeof(IdiomaRepository));
            containerRegistry.Register(typeof(IReportRespository), typeof(ReportRepository));
            containerRegistry.Register(typeof(MobiliVendas.Core.Helpers.ITranslateExtension), typeof(MobiliVendas.Core.Helpers.TranslateExtension));
            containerRegistry.Register(typeof(ICriticaView), typeof(CriticaView));
            containerRegistry.Register(typeof(ICriticaViewModel), typeof(CriticaViewModel));
            containerRegistry.Register(typeof(ITipoPedidoRepository), typeof(TipoPedidoRepository));
            containerRegistry.Register(typeof(ICoeficienteRepository), typeof(CoeficienteRepository));

            containerRegistry.Register(typeof(ISelecaoAtendimentoView), typeof(SelecaoAtendimentoView));
            containerRegistry.Register(typeof(ISelecaoAtendimentoViewModel), typeof(SelecaoAtendimentoViewModel));

            containerRegistry.Register(typeof(IZoomView), typeof(ZoomView));
            containerRegistry.Register(typeof(IZoomViewModel), typeof(ZoomViewModel));

            containerRegistry.Register(typeof(IJustificativaView), typeof(JustificativaView));
            containerRegistry.Register(typeof(IJustificativaViewModel), typeof(JustificativaViewModel));

            var unityServiceLocator = new UnityServiceLocator(containerRegistry.GetContainer());
            ServiceLocator.SetLocatorProvider(() => unityServiceLocator);
        }
    }
}
