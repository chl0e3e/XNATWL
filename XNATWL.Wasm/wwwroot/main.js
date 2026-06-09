import { dotnet } from './_framework/dotnet.js'

const canvas = document.getElementById('canvas');

// Log WebGL context creation so we can confirm FNA3D gets a WebGL2 context.
const origGetContext = HTMLCanvasElement.prototype.getContext;
HTMLCanvasElement.prototype.getContext = function (type, attrs) {
    const ctx = origGetContext.call(this, type, attrs);
    console.log('[gl] getContext(', type, ') ->', ctx ? 'OK' : 'NULL');
    return ctx;
};

const api = await dotnet
    .withModuleConfig({
        canvas: canvas,
        print: (m) => console.log('[native]', m),
        printErr: (m) => {
            // Harmless: SDL sets the swap interval before FNA installs the main loop.
            if (typeof m === 'string' && m.includes('emscripten_set_main_loop_timing')) return;
            console.error('[native]', m);
        },
    })
    .create();

if (api.Module) {
    api.Module.canvas = canvas;
}

// XNATWL loads assets via System.IO.File (the browser has no real FS), so fetch the served
// files and write them into the Emscripten VFS first: the TWL theme at /Theme, and the
// keyboard-layout files (XNAInput, AppDomain.BaseDirectory => "/") at /KeyboardLayouts.
await loadIntoVfs(api.Module, "Theme", [
    "simple.xml", "simple.png", "simple_demo.xml", "guiTheme.xml",
    "gui.xml", "gui.png", "cursors.xml", "cursors.png",
    "font.fnt", "font_00.png", "Eforen.xml", "Eforen.png", "EforenArrows.png",
    "chat.xml", "chat.png", "chaos_sphere_blue_800x600.png",
    "simpleGameMenu.xml", "license.html", "TWL Logo.png"
]);
await loadIntoVfs(api.Module, "KeyboardLayouts", ["1033.xml", "2057.xml"]);

// FNA drives the loop via emscripten_set_main_loop(simulate_infinite_loop=1), which throws
// the Emscripten 'unwind' marker to keep the runtime alive while requestAnimationFrame ticks.
// That's expected — swallow it so it doesn't surface as an uncaught error.
try {
    await api.runMain();
} catch (e) {
    const msg = (e && (e.message ?? e.toString?.())) ?? String(e);
    if (e === 'unwind' || msg === 'unwind' || msg.includes('unwind')) {
        console.log('[main] FNA main loop running (requestAnimationFrame)');
    } else {
        throw e;
    }
}

async function loadIntoVfs(Module, dir, files) {
    const FS = Module.FS;
    try { FS.mkdir('/' + dir); } catch (e) { /* exists */ }
    let ok = 0;
    for (const f of files) {
        try {
            const resp = await fetch(dir + '/' + encodeURIComponent(f));
            if (!resp.ok) { console.warn('[vfs] missing', dir + '/' + f, resp.status); continue; }
            const buf = new Uint8Array(await resp.arrayBuffer());
            FS.writeFile('/' + dir + '/' + f, buf);
            ok++;
        } catch (e) {
            console.warn('[vfs] failed', dir + '/' + f, e.message);
        }
    }
    console.log('[vfs] wrote', ok, '/', files.length, 'files to /' + dir);
}
