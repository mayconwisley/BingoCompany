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
                <span>Desenvolvido por Maycon Wisley e Vesão</span>
                <Link to="/ajuda">Ajuda</Link>
            </div>
        </footer>
    );
}
