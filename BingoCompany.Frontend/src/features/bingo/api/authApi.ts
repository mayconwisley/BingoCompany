import { http } from "../../../shared/api/httpClient";

export type AuthSession = { name: string; companyName: string; accountType?: "company" | "participant" };

export const authApi = {
	login: (email: string, password: string) =>
		http<AuthSession>("/api/auth/login", { method: "POST", body: JSON.stringify({ email, password }) }),
	register: (companyName: string, name: string, email: string, password: string) =>
		http<AuthSession>("/api/auth/register", { method: "POST", body: JSON.stringify({ companyName, name, email, password }) }),
	participantLogin: (email: string, password: string) =>
		http<{ name: string }>("/api/participant-auth/login", { method: "POST", body: JSON.stringify({ email, password }) }),
	participantRegister: (name: string, email: string, password: string) =>
		http<{ name: string }>("/api/participant-auth/register", { method: "POST", body: JSON.stringify({ name, email, password }) }),
	logout: () => http<void>("/api/auth/logout", { method: "POST" })
};
