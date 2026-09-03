import { useCallback } from "react";
import { useParams } from "react-router-dom";
import { bingoApi } from "../../features/bingo";
import { RegistrationQrCode } from "../../features/bingo/qr";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

export function RegistrationSharePage() {
	const { publicCode = "" } = useParams();
	const event = useAsyncResource(useCallback(() => bingoApi.getPublicEvent(publicCode), [publicCode]));
	const registrationPath = `/participar/${publicCode}`;
	const applicationBasePath = import.meta.env.BASE_URL.replace(/\/$/, "");
	const registrationUrl = new URL(`${applicationBasePath}${registrationPath}`, window.location.origin).toString();

	return (
		<AppShell showAdministration={false}>
			<main className="registration-share">
				<PageState loading={event.loading} error={event.error} onRetry={event.reload} />
				{event.data &&
					(event.data.status === "RegistrationOpen" ? (
						<>
							<p className="eyebrow">Inscrição no bingo</p>
							<h1>Entre no jogo</h1>
							<p className="subtitle">Aponte a câmera para o QR Code, faça sua inscrição e gere sua cartela digital.</p>
							<RegistrationQrCode registrationPath={registrationPath} />
							<code className="registration-link">{registrationUrl}</code>
						</>
					) : (
						<section className="panel">
							<p className="eyebrow">Inscrições encerradas</p>
							<h1>{event.data.name}</h1>
							<FeedbackMessage warning="As inscrições deste bingo estão fechadas." />
							<p>
								A primeira rodada já foi iniciada ou o evento foi encerrado. Não é mais possível gerar ou comprar cartelas.
							</p>
						</section>
					))}
			</main>
		</AppShell>
	);
}
