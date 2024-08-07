module.exports = {
  content: [
    '../**/*.razor',
    '../wwwroot/index.html',
    '../**/*.cshtml'
  ],
  darkMode: 'class',
  theme: {
    extend: {
      fontFamily: {
        inter: ['Inter', 'sans-serif'],
      }
    },
  },
  plugins: [
    require('@tailwindcss/forms')
  ],
};
