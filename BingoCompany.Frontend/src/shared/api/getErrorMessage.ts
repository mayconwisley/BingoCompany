import { ApiError } from "./httpClient";

export function getErrorMessage(error: unknown, fallback = "Não foi possível concluir esta operação. Tente novamente."): string {
    if (!(error instanceof ApiError)) return fallback;
    if (error.status === 0) return error.message;
    if (error.status === 404) return "O item solicitado não foi encontrado ou não está mais disponível.";
    if (error.status >= 500) return "O servidor não conseguiu concluir a operação. Tente novamente em alguns instantes.";

    return error.message || fallback;
}
