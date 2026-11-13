# Relatório: `UpdateCliente` do Pegada_upload retorna ORA-00942

> **Para o Claude que vai ler isto na máquina Windows:** este relatório vem de outra sessão, que trabalhou no app PegadaMV (Xamarin, iPad).
> O lado do app já foi conferido e corrigido. Falta investigar o **serviço WCF `Pegada_upload`**, operação **`UpdateCliente`**, que responde erro do Oracle.
> Objetivo: descobrir qual tabela ou view a operação usa e não encontra, e corrigir para que ela devolva o mesmo conteúdo que o `UpdateCliente` do serviço `WSGenerico` (usado pelo app iOS).

## 1. Sintoma

No app PegadaMV, o usuário abre a Edição de Atendimento, toca em "Atualizar Cadastro", marca um cliente e toca em "Atualizar Selecionados". O app mostra "Ocorreu um erro ao realizar a atualização."

## 2. O que trafegou (log do app, 12/11/2026)

**Requisição**
```
POST http://externo.pegada.com.br:1234/Pegada_upload/Service.svc/UpdateCliente
Accept: application/json
Body (JSON puro, sem compressão):
{"ATUALIZAR":[{"CodPessoaCliente":"C11384"}]}
```
O teste usou um cliente; quando há vários marcados, o array traz um item por cliente.

**Resposta** (HTTP 200)
```json
{"SUCCESS":"false","CODMENSAGEM":"ITM004","CODIGO":"","EXCEPTION":"ORA-00942: table or view does not exist"}
```
A mesma resposta voltou nas duas tentativas, às 09:27 e às 09:41.

## 3. Resposta esperada (contrato usado pelo app)

O app iOS (PegadaIOS) chama a mesma operação, mas no serviço **WSGenerico**:
```
Produção: http://externo.pegada.com.br:1234/WSGenerico/Service.svc/UpdateCliente
Teste:    http://externo.pegada.com.br:1234/WSGenerico_TST/Service.svc/UpdateCliente
```
Os dois apps enviam o mesmo body e esperam um **objeto**, não uma lista:
```json
{
  "SUCCESS": "TRUE",
  "CODMENSAGEM": "...",
  "CODIGO": [
    {
      "CODPESSOACLIENTE":   "C11384",
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
- O app só considera sucesso quando `SUCCESS` vale `"TRUE"`, sem diferenciar maiúsculas de minúsculas.
- Para cada item de `CODIGO`, o app grava no banco local:
  ```sql
  UPDATE CLIENTE SET CodSituacaoCliente, CreditoLiberado, IndAtivo, CodClasseRisco, UsaClasseRisco, UltimaAtualizacao WHERE CodPessoaCliente = ...
  ```
- Os nomes das chaves estão em maiúsculas, exatamente como acima.

## 4. O que investigar no WCF

1. No projeto do **Pegada_upload**, localizar a operação `UpdateCliente` (contrato `IService` e implementação `Service.svc.cs`), e o método da DAL/repositório que ela chama.
2. Achar o SQL que essa operação executa no Oracle e listar **todas** as tabelas, views e sinônimos que ele usa. Uma delas não existe, ou não está visível, para o usuário da connection string do Pegada_upload. É isso que o ORA-00942 indica.
3. Comparar com a mesma operação no projeto **WSGenerico**, que funciona para o iOS:
   - mesmo SQL?
   - mesma connection string, usuário e schema?
   - o WSGenerico prefixa o schema (`SCHEMA.TABELA`) ou usa sinônimos que o usuário do Pegada_upload não tem?
4. Verificar a connection string do Pegada_upload no `Web.config`, no IIS e em transforms de Release.
5. Verificar no Oracle, com o usuário do Pegada_upload:
   - `SELECT owner, object_name, object_type FROM all_objects WHERE object_name = '<TABELA>';`
   - se existe sinônimo e se há `GRANT SELECT` para esse usuário.
6. Corrigir de um destes jeitos:
   - criar sinônimo ou grant;
   - prefixar o schema no SQL;
   - ajustar a connection string.

   Em qualquer caso, garantir que a resposta siga o formato da seção 3, incluindo `ULTIMAATUALIZACAO`.

## 5. Como testar depois da correção

Use um cliente real. A operação é de consulta da situação comercial no ERP.
```bash
curl -X POST "http://externo.pegada.com.br:1234/Pegada_upload/Service.svc/UpdateCliente" ^
  -H "Accept: application/json" -H "Content-Type: text/x-json; charset=utf-8" ^
  -d "{\"ATUALIZAR\":[{\"CodPessoaCliente\":\"C11384\"}]}"
```
O esperado é `SUCCESS: "TRUE"` e `CODIGO` como array com um objeto por cliente.

## 6. Contexto do lado do app (já resolvido, apenas para referência)

- Antes, o app MV desserializava a resposta como lista e quebrava com `JsonSerializationException`. Isso foi corrigido: agora ele lê o objeto, igual ao iOS (`RestApiHelper.PostStringObjetoAsync`, `ServiceUtility.AtualizarSituacaoComercialClientes`).
- O MV usa como base a URL de upload dos parâmetros (`.../Pegada_upload/Service.svc`) e acrescenta `/UpdateCliente`.
- Alternativa, caso o Pegada_upload não seja corrigido: o app passaria a chamar o `UpdateCliente` do WSGenerico, como o iOS. A preferência é corrigir o Pegada_upload, porque o MV usa esse serviço para tudo.
