using CommonServiceLocator;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.ListView.XForms;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pegada.Core.Views.Negocio
{
    // Cadastro de Contato em tela cheia aberto pelo "Cadastrar Contato" do formulário de cliente
    // (CadastroContatoViewController com isVemdoCadastroCliente, segue CadastroContatoModal do PegadaIOS).
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CadastroContatoClientePage : ContentPage
    {
        private const double LarguraBloco = 1024;
        private const double AlturaBloco = 550;

        public CadastroContatoClientePage()
        {
            InitializeComponent();
        }

        // Salvar acumula na lista temporária do formulário (gravada junto com o cliente), limpa os campos e
        // continua na tela; Voltar fecha. codPessoaCliente vem preenchido no Editar Cadastro.
        public static async Task AbrirAsync(ObservableCollection<ContatoCommandResult> contatosTemporarios, string codPessoaCliente)
        {
            var viewModel = ServiceLocator.Current.GetInstance<FormCadastroContatoViewModel>();
            viewModel.SetContextoTemporario(contatosTemporarios, emPopup: false, codPessoaCliente: codPessoaCliente);

            await Application.Current.MainPage.Navigation.PushModalAsync(new CadastroContatoClientePage { BindingContext = viewModel });
        }

        // -ShowSelectedDate: o campo só mostra a data depois que o seletor é fechado.
        private void SeletorDataNascimento_Unfocused(object sender, FocusEventArgs e)
        {
            (BindingContext as FormCadastroContatoViewModel)?.ConfirmarDataNascimento();
        }

        // Fonte das células da lista (12 no iOS), multiplicada pelo fator da tela.
        private double _fonteCelula = 12;
        public double FonteCelula
        {
            get => _fonteCelula;
            private set { _fonteCelula = value; OnPropertyChanged(); }
        }

        private readonly Dictionary<View, Rectangle> _framesOriginais = new Dictionary<View, Rectangle>();
        private readonly Dictionary<View, double> _fontesOriginais = new Dictionary<View, double>();
        private readonly Dictionary<View, Thickness> _paddingsOriginais = new Dictionary<View, Thickness>();
        private double _fatorAplicado;

        // O iOS amplia a cena de 1024 por inteiro no iPad grande. Aqui não se usa Scale/TranslationY (deixavam uma faixa
        // branca em cima): cada frame, fonte e espaçamento é multiplicado pelo fator, e o bloco fica preso em Y = 0.
        private void Raiz_SizeChanged(object sender, EventArgs e)
        {
            if (Raiz.Width <= 0 || Raiz.Height <= 0)
                return;

            var fator = Math.Max(1, Math.Min(Raiz.Width / LarguraBloco, Raiz.Height / AlturaBloco));
            var x = Math.Max(0, (Raiz.Width - LarguraBloco * fator) / 2);
            AbsoluteLayout.SetLayoutBounds(Bloco, new Rectangle(x, 0, LarguraBloco * fator, AlturaBloco * fator));

            if (Math.Abs(fator - _fatorAplicado) < 0.001)
                return;

            _fatorAplicado = fator;
            Escalar(Bloco, fator);
            FonteCelula = 12 * fator;
        }

        private void Escalar(AbsoluteLayout layout, double fator)
        {
            foreach (var filho in layout.Children)
            {
                if (!_framesOriginais.TryGetValue(filho, out var r))
                    _framesOriginais[filho] = r = AbsoluteLayout.GetLayoutBounds(filho);

                if (filho is Switch)
                {
                    // Switch tem tamanho fixo: fica no tamanho do iOS centralizado no frame ampliado e cresce pelo próprio Scale.
                    var cx = (r.X + r.Width / 2) * fator;
                    var cy = (r.Y + r.Height / 2) * fator;
                    AbsoluteLayout.SetLayoutBounds(filho, new Rectangle(cx - r.Width / 2, cy - r.Height / 2, r.Width, r.Height));
                    filho.Scale = fator;
                    continue;
                }

                AbsoluteLayout.SetLayoutBounds(filho, new Rectangle(r.X * fator, r.Y * fator, r.Width * fator, r.Height * fator));
                EscalarFonte(filho, fator);

                switch (filho)
                {
                    case Frame frame:
                        if (!_paddingsOriginais.TryGetValue(frame, out var p))
                            _paddingsOriginais[frame] = p = frame.Padding;
                        frame.Padding = new Thickness(p.Left * fator, p.Top * fator, p.Right * fator, p.Bottom * fator);
                        if (frame.Content != null)
                            EscalarFonte(frame.Content, fator);
                        break;
                    case AbsoluteLayout interno:
                        Escalar(interno, fator);
                        break;
                    case SfListView lista:
                        lista.ItemSize = 20 * fator;
                        break;
                }
            }
        }

        private void EscalarFonte(View view, double fator)
        {
            double original;
            switch (view)
            {
                case Label label:
                    if (!_fontesOriginais.TryGetValue(view, out original))
                        _fontesOriginais[view] = original = label.FontSize;
                    label.FontSize = original * fator;
                    break;
                case Entry entry:
                    if (!_fontesOriginais.TryGetValue(view, out original))
                        _fontesOriginais[view] = original = entry.FontSize;
                    entry.FontSize = original * fator;
                    break;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            (BindingContext as FormCadastroContatoViewModel)?.FecharCommand.Execute(null);
            return true;
        }
    }
}
