import { http } from "../../../shared/api/httpClient";

export type AuthSession = { token: string; name: string; companyName: string };

export const authApi = {
    login: (email: string, password: string) =>
        http<AuthSession>("/api/auth/login", { method: "POST", body: JSON.stringify({ email, password }) }),
    register: (companyName: string, name: string, email: string, password: string) =>
        http<AuthSession>("/api/auth/register", { method: "POST", body: JSON.stringify({ companyName, name, email, password }) })
};
