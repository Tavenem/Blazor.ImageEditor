import { nodeResolve } from '@rollup/plugin-node-resolve';
import { terser } from 'rollup-plugin-terser';

export default [{
    input: "./ts/editor.js",
    output: {
        file: "./wwwroot/editor.js",
        format: 'es',
        sourcemap: true,
    },
    plugins: [nodeResolve(), terser()],
}];
