using System;
using Xamarin.Forms;

namespace Pegada.Core.Behaviors
{
    // Células desenhadas com posições fixas (frames do PegadaIOS, pensadas pro iPad de 9,7"): quando a célula fica
    // maior que o tamanho base (iPad de 11"/13"), aumenta tamanho e posição dos filhos na mesma proporção. Nunca
    // diminui: no iPad de 9,7" (ou célula menor) fica exatamente como no XAML. A fonte dos Labels não muda.
    // Uso: no Grid da célula, EscalaCelula.LarguraBase/AlturaBase; nos filhos, EscalaCelula.Modo quando não for "Tudo".
    public static class EscalaCelula
    {
        public enum ModoEscala
        {
            // Margem e WidthRequest/HeightRequest escalados
            Tudo,
            // Igual a Tudo, mas a margem de baixo fica fixa (ex.: foto que termina acima de labels fixos do rodapé)
            MantemBaixo,
            // Não mexe
            Nenhum
        }

        public static readonly BindableProperty LarguraBaseProperty =
            BindableProperty.CreateAttached("LarguraBase", typeof(double), typeof(EscalaCelula), 0d, propertyChanged: OnBaseChanged);

        public static readonly BindableProperty AlturaBaseProperty =
            BindableProperty.CreateAttached("AlturaBase", typeof(double), typeof(EscalaCelula), 0d);

        public static readonly BindableProperty ModoProperty =
            BindableProperty.CreateAttached("Modo", typeof(ModoEscala), typeof(EscalaCelula), ModoEscala.Tudo);

        private static readonly BindableProperty FatorAtualProperty =
            BindableProperty.CreateAttached("FatorAtual", typeof(double), typeof(EscalaCelula), 1d);

        private static readonly BindableProperty OriginalProperty =
            BindableProperty.CreateAttached("Original", typeof(TamanhoOriginal), typeof(EscalaCelula), null);

        public static double GetLarguraBase(BindableObject view) => (double)view.GetValue(LarguraBaseProperty);

        public static void SetLarguraBase(BindableObject view, double value) => view.SetValue(LarguraBaseProperty, value);

        public static double GetAlturaBase(BindableObject view) => (double)view.GetValue(AlturaBaseProperty);

        public static void SetAlturaBase(BindableObject view, double value) => view.SetValue(AlturaBaseProperty, value);

        public static ModoEscala GetModo(BindableObject view) => (ModoEscala)view.GetValue(ModoProperty);

        public static void SetModo(BindableObject view, ModoEscala value) => view.SetValue(ModoProperty, value);

        private class TamanhoOriginal
        {
            public Thickness Margem;
            public double Largura;
            public double Altura;
        }

        private static void OnBaseChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is Layout<View> layout))
                return;

            layout.SizeChanged -= Layout_SizeChanged;
            if ((double)newValue > 0)
                layout.SizeChanged += Layout_SizeChanged;
        }

        private static void Layout_SizeChanged(object sender, EventArgs e)
        {
            var layout = (Layout<View>)sender;
            if (layout.Width <= 0 || layout.Height <= 0)
                return;

            var fator = layout.Width / GetLarguraBase(layout);
            var alturaBase = GetAlturaBase(layout);
            if (alturaBase > 0)
                fator = Math.Min(fator, layout.Height / alturaBase);

            // Arredondado pra não reaplicar por diferenças de fração de ponto entre células
            fator = Math.Round(Math.Max(1, fator), 2);
            if (Math.Abs((double)layout.GetValue(FatorAtualProperty) - fator) < 0.001)
                return;

            layout.SetValue(FatorAtualProperty, fator);

            foreach (var filho in layout.Children)
            {
                var modo = GetModo(filho);
                if (modo == ModoEscala.Nenhum)
                    continue;

                // Guarda o tamanho do XAML na 1ª vez: a célula é reaproveitada e o fator é sempre sobre o original
                var original = (TamanhoOriginal)filho.GetValue(OriginalProperty);
                if (original == null)
                {
                    original = new TamanhoOriginal { Margem = filho.Margin, Largura = filho.WidthRequest, Altura = filho.HeightRequest };
                    filho.SetValue(OriginalProperty, original);
                }

                var m = original.Margem;
                filho.Margin = new Thickness(m.Left * fator, m.Top * fator, m.Right * fator,
                    modo == ModoEscala.MantemBaixo ? m.Bottom : m.Bottom * fator);

                if (original.Largura > 0)
                    filho.WidthRequest = original.Largura * fator;
                if (original.Altura > 0)
                    filho.HeightRequest = original.Altura * fator;
            }
        }
    }
}
