import { Link } from "react-router-dom";
import { AppShell } from "../../shared/ui/AppShell";

export function HelpPage() {
	return (
		<AppShell>
			<main className="help-page">
				<header className="pagehead">
					<div>
						<p className="eyebrow">Central de ajuda</p>
						<h1>Como usar o Bingo Company</h1>
						<p className="subtitle">
							Manual completo, em linguagem simples, para organizar, jogar, operar e conferir um bingo.
						</p>
					</div>
				</header>

				<nav className="help-navigation" aria-label="Tópicos da ajuda">
					<a href="#visao-geral">Começo rápido</a>
					<a href="#organizadores">Organizar</a>
					<a href="#participantes">Participar</a>
					<a href="#operacao">Operar</a>
					<a href="#conexao">Conexão</a>
					<a href="#consulta-publica">Telão e auditoria</a>
					<a href="#duvidas">Dúvidas</a>
				</nav>

				<section className="panel" id="visao-geral">
					<h2>O que o sistema faz</h2>
					<p>
						O Bingo Company realiza bingos corporativos ao vivo. A organização cria o evento, abre as inscrições, prepara os
						prêmios e conduz cada sorteio. Participantes usam cartelas digitais ou impressas; o telão acompanha o jogo em tempo
						real; e a auditoria registra o que aconteceu.
					</p>
					<p>
						O sistema é a fonte oficial do resultado: ele controla cartelas, pedras sorteadas, marcações válidas, participação
						na rodada, empates e vencedores. Não é preciso conferir manualmente uma cartela para declarar o ganhador.
					</p>
					<h3>Quem usa cada área</h3>
					<ul>
						<li>
							<strong>Organizador:</strong> cria e configura o evento, inscrições, cartelas e rodadas.
						</li>
						<li>
							<strong>Operador:</strong> inicia a rodada, sorteia as pedras e confirma a entrega do prêmio.
						</li>
						<li>
							<strong>Participante:</strong> faz a inscrição, abre sua cartela e, quando necessário, marca números.
						</li>
						<li>
							<strong>Público:</strong> acompanha o telão ou consulta a auditoria; essas duas páginas não exigem login.
						</li>
					</ul>
					<h3>Antes de abrir o evento</h3>
					<p>
						Faça um ensaio com uma cartela digital, uma cartela impressa se houver, um computador para a operação e outro para o
						telão. Confirme que todos mostram o mesmo evento e que a operação está <strong>Conectada</strong>. Revise prêmios,
						regras e imagens antes de iniciar uma rodada: depois do início, suas regras não podem ser editadas.
					</p>
					<p>
						O resultado oficial sempre vem do sistema. Se uma tela parecer atrasada, atualize-a e confira o estado recarregado;
						não declare vencedor a partir de anotação em papel, cache do navegador ou de uma tela desconectada.
					</p>
				</section>

				<section className="panel" id="organizadores">
					<p className="eyebrow">Para organizadores</p>
					<h2>Organize um evento do começo ao fim</h2>
					<ol>
						<li>
							<strong>Crie a conta da empresa.</strong> Na página inicial, escolha “Cadastrar empresa”, informe empresa, seu
							nome, e-mail e uma senha de pelo menos 12 caracteres. Se a empresa já possui conta, use “Entrar”.
						</li>
						<li>
							<strong>Crie o evento.</strong> Em Administração, informe o nome e escolha o modo de marcação. O código público
							exibido no cartão do evento identifica o bingo para participantes e auditoria.
						</li>
						<li>
							<strong>Configure o evento.</strong> Abra “Configurar”. Nessa tela ficam os atalhos de inscrição, QR Code,
							telão, auditoria, cartelas impressas e operação.
						</li>
						<li>
							<strong>Abra as inscrições.</strong> Use “Abrir inscrições”. Depois disso, o evento sai de preparação e pode
							receber participantes.
						</li>
						<li>
							<strong>Compartilhe a entrada.</strong> Use “Abrir QR Code de inscrição” para mostrar um QR Code ou “Visualizar
							inscrição” para copiar/abrir a página pública.
						</li>
						<li>
							<strong>Prepare as cartelas físicas, se houver.</strong> Gere o lote, imprima, associe cada cartela a uma pessoa
							e ative-a.
						</li>
						<li>
							<strong>Crie uma rodada e os prêmios.</strong> Dê um nome, preencha cada prêmio e selecione sua regra. Salve e
							abra a operação quando houver pelo menos uma cartela ativa.
						</li>
						<li>
							<strong>Ao terminar tudo, encerre o evento.</strong> Depois de finalizar ou cancelar todas as rodadas, use
							“Encerrar evento e publicar auditoria”. O evento fica somente para consulta.
						</li>
					</ol>
					<h3>Estados do evento</h3>
					<p>
						<strong>Em preparação:</strong> ainda permite abrir inscrições. <strong>Inscrições abertas:</strong> participantes
						podem gerar cartelas. <strong>Em andamento:</strong> começou ao iniciar a primeira rodada.{" "}
						<strong>Finalizado:</strong> não aceita mais alterações, inscrições nem novas cartelas; o atalho de telão fica
						desativado na administração e a auditoria permanece disponível.
					</p>
				</section>

				<section className="panel" id="cartelas">
					<h2>Cartelas e inscrições</h2>
					<h3>Cartela digital</h3>
					<p>
						Cada inscrição pública cria uma pessoa participante e uma cartela digital ativa. Guarde o link da cartela no
						celular; ele abre a mesma cartela novamente. A inscrição fica disponível apenas enquanto o evento estiver com
						inscrições abertas.
					</p>
					<h3>Cartela impressa</h3>
					<ol>
						<li>
							Informe de 1 a 1.000 em “Quantidade” e escolha “Gerar lote para impressão”. É possível gerar mais de um lote.
						</li>
						<li>
							Abra “Imprimir / salvar PDF” para usar a impressora ou gravar as cartelas em PDF. Cada cartela tem código, QR
							Code e fingerprint de conferência.
						</li>
						<li>
							De volta à configuração, selecione ou leia pelo QR Code a cartela impressa. Escolha um participante para
							associá-la ou use “Registrar e ativar cartela” para cadastrar a pessoa diretamente nessa cartela.
						</li>
						<li>
							Quando a cartela foi apenas associada, use “Ativar cartela”. O cadastro direto já registra e ativa conforme a
							confirmação exibida. Só uma cartela associada e ativa concorre.
						</li>
					</ol>
					<p>
						Uma cartela ativa que já estava no evento quando a rodada começa entra no conjunto daquela rodada. Cartelas
						inscritas, associadas ou ativadas depois do início só poderão concorrer em uma rodada futura.
					</p>
					<h3>Modos de marcação</h3>
					<ul>
						<li>
							<strong>Automática:</strong> toda pedra sorteada é marcada pelo sistema em todas as cartelas. O participante
							apenas acompanha.
						</li>
						<li>
							<strong>Manual obrigatória:</strong> na cartela digital, a pessoa deve marcar a pedra atual antes do próximo
							sorteio. Só a marcação salva pelo sistema conta para vencer.
						</li>
						<li>
							<strong>Manual assistida:</strong> a pessoa pode tocar em qualquer número da própria cartela que já tenha sido
							sorteado. Também aqui, somente as marcações registradas contam.
						</li>
					</ul>
					<p>
						A estrela no centro é a casa livre e já conta como marcada. Não é possível marcar número que não esteja na cartela,
						que não tenha sido sorteado ou que já esteja marcado.
					</p>
					<p>
						Em modo manual, aguarde a confirmação visual depois de tocar no número. A cartela fica bloqueada por instantes
						enquanto a marca é salva para evitar toque duplicado. Se aparecer erro, atualize a página e só tente novamente se o
						número continuar habilitado.
					</p>
				</section>

				<section className="panel">
					<h2>Rodadas, prêmios e regras</h2>
					<p>
						Um evento pode ter várias rodadas. Cada rodada tem uma nova sequência de 75 pedras, mas uma cartela pertence ao
						evento e pode ser reutilizada em rodadas futuras. A rodada fica “Pronta para iniciar” até o primeiro sorteio;
						somente nesse estado ela pode ser editada.
					</p>
					<p>
						Crie ao menos uma etapa de prêmio. Você pode adicionar ou remover etapas, dar nome a cada prêmio e anexar uma foto
						JPEG, PNG ou WebP de até 2 MB; a imagem aparece no telão. Uma mesma regra só pode ser usada uma vez na mesma rodada.
					</p>
					<p>
						As etapas são automaticamente organizadas da regra mais simples à mais difícil, mesmo que tenham sido adicionadas em
						outra ordem. Após entregar um prêmio, a rodada avança para a próxima etapa sem reiniciar as pedras.
					</p>
					<h3>Como cada regra é ganha</h3>
					<ul>
						<li>
							<strong>Coluna B, I, N, G ou O:</strong> complete a coluna indicada. Nesses prêmios, o sorteio usa apenas a
							faixa da coluna: B 1–15, I 16–30, N 31–45, G 46–60 e O 61–75.
						</li>
						<li>
							<strong>Quatro cantos:</strong> marque os quatro cantos da cartela.
						</li>
						<li>
							<strong>Diagonal B ou diagonal O:</strong> complete a diagonal indicada.
						</li>
						<li>
							<strong>Uma linha ou duas linhas:</strong> complete uma ou duas linhas horizontais.
						</li>
						<li>
							<strong>X:</strong> complete as duas diagonais.
						</li>
						<li>
							<strong>T:</strong> complete a linha superior e a coluna central.
						</li>
						<li>
							<strong>Cruz:</strong> complete a linha central e a coluna central.
						</li>
						<li>
							<strong>Moldura:</strong> complete toda a borda externa.
						</li>
						<li>
							<strong>Cartela cheia:</strong> complete todos os números da cartela.
						</li>
					</ul>
				</section>

				<section className="panel" id="participantes">
					<p className="eyebrow">Para participantes</p>
					<h2>Entre no bingo e use sua cartela</h2>
					<ol>
						<li>
							Abra o link compartilhado pela organização, leia o QR Code ou, na página inicial, escolha “Participar” e informe
							o código do evento.
						</li>
						<li>
							Informe seu nome e escolha: Colaborador, Familiar ou Convidado. Para familiar e convidado, informe o nome do
							colaborador responsável. Matrícula é opcional para colaborador.
						</li>
						<li>
							Escolha “Gerar minha cartela”. A tela da cartela mostra o prêmio e a regra atuais, as pedras já sorteadas e o
							modo de marcação.
						</li>
						<li>
							Na marcação automática, só acompanhe. Em modo manual, toque exclusivamente nos números liberados pelo sorteio,
							conforme a orientação mostrada na própria cartela.
						</li>
						<li>
							Se sua cartela for vencedora, a página mostrará a confirmação. Aguarde o operador revelar o resultado no telão e
							siga a orientação da organização para retirar o prêmio.
						</li>
					</ol>
					<p>
						Após uma rodada concluída, a opção “Gerar nova cartela” aparece apenas para a cartela digital que participou dela e
						está completa conforme o modo de marcação. A cartela antiga é cancelada e a nova vale para a próxima rodada, desde
						que ela ainda não tenha começado.
					</p>
					<h3>Compra de cartelas</h3>
					<p>
						Quando a organização habilitar a compra de cartelas, crie ou acesse uma conta de participante com nome, e-mail e
						senha. A compra fica vinculada a essa conta e as cartelas podem ser reencontradas em “Minhas cartelas”. Ative cada
						cartela que deseja usar; somente cartela ativa concorre na próxima rodada. A organização pode limitar ou encerrar a
						venda sem fechar as inscrições comuns.
					</p>
					<p>
						Se a página parecer parada, atualize-a. A cartela recarrega o estado oficial ao reconectar; não é necessário criar
						outra inscrição.
					</p>
				</section>

				<section className="panel" id="operacao">
					<p className="eyebrow">Para o operador</p>
					<h2>Conduza o sorteio ao vivo</h2>
					<ol>
						<li>Na lista “Rodadas criadas”, escolha “Abrir operação” na rodada correta.</li>
						<li>
							Confira se há ao menos uma cartela ativa. Clique em “INICIAR RODADA”. O sistema congela as cartelas que irão
							concorrer, prepara as 75 pedras sem repetição e publica o hash de auditoria antes da primeira pedra.
						</li>
						<li>
							Confira o indicador de conexão e use “SORTEAR PRÓXIMA PEDRA” apenas quando estiver <strong>Conectado</strong>.
							Clique uma vez, aguarde a confirmação e só então faça outra ação. A última pedra, a quantidade sorteada,
							cartelas e telão são atualizados em tempo real.
						</li>
						<li>
							Ao detectar uma ou mais cartelas vencedoras, o sorteio pausa. Use “REVELAR VENCEDOR”; se houver empate, o botão
							será “REALIZAR DESEMPATE”.
						</li>
						<li>Depois da revelação, confirme “PRÊMIO ENTREGUE” ou “VENCEDOR NÃO RETIROU O PRÊMIO”.</li>
						<li>
							Quando o prêmio foi entregue, use “CONTINUAR SORTEIO NO TELÃO”. Isso fecha a apresentação e libera a próxima
							etapa ou a próxima pedra. Se foi a última etapa, a rodada termina.
						</li>
					</ol>
					<h3>Empate, não retirada e cancelamento</h3>
					<p>
						Se mais de uma cartela completa a mesma regra na mesma pedra, o sistema sorteia posições únicas de desempate de
						forma segura e revela a classificação no telão. Não há limite de 75 posições nesse desempate.
					</p>
					<p>
						Se o vencedor não retirar o prêmio, escolha a opção correspondente. A cartela recusada deixa de disputar apenas
						aquela etapa; a mesma regra continua ativa e o sorteio procura outro vencedor.
					</p>
					<p>
						“CANCELAR RODADA” só fica disponível enquanto a rodada está em sorteio e ainda não há vencedor aguardando revelação.
						Uma rodada cancelada termina sem vencedor e permite preparar outra rodada.
					</p>
				</section>

				<section className="panel" id="conexao">
					<p className="eyebrow">Confiabilidade da operação</p>
					<h2>Conexão, confirmação e recuperação</h2>
					<p>
						Enquanto uma ação está sendo enviada, o botão fica como <strong>PROCESSANDO</strong>; isso impede dois cliques no
						mesmo comando. Se a conexão estiver reconectando ou indisponível, iniciar a rodada e sortear ficam bloqueados. Esse
						cuidado evita que a operação avance a partir de uma tela sem a última atualização.
					</p>
					<ol>
						<li>
							Se houver timeout, falha ou reconexão, não repita imediatamente o sorteio e não abra outra operação para tentar
							de novo.
						</li>
						<li>Aguarde o estado voltar a “Conectado” ou atualize a página.</li>
						<li>
							Confira a última pedra e o histórico fornecidos pelo sistema. Se a pedra apareceu, o comando já foi concluído.
						</li>
						<li>
							Somente depois execute a próxima ação permitida pela rodada. Em caso de conflito, recarregar é a recuperação
							correta.
						</li>
					</ol>
					<p>
						A versão instalável do aplicativo pode manter a interface disponível depois de uma visita, mas não guarda estado de
						jogo, pedras, marcações ou comandos. Para participar, marcar ou operar, a conexão com o sistema continua
						obrigatória.
					</p>
				</section>

				<section className="panel" id="consulta-publica">
					<p className="eyebrow">Páginas públicas</p>
					<h2>Telão e auditoria</h2>
					<h3>Telão</h3>
					<p>
						Na configuração do evento, escolha “Telão” ou “Abrir telão” e abra a página em um computador ligado ao projetor. Ela
						não exige login. Mostra prêmio, foto do prêmio, última pedra, histórico, etapas e estatísticas de quantas cartelas
						estão a uma, duas ou três pedras de vencer. Use o botão de tema para adaptar a imagem ao ambiente.
					</p>
					<p>
						Quando há vencedor, o telão faz suspense e mostra apenas a pedra vencedora, o prêmio e a quantidade de cartelas
						empatadas, quando houver. Nome e resultado do desempate aparecem somente depois da ação do operador.
					</p>
					<p>
						O telão recebe a última pedra e as estatísticas agregadas sem precisar expor nomes antes da revelação. Após uma
						reconexão, ele busca de novo o estado oficial. Se permanecer atrasado, atualize a página antes de anunciar qualquer
						resultado.
					</p>
					<h3>Auditoria pública</h3>
					<p>
						Na página inicial, informe o código do evento e escolha “Consultar auditoria”, ou use o atalho “Auditoria” na
						configuração. A consulta mostra participantes, cartelas, rodadas, hash da sequência, pedras sorteadas, regras,
						vencedores e uma linha do tempo das ações.
					</p>
					<p>
						O hash SHA-256 é publicado no começo da rodada para demonstrar que a sequência foi definida antes do sorteio. A
						sequência completa de 75 pedras só aparece depois que a rodada termina. Ao encerrar o evento, a auditoria é
						publicada definitivamente.
					</p>
				</section>

				<section className="panel" id="duvidas">
					<h2>Dúvidas e problemas comuns</h2>
					<dl>
						<dt>Não consigo entrar ou gerar cartela.</dt>
						<dd>
							Confira o código e peça ao organizador para verificar se as inscrições foram abertas. Eventos finalizados não
							aceitam novas inscrições.
						</dd>
						<dt>Minha cartela não concorreu.</dt>
						<dd>
							Ela precisa estar ativa antes de a rodada iniciar. Para cartela impressa, confirme associação e ativação. Depois
							do início, a lista de concorrentes não muda.
						</dd>
						<dt>Não consigo marcar um número.</dt>
						<dd>
							Em modo automático a marcação é feita pelo sistema. Em modos manuais, o número precisa estar na sua cartela, já
							ter sido sorteado e ainda não estar marcado. Na manual obrigatória, somente a pedra atual pode ser marcada.
						</dd>
						<dt>O sorteio não avança.</dt>
						<dd>
							Verifique se há vencedor aguardando revelação, entrega do prêmio ou fechamento da apresentação. Sem vencedores,
							a rodada pode ter sido cancelada ou não haver mais pedras compatíveis com a regra ativa.
						</dd>
						<dt>A operação mostra reconectando ou falhou ao sortear.</dt>
						<dd>
							Aguarde o indicador voltar a Conectado e atualize a operação. Confira a última pedra no histórico antes de tocar
							em qualquer botão novamente; não repita um sorteio apenas porque houve demora na resposta.
						</dd>
						<dt>Toquei para marcar e apareceu um erro.</dt>
						<dd>
							Aguarde a mensagem, atualize a cartela e confirme se a marca foi salva. Tente de novo somente se aquele número
							ainda estiver habilitado pela cartela.
						</dd>
						<dt>Posso editar ou excluir uma rodada?</dt>
						<dd>
							A tela permite editar somente rodadas prontas para iniciar. Não há exclusão de rodada; antes do encerramento do
							evento, uma rodada em sorteio pode ser cancelada.
						</dd>
						<dt>O que fazer se a tela atrasar?</dt>
						<dd>Atualize a página e confira a conexão. O sistema busca novamente o estado oficial após reconectar.</dd>
					</dl>
					<div className="actions">
						<Link className="primary button" to="/">
							Voltar para o início
						</Link>
					</div>
				</section>
			</main>
		</AppShell>
	);
}
