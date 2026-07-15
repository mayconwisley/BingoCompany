import { createContext, useContext, useEffect, useState } from "react";
import type { PropsWithChildren } from "react";
import { authApi } from "../../features/bingo";
import type { AuthSession } from "../../features/bingo";
import { clearSession, getSession, saveSession, sessionChangedEvent } from "./session";

type SessionContextValue = {
	session: AuthSession | undefined;
	isLoading: boolean;
};

const SessionContext = createContext<SessionContextValue | undefined>(undefined);

export function SessionProvider({ children }: PropsWithChildren) {
	const [session, setSession] = useState<AuthSession | undefined>();
	const [isLoading, setIsLoading] = useState(true);

	useEffect(() => {
		let isMounted = true;

		const loadSession = async () => {
			try {
				const authenticatedSession = await authApi.getSession();
				if (!isMounted) return;
				saveSession(authenticatedSession);
				setSession(authenticatedSession);
			} catch {
				if (!isMounted) return;
				clearSession();
				setSession(undefined);
			} finally {
				if (isMounted) setIsLoading(false);
			}
		};

		void loadSession();
		return () => {
			isMounted = false;
		};
	}, []);

	useEffect(() => {
		const synchronizeSession = () => setSession(getSession());
		window.addEventListener(sessionChangedEvent, synchronizeSession);
		return () => window.removeEventListener(sessionChangedEvent, synchronizeSession);
	}, []);

	return <SessionContext.Provider value={{ session, isLoading }}>{children}</SessionContext.Provider>;
}

export function useSession(): SessionContextValue {
	return useContext(SessionContext) ?? { session: getSession(), isLoading: false };
}
