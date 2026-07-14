import { Link } from "react-router-dom";
import type { ApplicationInfo } from "../../features/bingo/api/applicationInfoApi";

type Props = {
    application?: ApplicationInfo;
};

export function ApplicationFooter({ application }: Props) {
    return (
        <footer className="application-footer">
            <div>
                {application ? (
                    <>
                        <strong>{application.name}</strong>
                        <span>{application.description}</span>
                    </>
                ) : (
                    <span>Carregando informações do sistema...</span>
                )}
            </div>
            <div>
                <span>{application ? `Versão ${application.version}` : ""}</span>
                <span>Desenvolvido por Maycon Wisley</span>
                <a href="https://github.com/mayconwisley/BingoCompany" target="_blank" rel="noreferrer">
                    Projeto open source
                </a>
                <Link to="/ajuda">Ajuda</Link>
            </div>
            <p className="application-footer-notice">Projeto demonstrativo online: não informe dados pessoais, reais ou sensíveis.</p>
        </footer>
    );
}
