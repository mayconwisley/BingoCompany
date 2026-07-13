export function FeedbackMessage({ error, success }: { error?: string; success?: string })
{
    if (error) return <p className="feedback feedback-error" role="alert">{error}</p>;
    if (success) return <p className="feedback feedback-success" role="status" aria-live="polite">{success}</p>;
    return null;
}
