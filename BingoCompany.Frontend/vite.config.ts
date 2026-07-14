import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig(({ mode }) => {
	const environment = loadEnv(mode, "", "VITE_");

	return {
		base: environment.VITE_BASE_PATH ?? "/",
		plugins: [react()],
		build: {
			cssCodeSplit: true
		}
	};
});
