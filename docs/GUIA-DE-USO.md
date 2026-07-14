# Guia de uso do Bingo Company

Este guia explica como conduzir um bingo do cadastro da organização até a auditoria. A própria aplicação também oferece esta orientação na página **Ajuda**.

## Organização: antes do evento

1. Na página inicial, escolha **Cadastrar empresa**. Informe empresa, nome, e-mail e uma senha de ao menos 12 caracteres.
2. Entre na conta e, na área administrativa, crie um evento. Escolha um nome, a quantidade de cartelas digitais por participante e o modo de marcação.
3. Abra o evento e use **Abrir inscrições**. Compartilhe o link ou QR Code de inscrição com os participantes.
4. Se forem usadas cartelas físicas, gere o lote na área de impressão. Depois, associe cada cartela a um participante e ative-a. Apenas cartelas ativas e associadas participam da próxima rodada.
5. Crie uma rodada e defina uma ou mais etapas de prêmio. Uma mesma regra não pode aparecer duas vezes na mesma rodada. Enquanto a rodada estiver pronta, ela pode ser editada.

## Marcação e cartelas

Há três modos exibidos na criação do evento:

- **Automática:** toda pedra sorteada conta automaticamente para todas as cartelas elegíveis.
- **Manual obrigatória** e **manual assistida:** o participante marca, na própria cartela, somente números já sorteados. Para vencer, somente as marcações persistidas pelo sistema contam.

A cartela digital é entregue ao participante ao concluir a inscrição. A cartela física precisa estar associada e ativada. Quando uma rodada começa, o conjunto de cartelas elegíveis é congelado: novas inscrições ou ativações não entram naquela rodada, mas podem participar de uma rodada futura.

## Padrões de prêmio

As etapas são disputadas na ordem configurada, sem reiniciar as pedras entre uma etapa e outra. A casa livre central conta quando o padrão a utiliza.

| Padrão | Como vencer |
| --- | --- |
| Uma linha / duas linhas | Complete uma ou duas linhas horizontais. |
| Quatro cantos | Marque os quatro cantos. |
| Cartela cheia | Complete todos os números da cartela. |
| Diagonal B / diagonal O | Complete a diagonal correspondente. |
| Coluna B, I, N, G ou O | Complete a coluna correspondente. |
| X | Complete as duas diagonais. |
| T | Complete a barra superior e a coluna central. |
| Moldura | Complete a borda externa. |
| Cruz | Complete a linha e a coluna centrais. |

Para prêmios de coluna, o sorteio seleciona somente pedras da faixa da coluna ativa: B (1–15), I (16–30), N (31–45), G (46–60) e O (61–75).

## Operação ao vivo

1. Abra a operação da rodada e escolha **Iniciar rodada**. O sistema gera as 75 pedras sem repetição e publica o hash da sequência antes do primeiro sorteio.
2. Sorteie uma pedra por vez. O telão e as cartelas conectadas são atualizados em tempo real.
3. Quando o sistema detectar vencedores, o sorteio pausa. Antes da revelação, o telão mostra apenas informações agregadas; nunca o nome do participante ou dados da cartela vencedora.
4. Escolha **Revelar vencedor**. Em caso de empate, o sistema aplica o desempate e então revela o resultado.
5. Confirme **Prêmio entregue** para concluir a etapa. Depois, feche a apresentação do vencedor antes de seguir. Se houver outra etapa, ela começa usando a mesma sequência; se não houver, a rodada termina.
6. Se o vencedor não retirar o prêmio, escolha **Vencedor não retirou o prêmio**. O telão volta ao sorteio, a cartela recusada deixa de concorrer apenas nessa etapa e a mesma regra permanece ativa para validar outro vencedor.
7. Depois que todas as rodadas terminarem, finalize o evento. Um evento finalizado não pode mais ser alterado.

## Participantes

1. Abra o link de inscrição ou leia o QR Code compartilhado pela organização.
2. Informe o nome e o tipo de participação. Para familiar ou convidado, informe o colaborador responsável quando solicitado.
3. Guarde ou abra o link da cartela. Em marcação automática, acompanhe os números; em marcação manual, toque somente em pedras já sorteadas.
4. Quando o código da cartela aparecer no telão, avise a organização. O nome só é exibido depois da revelação do operador.

Uma nova cartela digital só pode ser gerada depois de uma rodada concluída se a cartela anterior participou dela, está completa segundo o modo de marcação e a rodada seguinte ainda não começou. A cartela anterior é cancelada quando a nova é criada.

## Telão e auditoria

- **Telão:** abra a opção de telão do evento em um computador conectado ao projetor. Não exige login e mostra a pedra recente, o prêmio, as etapas e a apresentação do resultado.
- **Auditoria pública:** informe o código do evento na página inicial ou abra o link de auditoria. A consulta mostra histórico, participantes, cartelas, rodadas, pedras e resultados. A sequência completa só é exibida após o encerramento da rodada; antes disso, o hash permite conferir que ela não foi alterada.

## Problemas comuns

| Situação | O que verificar |
| --- | --- |
| Não consigo entrar na rodada | As inscrições podem não estar abertas, ou a rodada pode já ter congelado a elegibilidade. |
| Uma cartela física não concorre | Confirme se ela foi associada ao participante e ativada antes do início da rodada. |
| Não consigo marcar um número | Em modo automático não há marcação manual. Nos modos manuais, o número precisa estar na cartela e já ter sido sorteado. |
| O sorteio não avança | Revele o vencedor e registre se o prêmio foi entregue ou não. Após uma entrega, feche a apresentação; em caso de não retirada, o sorteio é retomado na mesma etapa. A rodada também pode ter chegado ao fim. |
| A tela parece atrasada | Atualize a página. Ao reconectar ao tempo real, a aplicação recarrega o estado atual da API. |
