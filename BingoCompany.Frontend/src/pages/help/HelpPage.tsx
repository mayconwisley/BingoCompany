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
							Um guia simples para organizar o bingo, participar da partida e acompanhar os resultados.
						</p>
					</div>
				</header>
				<nav className="help-navigation" aria-label="Tópicos da ajuda">
					<a href="#organizadores">Organizar</a>
					<a href="#participantes">Participar</a>
					<a href="#telao">Telão</a>
					<a href="#auditoria">Auditoria</a>
				</nav>
				<section className="panel">
					<h2>O que é o Bingo Company?</h2>
					<p>
						É uma plataforma para realizar bingos corporativos em tempo real. A empresa cria o evento, configura os prêmios e
						conduz o sorteio. As pessoas entram usando o código ou o QR Code do evento e recebem cartelas digitais. Também é
						possível gerar cartelas físicas.
					</p>
					<p>
						O sistema controla o sorteio, as marcações, a elegibilidade e os vencedores. Assim, o resultado não depende de uma
						conferência manual.
					</p>
					<p>
						O núcleo do sorteio foi validado com 1.000 cartelas elegíveis na mesma rodada, inclusive em um empate com 1.000
						candidatos. Em eventos maiores, a organização também deve confirmar a capacidade da infraestrutura e da conexão
						local.
					</p>
				</section>
				<section className="panel" id="organizadores">
					<p className="eyebrow">Para organizadores</p>
					<h2>Organize um evento do começo ao fim</h2>
					<ol>
						<li>
							<strong>Cadastre a empresa e entre.</strong> Na página inicial, escolha “Cadastrar empresa”. Depois, use
							“Entrar” para acessar a área administrativa.
						</li>
						<li>
							<strong>Crie o evento.</strong> Informe um nome e escolha o modo de marcação. Na marcação automática, os números
							sorteados são marcados pelo sistema. Nos modos manuais, cada participante confirma os números na própria
							cartela.
						</li>
						<li>
							<strong>Abra as inscrições.</strong> Na configuração do evento, use o QR Code de inscrição ou compartilhe o link
							público. Os participantes poderão informar seus dados e gerar a cartela digital.
						</li>
						<li>
							<strong>Prepare cartelas físicas, se desejar.</strong> Use a área “Cartelas” para gerar, imprimir, associar a um
							participante e ativar cada cartela.
						</li>
						<li>
							<strong>Crie a rodada.</strong> Defina o nome, os prêmios e as regras. Antes de iniciar, a rodada pode ser
							editada. Cada regra de premiação só pode ser usada uma vez na mesma rodada.
						</li>
						<li>
							<strong>Abra a operação.</strong> Inicie a rodada e faça um sorteio por vez. O sistema registra cada pedra e
							atualiza o telão e as cartelas conectadas.
						</li>
					</ol>
				</section>
				<section className="panel">
					<h2>Prêmios e regras de vitória</h2>
					<p>
						Uma rodada pode ter várias etapas de prêmio. Elas são disputadas na ordem em que você as configurou. Depois de
						confirmar a entrega do prêmio, a próxima etapa começa sem reiniciar a sequência de pedras.
					</p>
					<ul>
						<li>
							<strong>Uma linha e duas linhas:</strong> completa uma ou duas linhas horizontais.
						</li>
						<li>
							<strong>Colunas B, I, N, G e O:</strong> completa a coluna correspondente.
						</li>
						<li>
							<strong>Diagonal B, diagonal O e X:</strong> completa a diagonal indicada ou as duas diagonais no X.
						</li>
						<li>
							<strong>Quatro cantos, T, moldura e cruz:</strong> completa o desenho correspondente na cartela.
						</li>
						<li>
							<strong>Cartela cheia:</strong> completa todos os números da cartela.
						</li>
					</ul>
					<p>A casa livre no centro da cartela já conta como marcada quando a regra precisar dela.</p>
				</section>
				<section className="panel">
					<h2>O que acontece quando há um vencedor?</h2>
					<p>
						Ao completar a regra do prêmio ativo, o sorteio pausa. Antes da revelação, o telão mostra apenas o suspense e a
						quantidade de cartelas detectadas. O nome só aparece quando o operador usa “Revelar vencedor”.
					</p>
					<p>
						Se mais de uma cartela completar a regra na mesma pedra, o sistema inicia o desempate. Depois da revelação, o
						operador registra se o prêmio foi entregue. Se o vencedor não retirar, a cartela deixa de concorrer somente nessa
						etapa e o sorteio continua com a mesma regra para encontrar outro vencedor.
					</p>
				</section>
				<section className="panel" id="participantes">
					<p className="eyebrow">Para participantes</p>
					<h2>Entre no bingo e use sua cartela</h2>
					<ol>
						<li>
							Na página inicial, escolha “Participar” e informe o código do evento, ou aponte a câmera para o QR Code
							compartilhado pela organização.
						</li>
						<li>
							Preencha seu nome e o tipo de participação. Se for familiar ou convidado, informe o colaborador responsável.
						</li>
						<li>
							Depois de gerar a cartela, acompanhe os números sorteados. Na marcação automática, nada precisa ser feito. Na
							marcação manual, toque somente nos números já sorteados.
						</li>
						<li>Quando seu código aparecer no telão, avise a organização e aguarde a revelação do nome.</li>
					</ol>
					<p>
						Uma cartela completa pode gerar uma nova cartela somente depois que a rodada anterior for encerrada, desde que tenha
						participado dela e a próxima rodada ainda não tenha começado.
					</p>
				</section>
				<section className="panel" id="telao">
					<p className="eyebrow">Para o telão</p>
					<h2>Abra uma visualização pública</h2>
					<p>
						Na tela de configuração do evento, selecione “Telão”. Essa página pode ser aberta em outra aba ou em um computador
						conectado ao projetor. Ela mostra a pedra mais recente, o prêmio em disputa, as etapas da rodada e o suspense antes
						da revelação.
					</p>
					<p>O telão não exige login. Use o botão de tema para adaptar a visualização ao ambiente.</p>
				</section>
				<section className="panel" id="auditoria">
					<p className="eyebrow">Consulta pública</p>
					<h2>Veja a auditoria sem fazer login</h2>
					<p>
						Na página inicial, escolha “Auditoria pública”, informe o código do evento e selecione “Consultar auditoria”. A
						consulta mostra o histórico do evento, as rodadas, as pedras sorteadas, os prêmios e os resultados.
					</p>
					<p>
						Quando uma rodada termina, a sequência completa de pedras fica disponível para conferência. Antes do início, o
						sistema mantém apenas o registro de segurança da sequência para garantir que o sorteio não seja alterado.
					</p>
				</section>
				<section className="panel">
					<h2>Dúvidas rápidas</h2>
					<dl>
						<dt>Onde encontro o código do evento?</dt>
						<dd>
							Na área administrativa, ao abrir a configuração do evento. Ele também aparece nos links e QR Codes
							compartilhados.
						</dd>
						<dt>Posso editar uma rodada?</dt>
						<dd>
							Sim. Enquanto estiver “Pronta para iniciar”, use “Editar rodada”. Depois do início, a configuração fica
							bloqueada para preservar o resultado.
						</dd>
						<dt>Posso encerrar o evento?</dt>
						<dd>
							Sim, após finalizar todas as rodadas. Ao encerrar, o evento fica imutável e a auditoria continua disponível
							publicamente.
						</dd>
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
