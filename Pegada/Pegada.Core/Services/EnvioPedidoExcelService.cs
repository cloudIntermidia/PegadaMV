using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MobiliVendas.Core;
using MobiliVendas.Core.Domain.Commands.Results;
using MobiliVendas.Core.Domain.Repositories;
using MobiliVendas.Core.Domain.StaticObject;
using MobiliVendas.Core.Infra.DataContext;
using MobiliVendas.Core.Services.Contracts;
using MobiliVendas.Core.Utils;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Services;
using SQLite;

namespace Pegada.Core.Services
{
    // WebservicePedidoHelper transmitirPedidoExcel:andEmails:andCheckEnviaTodos: (PegadaIOS): manda os pedidos marcados
    // para o putPedidoExcel, que gera a planilha e envia por e-mail. JSON montado como o jsonDataForPedidoExcel.
    public class EnvioPedidoExcelService : IEnvioPedidoExcel
    {
        private readonly SQLiteAsyncConnection _sql;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IParametroSincronizacaoRepository _parametroSincronizacaoRepository;

        public EnvioPedidoExcelService(ISqliteConnection context, IPedidoRepository pedidoRepository,
                                       IParametroSincronizacaoRepository parametroSincronizacaoRepository)
        {
            _sql = context.DbConnectionAsync();
            _pedidoRepository = pedidoRepository;
            _parametroSincronizacaoRepository = parametroSincronizacaoRepository;
        }

        // modalEnviaPedidoExcel (PedidoViewController): tela "Envio de Pedido" com os pedidos marcados.
        public async Task AbrirAsync(List<CarrinhoCommandResult> pedidos)
        {
            await PopupNavigation.Instance.PushAsync(new Views.Negocio.EnvioPedidoExcelView(pedidos, this));
        }

        /// <summary>true quando o servidor responde SUCCESS = TRUE.</summary>
        public async Task<bool> EnviarAsync(List<CarrinhoCommandResult> pedidos, string emailsDigitados, bool enviaSoEmailsDigitados)
        {
            string body = await MontarJsonAsync(pedidos, emailsDigitados, enviaSoEmailsDigitados);
            string url = await UrlPedidoExcelAsync();

            var retorno = await RestApiHelper.PostStringAsync(url, body, false, ApiConfig.ReceberJsonCompactadoPedido);
            var model = new WcfModelResult(retorno);
            return string.Equals(model.SUCCESS?.ToString(), "TRUE", StringComparison.OrdinalIgnoreCase);
        }

        // urlPedidoExcel: parâmetro de sincronização urlExportPedidoExcel. Sem ele no banco, o mesmo serviço do
        // PutCatalogoExcel (UPLOADURL) com o método putPedidoExcel.
        private async Task<string> UrlPedidoExcelAsync()
        {
            string url = await _parametroSincronizacaoRepository.BuscarValorParametro("urlExportPedidoExcel");
            if (!string.IsNullOrWhiteSpace(url))
                return url.Trim();

            string baseUrl = await _parametroSincronizacaoRepository.BuscarValorParametro(ParametrosSistema.UPLOADURL);
            return $"{baseUrl}/putPedidoExcel";
        }

        // jsonDataForPedidoExcel:andEmails:andEnviaEmail:
        private async Task<string> MontarJsonAsync(List<CarrinhoCommandResult> pedidos, string emails, bool enviaSoEmailsDigitados)
        {
            var pedidosList = new List<Dictionary<string, object>>();

            foreach (var pedido in pedidos)
            {
                bool carrinho = pedido.Origem == "C";
                string codPedido = carrinho ? pedido.CodCarrinho : pedido.CodPedido;
                var extra = await BuscarCabecalhoAsync(pedido, carrinho);

                var p = new Dictionary<string, object>();
                p["CODPEDIDO"] = codPedido;
                p["CODMARCA"] = pedido.CodMarca;
                if (!string.IsNullOrEmpty(pedido.CodTabelaPreco))
                    p["CODTABELAPRECO"] = pedido.CodTabelaPreco;
                p["CODTIPOPEDIDO"] = pedido.CodTipoPedido;
                p["CODSITUACAOPEDIDO"] = pedido.CodSituacaoPedido;
                p["CODPESSOAREPRESENTANTE"] = pedido.CodPessoaRepresentante;
                if (!string.IsNullOrEmpty(pedido.CodPessoaPreposto) && pedido.CodPessoaPreposto != "(null)")
                    p["CODPESSOAPREPOSTO"] = pedido.CodPessoaPreposto;
                p["CODPESSOACLIENTE"] = pedido.CodPessoaCliente;
                if (!string.IsNullOrEmpty(pedido.CodColecao))
                    p["CODCOLECAO"] = pedido.CodColecao;
                p["CODUSUARIO"] = Numero(pedido.CodUsuario != 0 ? pedido.CodUsuario : (Session.USUARIO_LOGADO?.CodUsuario ?? 0));
                if (!string.IsNullOrEmpty(pedido.OrdemCompra))
                    p["NUMEROPEDIDOCLIENTE"] = pedido.OrdemCompra;
                if (pedido.DataEmissao != default(DateTime))
                    p["DATAEMISSAO"] = pedido.DataEmissao.ToString("yyyy-MM-dd");
                if (pedido.DataEntrega.HasValue)
                    p["DATAENTREGA"] = pedido.DataEntrega.Value.ToString("yyyy-MM-dd");
                p["VALORTOTAL"] = Numero(pedido.ValorTotal);
                p["VALORTOTALLIQUIDO"] = DuasCasas(pedido.ValorTotalLiquido);
                p["EMAILNOTIFICACAO"] = enviaSoEmailsDigitados ? (emails ?? string.Empty) : $"{pedido.Email ?? string.Empty};{emails ?? string.Empty}";

                string observacao = !string.IsNullOrEmpty(pedido.Observacoes) ? pedido.Observacoes : extra?.Observacoes;
                if (!string.IsNullOrEmpty(observacao))
                    p["OBSERVACAO"] = observacao;
                p["QTDTOTAL"] = Numero(pedido.QtdTotal);

                string codSemana = !string.IsNullOrEmpty(pedido.CodSemana) ? pedido.CodSemana : extra?.CodSemana;
                if (!string.IsNullOrEmpty(codSemana))
                    p["CUSTOM1"] = codSemana;
                if (!string.IsNullOrEmpty(extra?.CodFabrica))
                    p["CUSTOM2"] = extra.CodFabrica;
                string diasBonificacao = !string.IsNullOrEmpty(pedido.DiasBonificacao) ? pedido.DiasBonificacao : extra?.DiasBonificacao;
                if (!string.IsNullOrEmpty(diasBonificacao))
                    p["CUSTOM3"] = diasBonificacao;
                if (!string.IsNullOrEmpty(extra?.CodTransportadora))
                    p["CUSTOM4"] = extra.CodTransportadora;

                // aceitaFaturamentoAntecipado "2" = sim -> CUSTOM9 "1"; qualquer outro valor -> "0".
                int? faturamentoAntecipado = pedido.AceitaFaturamentoAntecipado ?? extra?.AceitaFaturamentoAntecipado;
                if (faturamentoAntecipado.HasValue)
                    p["CUSTOM9"] = faturamentoAntecipado.Value == 2 ? "1" : "0";

                if (!string.IsNullOrEmpty(pedido.CodCondicaoPagamento))
                    p["CODCONDICAOPAGAMENTO"] = pedido.CodCondicaoPagamento;
                if (!string.IsNullOrEmpty(extra?.CodRedespacho) && extra.CodRedespacho != "NULL")
                    p["CUSTOM5"] = extra.CodRedespacho;

                var cliente = (await _sql.QueryAsync<ClienteRow>(
                    "SELECT CNPJ, RazaoSocial, NomeFantasia, Telefone1 FROM TBT_CLIENTE WHERE CodPessoaCliente = ?",
                    pedido.CodPessoaCliente)).FirstOrDefault();
                if (!string.IsNullOrEmpty(cliente?.CNPJ))
                    p["CNPJ"] = cliente.CNPJ;
                if (!string.IsNullOrEmpty(cliente?.RazaoSocial))
                    p["RAZAOSOCIAL"] = cliente.RazaoSocial;
                if (!string.IsNullOrEmpty(cliente?.NomeFantasia))
                    p["NOMEFANTASIA"] = cliente.NomeFantasia;
                if (!string.IsNullOrEmpty(cliente?.Telefone1))
                    p["TELEFONECLIENTE"] = cliente.Telefone1;

                string condicao = await Escalar("SELECT Descricao FROM TBT_CONDICAO_PAGAMENTO WHERE CodCondicaoPagamento = ?", pedido.CodCondicaoPagamento);
                if (!string.IsNullOrEmpty(condicao))
                    p["DESCRICAOCONDICAOPAGAMENTO"] = condicao;
                string representante = await Escalar("SELECT Nome FROM TBT_PESSOA WHERE CodPessoa = ?", pedido.CodPessoaRepresentante);
                if (!string.IsNullOrEmpty(representante))
                    p["REPRESENTANTE"] = representante;
                string preposto = await Escalar("SELECT Nome FROM TBT_PESSOA WHERE CodPessoa = ?", pedido.CodPessoaPreposto);
                if (!string.IsNullOrEmpty(preposto))
                    p["PREPOSTO"] = preposto;
                string tipoPedido = await Escalar("SELECT Descricao FROM TBT_TIPO_PEDIDO WHERE CodTipoPedido = ?", pedido.CodTipoPedido);
                if (!string.IsNullOrEmpty(tipoPedido))
                    p["TIPOPEDIDO"] = tipoPedido;

                p["ITENS"] = await MontarItensAsync(pedido, codPedido);
                pedidosList.Add(p);
            }

            var post = new Dictionary<string, object>
            {
                ["LANGUAGE"] = Idioma(),
                ["PEDIDOS"] = pedidosList
            };
            return JsonConvert.SerializeObject(new Dictionary<string, object> { ["PEDIDOEXCEL"] = post });
        }

        // Itens completos (itensForPedido:filtros:nil) e a grade no país do cliente, só tamanhos com quantidade.
        private async Task<List<Dictionary<string, object>>> MontarItensAsync(CarrinhoCommandResult pedido, string codPedido)
        {
            var itensJson = new List<Dictionary<string, object>>();
            var itens = await _pedidoRepository.GetItensPedidos(pedido);
            var grades = _pedidoRepository is Repositories.PedidoRepository pedidoRepositoryPegada
                ? await pedidoRepositoryPegada.BuscarGradesPorPaisCliente(pedido)
                : null;

            foreach (var item in itens)
            {
                string seqItem = !string.IsNullOrEmpty(item.CodItemPedido) ? item.CodItemPedido : Numero(item.CodItemCarrinho);

                var i = new Dictionary<string, object>();
                i["CODPEDIDO"] = codPedido;
                i["SEQITEMPEDIDO"] = seqItem;
                i["CODPRODUTO"] = item.CodProduto;

                var produto = (await _sql.QueryAsync<ProdutoRow>(
                    "SELECT Descricao, CodProdutoFoto FROM TBT_PRODUTO WHERE CodProduto = ?", item.CodProduto)).FirstOrDefault();
                if (!string.IsNullOrEmpty(produto?.Descricao))
                    i["DESCRICAOPRODUTO"] = produto.Descricao;
                if (!string.IsNullOrEmpty(produto?.CodProdutoFoto))
                    i["CODFOTO"] = produto.CodProdutoFoto;
                if (!string.IsNullOrEmpty(item.CodGrade))
                    i["CODGRADE"] = item.CodGrade;

                i["QTDCAIXA"] = Numero(item.QtdCaixa);
                i["QTDTOTAL"] = Numero(item.QtdTotal);
                i["VALORUNITARIO"] = Numero(item.ValorUnitario);
                i["VALORUNITARIOLIQUIDO"] = DuasCasas(item.ValorUnitarioLiquido ?? 0);
                i["VALORTOTALLIQUIDO"] = DuasCasas(item.ValorTotalLiquido);
                i["PERCENTUALDESCONTO"] = Numero(item.PercDesc);
                i["PERCENTUALDESCONTO1"] = Numero(item.PercDesc1);
                i["PERCENTUALDESCONTO2"] = Numero(item.PercDesc2);
                i["PERCENTUALDESCONTO3"] = Numero(item.PercDesc3);
                i["PERCENTUALDESCONTO4"] = Numero(item.PercDesc4);
                i["PERCENTUALDESCONTO5"] = Numero(item.PercDesc5);
                i["CUSTOM1"] = Numero(item.QtdTotal);

                var gradesJson = new List<Dictionary<string, object>>();
                if (grades != null)
                {
                    foreach (var grade in grades[seqItem].Where(g => g.Qtd > 0))
                    {
                        gradesJson.Add(new Dictionary<string, object>
                        {
                            ["CODPEDIDO"] = codPedido,
                            ["SEQITEMPEDIDO"] = seqItem,
                            ["SEQGRADEITEMPEDIDO"] = grade.CodGradeItem,
                            ["CODDERIVACAO"] = grade.CodDerivacao,
                            ["DESCRICAODERIVACAO"] = grade.Descricao,
                            ["QTD"] = Numero(grade.Qtd)
                        });
                    }
                }
                i["GRADES"] = gradesJson;

                itensJson.Add(i);
            }

            return itensJson;
        }

        // Campos do cabeçalho que o CarrinhoCommandResult não traz. Pedido do ERP: fábrica do carrinho de origem
        // (TBT_PEDIDO não tem CodFabrica, como no PRO_PEDIDO_GET).
        private async Task<CabecalhoRow> BuscarCabecalhoAsync(CarrinhoCommandResult pedido, bool carrinho)
        {
            if (carrinho)
            {
                return (await _sql.QueryAsync<CabecalhoRow>(
                    "SELECT CodFabrica, CodTransportadora, NULL AS CodRedespacho, CodSemana, DiasBonificacao, " +
                    "AceitaFaturamentoAntecipado, Observacoes FROM TBT_CARRINHO WHERE CodCarrinho = ?",
                    pedido.CodCarrinho)).FirstOrDefault();
            }

            return (await _sql.QueryAsync<CabecalhoRow>(
                "SELECT C.CodFabrica, P.CodTransportadora, P.CodRedespacho, P.CodSemana, P.DiasBonificacao, " +
                "P.AceitaFaturamentoAntecipado, P.Observacoes FROM TBT_PEDIDO P " +
                "LEFT JOIN TBT_CARRINHO C ON C.CodCarrinho = P.CodCarrinho WHERE P.CodPedido = ?",
                pedido.CodPedido)).FirstOrDefault();
        }

        private async Task<string> Escalar(string sql, string parametro)
        {
            if (string.IsNullOrEmpty(parametro))
                return null;
            return await _sql.ExecuteScalarAsync<string>(sql, parametro);
        }

        // [Language getLanguage]: "pt", "en" ou "es".
        private static string Idioma()
        {
            string idioma = string.IsNullOrEmpty(Session.Idioma) ? "pt" : Session.Idioma;
            return idioma.Substring(0, Math.Min(2, idioma.Length)).ToLowerInvariant();
        }

        // "%@" de NSDecimalNumber: ponto decimal e sem zeros à direita.
        private static string Numero(decimal valor) => valor.ToString("0.##########", CultureInfo.InvariantCulture);

        // "%.2f"
        private static string DuasCasas(decimal valor) => valor.ToString("0.00", CultureInfo.InvariantCulture);

        private class CabecalhoRow
        {
            public string CodFabrica { get; set; }
            public string CodTransportadora { get; set; }
            public string CodRedespacho { get; set; }
            public string CodSemana { get; set; }
            public string DiasBonificacao { get; set; }
            public int? AceitaFaturamentoAntecipado { get; set; }
            public string Observacoes { get; set; }
        }

        private class ClienteRow
        {
            public string CNPJ { get; set; }
            public string RazaoSocial { get; set; }
            public string NomeFantasia { get; set; }
            public string Telefone1 { get; set; }
        }

        private class ProdutoRow
        {
            public string Descricao { get; set; }
            public string CodProdutoFoto { get; set; }
        }
    }
}
