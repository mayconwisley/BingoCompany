import { useParams } from "react-router-dom";
import { RegistrationQrCode } from "../../features/bingo";
import { AppShell } from "../../shared/ui/AppShell";

export function RegistrationSharePage() {
	const { publicCode = "" } = useParams();
	const registrationPath = `/bingo/participar/${publicCode}`;

	return (
		<AppShell showAdministration={false}>
			<main className="registration-share">
				<p className="eyebrow">Inscrição no bingo</p>
				<h1>Entre no jogo</h1>
				<p className="subtitle">Aponte a câmera para o QR Code, faça sua inscrição e gere sua cartela digital.</p>
				<RegistrationQrCode registrationPath={registrationPath} />
				<code className="registration-link">{registrationPath}</code>
			</main>
		</AppShell>
	);
}
