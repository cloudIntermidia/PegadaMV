using MobiliVendas.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pegada.Core.Entities
{
    public class Integracao_TBT_CARRINHO : TBT_CARRINHO
    {
        public decimal? PercentualDesconto4 { get; set; }
        public decimal? PercentualDesconto5 { get; set; }
        public string CodTransportadora { get; set; }
        public string ObservacoesSeparacao { get; set; }
        public DateTime? DataLimite { get; set; }
    }
}
