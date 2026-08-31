import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { App } from "./app/App";
import { ApplicationInfoProvider } from "./app/ApplicationInfoContext";
import { SessionProvider } from "./shared/auth/SessionContext";
import { ApplicationThemeProvider } from "./shared/ui/ApplicationTheme";
import "./styles.css";
import "./shared/ui/components.css";
createRoot(document.getElementById("root")!).render(
	<StrictMode>
		<ApplicationThemeProvider>
			<BrowserRouter basename={import.meta.env.BASE_URL}>
				<SessionProvider>
					<ApplicationInfoProvider>
						<App />
					</ApplicationInfoProvider>
				</SessionProvider>
			</BrowserRouter>
		</ApplicationThemeProvider>
	</StrictMode>
);
