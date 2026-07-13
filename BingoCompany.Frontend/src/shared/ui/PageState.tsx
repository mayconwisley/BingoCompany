export function PageState({ loading, error, onRetry }: { loading?: boolean; error?: string; onRetry?: () => void }) {
    if (loading)
        return (
            <section className="page-state" aria-live="polite">
                <span className="spinner" aria-hidden="true" />
                Carregando informações...
            </section>
        );
    if (error)
        return (
            <section className="page-state page-state-error" role="alert">
                <p>{error}</p>
                {onRetry && (
                    <button type="button" onClick={onRetry}>
                        Tentar novamente
                    </button>
                )}
            </section>
        );
    return null;
}
