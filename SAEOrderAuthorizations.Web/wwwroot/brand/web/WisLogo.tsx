import type { CSSProperties } from "react";

export type WisLogoVariant = "horizontal" | "stacked" | "icon";
export type WisLogoTheme = "auto" | "light" | "dark";
export type WisLogoSize = "sm" | "md" | "lg" | number;

export type WisLogoProps = {
  /** Horizontal for a navbar; stacked for a large centered placement. */
  variant?: WisLogoVariant;
  /** Auto inherits the nearest data-theme and the operating-system preference. */
  theme?: WisLogoTheme;
  /** Width in CSS pixels, or a preset. Height always keeps the SVG proportion. */
  size?: WisLogoSize;
  /** Use true when a parent link already supplies an aria-label. */
  decorative?: boolean;
  label?: string;
  className?: string;
  /** Default public path. Set e.g. '/portfolio/brand' for a subpath deployment. */
  assetBasePath?: string;
  style?: CSSProperties;
};

const ASSETS = {
  horizontal: { stem: "wis-logo-horizontal", width: 1200, height: 360 },
  stacked: { stem: "wis-logo-stacked", width: 800, height: 800 },
  icon: { stem: "wis-symbol", width: 800, height: 800 },
} as const;

const SIZES = {
  horizontal: { sm: 180, md: 240, lg: 360 },
  stacked: { sm: 160, md: 240, lg: 360 },
  icon: { sm: 32, md: 48, lg: 80 },
} as const;

/**
 * Import brand-tokens.css once in your app entry/layout.
 * The SVG artwork is outlined and does not depend on fonts loading.
 * This component uses no effects or browser globals and can render on a server.
 */
export function WisLogo({
  variant = "horizontal",
  theme = "auto",
  size = "md",
  decorative = false,
  label = "WIS Dev Code",
  className = "",
  assetBasePath = "/brand",
  style,
}: WisLogoProps) {
  const asset = ASSETS[variant];
  const width = typeof size === "number" ? Math.max(1, size) : SIZES[variant][size];
  const base = assetBasePath.replace(/\/$/, "");
  const source = (surface: "light" | "dark") =>
    `${base}/logos/svg/${asset.stem}-on-${surface}.svg`;
  const imageProps = {
    width: asset.width,
    height: asset.height,
    alt: "",
    "aria-hidden": true as const,
    decoding: "async" as const,
    draggable: false,
  };

  return (
    <span
      className={`wis-logo ${className}`.trim()}
      data-logo-theme={theme}
      data-logo-variant={variant}
      role={decorative ? undefined : "img"}
      aria-label={decorative ? undefined : label}
      aria-hidden={decorative ? true : undefined}
      style={{ width, aspectRatio: `${asset.width} / ${asset.height}`, ...style }}
    >
      {theme === "auto" ? (
        <>
          <img {...imageProps} className="wis-logo__on-light" src={source("light")} />
          <img {...imageProps} className="wis-logo__on-dark" src={source("dark")} />
        </>
      ) : (
        <img {...imageProps} src={source(theme)} />
      )}
    </span>
  );
}

export default WisLogo;
