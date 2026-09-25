using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MobiliVendas.Core;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Domain.StaticObject;
using MobiliVendas.Core.Services.Contracts;
using MobiliVendas.Core.Utils;
using Pegada.Core.Repositories;
using Xamarin.Essentials;

namespace Pegada.Core.Services
{
    // E-mail automático do pedido transmitido, como no CarrinhoViewController do PegadaIOS
    // (enviaPedidosPorEmail -> enviaEmailPedidoAutomatico -> montaPedidoEmail): junta os e-mails dos contatos com
    // "cópia do pedido", pergunta se envia e abre o compositor de e-mail com o PDF dos pedidos em anexo.
    public class PedidoEmailAutomatico
    {
        private const string Sistema = "Pegada";

        private readonly EmailPedidoRepository _emailRepository;
        private readonly IPrintService _printService;
        private readonly IClienteRepository _clienteRepository;
        private readonly IFotoRepository _fotoRepository;
        private readonly IParametroSincronizacaoRepository _parametroSincronizacaoRepository;

        public PedidoEmailAutomatico(EmailPedidoRepository emailRepository, IPrintService printService, IClienteRepository clienteRepository,
                                     IFotoRepository fotoRepository, IParametroSincronizacaoRepository parametroSincronizacaoRepository)
        {
            _emailRepository = emailRepository;
            _printService = printService;
            _clienteRepository = clienteRepository;
            _fotoRepository = fotoRepository;
            _parametroSincronizacaoRepository = parametroSincronizacaoRepository;
        }

        // enviaEmailPedidoAutomatico:
        public async Task EnviarAsync(List<CarrinhoCommandResult> pedidos)
        {
            if (pedidos == null || pedidos.Count == 0)
                return;

            var textos = Textos.Do(Session.Idioma);

            var emails = new List<string>();
            foreach (var pedido in pedidos)
            {
                foreach (var email in await _emailRepository.BuscarEmailsCopiaPedido(pedido.CodPessoaCliente))
                {
                    if (!emails.Any(e => string.Equals(e, email, StringComparison.OrdinalIgnoreCase)))
                        emails.Add(email);
                }
            }

            if (emails.Count == 0)
            {
                await UserDialogs.Instance.AlertAsync(textos.EmailNaoEncontrado, textos.TituloTransmissao, "OK");
                return;
            }

            bool enviar = await UserDialogs.Instance.ConfirmAsync(
                $"{textos.PerguntaEnviar} {string.Join("; ", emails)} ?", textos.TituloTransmissao, textos.Enviar, textos.Nao);
            if (!enviar)
                return;

            await MontarEmailAsync(pedidos, emails, textos);
        }

        // montaPedidoEmail:
        private async Task MontarEmailAsync(List<CarrinhoCommandResult> pedidos, List<string> emails, Textos textos)
        {
            try
            {
                UserDialogs.Instance.ShowLoading(textos.GerandoPdf);
                string pdf = await GerarPdfAsync(pedidos);
                UserDialogs.Instance.HideLoading();

                string codigos = string.Join(" ", pedidos.Select(p => !string.IsNullOrEmpty(p.CodPedido) ? p.CodPedido : p.CodCarrinho));

                var mensagem = new EmailMessage
                {
                    Subject = $"{Sistema} - {textos.RecebemosSeuPedido} {codigos}",
                    Body = textos.MontarCorpo(codigos),
                    BodyFormat = EmailBodyFormat.PlainText,
                    To = emails
                };
                mensagem.Attachments.Add(new EmailAttachment(pdf));

                await Email.ComposeAsync(mensagem);
            }
            catch (FeatureNotSupportedException)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(textos.EmailNaoConfigurado, textos.TituloTransmissao, "OK");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(ex.Message, textos.TituloTransmissao, "OK");
            }
        }

        // geraPdfWithPedidoChecked: PDF dos pedidos com itens, fotos, observação e descontos (mesmas opções padrão da impressão).
        private async Task<string> GerarPdfAsync(List<CarrinhoCommandResult> pedidos)
        {
            foreach (var pedido in pedidos)
                pedido.Endereco = await _clienteRepository.BuscarEnderecoPrincipal(pedido.CodPessoaCliente);

            _printService.SetConfig(new PrintServiceConfig()
            {
                Usuario = Session.USUARIO_LOGADO.Login,
                Versao = await _parametroSincronizacaoRepository.BuscarValorParametro(ParametrosSistema.VERSAOSISTEMA_XAM),
                Labels = ConfiguracaoVisual.PrintLabels,
                Dimensoes = ConfiguracaoVisual.PrintDimensoes
            });

            string fotoMarca = await _fotoRepository.BuscarFotoMarca("Logo", Session.USUARIO_LOGADO.CodMarca);

            return _printService.PrintOrder(pedidos, true, true, fotoMarca, true, false, true, true, true);
        }

        // Textos do iOS (Localizable.strings pt/en/es).
        private class Textos
        {
            public string TituloTransmissao;
            public string PerguntaEnviar;
            public string EmailNaoEncontrado;
            public string Enviar;
            public string Nao;
            public string RecebemosSeuPedido;
            public string Prezado;
            public string RecebemosPedido;
            public string ConformeAnexo;
            public string EmailNaoConfigurado;
            public string GerandoPdf;
            public bool Portugues;

            public static Textos Do(string idioma)
            {
                string codigo = (idioma ?? "pt").Substring(0, Math.Min(2, (idioma ?? "pt").Length)).ToLowerInvariant();

                switch (codigo)
                {
                    case "en":
                        return new Textos
                        {
                            TituloTransmissao = "Broadcasting order",
                            PerguntaEnviar = "Do you want to send this order in PDF format to the email address",
                            EmailNaoEncontrado = "No email found for the order customer",
                            Enviar = "Send",
                            Nao = "No",
                            RecebemosSeuPedido = "We have received your order number",
                            Prezado = "Dear Customer",
                            RecebemosPedido = " we have received your order number, following order attached",
                            ConformeAnexo = "see attached",
                            EmailNaoConfigurado = "Could not send the e-mail. Check the e-mail configuration of your iPad!",
                            GerandoPdf = "Generating PDF"
                        };
                    case "es":
                        return new Textos
                        {
                            TituloTransmissao = "Transmitir el pedido",
                            PerguntaEnviar = "¿Quieres enviar este pedido en formato PDF a la dirección de correo electrónico",
                            EmailNaoEncontrado = "No se encontró ningún correo electrónico para el cliente del pedido",
                            Enviar = "Enviar",
                            Nao = "No",
                            RecebemosSeuPedido = "Recibimos su pedido de número",
                            Prezado = "Estimado Cliente",
                            RecebemosPedido = "Recibimos su pedido de número",
                            ConformeAnexo = "conforme anexo",
                            EmailNaoConfigurado = "No se pudo enviar el correo. ¡Verifique la configuración de correo de su iPad!",
                            GerandoPdf = "Generando PDF"
                        };
                    default:
                        return new Textos
                        {
                            Portugues = true,
                            TituloTransmissao = "Transmissão do Pedido",
                            PerguntaEnviar = "Deseja enviar esse pedido em PDF para o e-mail",
                            EmailNaoEncontrado = "Não foi encontrado email para o cliente do pedido",
                            Enviar = "Enviar",
                            Nao = "Não",
                            RecebemosSeuPedido = "Recebemos seu pedido número",
                            Prezado = "Prezado cliente, Informamos que seu pedido da PEGADA foi recebido com sucesso.",
                            RecebemosPedido = "ATENÇÃO-Para evitar qualquer tipo de problema na entrega, favor revisar em sua cópia de pedido as seguintes informações: Endereço da loja, Preços, Prazo, Modelos e Quantidades. Caso note alguma divergência informe o seu representante comercial.",
                            ConformeAnexo = "\n\nObrigado. Calçados Pegada Nordeste LTDA",
                            EmailNaoConfigurado = "Não foi possível enviar o e-mail. Verifique a configuração de e-mail do seu iPad!",
                            GerandoPdf = "Gerando PDF"
                        };
                }
            }

            // Corpo do e-mail: em português não cita os números; nos demais idiomas sim (montaPedidoEmail).
            public string MontarCorpo(string codigos)
            {
                if (Portugues)
                    return $"{Prezado}\n\n    {RecebemosPedido} {ConformeAnexo}.\n\n{Sistema}";

                return $"{Prezado},\n\n    {codigos} {RecebemosPedido}, {ConformeAnexo}.\n\n{Sistema}";
            }
        }
    }
}
