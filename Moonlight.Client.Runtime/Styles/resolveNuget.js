const fs = require('fs');
const path = require('path');
const os = require('os');
const xml2js = require('xml2js');

// Helpers
function getPackageRefs(csprojPath) {
    const xml = fs.readFileSync(csprojPath, 'utf8');
    const parser = new xml2js.Parser();

    return new Promise((resolve, reject) => {
        parser.parseString(xml, (err, result) => {
            if (err) return reject(err);

            const itemGroups = result.Project.ItemGroup || [];
            const refs = [];

            for (const group of itemGroups) {
                const packages = group.PackageReference || [];
                for (const pkg of packages) {
                    const name = pkg.$.Include;
                    const version = pkg.$.Version || (pkg.Version && pkg.Version[0]);
                    if (name && version) {
                        refs.push({ name: name.toLowerCase(), version });
                    }
                }
            }
            resolve(refs);
        });
    });
}

async function main() {
    const csprojPath = process.argv[2];
    if (!csprojPath || !fs.existsSync(csprojPath)) {
        console.error('Usage: Missing csproj path');
        process.exit(1);
    }

    const nugetPath = path.join(os.homedir(), '.nuget', 'packages');
    const moonlightDir = path.join(__dirname, 'node_modules', 'moonlight');
    fs.mkdirSync(moonlightDir, { recursive: true });

    const refs = await getPackageRefs(csprojPath);

    var outputCss = "";
    var preOutputCss = "";

    for (const { name, version } of refs) {
        const packagePath = path.join(nugetPath, name, version);
        const exportsFile = path.join(packagePath, 'styles', 'exports.css');
        const preTailwindFile = path.join(packagePath, 'styles', 'preTailwind.css');
        const sourceFolder = path.join(packagePath, 'src');

        const rel = (p) => p.replace(/\\/g, '/');

        if (fs.existsSync(exportsFile)) {
            outputCss += `@import "${rel(exportsFile)}";\n`;
        }

        if (fs.existsSync(preTailwindFile)) {
            preOutputCss += `@import "${rel(preTailwindFile)}";\n`;
        }

        if (fs.existsSync(sourceFolder)) {
            outputCss += `@source "${rel(path.join(sourceFolder, "**", "*.razor"))}";\n`;
            outputCss += `@source "${rel(path.join(sourceFolder, "**", "*.cs"))}";\n`;
            outputCss += `@source "${rel(path.join(sourceFolder, "**", "*.html"))}";\n`;
        }
    }

    fs.writeFileSync(path.join(moonlightDir, 'nuget.css'), outputCss);
    fs.writeFileSync(path.join(moonlightDir, 'preTailwind.nuget.css'), preOutputCss);
    console.log(`Generated nuget.css in ${moonlightDir}`);
}

main().catch(err => {
    console.error(err);
    process.exit(1);
});