module.exports = (opts = {}) => ({
    postcssPlugin: 'postcss-wrap-scope',
    Once(root) {
        const postcss = require('postcss');
        const scopeRule = new postcss.AtRule({
            name: 'scope',
            params: `(${opts.root}) to (${opts.exclude})`,
        });
        scopeRule.append(root.nodes);
        root.removeAll();
        root.append(scopeRule);
    },
});

module.exports.postcss = true;