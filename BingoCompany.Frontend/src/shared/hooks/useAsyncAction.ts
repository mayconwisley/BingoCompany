import { useCallback, useState } from "react";
import { getErrorMessage } from "../api/getErrorMessage";

type AsyncActionState = {
	error?: string;
	success?: string;
};

export function useAsyncAction() {
	const [state, setState] = useState<AsyncActionState>({});
	const [isPending, setIsPending] = useState(false);

	const execute = useCallback(async <T>(action: () => Promise<T>, success?: string, fallback?: string): Promise<T | undefined> => {
		setIsPending(true);
		setState({});

		try {
			const result = await action();
			setState(success ? { success } : {});
			return result;
		} catch (error) {
			setState({ error: getErrorMessage(error, fallback) });
			return undefined;
		} finally {
			setIsPending(false);
		}
	}, []);

	const clear = useCallback(() => setState({}), []);

	return { ...state, isPending, execute, clear };
}
