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
        scp: ['Source Code Pro', 'mono'],
      }
    },
  },
  plugins: [
    require('@tailwindcss/forms')
  ],
};
