import type { AuthSession } from "../../features/bingo/api/authApi";

const sessionKey = "bingo-company-session";
export const sessionChangedEvent = "bingo-company-session-changed";

export function getSession(): AuthSession | undefined {
	try {
		const stored = window.localStorage.getItem(sessionKey);
		return stored ? (JSON.parse(stored) as AuthSession) : undefined;
	} catch {
		return undefined;
	}
}

export function saveSession(session: AuthSession): void {
	window.localStorage.setItem(sessionKey, JSON.stringify(session));
	window.dispatchEvent(new Event(sessionChangedEvent));
}

export function clearSession(): void {
	window.localStorage.removeItem(sessionKey);
	window.dispatchEvent(new Event(sessionChangedEvent));
}
