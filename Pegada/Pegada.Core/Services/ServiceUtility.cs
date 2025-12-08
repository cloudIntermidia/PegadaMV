using Acr.UserDialogs;
using Pegada.Core.Entities;
using MobiliVendas.Core;
using MobiliVendas.Core.Domain.Commands.Inputs;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Entities;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Domain.StaticObject;
using MobiliVendas.Core.Infra.DataContext;
using MobiliVendas.Core.Services;
using MobiliVendas.Core.Services.Contracts;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegada.Core.Services
{
    public class ServiceUtility
    {
        protected SQLiteAsyncConnection _sqlAsyncConnection;

        private readonly ICarrinhoRepository _carrinhoRepository;
        private readonly IParametroSincronizacaoRepository _parametroSincronizacaRepository;
        private readonly IParametroRepository _parametroRepository;
        private readonly IFotoRepository _fotoRepository;
        private readonly IPrintService _printService;
        private readonly IClienteRepository _clienteRepository;
        public ServiceUtility()
        {
            _sqlAsyncConnection = CommonServiceLocator.ServiceLocator.Current.GetInstance<ISqliteConnection>().DbConnectionAsync();
            _carrinhoRepository = CommonServiceLocator.ServiceLocator.Current.GetInstance<ICarrinhoRepository>();
            _parametroSincronizacaRepository = CommonServiceLocator.ServiceLocator.Current.GetInstance<IParametroSincronizacaoRepository>();
            _parametroRepository = CommonServiceLocator.ServiceLocator.Current.GetInstance<IParametroRepository>();
            _fotoRepository = CommonServiceLocator.ServiceLocator.Current.GetInstance<IFotoRepository>();
            _printService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IPrintService>();
            _clienteRepository = CommonServiceLocator.ServiceLocator.Current.GetInstance<IClienteRepository>();
        }


        public async Task<List<string>> EnviarEmailAprovacao(List<string> lstCarrinhos)
        {
            List<string> lstErros = new List<string>();
            string erro = string.Empty;
            lstCarrinhos = lstCarrinhos.Distinct().ToList();
            foreach (var codCarrinho in lstCarrinhos)
            {
                if (!string.IsNullOrEmpty(codCarrinho))
                {
                    if (codCarrinho.Contains("."))
                    {
                        erro = await EnviarEmailParaAprovacao(codCarrinho);
                        if (!string.IsNullOrEmpty(erro))
                            lstErros.Add(erro + "\n");
                    }
                }
            }
            UserDialogs.Instance.HideLoading();
            return lstErros;
        }

        public async Task<string> EnviarEmailParaAprovacao(string codCarrinho)
        {
            try
            {
                string motivo = string.Empty;
                var camposCarrinho = (await _sqlAsyncConnection.QueryAsync<SqliteTableInfoCommandResult>("PRAGMA table_info(TBT_CARRINHO);")).Select(x => x.name).ToList();
                string sqlCamposCarrinho = string.Join(",", camposCarrinho);
                var carrinho = (await _sqlAsyncConnection.QueryAsync<TBT_CARRINHO>($"SELECT {sqlCamposCarrinho} FROM TBT_CARRINHO WHERE CodCarrinho = '{codCarrinho}';")).FirstOrDefault();
                if (!carrinho.CodPessoaCliente.Contains(".") &&
                    carrinho.Observacoes != null)
                {
                    UserDialogs.Instance.ShowLoading($"Enviando e-mail de aprovação pedido {carrinho.CodCarrinho}");

                    var camposItem = (await _sqlAsyncConnection.QueryAsync<SqliteTableInfoCommandResult>("PRAGMA table_info(TBT_ITEM_CARRINHO);")).Select(x => x.name).ToList();
                    string sqlCamposItem = string.Join(",", camposItem);
                    sqlCamposItem = sqlCamposItem.Substring(0, sqlCamposItem.Length);
                    var itens = await _sqlAsyncConnection.QueryAsync<TBT_ITEM_CARRINHO>($"SELECT {sqlCamposItem} FROM TBT_ITEM_CARRINHO WHERE CodCarrinho = '{codCarrinho}';");
                    var grades = await _sqlAsyncConnection.Table<TBT_GRADE_ITEM_CARRINHO>().Where(x => x.CodCarrinho == codCarrinho).ToListAsync();

                    List<string> lstMotivos = new List<string>();

                    if (carrinho.PercentualDesconto > 0)
                    {
                        motivo = $"<br/>Pedido com desconto de {carrinho.PercentualDesconto}%. " +
                            $"<br/>";

                        if (string.IsNullOrEmpty(motivo))
                            motivo = "Pedido com desconto!";

                        lstMotivos.Add(motivo);
                    }

                    if (itens.Any(x => x.PercDesc > 0))
                    {
                        foreach (var item in itens)
                        {
                            motivo = $"Item {item.CodProduto} com desconto de {item.PercDesc}%" +
                                        $"<br/> Valor do item sem descontos aplicado {item.ValorUnitario}" +
                                        $"<br/> Valor do item com descontos aplicado {item.ValorUnitarioLiquido}" +
                                        $"<br/>";

                            if (string.IsNullOrEmpty(motivo))
                                motivo = "Item com desconto!";

                            lstMotivos.Add(motivo);
                        }
                    }

                    if (lstMotivos.Count != 0)
                    {
                        string url = await _parametroSincronizacaRepository.BuscarValorParametro(ParametrosSistema.UPLOADURL);
                        url = string.Format("{0}/AprovarRejeitarPedidoComToken", url);
                        string emails = await _parametroRepository.BuscarValorParametro("EMAILAPROVACAO");
                        //emails = "lucas@intermidia.com.br";
                        string bodyHtml = "<!DOCTYPE html><html lang=\"en\" style=\"width: 100%;\"><head> <title>E-mail Template</title> <meta content=\"charset=UTF-8\"></head><body style=\"width: 100%;\"><div id=\"wrapper\" style=\"width: 85%;margin: 20px auto;overflow: hidden; font-family: 'Lucida Grande',Verdana,Arial,Helvetica,sans-serif;color: #333;text-align: center;letter-spacing: -0.5pt;\"> <img src=\"http://ws.mobilivendas.com.br/Intermidia/imagens/logo_email.png\" class=\"logo\" alt=\"logo da Intermidia\" style=\"max-width: 100%;width: 20%;margin-bottom:15px; font-family: 'Lucida Grande',Verdana,Arial,Helvetica,sans-serif;color: #333;text-align: center;letter-spacing: -0.5pt;\"/> <h1 style=\"font-size: 1.3em;padding: 15px 0;font-weight: normal;font-family: 'Lucida Grande',Verdana,Arial,Helvetica,sans-serif;color: #333;text-align: center;letter-spacing: -0.5pt;\"> E-mail de aprovação de pedido! </h1> <p style=\"font-size: 1em;font-family: 'Lucida Grande',Verdana,Arial,Helvetica,sans-serif;color: #333;text-align: center;letter-spacing: -0.5pt;\"> Olá, </p> <p style=\"font-size: 1em;font-family: 'Lucida Grande',Verdana,Arial,Helvetica,sans-serif;color: #333;text-align: center;letter-spacing: -0.5pt;\"> Segue em anexo um cópia do pedido #ITMPEDIDO# <br /> #ITMMotivos# <br /> </p>  <p style=\"font-size: 1em;font-family: 'Lucida Grande',Verdana,Arial,Helvetica,sans-serif;color: #333;text-align: center;letter-spacing: -0.5pt;\"> Mobilivendas #ITMCliente# </p> </div></body></html>";
                        string codCarrinhoEncode = (Convert.ToBase64String(Encoding.UTF8.GetBytes(codCarrinho)));
                        string codAprovacaoEncode = Uri.EscapeUriString(Convert.ToBase64String(Encoding.UTF8.GetBytes("1")));
                        string codRejeicaoEncode = Uri.EscapeUriString(Convert.ToBase64String(Encoding.UTF8.GetBytes("0")));
                        string codUsuarioEncode = Uri.EscapeUriString(Convert.ToBase64String(Encoding.UTF8.GetBytes(Session.USUARIO_LOGADO.CodUsuario.ToString())));
                        //bodyHtml = bodyHtml.Replace("#ITMLINKAPROVAR#", $"{url}/{codCarrinhoEncode}/{codAprovacaoEncode}/{codUsuarioEncode}");
                        //bodyHtml = bodyHtml.Replace("#ITMLINKREJEITAR#", $"{url}/{codCarrinhoEncode}/{codRejeicaoEncode}/{codUsuarioEncode}");
                        bodyHtml = bodyHtml.Replace("#ITMPEDIDO#", carrinho.CodCarrinho);
                        bodyHtml = bodyHtml.Replace("#ITMMotivos#", "Pedido necessita de aprovação gerencial!<br/>" + string.Join("<br/>", lstMotivos.ToArray()));
                        bodyHtml = bodyHtml.Replace("#ITMCliente#", "Pegada");

                        string dataAtual = DateTime.Now.ToString("HH_mm_ss");
                        string fotoMarca = await _fotoRepository.BuscarFotoMarca("Logo", Session.USUARIO_LOGADO.CodMarca);
                        var listCarrinhos = await _carrinhoRepository.GetCarrinhos(new BuscarCarrinhoCommand(Session.ATENDIMENTO_ATUAL, Session.USUARIO_LOGADO, "1"));
                        foreach (var pedido in listCarrinhos)
                        {
                            pedido.Endereco = await _clienteRepository.BuscarEnderecoPrincipal(pedido.CodPessoaCliente);
                        }
                        var pdfFile = _printService.PrintOrder(listCarrinhos, true, false, fotoMarca);
                        new EnvioEmailService().EnviarPedidoEmail(carrinho.CodCarrinho + dataAtual, emails, "Pegada", bodyHtml, "Aprovação de pedido", true, pdfFile);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return string.Format("O pedido foi transmitido, mas ocorreu um erro ao enviar o e-mail para que gerente possa aprovar o pedido " + codCarrinho);
            }
        }


    }
}
