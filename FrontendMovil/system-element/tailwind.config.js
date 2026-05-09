/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      fontFamily: {
        sans: ["Roboto", "Helvetica", "Arial", "sans-serif"],
      },
      maxWidth: {
        8: "8rem",
        10: "10rem",
        15: "15rem",
        17: "17rem",
      },
      height: {
        100: "28rem",
        22: "5.5rem",
      },
    },
  },
  plugins: [],
}

