export class ApiError extends Error {
	public constructor(
		message: string,
		public readonly status: number
	) {
		super(message);
		this.name = "ApiError";
	}
}

export const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5138";

export async function http<T>(path: string, init?: RequestInit): Promise<T> {
	let response: Response;

	try {
		response = await fetch(`${API_URL}${path}`, {
			...init,
			credentials: "include",
			headers: { "Content-Type": "application/json", ...init?.headers }
		});
	} catch {
		throw new ApiError("Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.", 0);
	}

	if (!response.ok) {
		if (response.status === 401) {
			clearSession();
		}

		const message = await response.text();
		throw new ApiError(message || "Não foi possível concluir esta operação.", response.status);
	}

	return response.status === 204 ? (undefined as T) : (response.json() as Promise<T>);
}
import { clearSession } from "../auth/session";
