const tailwindcss = require('@tailwindcss/postcss');
const extractClasses = require('./extract-classes');

module.exports = {
    plugins: [
        tailwindcss
    ],
};

if(process.env.EXTRACT_CLASSES === "true")
    module.exports.plugins.push(extractClasses);