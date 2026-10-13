using System;
using Xamarin.Forms;

namespace Pegada.Core.Behaviors
{
    // Cabeçalhos e valores das grades: diminui a fonte até o texto caber na largura da coluna, em até MaxLines
    // linhas (sem MaxLines = 1 linha). O Label só quebra linha em espaço, então palavra maior que a coluna
    // ("Situação", "R$ 5.472,90"...) era truncada. A fonte definida no Label (estilo ou local) é a máxima.
    // A largura do texto é estimada por caractere (sem medir no nativo) pra não disparar novo ciclo de layout.
    public static class AjusteFonte
    {
        private const double FonteMinima = 7;
        private const double PassoFonte = 0.5;
        private const double FatorAlturaLinha = 1.19;
        private const double FatorFonteNormal = 0.94;

        public static readonly BindableProperty AtivoProperty =
            BindableProperty.CreateAttached("Ativo", typeof(bool), typeof(AjusteFonte), false, propertyChanged: OnAtivoChanged);

        private static readonly BindableProperty FonteMaximaProperty =
            BindableProperty.CreateAttached("FonteMaxima", typeof(double), typeof(AjusteFonte), 0d);

        public static bool GetAtivo(BindableObject view) => (bool)view.GetValue(AtivoProperty);

        public static void SetAtivo(BindableObject view, bool value) => view.SetValue(AtivoProperty, value);

        private static void OnAtivoChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is Label label))
                return;

            label.SizeChanged -= OnLabelSizeChanged;
            label.PropertyChanged -= OnLabelPropertyChanged;

            if ((bool)newValue)
            {
                label.SizeChanged += OnLabelSizeChanged;
                label.PropertyChanged += OnLabelPropertyChanged;
            }
        }

        private static void OnLabelSizeChanged(object sender, EventArgs e) => Ajustar((Label)sender);

        private static void OnLabelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Células reaproveitadas das listas trocam só o Text: recalcula a partir da fonte máxima.
            if (e.PropertyName == Label.TextProperty.PropertyName)
                Ajustar((Label)sender);
        }

        private static void Ajustar(Label label)
        {
            if (label.Width <= 0 || label.Height <= 0)
                return;

            // Guarda a fonte original na 1ª passada (todos os atributos do XAML já foram aplicados).
            double maxima = (double)label.GetValue(FonteMaximaProperty);
            if (maxima <= 0)
            {
                maxima = label.FontSize;
                label.SetValue(FonteMaximaProperty, maxima);
            }

            string texto = label.Text;
            double fonte = maxima;
            if (!string.IsNullOrWhiteSpace(texto))
            {
                string[] palavras = texto.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                double largura = label.Width - label.Padding.Left - label.Padding.Right - 2;
                double altura = label.Height - label.Padding.Top - label.Padding.Bottom;
                int maximoLinhas = label.MaxLines > 0 ? label.MaxLines : 1;
                double fator = label.FontAttributes.HasFlag(FontAttributes.Bold) ? 1 : FatorFonteNormal;
                while (fonte > FonteMinima && !Cabe(palavras, fonte, fator, largura, altura, maximoLinhas))
                    fonte -= PassoFonte;
            }

            if (Math.Abs(label.FontSize - fonte) > 0.01)
                label.FontSize = fonte;
        }

        private static bool Cabe(string[] palavras, double fonte, double fator, double largura, double altura, int maximoLinhas)
        {
            int linhasPermitidas = Math.Max(1, Math.Min(maximoLinhas, (int)Math.Floor(altura / (fonte * FatorAlturaLinha))));
            double espaco = LarguraTexto(" ", fonte, fator);
            int linhas = 1;
            double atual = 0;

            foreach (string palavra in palavras)
            {
                double w = LarguraTexto(palavra, fonte, fator);
                if (w > largura)
                    return false;

                if (atual == 0)
                    atual = w;
                else if (atual + espaco + w <= largura)
                    atual += espaco + w;
                else
                {
                    linhas++;
                    atual = w;
                }
            }

            return linhas <= linhasPermitidas;
        }

        // Larguras médias (em fração do tamanho da fonte) da fonte do sistema em negrito; a normal é ~6% mais estreita.
        private static double LarguraTexto(string texto, double fonte, double fator)
        {
            double total = 0;
            foreach (char c in texto)
            {
                if (" .,:;'|!iIlj".IndexOf(c) >= 0) total += 0.30;
                else if ("ftr()-/".IndexOf(c) >= 0) total += 0.40;
                else if ("mwMW%".IndexOf(c) >= 0) total += 0.88;
                else if (char.IsDigit(c)) total += 0.60;
                else if (char.IsUpper(c)) total += 0.68;
                else total += 0.58;
            }
            return total * fonte * fator;
        }
    }
}
