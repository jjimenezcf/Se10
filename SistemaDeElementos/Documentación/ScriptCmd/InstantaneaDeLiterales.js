// Instantánea de los valores de los literales del front (wwwroot/ts/_Literales).
//
// Ejecuta los .js de los literales, en el orden que se le pasen (el de la lista de scripts de ListaDeScripts.cs), dentro de un
// contexto de node aislado y escribe una línea "ruta = valor" por cada valor de cada objeto global que declaran. Sirve para:
//   - detectar un orden de carga incorrecto: un literal que lee otro que aún no se ha cargado da ReferenceError aquí,
//     igual que en el navegador (tsc no lo detecta);
//   - comparar los valores antes y después de un cambio en los literales (fc / git diff --no-index de dos instantáneas).
//
// Uso (node no está en el PATH, se usa el de Visual Studio):
//   & "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Microsoft\VisualStudio\NodeJs\node.exe" `
//       Documentación\ScriptCmd\InstantaneaDeLiterales.js wwwroot\js\_Literales\literales_Basicos.js ... > instantanea.txt
//
// Los literales no usan el DOM, así que no hace falta navegador.

const fs = require('fs');
const vm = require('vm');

const ficheros = process.argv.slice(2);
if (ficheros.length === 0) {
    console.error('Indica los .js de los literales en orden de carga');
    process.exit(1);
}

// nombres globales que declara cada fichero: const/let/var de primer nivel (los enum y namespace de TS compilan a "var X;")
const nombres = [];
for (const fichero of ficheros) {
    const codigo = fs.readFileSync(fichero, 'utf8');
    for (const m of codigo.matchAll(/^(?:const|let|var)\s+([A-Za-z_$][\w$]*)/gm))
        if (!nombres.includes(m[1])) nombres.push(m[1]);
}

const contexto = vm.createContext({});
for (const fichero of ficheros) {
    try {
        vm.runInContext(fs.readFileSync(fichero, 'utf8'), contexto, { filename: fichero });
    }
    catch (e) {
        console.error(`ERROR al cargar ${fichero}: ${e.message}`);
        process.exit(2);
    }
}

const valores = vm.runInContext(`({ ${nombres.map(n => `${n}: typeof ${n} === 'undefined' ? undefined : ${n}`).join(', ')} })`, contexto);

const lineas = [];
function recorrer(ruta, valor) {
    if (valor !== null && typeof valor === 'object') {
        const claves = Object.keys(valor);
        if (claves.length === 0) lineas.push(`${ruta} = {}`);
        for (const clave of claves) recorrer(Array.isArray(valor) ? `${ruta}[${clave}]` : `${ruta}.${clave}`, valor[clave]);
    }
    else
        lineas.push(`${ruta} = ${JSON.stringify(valor)}`);
}
for (const nombre of nombres) recorrer(nombre, valores[nombre]);

lineas.sort();
process.stdout.write(lineas.join('\n') + '\n');
