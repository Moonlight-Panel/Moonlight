export default {
    plugins: {
        "@tailwindcss/postcss": {},
        "./postcss-wrap-scope.js":{
            "root":"html",
            "exclude":"[data-module]"
        },
        "cssnano": {
            preset: 'default',
        },
    }
}