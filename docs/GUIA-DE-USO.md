# Manual de uso — Bingo Company

Este manual explica, sem termos técnicos desnecessários, como usar o Bingo Company para criar, organizar, jogar, operar e conferir um bingo corporativo.

> A mesma orientação está disponível dentro do sistema, no menu **Ajuda**.

## Para que serve

O Bingo Company realiza bingos ao vivo com cartelas digitais e impressas. A organização define as regras e os prêmios; as pessoas se inscrevem; o operador faz o sorteio; o telão mostra a partida; e a auditoria registra o resultado.

O sistema é a fonte oficial do jogo. Ele controla as pedras sorteadas, marcações que valem, cartelas que concorrem, empates e vencedores. Portanto, uma cartela não precisa ser conferida manualmente para validar o prêmio.

## Visão rápida dos papéis

| Pessoa | O que faz |
| --- | --- |
| Organizador | Cria e configura o evento, as inscrições, cartelas, rodadas e prêmios. |
| Operador | Inicia e conduz cada rodada, revela vencedores e registra a entrega dos prêmios. |
| Participante | Entra por código ou QR Code, recebe e usa a cartela. |
| Público | Acompanha o telão ou consulta a auditoria, sem precisar entrar na conta. |

## 1. Criar conta e evento

1. Na página inicial, escolha **Cadastrar empresa**.
2. Informe o nome da empresa, seu nome, e-mail e uma senha de pelo menos 12 caracteres.
3. Se já houver conta, escolha **Entrar**.
4. Em **Administração**, informe o nome do evento, escolha o modo de marcação e clique em **Criar evento**.
5. Anote o **código público** mostrado no cartão do evento. Ele é usado para inscrição e auditoria.
6. Clique em **Configurar** para abrir a central daquele evento.

### Estados do evento

| Estado | Significado |
| --- | --- |
| Em preparação | Evento recém-criado; ainda é preciso abrir inscrições. |
| Inscrições abertas | Pessoas podem se inscrever e gerar cartelas digitais. |
| Em andamento | A primeira rodada já foi iniciada. |
| Finalizado | Não aceita alterações ou novas inscrições. A auditoria continua pública. |

## 2. Abrir e compartilhar as inscrições

Na tela de configuração, use **Abrir inscrições**. Depois, escolha uma destas formas de divulgação:

- **Abrir QR Code de inscrição**: exibe o QR Code para projetar ou mostrar no celular.
- **Visualizar inscrição**: abre a página pública que pode ser compartilhada por link.
- Na página inicial, qualquer pessoa também pode escolher **Participar** e informar o código do evento.

O participante informa nome e tipo de participação:

- **Colaborador**: matrícula é opcional.
- **Familiar** ou **Convidado**: é obrigatório informar o colaborador responsável.

Ao selecionar **Gerar minha cartela**, o participante recebe uma cartela digital ativa. O link da cartela deve ser guardado para abrir a mesma cartela novamente.

## 3. Escolher o modo de marcação

| Modo | Como funciona |
| --- | --- |
| Automática | Cada pedra sorteada é marcada pelo sistema em todas as cartelas. O participante apenas acompanha. |
| Manual obrigatória | O participante deve marcar a pedra atual antes do próximo sorteio. Só o que foi marcado no sistema vale. |
| Manual assistida | O participante pode marcar qualquer número da própria cartela que já tenha sido sorteado. Só o que foi marcado no sistema vale. |

A casa central com estrela é livre e já vale como marcada. Em modo manual, não é possível marcar número fora da cartela, ainda não sorteado ou que já esteja marcado.

## 4. Gerar e usar cartelas impressas

Cartelas físicas não entram automaticamente no jogo. Na configuração do evento:

1. Informe uma quantidade entre 1 e 1.000 e clique em **Gerar lote para impressão**. Podem ser criados vários lotes.
2. Escolha **Abrir para impressão** e, na nova tela, use **Imprimir / salvar PDF**.
3. Cada cartela possui código, QR Code e fingerprint para identificação.
4. Volte à configuração. Selecione a cartela, ou leia seu QR Code, e selecione o participante.
5. Clique em **Associar cartela**.
6. Clique em **Ativar cartela**.

Somente cartela associada a uma pessoa e ativa concorre.

## 5. Criar uma rodada e configurar os prêmios

Um evento pode ter várias rodadas. Cada rodada recebe uma nova sequência de 75 pedras; as cartelas continuam pertencendo ao mesmo evento e podem ser reaproveitadas nas próximas rodadas.

1. Em **Configuração da rodada**, informe o nome.
2. Preencha o nome de cada prêmio e selecione sua regra.
3. Opcionalmente, anexe uma imagem JPEG, PNG ou WebP de até 2 MB. A imagem aparece no telão.
4. Use **+ Adicionar prêmio** para criar mais etapas ou **Remover prêmio** para tirar uma etapa.
5. Clique em **Salvar e ir para operação**.

Uma regra não pode ser usada duas vezes na mesma rodada. A aplicação organiza automaticamente as etapas da mais simples à mais difícil, mesmo que você as adicione em outra ordem. A rodada só pode ser editada enquanto estiver **Pronta para iniciar**.

### Regras disponíveis

| Regra | Como ganhar |
| --- | --- |
| Coluna B, I, N, G ou O | Complete a coluna indicada. O sorteio usa somente a faixa daquela coluna: B 1–15, I 16–30, N 31–45, G 46–60 e O 61–75. |
| Quatro cantos | Marque os quatro cantos. |
| Diagonal B ou O | Complete a diagonal indicada. |
| Uma linha / Duas linhas | Complete uma ou duas linhas horizontais. |
| X | Complete as duas diagonais. |
| T | Complete a linha superior e a coluna central. |
| Cruz | Complete a linha e a coluna centrais. |
| Moldura | Complete toda a borda externa. |
| Cartela cheia | Complete todos os números. |

## 6. Quem concorre em uma rodada

Ao clicar em **INICIAR RODADA**, o sistema tira uma fotografia da lista de cartelas elegíveis: somente cartelas ativas e associadas naquele instante entram na rodada. Novas inscrições, ativações ou associações feitas depois disso ficam para uma rodada futura.

É preciso existir pelo menos uma cartela ativa para abrir a operação. Depois da conclusão de uma rodada, participantes podem gerar uma nova cartela digital quando essa opção aparecer em sua tela; também é possível gerar, associar e ativar novas cartelas impressas antes da próxima rodada.

## 7. Operar o sorteio

1. Em **Rodadas criadas**, clique em **Abrir operação** na rodada correta.
2. Clique em **INICIAR RODADA**. O sistema prepara 75 pedras sem repetição e publica um hash de auditoria antes do primeiro sorteio.
3. Clique em **SORTEAR PRÓXIMA PEDRA** uma vez por vez.
4. A última pedra, a lista de pedras, as cartelas conectadas e o telão são atualizados ao vivo.
5. Ao detectar vencedor, o sorteio pausa. Clique em **REVELAR VENCEDOR**. Em empate, o botão passa a ser **REALIZAR DESEMPATE**.
6. Depois da revelação, clique em **PRÊMIO ENTREGUE** ou **VENCEDOR NÃO RETIROU O PRÊMIO**.
7. Se o prêmio foi entregue, clique em **CONTINUAR SORTEIO NO TELÃO**. A apresentação é fechada e a próxima etapa é liberada. Ao terminar a última etapa, a rodada é finalizada.

### Empate, não retirada e cancelamento

- Se duas ou mais cartelas completarem a regra na mesma pedra, o sistema atribui posições únicas de desempate de forma segura e revela o resultado no telão. Esse desempate não se limita às 75 pedras.
- Se o vencedor não retirar o prêmio, a cartela dele deixa de disputar somente aquela etapa. O sorteio continua com a mesma regra para encontrar outro vencedor.
- **CANCELAR RODADA** só pode ser usado durante o sorteio e antes de haver vencedor aguardando revelação. A rodada é encerrada sem vencedor.

## 8. O que o participante vê e faz

A página **Minha cartela** apresenta o prêmio e regra atuais, o modo de marcação, a cartela e todas as pedras já sorteadas.

- Em marcação automática, o participante não precisa tocar em nada.
- Em marcação manual, deve tocar apenas nos números habilitados. Na manual obrigatória, deve marcar a pedra atual antes da próxima.
- Quando a cartela vence, a própria tela mostra a confirmação. O participante deve aguardar o nome ser revelado e seguir a orientação da organização para retirada do prêmio.
- Depois de uma rodada concluída, a opção **Gerar nova cartela** só aparece para cartela digital que participou dela e está completa conforme o modo de marcação. A cartela anterior é cancelada e a nova precisa ser criada antes da próxima rodada iniciar.

Se a tela parecer desatualizada, atualize a página. Ela busca novamente o estado oficial quando a conexão em tempo real volta.

## 9. Usar o telão

Escolha **Telão** na configuração do evento ou **Abrir telão** na administração. Abra essa página em outro computador ligado ao projetor; ela não pede login.

O telão mostra:

- rodada, prêmio e foto do prêmio;
- última pedra, histórico e quantidade de pedras;
- etapas já concluídas e etapa atual;
- quantidade de cartelas a uma, duas ou três pedras de vencer;
- suspense antes da revelação;
- vencedor e, quando aplicável, resultado do desempate.

Antes da ação do operador, o telão não mostra o nome do vencedor: somente a pedra vencedora, o prêmio e a quantidade de cartelas empatadas. Há também um botão de tema para adaptar a visualização ao ambiente.

## 10. Conferir a auditoria pública

Na página inicial, informe o código do evento e clique em **Consultar auditoria**; também há o atalho **Auditoria** na configuração do evento. A consulta mostra participantes, cartelas, rodadas, hash, pedras sorteadas, regras, vencedores e a linha do tempo das ações.

O hash SHA-256 é publicado no início da rodada para demonstrar que a sequência foi definida antes do sorteio. A sequência completa das 75 pedras só é exibida depois que a rodada termina. Ao finalizar o evento, a auditoria permanece disponível para consulta pública.

## 11. Encerrar o evento

Depois que todas as rodadas estiverem finalizadas ou canceladas, use **Encerrar evento e publicar auditoria**. Essa ação torna o evento imutável: não é mais possível inscrever pessoas, gerar ou ativar cartelas, alterar rodadas ou abrir o telão.

## Dúvidas rápidas

| Situação | O que fazer |
| --- | --- |
| Não consigo me inscrever | Confira o código e peça para a organização abrir inscrições. Evento finalizado não aceita inscrições. |
| Minha cartela não concorre | Confirme que ela estava ativa antes do início da rodada. Cartela impressa também precisa estar associada. |
| Não consigo marcar | Em automático, o sistema marca. Nos modos manuais, o número precisa estar na cartela, já ter sido sorteado e ainda não estar marcado. |
| O sorteio está parado | Revele o vencedor, registre a entrega ou não retirada e, após entrega, feche a apresentação no telão. A rodada pode também estar cancelada. |
| Posso editar a rodada? | Sim, somente enquanto estiver pronta para iniciar. Não há exclusão; uma rodada em sorteio pode ser cancelada antes da detecção de vencedor. |
| A tela está atrasada | Atualize a página e verifique a conexão. O estado oficial será recarregado. |

## Limite validado

O núcleo do jogo foi validado com 1.000 cartelas elegíveis, inclusive com 1.000 empates simultâneos. Para eventos maiores, a organização deve validar também a capacidade da rede, do banco de dados e dos dispositivos no local.
