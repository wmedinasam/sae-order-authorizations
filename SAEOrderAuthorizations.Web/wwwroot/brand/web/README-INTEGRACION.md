# WIS Dev Code · Integración de marca en tu landing

Este kit reúne los recursos de identidad y un punto de partida para integrarlos en tu sitio. `web/index.html` es el catálogo local; `integration-example.html` muestra una cabecera, tipografía y componentes con las rutas de producción. No reemplaza el contenido de tu landing.

## 1. Qué copiar al proyecto

Crea `public/brand/` y conserva estas subcarpetas del paquete:

| Carpeta | Contenido | Ruta pública resultante |
|---|---|---|
| `logos/svg/` | Logotipos y símbolos vectoriales | `/brand/logos/svg/…` |
| `fonts/` | Fuentes locales y sus licencias | `/brand/fonts/…` |
| `web-icons/` | Favicon e iconos de app | `/brand/web-icons/…` |
| `web/` | Tokens, inicialización de tema y manifest | `/brand/web/…` |

En proyectos Vite, los archivos dentro de `public/` se sirven desde la raíz; no escribas `/public/` en sus URLs. [Documentación de Vite](https://vite.dev/guide/assets.html#the-public-directory).

Puedes dejar el catálogo, el ejemplo, la documentación y el archivo TSX fuera de los archivos públicos. Copia `WisLogo.tsx` a tu carpeta de componentes React; los únicos archivos de `web/` que necesita el sitio son `brand-tokens.css` y, si utilizas esas funciones, `theme-init.js` y `manifest.webmanifest`.

## 2. Activar estilos y temas

Añade en el `<head>` de tu HTML:

```html
<meta name="color-scheme" content="light dark">
<meta name="theme-color" content="#F6F8FB" data-wis-theme-color>
<script src="/brand/web/theme-init.js"></script>
<link rel="stylesheet" href="/brand/web/brand-tokens.css">
```

Y en la página o la sección donde quieras aplicar el sistema:

```html
<html lang="es" data-theme="auto">
  <!-- head -->
  <body class="wis-brand">
    <!-- Tu contenido -->
  </body>
</html>
```

`data-theme="auto"` sigue la preferencia del sistema. `light` y `dark` fuerzan una apariencia; también puedes aplicarlos a una sección para crear un bloque de fondo diferente. Una selección clara explícita se conserva aunque el sistema esté en modo oscuro. El CSS funciona sin JavaScript; el script añade persistencia de la elección y actualiza el color de la barra del navegador cuando existe la etiqueta correspondiente. [MDN: prefers-color-scheme](https://developer.mozilla.org/en-US/docs/Web/CSS/@media/prefers-color-scheme).

El script carga antes de la hoja de estilos para recuperar la preferencia al inicio. Usa únicamente la clave local `wis-theme`. Deja `data-theme="auto"` en el HTML para restaurar la elección guardada del visitante; un valor `light` o `dark` escrito en el HTML tiene prioridad sobre esa elección al cargar. Si el navegador bloquea el almacenamiento, sigue funcionando durante la sesión.

Para cambiar el tema desde un control existente:

```js
window.WISBrandTheme.set("dark"); // "light", "dark" o "auto"
```

Si tu app ya tiene un sistema de temas, conserva ese sistema y actualiza `data-theme` en lugar de añadir otro selector. El componente de logo no requiere `theme-init.js`. En un framework con hidratación, integra la preferencia con su gestor de temas para que el valor renderizado por el servidor y el cliente coincidan.

## 3. Logo en HTML

Para una cabecera siempre clara basta con un SVG:

```html
<a href="/" aria-label="WIS Dev Code — inicio">
  <img
    src="/brand/logos/svg/wis-logo-horizontal-on-light.svg"
    width="240"
    height="72"
    alt=""
  >
</a>
```

Para adaptarlo automáticamente al tema, usa las dos variantes. Solo una se muestra y el enlace aporta un único nombre accesible:

```html
<a href="/" aria-label="WIS Dev Code — inicio">
  <span class="wis-logo" aria-hidden="true"
        style="width: 240px; aspect-ratio: 10 / 3;">
    <img class="wis-logo__on-light"
         src="/brand/logos/svg/wis-logo-horizontal-on-light.svg"
         width="1200" height="360" alt="">
    <img class="wis-logo__on-dark"
         src="/brand/logos/svg/wis-logo-horizontal-on-dark.svg"
         width="1200" height="360" alt="">
  </span>
</a>
```

No uses filtros CSS para invertir el logo a color: selecciona la variante preparada para el fondo. Los SVG conservan el texto como trazados, por lo que el logo no cambia si una fuente tarda en cargar.

## 4. Logo en React + TypeScript

Copia `WisLogo.tsx` a `src/components/` e impórtalo:

```tsx
import { WisLogo } from "./components/WisLogo";

export function BrandHeader() {
  return (
    <header className="wis-brand">
      <a href="/" aria-label="WIS Dev Code — inicio">
        <WisLogo variant="horizontal" theme="auto" size="md" decorative />
      </a>
    </header>
  );
}
```

El CSS debe estar cargado una vez en el documento. El componente no añade paquetes ni consulta el navegador durante el render. Utiliza el transformador JSX automático habitual de un proyecto React + TypeScript.

| Propiedad | Opciones | Valor inicial |
|---|---|---|
| `variant` | `horizontal`, `stacked`, `icon` | `horizontal` |
| `theme` | `auto`, `light`, `dark` | `auto` |
| `size` | `sm`, `md`, `lg` o ancho numérico en px | `md` |
| `decorative` | `true` cuando el contenedor ya tiene nombre accesible | `false` |
| `label` | Nombre accesible cuando no es decorativo | `WIS Dev Code` |
| `assetBasePath` | Ubicación pública de la carpeta de marca | `/brand` |
| `className`, `style` | Personalización de posición y espacio exterior | — |

```tsx
<WisLogo variant="icon" size={48} />
<WisLogo variant="stacked" theme="dark" size={240} />
<WisLogo theme="light" size={200} assetBasePath="/portfolio/brand" />
```

El tema se refiere al **fondo donde colocas el logo**. Elige `dark` sobre un bloque oscuro aunque el resto de la página esté en claro. Con `auto`, se hereda el tema más próximo. Para un logo informativo sin enlace, conserva su `label`; para un logo junto a un texto equivalente o dentro de un enlace con `aria-label`, usa `decorative` y evita lecturas duplicadas.

| Variante | Ancho mínimo recomendado | Presets sm / md / lg |
|---|---:|---:|
| Horizontal | 160 px | 180 / 240 / 360 px |
| Vertical | 140 px | 160 / 240 / 360 px |
| Símbolo | 24 px | 32 / 48 / 80 px |

Para el favicon de 16 px utiliza el archivo dedicado; no reduzcas el logo con el nombre completo a ese tamaño. Mantén la proporción y el espacio que incorpora cada maestro.

## 5. Tipografía y componentes

| Uso | Familia | Peso | Tamaño / interlineado de referencia |
|---|---|---|---|
| H1 móvil → escritorio | Montserrat | 700 | 40/44 → 64/68 px |
| H2 móvil → escritorio | Montserrat | 700 | 32/40 → 40/48 px |
| H3 | Montserrat | 600 | 24/32 px |
| Texto | Inter | 400 | 16/26 px |
| Botones y etiquetas | Inter | 600 | 16 px |
| Texto secundario | Inter | 400 | 14/22 px |
| Código opcional | JetBrains Mono | 400 | 0.875 em del contenedor |

Los tamaños CSS se expresan en `rem`; la equivalencia en píxeles supone una raíz de 16 px. El cambio de escala se aplica a partir de 48 rem. `font-display: swap` permite mostrar una fuente alternativa mientras carga la fuente local. Las tres declaraciones de fuentes son de estilo normal. [MDN: font-display](https://developer.mozilla.org/en-US/docs/Web/CSS/@font-face/font-display).

```html
<h1 class="wis-h1">Tu título</h1>
<p class="wis-body wis-muted wis-measure">Tu descripción.</p>
<a class="wis-button" href="#contacto">Hablemos del proyecto</a>
<a class="wis-button wis-button--secondary" href="#trabajo">Ver trabajo</a>
```

Las clases son opcionales. Si ya utilizas Tailwind u otra hoja de estilos, puedes aplicar directamente `var(--wis-font-body)`, `var(--wis-action)` y las demás variables. `brand-tokens.json` ofrece un mapa descriptivo independiente del framework; no es una configuración que Tailwind importe automáticamente.

Las rutas de fuentes dentro de `brand-tokens.css` son relativas a esa hoja: desde `/brand/web/` apuntan a `/brand/fonts/`. Así el catálogo local también puede resolverlas sin cambiar el paquete. Conserva la distribución de carpetas y las licencias que acompañan las fuentes.

## 6. Color y legibilidad

Los colores de identidad Azure `#079EF3` y Coral `#FF654F` conservan su función visual. Para botones, enlaces y textos, utiliza las variables semánticas del tema.

| Combinación | Contraste calculado | Uso previsto |
|---|---:|---|
| Blanco sobre azul funcional `#0068A8` | 5.92:1 | Botón principal en claro |
| Navy sobre Azure | 5.80:1 | Botón principal en oscuro |
| Navy sobre Coral | 5.82:1 | Acento con texto legible |
| Texto `#172F42` sobre blanco | 13.80:1 | Texto principal claro |
| Muted `#526579` sobre `#F6F8FB` | 5.65:1 | Texto secundario claro |
| Blanco sobre Navy | 16.92:1 | Texto principal oscuro |
| Blanco sobre Azure | 2.92:1 | Evitar en texto funcional |
| Blanco sobre Coral | 2.91:1 | Evitar en texto funcional |

Ratios calculados a partir de los valores sRGB, sin redondearlos para decidir si superan el umbral. WCAG 2.2 pide al menos 4.5:1 para texto normal y 3:1 para texto grande, con excepciones como los logotipos. Esto verifica las combinaciones indicadas; no certifica la accesibilidad de tu sitio completo. [W3C: contraste mínimo](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html).

`--wis-border` está previsto para separadores decorativos. Los controles usan `--wis-border-control`, con mayor contraste. El kit incluye foco visible, enlaces subrayados y reducción de sus transiciones si el usuario solicita menos movimiento. Mantén esas señales al adaptar componentes. [MDN: prefers-reduced-motion](https://developer.mozilla.org/en-US/docs/Web/CSS/@media/prefers-reduced-motion).

## 7. Favicon e iconos

`favicon-head.html` contiene el bloque listo para añadir al `<head>`, con favicon SVG/ICO, icono de Apple y manifest. Las rutas corresponden a `/brand/web-icons/`.

El manifest utiliza `display: "browser"` porque este paquete acompaña una landing. Si desarrollas una aplicación instalable, decide su comportamiento de instalación en el proyecto y adapta `display`, `start_url`, `scope` e `id`. El archivo no agrega funciones sin conexión ni un service worker. El icono `maskable` está declarado por separado para permitir su recorte. [MDN: iconos del manifest](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps/Manifest/Reference/icons).

Para un sitio publicado en una subruta, por ejemplo `/portfolio/`, actualiza las rutas del bloque de favicon, el manifest y los ejemplos a `/portfolio/brand/`. En React, ajusta `assetBasePath`; las fuentes seguirán resolviéndose de manera relativa.

## 8. Revisión antes de integrar

Abre `web/index.html` desde la carpeta descomprimida para consultar el catálogo. `integration-example.html` utiliza rutas absolutas para producción y debe servirse desde un proyecto que tenga la carpeta `/brand/`.

Comprueba que los SVG, fuentes e iconos carguen; que el logo claro y oscuro coincida con su superficie; que pueda recorrerse la página con Tab; y que el texto siga siendo cómodo en móvil y al ampliar el navegador. Los enlaces y textos de ejemplo deben sustituirse por los de tu proyecto. Esta entrega prepara los recursos; la integración en tu landing depende de aplicarlos a su código.
