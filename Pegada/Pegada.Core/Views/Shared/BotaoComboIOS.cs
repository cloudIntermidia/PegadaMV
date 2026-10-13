using System.Windows.Input;
using Xamarin.Forms;

namespace Pegada.Core.Views.Shared
{
    // IMComboButton do PegadaIOS: sem filtro, só o título (system 15, cinza escuro); com filtro, o título encolhe
    // para a metade de cima (negrito 12) e o valor escolhido aparece na metade de baixo (12, preto).
    public class BotaoComboIOS : ContentView
    {
        private static readonly Color CinzaEscuro = Color.FromHex("#555555");

        public static readonly BindableProperty TituloProperty =
            BindableProperty.Create(nameof(Titulo), typeof(string), typeof(BotaoComboIOS), null, propertyChanged: (b, o, n) => ((BotaoComboIOS)b).Atualizar());
        public static readonly BindableProperty ValorProperty =
            BindableProperty.Create(nameof(Valor), typeof(string), typeof(BotaoComboIOS), null, propertyChanged: (b, o, n) => ((BotaoComboIOS)b).Atualizar());
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(BotaoComboIOS));
        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(BotaoComboIOS));

        public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
        public string Valor { get => (string)GetValue(ValorProperty); set => SetValue(ValorProperty, value); }
        public ICommand Command { get => (ICommand)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
        public object CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

        private readonly Label _tituloUnico;
        private readonly Label _tituloFiltro;
        private readonly Label _valor;
        private readonly Grid _comFiltro;

        public BotaoComboIOS()
        {
            _tituloUnico = new Label
            {
                FontSize = 15,
                TextColor = CinzaEscuro,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.MiddleTruncation
            };

            _tituloFiltro = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = CinzaEscuro,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            };

            _valor = new Label
            {
                FontSize = 12,
                TextColor = Color.Black,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                Margin = new Thickness(0, 0, 0, 5)
            };

            _comFiltro = new Grid { RowSpacing = 0, Padding = new Thickness(5, 0) };
            _comFiltro.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            _comFiltro.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            _comFiltro.Children.Add(_tituloFiltro, 0, 0);
            _comFiltro.Children.Add(_valor, 0, 1);

            var raiz = new Grid { BackgroundColor = Color.Transparent };
            raiz.Children.Add(_tituloUnico);
            raiz.Children.Add(_comFiltro);
            Content = raiz;

            var toque = new TapGestureRecognizer();
            toque.Tapped += (s, e) =>
            {
                if (Command?.CanExecute(CommandParameter) == true)
                    Command.Execute(CommandParameter);
            };
            GestureRecognizers.Add(toque);

            Atualizar();
        }

        private void Atualizar()
        {
            bool comFiltro = !string.IsNullOrEmpty(Valor);
            _tituloUnico.Text = Titulo;
            _tituloFiltro.Text = Titulo;
            _valor.Text = Valor;
            _tituloUnico.IsVisible = !comFiltro;
            _comFiltro.IsVisible = comFiltro;
        }
    }
}
