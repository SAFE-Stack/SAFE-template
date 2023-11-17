import { defineConfig } from "vite";


const proxyPort = process.env.SERVER_PROXY_PORT || "5000";
const proxyTarget = "http://localhost:" + proxyPort;

// https://vitejs.dev/config/
export default defineConfig({
    build: {
        outDir: "../../deploy/public",
    },
    server: {
        port: 8080,
        proxy: {
            // redirect requests that start with /api/ to the server on port 5000
            "/api/": {
                target: proxyTarget,
                changeOrigin: true,
            },
            // redirect websocket requests that start with /socket/ to the server on the port 5000
            "/socket/": {
                target: proxyTarget,
                ws: true,
            },
        },
    },
});
