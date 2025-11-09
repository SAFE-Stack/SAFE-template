/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./index.html",
        "./**/*.{fs,js,ts,jsx,tsx}",
        "./output/**/*.js", // Scan Fable compiled output
        "!./node_modules/**/*",
    ],
    theme: {
        extend: {},
    },
    plugins: [],
    daisyui: {
        // themes: ["light", "dark", "cupcake", "bumblebee", "emerald", "corporate", "synthwave", "retro", "cyberpunk", "valentine", "halloween", "garden", "forest", "aqua", "lofi", "pastel", "fantasy", "wireframe", "black", "luxury", "dracula"],
        // darkTheme: "dark",
        // base: true,
        // styled: true,
        // utils: true,
        // prefix: "",
        // logs: true,
        // themeRoot: ":root",
    },
}
