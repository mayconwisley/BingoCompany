import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
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
});
