/** @type {import('tailwindcss').Config} */
module.exports = {
    mode: "jit",
    content: [
        "./index.html",
        "./**/*.{fs,js,ts,jsx,tsx}",
        "!./node_modules/**/*",
    ],
    theme: {
        extend: {},
    },
    plugins: []
}
