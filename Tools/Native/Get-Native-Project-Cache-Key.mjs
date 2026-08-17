import { Getˉnativeˉprojectˉcacheˉkey } from './Native-Project-Cache-Key-Core.mjs';

if (process.argv.length < 5) {
    process.stderr.write(
        'Usage: node Tools/Native/Get-Native-Project-Cache-Key.mjs ' +
        '<namespace> <project.wvproj> <producer>...\n'
    );
    process.exit(1);
}

try {
    const key = await Getˉnativeˉprojectˉcacheˉkey(
        process.argv[2],
        process.argv[3],
        process.argv.slice(4)
    );
    process.stdout.write(`${key}\n`);
} catch (error) {
    process.stderr.write(`${error.message}\n`);
    process.exit(1);
}
