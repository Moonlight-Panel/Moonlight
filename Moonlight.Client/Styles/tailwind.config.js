/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        '../**/*.razor',
        'mappings/*.map'
    ],
    theme: {
        extend: {
            fontFamily: {
                inter: ['Inter', 'sans-serif'],
                scp: ['Source Code Pro', 'mono'],
            },
            colors: {
                primary: {
                    50: 'rgb(var(--color-primary-50))',
                    100: 'rgb(var(--color-primary-100))',
                    200: 'rgb(var(--color-primary-200))',
                    300: 'rgb(var(--color-primary-300))',
                    400: 'rgb(var(--color-primary-400))',
                    500: 'rgb(var(--color-primary-500))',
                    600: 'rgb(var(--color-primary-600))',
                    700: 'rgb(var(--color-primary-700))',
                    800: 'rgb(var(--color-primary-800))',
                    900: 'rgb(var(--color-primary-900))',
                    950: 'rgb(var(--color-primary-950))'
                },
                secondary: {
                    100: 'rgb(var(--color-secondary-100))',
                    200: 'rgb(var(--color-secondary-200))',
                    300: 'rgb(var(--color-secondary-300))',
                    400: 'rgb(var(--color-secondary-400))',
                    500: 'rgb(var(--color-secondary-500))',
                    600: 'rgb(var(--color-secondary-600))',
                    700: 'rgb(var(--color-secondary-700))',
                    800: 'rgb(var(--color-secondary-800))',
                    900: 'rgb(var(--color-secondary-900))',
                    950: 'rgb(var(--color-secondary-950))'
                },
                tertiary: {
                    50: 'rgb(var(--color-tertiary-50))',
                    100: 'rgb(var(--color-tertiary-100))',
                    200: 'rgb(var(--color-tertiary-200))',
                    300: 'rgb(var(--color-tertiary-300))',
                    400: 'rgb(var(--color-tertiary-400))',
                    500: 'rgb(var(--color-tertiary-500))',
                    600: 'rgb(var(--color-tertiary-600))',
                    700: 'rgb(var(--color-tertiary-700))',
                    800: 'rgb(var(--color-tertiary-800))',
                    900: 'rgb(var(--color-tertiary-900))',
                    950: 'rgb(var(--color-tertiary-950))'
                },
                warning: {
                    50: 'rgb(var(--color-warning-50))',
                    100: 'rgb(var(--color-warning-100))',
                    200: 'rgb(var(--color-warning-200))',
                    300: 'rgb(var(--color-warning-300))',
                    400: 'rgb(var(--color-warning-400))',
                    500: 'rgb(var(--color-warning-500))',
                    600: 'rgb(var(--color-warning-600))',
                    700: 'rgb(var(--color-warning-700))',
                    800: 'rgb(var(--color-warning-800))',
                    900: 'rgb(var(--color-warning-900))',
                    950: 'rgb(var(--color-warning-950))'
                },
                danger: {
                    50: 'rgb(var(--color-danger-50))',
                    100: 'rgb(var(--color-danger-100))',
                    200: 'rgb(var(--color-danger-200))',
                    300: 'rgb(var(--color-danger-300))',
                    400: 'rgb(var(--color-danger-400))',
                    500: 'rgb(var(--color-danger-500))',
                    600: 'rgb(var(--color-danger-600))',
                    700: 'rgb(var(--color-danger-700))',
                    800: 'rgb(var(--color-danger-800))',
                    900: 'rgb(var(--color-danger-900))',
                    950: 'rgb(var(--color-danger-950))'
                },
                success: {
                    50: 'rgb(var(--color-success-50))',
                    100: 'rgb(var(--color-success-100))',
                    200: 'rgb(var(--color-success-200))',
                    300: 'rgb(var(--color-success-300))',
                    400: 'rgb(var(--color-success-400))',
                    500: 'rgb(var(--color-success-500))',
                    600: 'rgb(var(--color-success-600))',
                    700: 'rgb(var(--color-success-700))',
                    800: 'rgb(var(--color-success-800))',
                    900: 'rgb(var(--color-success-900))',
                    950: 'rgb(var(--color-success-950))'
                },
                info: {
                    50: 'rgb(var(--color-info-50))',
                    100: 'rgb(var(--color-info-100))',
                    200: 'rgb(var(--color-info-200))',
                    300: 'rgb(var(--color-info-300))',
                    400: 'rgb(var(--color-info-400))',
                    500: 'rgb(var(--color-info-500))',
                    600: 'rgb(var(--color-info-600))',
                    700: 'rgb(var(--color-info-700))',
                    800: 'rgb(var(--color-info-800))',
                    900: 'rgb(var(--color-info-900))',
                    950: 'rgb(var(--color-info-950))'
                },
                gray: {
                    100: 'rgb(var(--color-gray-100))',
                    200: 'rgb(var(--color-gray-200))',
                    300: 'rgb(var(--color-gray-300))',
                    400: 'rgb(var(--color-gray-400))',
                    500: 'rgb(var(--color-gray-500))',
                    600: 'rgb(var(--color-gray-600))',
                    700: 'rgb(var(--color-gray-700))',
                    750: 'rgb(var(--color-gray-750))',
                    800: 'rgb(var(--color-gray-800))',
                    900: 'rgb(var(--color-gray-900))',
                    950: 'rgb(var(--color-gray-950))'
                },
                white: 'rgb(var(--color-light))',
                black: 'rgb(var(--color-dark))'
            },
            animation: {
                'shimmer': 'shimmer 2s linear infinite',
            }
        },
    },
    plugins: [
        require('@tailwindcss/forms')
    ],
}

