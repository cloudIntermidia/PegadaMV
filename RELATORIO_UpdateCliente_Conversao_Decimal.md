# Relatório: `UpdateCliente` do Pegada_upload falha com erro de conversão String → Decimal?

> **Para o Claude que vai ler isto na máquina Windows:** este relatório vem de outra sessão, que trabalhou no app PegadaMV (Xamarin, iPad). O lado do app foi conferido e **não tem erro**: ele envia o pedido certo e trata a resposta como o app iOS.
> O problema está no **serviço WCF `Pegada_upload`**, operação **`UpdateCliente`**.
> Objetivo: achar o campo da classe de retorno cujo tipo não bate com a coluna do Oracle, corrigir e garantir que a resposta siga o contrato da seção 4.
>
> Histórico: o relatório anterior (`RELATORIO_UpdateCliente_ORA-00942.md`) tratava do erro ORA-00942 nessa mesma operação. Esse erro já foi corrigido no servidor. O erro abaixo apareceu depois dessa correção.

## 1. Sintoma

No app PegadaMV, o usuário abre a Edição de Atendimento, toca em "Atualizar Cadastro", marca um cliente e toca em "Atualizar Selecionados".
- Se o cliente **não tem** alteração de situação comercial no ERP, a operação dá certo.
- Se o cliente **tem** alteração, o app mostra "Ocorreu um erro ao realizar a atualização."

Cliente para reproduzir: **C246693** (60.029.775 VALQUIRIA DOS SANTOS ...).

## 2. O que trafegou (log do app)

**Requisição** (igual nos dois casos)
```
POST http://externo.pegada.com.br:1234/Pegada_upload/Service.svc/UpdateCliente
Accept: application/json
Body (JSON puro, sem compressão):
{"ATUALIZAR":[{"CodPessoaCliente":"C246693"}]}
```

**Resposta com falha** (16/11/2026 15:27, cliente C246693, HTTP 200)
```json
{"SUCCESS":"false","CODMENSAGEM":"ITM004","CODIGO":"",
 "EXCEPTION":"A conversão especificada de um tipo materializado 'System.String' para um tipo anulável 'System.Decimal' não é válida."}
```

**Resposta com sucesso** (12/11/2026 17:25, cliente sem alteração, HTTP 200)
```json
{"SUCCESS":"true","CODMENSAGEM":"ITM001","CODIGO":[],"EXCEPTION":null}
```

## 3. Diagnóstico provável

- A mensagem é do **Entity Framework**. Ela aparece quando o EF materializa o resultado de uma consulta (`Database.SqlQuery<T>`, `ObjectContext.ExecuteStoreQuery<T>` ou um mapeamento de entidade) e uma coluna vem como **texto** (VARCHAR2/CHAR) enquanto a propriedade de `T` é **`decimal?`**.
- Quando a consulta não traz linhas, nada é materializado e não há erro. Por isso "sem alteração" funciona e "com alteração" falha.
- Colunas candidatas, todas devolvidas ao app: `CODSITUACAOCLIENTE`, `CREDITOLIBERADO`, `INDATIVO`, `CODCLASSERISCO`, `USACLASSERISCO`. A mais provável é `CODCLASSERISCO`, que costuma ser um código alfanumérico. Também vale conferir qualquer coluna usada só internamente pela operação (filtros, joins, comparação de "houve alteração").
- Outra causa possível: o SQL usa `NVL`, `DECODE`, `CASE` ou `UNION` misturando número e texto. Nesse caso o Oracle devolve a coluna como texto, mesmo que a coluna original da tabela seja numérica.

## 4. Contrato esperado pelo app (não mudar)

O app PegadaMV e o app iOS (PegadaIOS, via `WSGenerico/Service.svc/UpdateCliente`) esperam um **objeto**:
```json
{
  "SUCCESS": "TRUE",
  "CODMENSAGEM": "...",
  "CODIGO": [
    {
      "CODPESSOACLIENTE":   "C246693",
      "CODSITUACAOCLIENTE": "0",
      "CREDITOLIBERADO":    "1",
      "INDATIVO":           "1",
      "CODCLASSERISCO":     "...",
      "USACLASSERISCO":     "1",
      "ULTIMAATUALIZACAO":  "..."
    }
  ],
  "EXCEPTION": ""
}
```
- O app considera sucesso quando `SUCCESS` vale `"TRUE"`, sem diferenciar maiúsculas de minúsculas.
- `CODIGO` é uma **lista** com um objeto por cliente que mudou. Uma lista vazia é válida.
- O app lê cada valor como texto (`ToString()`), então tanto número quanto string no JSON funcionam.
- As chaves precisam estar em maiúsculas, exatamente como acima.
- Para cada item, o app grava no banco local:
  ```sql
  UPDATE CLIENTE SET CodSituacaoCliente, CreditoLiberado, IndAtivo, CodClasseRisco, UsaClasseRisco, UltimaAtualizacao WHERE CodPessoaCliente = ...
  ```

## 5. O que fazer no WCF

1. No projeto **Pegada_upload**, abrir a operação `UpdateCliente` (contrato `IService` e `Service.svc.cs`) e seguir até o método da DAL/repositório que executa o SQL.
2. Identificar a classe de retorno (o `T` do `SqlQuery<T>` ou a entidade) e listar as propriedades `decimal?`.
3. Para cada uma, conferir o tipo real da coluna no Oracle:
   ```sql
   SELECT table_name, column_name, data_type
     FROM all_tab_columns
    WHERE column_name IN ('CODSITUACAOCLIENTE','CREDITOLIBERADO','INDATIVO',
                          'CODCLASSERISCO','USACLASSERISCO','ULTIMAATUALIZACAO');
   ```
   Se possível, rodar o SQL da operação direto no Oracle com `C246693` e ver o tipo e o valor de cada coluna.
4. Comparar com o `UpdateCliente` do projeto **WSGenerico**, que funciona para o iOS: qual classe ou tipos ele usa para essas colunas?
5. Corrigir de um destes jeitos, priorizando o menos invasivo:
   - declarar a propriedade como `string` na classe de retorno, já que o app lê tudo como texto; ou
   - converter no SQL (`TO_NUMBER(...)` ou `TO_CHAR(...)`) para o tipo da propriedade.
6. Melhoria opcional: no `catch`, registrar no `EXCEPTION` também o nome da coluna ou propriedade, ou o `InnerException`, para facilitar o próximo diagnóstico.

## 6. Como testar depois da correção

```bash
curl -X POST "http://externo.pegada.com.br:1234/Pegada_upload/Service.svc/UpdateCliente" ^
  -H "Accept: application/json" -H "Content-Type: text/x-json; charset=utf-8" ^
  -d "{\"ATUALIZAR\":[{\"CodPessoaCliente\":\"C246693\"}]}"
```
O esperado é `SUCCESS: "TRUE"` e `CODIGO` como lista com o objeto do cliente C246693 (ou lista vazia, se o cliente já tiver sido atualizado).

Atenção: depois que o app gravar a atualização, o cliente pode deixar de ter alteração e passar a voltar `CODIGO: []`. Para retestar o caso com dados, use outro cliente que tenha mudança de situação no ERP, ou teste antes de atualizar pelo app.

## 7. Contexto do app (só referência, nada a fazer lá)

- Chamada: `ServiceUtility.AtualizarSituacaoComercialClientes` → `RestApiHelper.PostStringObjetoAsync` (IntermidiaXamarin, `MobiliVendas.Core`).
- URL: parâmetro de upload (`.../Pegada_upload/Service.svc`) + `/UpdateCliente`.
- Tela: `NovoAtendimentoViewViewModel.AtualizarSituacaoComercialSelecionados` (Pegada.Core).
