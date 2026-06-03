/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        surface: {
          900: '#0f1117',
          800: '#1a1d27',
          700: '#222636',
          600: '#2d3148',
        },
      },
    },
  },
  plugins: [],
};
