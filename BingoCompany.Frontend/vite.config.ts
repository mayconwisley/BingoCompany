import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig(({ mode }) => {
	const environment = loadEnv(mode, "", "VITE_");

	return {
		base: environment.VITE_BASE_PATH ?? "/",
		plugins: [react()],
		build: {
			cssCodeSplit: true,
			rollupOptions: {
				output: {
					manualChunks(id) {
						if (!id.includes("node_modules")) {
							return undefined;
						}

						if (id.includes("@microsoft/signalr")) {
							return "signalr";
						}

						if (id.includes("@zxing")) {
							return "qr-scanner";
						}

						if (id.includes("qrcode")) {
							return "qrcode";
						}

						if (id.includes("react") || id.includes("scheduler")) {
							return "react";
						}

						return "vendor";
					}
				}
			}
		}
	};
});
