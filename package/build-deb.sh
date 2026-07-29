#!/bin/bash
# Builds a self-contained linux-x64 .deb package for QuickCopyTags.
#
# Usage:
#   ./package/build-deb.sh [version]
#
# Output:
#   dist/quickcopytags_<version>_amd64.deb
set -euo pipefail

VERSION="${1:-1.0.0}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_DIR="$REPO_ROOT/QuickCopyTags"
BUILD_DIR="$(mktemp -d)"
DIST_DIR="$REPO_ROOT/dist"
PKG_NAME="quickcopytags_${VERSION}_amd64"
PKG_DIR="$BUILD_DIR/$PKG_NAME"

cleanup() { rm -rf "$BUILD_DIR"; }
trap cleanup EXIT

echo "==> Publishing self-contained linux-x64 build"
dotnet publish "$PROJECT_DIR" -c Release -r linux-x64 --self-contained \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o "$BUILD_DIR/publish"

echo "==> Assembling package tree"
mkdir -p "$PKG_DIR/DEBIAN"
mkdir -p "$PKG_DIR/opt/quickcopytags"
mkdir -p "$PKG_DIR/usr/bin"
mkdir -p "$PKG_DIR/usr/share/applications"
mkdir -p "$PKG_DIR/usr/share/icons/hicolor/256x256/apps"

cp "$BUILD_DIR/publish/QuickCopyTags" "$PKG_DIR/opt/quickcopytags/QuickCopyTags"
chmod 755 "$PKG_DIR/opt/quickcopytags/QuickCopyTags"

cp "$PROJECT_DIR/Assets/icon.png" "$PKG_DIR/usr/share/icons/hicolor/256x256/apps/quickcopytags.png"

ln -s /opt/quickcopytags/QuickCopyTags "$PKG_DIR/usr/bin/quickcopytags"

cat > "$PKG_DIR/usr/share/applications/quickcopytags.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=QuickCopyTags
Comment=Quick-copy tag launcher
Exec=/opt/quickcopytags/QuickCopyTags
Icon=quickcopytags
Terminal=false
Categories=Utility;
StartupWMClass=QuickCopyTags
EOF
chmod 644 "$PKG_DIR/usr/share/applications/quickcopytags.desktop"

INSTALLED_SIZE_KB="$(du -sk --exclude=DEBIAN "$PKG_DIR" | cut -f1)"

cat > "$PKG_DIR/DEBIAN/control" <<EOF
Package: quickcopytags
Version: $VERSION
Section: utils
Priority: optional
Architecture: amd64
Installed-Size: $INSTALLED_SIZE_KB
Maintainer: QuickCopyTags <boneyp003@gmail.com>
Description: Quick-copy tag launcher
 A small desktop utility for storing predefined text snippets as tags
 and copying them to the clipboard with a single click. Useful for job
 applications and other tasks involving repetitive copy-pasting.
EOF

cat > "$PKG_DIR/DEBIAN/postinst" <<'EOF'
#!/bin/sh
set -e
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database -q /usr/share/applications || true
fi
if command -v gtk-update-icon-cache >/dev/null 2>&1; then
    gtk-update-icon-cache -q -t /usr/share/icons/hicolor || true
fi
exit 0
EOF

cat > "$PKG_DIR/DEBIAN/postrm" <<'EOF'
#!/bin/sh
set -e
if command -v update-desktop-database >/dev/null 2>&1; then
    update-desktop-database -q /usr/share/applications || true
fi
if command -v gtk-update-icon-cache >/dev/null 2>&1; then
    gtk-update-icon-cache -q -t /usr/share/icons/hicolor || true
fi
exit 0
EOF

chmod 755 "$PKG_DIR/DEBIAN/postinst" "$PKG_DIR/DEBIAN/postrm"

echo "==> Building .deb"
mkdir -p "$DIST_DIR"
fakeroot dpkg-deb --build --root-owner-group "$PKG_DIR" "$DIST_DIR/${PKG_NAME}.deb"

echo "==> Done: $DIST_DIR/${PKG_NAME}.deb"
