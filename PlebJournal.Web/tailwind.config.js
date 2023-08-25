/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
      "./Views/**/*.{html,cshtml}",
      "./node_modules/flowbite/**/*.js"
  ],
  theme: {
    extend: {},
  },
  plugins: [
    require('flowbite/plugin')
  ],
}

