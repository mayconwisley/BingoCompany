import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { App } from "./app/App";
import { ApplicationInfoProvider } from "./app/ApplicationInfoContext";
import { SessionProvider } from "./shared/auth/SessionContext";
import { ApplicationThemeProvider } from "./shared/ui/ApplicationTheme";
import "./styles.css";
import "./shared/ui/components.css";

if ("serviceWorker" in navigator && import.meta.env.PROD) {
	window.addEventListener("load", () => {
		void navigator.serviceWorker.register(`${import.meta.env.BASE_URL}service-worker.js`, { scope: import.meta.env.BASE_URL });
	});
}

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
