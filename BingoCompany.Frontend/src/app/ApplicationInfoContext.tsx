import { createContext, useCallback, useContext } from "react";
import type { PropsWithChildren } from "react";
import { applicationInfoApi } from "../features/bingo/api/applicationInfoApi";
import type { ApplicationInfo } from "../features/bingo/api/applicationInfoApi";
import { useAsyncResource } from "../shared/hooks/useAsyncResource";

const ApplicationInfoContext = createContext<ApplicationInfo | undefined>(undefined);

export function ApplicationInfoProvider({ children }: PropsWithChildren) {
	const loader = useCallback(() => applicationInfoApi.get(), []);
	const application = useAsyncResource(loader);

	return <ApplicationInfoContext.Provider value={application.data}>{children}</ApplicationInfoContext.Provider>;
}

export function useApplicationInfo(): ApplicationInfo | undefined {
	return useContext(ApplicationInfoContext);
}
