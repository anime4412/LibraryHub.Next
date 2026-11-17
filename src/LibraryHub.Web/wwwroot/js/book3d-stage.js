import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.170.0/build/three.module.js';

const FONT_STACK_SERIF = '"Newsreader", Georgia, "Times New Roman", serif';
const FONT_STACK_SANS = '"Be Vietnam Pro", system-ui, -apple-system, sans-serif';

function rgbToCss(r, g, b) {
  const clamp = (v) => Math.max(0, Math.min(255, Math.round(v)));
  return `rgb(${clamp(r)}, ${clamp(g)}, ${clamp(b)})`;
}

function extractPaletteFromImage(img) {
  try {
    const sample = document.createElement('canvas');
    sample.width = 32;
    sample.height = 32;
    const sctx = sample.getContext('2d');
    sctx.drawImage(img, 0, 0, 32, 32);
    const data = sctx.getImageData(0, 0, 32, 32).data;

    let rSum = 0;
    let gSum = 0;
    let bSum = 0;
    let count = 0;

    // Sample outer perimeter strips (background color of book cover)
    for (let y = 0; y < 32; y++) {
      for (let x = 0; x < 32; x++) {
        if (x > 3 && x < 28 && y > 3 && y < 28) continue;
        const idx = (y * 32 + x) * 4;
        if (data[idx + 3] < 128) continue;
        rSum += data[idx];
        gSum += data[idx + 1];
        bSum += data[idx + 2];
        count++;
      }
    }

    if (count === 0) return null;
    const r = rSum / count;
    const g = gSum / count;
    const b = bSum / count;
    const luminance = 0.299 * r + 0.587 * g + 0.114 * b;
    const isLight = luminance > 140;

    return {
      isLight,
      coverColor: rgbToCss(r, g, b),
      coverMidColor: rgbToCss(r * (isLight ? 0.98 : 1.08), g * (isLight ? 0.98 : 1.08), b * (isLight ? 0.98 : 1.08)),
      coverDeepColor: rgbToCss(r * 0.88, g * 0.88, b * 0.88),
      edgeColor: rgbToCss(r * 0.78, g * 0.78, b * 0.78),
      accentColor: isLight ? '#99752c' : '#d4af37',
      textColor: isLight ? '#232825' : '#f5e6b3'
    };
  } catch {
    return null;
  }
}

function buildCoverCanvas({
  title = 'LIBRARYHUB',
  subtitle = 'THE ARCHITECTURAL EDITION',
  author = 'SCHOLASTIC ARCHIVE',
  badge = 'VOL. I',
  coverColor = '#101c19',
  coverMidColor = '#172924',
  coverDeepColor = '#09110f',
  accentColor = '#d4af37',
  textColor = '#f5e6b3',
  isLight = false,
  isBack = false
}) {
  const canvas = document.createElement('canvas');
  canvas.width = 1024;
  canvas.height = 1448;
  const ctx = canvas.getContext('2d');

  // Textured cover base gradient matching the book's palette
  const grad = ctx.createLinearGradient(0, 0, canvas.width, canvas.height);
  grad.addColorStop(0, coverColor);
  grad.addColorStop(0.55, coverMidColor);
  grad.addColorStop(1, coverDeepColor);
  ctx.fillStyle = grad;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  // Fine paper/leather grain noise
  ctx.fillStyle = isLight ? 'rgba(0,0,0,0.018)' : 'rgba(255,255,255,0.025)';
  for (let i = 0; i < 4800; i++) {
    const x = (Math.sin(i * 12.9898) * 43758.5453) % 1;
    const y = (Math.sin(i * 78.233) * 43758.5453) % 1;
    ctx.fillRect(Math.abs(x) * canvas.width, Math.abs(y) * canvas.height, 2, 2);
  }

  // Hinge crease shadow (left side for front cover, right side for back cover next to spine)
  const creaseAlpha = isLight ? 0.16 : 0.55;
  if (isBack) {
    const crease = ctx.createLinearGradient(canvas.width, 0, canvas.width - 72, 0);
    crease.addColorStop(0, `rgba(0,0,0,${creaseAlpha})`);
    crease.addColorStop(0.35, 'rgba(255,255,255,0.06)');
    crease.addColorStop(1, 'rgba(0,0,0,0)');
    ctx.fillStyle = crease;
    ctx.fillRect(canvas.width - 72, 0, 72, canvas.height);
  } else {
    const crease = ctx.createLinearGradient(0, 0, 72, 0);
    crease.addColorStop(0, 'rgba(0,0,0,0.55)');
    crease.addColorStop(0.35, 'rgba(255,255,255,0.06)');
    crease.addColorStop(1, 'rgba(0,0,0,0)');
    ctx.fillStyle = crease;
    ctx.fillRect(0, 0, 72, canvas.height);
  }

  // Outer embossed gold foil double border
  const leftInset = isBack ? 56 : 78;
  const innerLeft = isBack ? 72 : 94;
  const rightEdge = isBack ? canvas.width - 94 : canvas.width - 72;

  ctx.strokeStyle = accentColor;
  ctx.lineWidth = 5;
  ctx.strokeRect(leftInset, 66, canvas.width - 134, canvas.height - 132);
  ctx.lineWidth = 1.5;
  ctx.globalAlpha = 0.72;
  ctx.strokeRect(innerLeft, 82, canvas.width - 166, canvas.height - 164);
  ctx.globalAlpha = 1;

  // Corner ornaments
  const corners = [
    [innerLeft, 82, 1, 1],
    [rightEdge, 82, -1, 1],
    [innerLeft, canvas.height - 82, 1, -1],
    [rightEdge, canvas.height - 82, -1, -1]
  ];
  ctx.strokeStyle = accentColor;
  ctx.lineWidth = 2.5;
  for (const [cx, cy, sx, sy] of corners) {
    ctx.beginPath();
    ctx.moveTo(cx, cy + sy * 28);
    ctx.lineTo(cx + sx * 28, cy + sy * 28);
    ctx.lineTo(cx + sx * 28, cy);
    ctx.stroke();
  }

  const centerX = isBack ? canvas.width / 2 - 12 : canvas.width / 2 + 12;

  if (isBack) {
    // Minimalist crest & colophon matching front cover palette
    const monogram = (String(title || 'L').trim()[0] || 'L').toUpperCase();
    ctx.strokeStyle = accentColor;
    ctx.lineWidth = 2.5;
    ctx.beginPath();
    ctx.arc(centerX, canvas.height / 2 - 48, 72, 0, Math.PI * 2);
    ctx.stroke();
    ctx.fillStyle = accentColor;
    ctx.font = `600 46px ${FONT_STACK_SERIF}`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(monogram, centerX, canvas.height / 2 - 46);

    ctx.fillStyle = textColor;
    ctx.textBaseline = 'alphabetic';
    ctx.font = `600 32px ${FONT_STACK_SERIF}`;
    ctx.fillText(String(title || 'LIBRARYHUB').slice(0, 32), centerX, canvas.height / 2 + 86);

    if (author) {
      ctx.font = `500 22px ${FONT_STACK_SANS}`;
      ctx.fillStyle = accentColor;
      ctx.fillText(String(author).slice(0, 34), centerX, canvas.height / 2 + 130);
    }

    ctx.font = `500 20px ${FONT_STACK_SANS}`;
    ctx.fillStyle = accentColor;
    ctx.globalAlpha = 0.8;
    ctx.fillText('LIBRARYHUB · SCHOLASTIC EDITION', centerX, canvas.height - 165);
    ctx.globalAlpha = 1;

    const backTex = new THREE.CanvasTexture(canvas);
    backTex.colorSpace = THREE.SRGBColorSpace;
    backTex.anisotropy = 4;
    return backTex;
  }

  // Top badge
  ctx.fillStyle = accentColor;
  ctx.textAlign = 'center';
  ctx.font = `600 24px ${FONT_STACK_SANS}`;
  ctx.fillText(badge.toUpperCase(), centerX, 175);

  // Subtle divider line
  ctx.beginPath();
  ctx.moveTo(centerX - 80, 212);
  ctx.lineTo(centerX + 80, 212);
  ctx.lineWidth = 1.8;
  ctx.strokeStyle = accentColor;
  ctx.stroke();

  // Architectural Emblem in Center
  const emblemY = 510;
  ctx.save();
  ctx.translate(centerX, emblemY);
  ctx.strokeStyle = accentColor;
  ctx.lineWidth = 3;
  ctx.beginPath();
  ctx.arc(0, 0, 146, 0, Math.PI * 2);
  ctx.stroke();

  ctx.lineWidth = 1.4;
  ctx.globalAlpha = 0.65;
  ctx.beginPath();
  ctx.arc(0, 0, 122, 0, Math.PI * 2);
  ctx.stroke();

  // Rotated diamond inside circle
  ctx.beginPath();
  ctx.moveTo(0, -146);
  ctx.lineTo(146, 0);
  ctx.lineTo(0, 146);
  ctx.lineTo(-146, 0);
  ctx.closePath();
  ctx.stroke();
  ctx.globalAlpha = 1;

  // Monogram "L"
  ctx.fillStyle = accentColor;
  ctx.font = `700 128px ${FONT_STACK_SERIF}`;
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.fillText('L', 0, 8);
  ctx.restore();

  // Main Title (wrapped)
  ctx.fillStyle = '#f5e6b3';
  ctx.textAlign = 'center';
  ctx.textBaseline = 'alphabetic';
  const words = String(title || 'LIBRARYHUB').split(/\s+/);
  const lines = [];
  let currentLine = '';
  const maxWidth = canvas.width - 250;
  const titleSize = words.length > 5 ? 54 : 68;
  ctx.font = `700 ${titleSize}px ${FONT_STACK_SERIF}`;

  for (const w of words) {
    const test = currentLine ? `${currentLine} ${w}` : w;
    if (ctx.measureText(test).width > maxWidth && currentLine) {
      lines.push(currentLine);
      currentLine = w;
    } else {
      currentLine = test;
    }
  }
  if (currentLine) lines.push(currentLine);

  const visibleLines = lines.slice(0, 3);
  let startY = 820;
  for (const line of visibleLines) {
    ctx.fillText(line, centerX, startY);
    startY += titleSize * 1.18;
  }

  // Subtitle
  ctx.fillStyle = 'rgba(212, 175, 55, 0.85)';
  ctx.font = `500 24px ${FONT_STACK_SANS}`;
  const cleanSub = String(subtitle).slice(0, 42);
  ctx.fillText(cleanSub.toUpperCase(), centerX, startY + 26);

  // Bottom Author / Archive plaque
  ctx.strokeStyle = 'rgba(212, 175, 55, 0.45)';
  ctx.lineWidth = 1.5;
  ctx.beginPath();
  ctx.moveTo(centerX - 110, canvas.height - 235);
  ctx.lineTo(centerX + 110, canvas.height - 235);
  ctx.stroke();

  ctx.fillStyle = accentColor;
  ctx.font = `600 28px ${FONT_STACK_SERIF}`;
  ctx.fillText(String(author).slice(0, 34), centerX, canvas.height - 178);

  const tex = new THREE.CanvasTexture(canvas);
  tex.colorSpace = THREE.SRGBColorSpace;
  tex.anisotropy = 4;
  return tex;
}

function buildSpineCanvas({
  title = 'LIBRARYHUB',
  accentColor = '#d4af37',
  coverColor = '#0c1614',
  coverDeepColor = '#070d0c',
  textColor = '#f5e6b3'
}) {
  const canvas = document.createElement('canvas');
  canvas.width = 256;
  canvas.height = 1448;
  const ctx = canvas.getContext('2d');

  const grad = ctx.createLinearGradient(0, 0, canvas.width, 0);
  grad.addColorStop(0, coverDeepColor);
  grad.addColorStop(0.5, coverColor);
  grad.addColorStop(1, coverDeepColor);
  ctx.fillStyle = grad;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  // Raised gold/accent spine hubs
  const bands = [150, 195, 1250, 1295];
  ctx.fillStyle = accentColor;
  for (const y of bands) {
    ctx.fillRect(28, y, canvas.width - 56, 7);
  }

  // Vertical title along spine
  ctx.save();
  ctx.translate(canvas.width / 2, canvas.height / 2);
  ctx.rotate(-Math.PI / 2);
  ctx.fillStyle = textColor;
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.font = `700 44px ${FONT_STACK_SERIF}`;
  ctx.fillText(String(title).slice(0, 24).toUpperCase(), 0, 0);
  ctx.restore();

  const tex = new THREE.CanvasTexture(canvas);
  tex.colorSpace = THREE.SRGBColorSpace;
  return tex;
}

function buildPageEdgeCanvas(vertical = true) {
  const canvas = document.createElement('canvas');
  canvas.width = vertical ? 256 : 512;
  canvas.height = vertical ? 1024 : 256;
  const ctx = canvas.getContext('2d');

  ctx.fillStyle = '#efe7d2';
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  const count = 90;
  for (let i = 0; i < count; i++) {
    ctx.fillStyle = i % 2 === 0 ? 'rgba(110, 92, 65, 0.14)' : 'rgba(255, 255, 255, 0.35)';
    if (vertical) {
      const x = (i / count) * canvas.width;
      ctx.fillRect(x, 0, 1.5, canvas.height);
    } else {
      const y = (i / count) * canvas.height;
      ctx.fillRect(0, y, canvas.width, 1.5);
    }
  }

  // Inner edge shadow
  const vignette = ctx.createLinearGradient(
    0, 0,
    vertical ? canvas.width : 0,
    vertical ? 0 : canvas.height
  );
  vignette.addColorStop(0, 'rgba(65, 52, 34, 0.22)');
  vignette.addColorStop(0.15, 'rgba(0,0,0,0)');
  vignette.addColorStop(0.85, 'rgba(0,0,0,0)');
  vignette.addColorStop(1, 'rgba(65, 52, 34, 0.22)');
  ctx.fillStyle = vignette;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  const tex = new THREE.CanvasTexture(canvas);
  tex.colorSpace = THREE.SRGBColorSpace;
  return tex;
}

function buildLeatherBumpTexture() {
  const canvas = document.createElement('canvas');
  canvas.width = 256;
  canvas.height = 256;
  const ctx = canvas.getContext('2d');
  ctx.fillStyle = '#808080';
  ctx.fillRect(0, 0, 256, 256);
  for (let i = 0; i < 3600; i++) {
    const x = Math.abs((Math.sin(i * 19.31) * 43758.54) % 1) * 256;
    const y = Math.abs((Math.sin(i * 41.73) * 43758.54) % 1) * 256;
    ctx.fillStyle = i % 2 === 0 ? 'rgba(255,255,255,0.18)' : 'rgba(0,0,0,0.18)';
    ctx.fillRect(x, y, 2, 2);
  }
  const tex = new THREE.CanvasTexture(canvas);
  tex.wrapS = THREE.RepeatWrapping;
  tex.wrapT = THREE.RepeatWrapping;
  tex.repeat.set(3, 4);
  return tex;
}

export function createBook3DViewer(container, options = {}) {
  if (!container) return null;
  const canvas = container.querySelector('canvas');
  if (!canvas) return null;

  const opts = {
    title: options.title || 'LIBRARYHUB',
    subtitle: options.subtitle || 'THE ARCHITECTURAL EDITION',
    author: options.author || 'SCHOLASTIC ARCHIVE',
    badge: options.badge || 'CURATED COLLECTION',
    coverUrl: options.coverUrl || null,
    paddingFactor: options.paddingFactor || 1.38,
    particles: options.particles !== false,
    reducedMotion: window.matchMedia('(prefers-reduced-motion: reduce)').matches
  };

  let renderer;
  try {
    renderer = new THREE.WebGLRenderer({
      canvas,
      antialias: true,
      alpha: true,
      powerPreference: 'high-performance'
    });
  } catch {
    container.dataset.webglFailed = 'true';
    return null;
  }

  renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
  renderer.outputColorSpace = THREE.SRGBColorSpace;
  renderer.toneMapping = THREE.ACESFilmicToneMapping;
  renderer.toneMappingExposure = 1.15;
  renderer.shadowMap.enabled = true;
  renderer.shadowMap.type = THREE.PCFSoftShadowMap;

  const scene = new THREE.Scene();
  const camera = new THREE.PerspectiveCamera(36, 1, 0.1, 100);

  // Studio Lighting
  const ambientLight = new THREE.AmbientLight(0xfff5e4, 0.72);
  scene.add(ambientLight);

  const keyLight = new THREE.DirectionalLight(0xfff8ed, 2.35);
  keyLight.position.set(4.5, 6.2, 6.0);
  keyLight.castShadow = true;
  scene.add(keyLight);

  const fillLight = new THREE.DirectionalLight(0x9bd7c7, 0.95);
  fillLight.position.set(-5.0, -1.5, 4.0);
  scene.add(fillLight);

  const backKeyLight = new THREE.DirectionalLight(0xfff8ed, 2.35);
  backKeyLight.position.set(-4.5, 6.2, -6.0);
  scene.add(backKeyLight);

  const backFillLight = new THREE.DirectionalLight(0x9bd7c7, 0.95);
  backFillLight.position.set(5.0, -1.5, -4.0);
  scene.add(backFillLight);

  const rimLight = new THREE.PointLight(0xd4af37, 2.6, 16);
  rimLight.position.set(-2.5, 3.2, -3.6);
  scene.add(rimLight);

  // Build Hardcover Book Assembly
  const bookPivot = new THREE.Group();
  const bookGroup = new THREE.Group();

  const bookWidth = 2.08;
  const bookHeight = 2.96;
  const bookDepth = 0.34;
  const boardThickness = 0.045;
  const spineThickness = 0.055;

  const bumpMap = buildLeatherBumpTexture();
  let frontTex = buildCoverCanvas({
    title: opts.title,
    subtitle: opts.subtitle,
    author: opts.author,
    badge: opts.badge
  });
  const backTex = buildCoverCanvas({
    title: opts.title,
    subtitle: opts.subtitle,
    author: opts.author,
    isBack: true
  });
  const spineTex = buildSpineCanvas({ title: opts.title });
  const pageVertTex = buildPageEdgeCanvas(true);
  const pageHorizTex = buildPageEdgeCanvas(false);

  const frontMat = new THREE.MeshStandardMaterial({
    map: frontTex,
    bumpMap,
    bumpScale: 0.015,
    roughness: 0.42,
    metalness: 0.18
  });

  const backMat = new THREE.MeshStandardMaterial({
    map: backTex,
    bumpMap,
    bumpScale: 0.015,
    roughness: 0.42,
    metalness: 0.18
  });

  const spineMat = new THREE.MeshStandardMaterial({
    map: spineTex,
    bumpMap,
    bumpScale: 0.016,
    roughness: 0.44,
    metalness: 0.22
  });

  const darkEdgeMat = new THREE.MeshStandardMaterial({
    color: 0x0f1a17,
    roughness: 0.52,
    metalness: 0.16
  });

  // Front Board
  const frontBoard = new THREE.Mesh(
    new THREE.BoxGeometry(bookWidth, bookHeight, boardThickness),
    [darkEdgeMat, darkEdgeMat, darkEdgeMat, darkEdgeMat, frontMat, darkEdgeMat]
  );
  frontBoard.position.set(0, 0, bookDepth / 2 - boardThickness / 2);
  frontBoard.castShadow = true;
  frontBoard.receiveShadow = true;
  bookGroup.add(frontBoard);

  // Back Board
  const backBoard = new THREE.Mesh(
    new THREE.BoxGeometry(bookWidth, bookHeight, boardThickness),
    [darkEdgeMat, darkEdgeMat, darkEdgeMat, darkEdgeMat, darkEdgeMat, backMat]
  );
  backBoard.position.set(0, 0, -bookDepth / 2 + boardThickness / 2);
  backBoard.castShadow = true;
  backBoard.receiveShadow = true;
  bookGroup.add(backBoard);

  // Spine
  const spineMesh = new THREE.Mesh(
    new THREE.BoxGeometry(spineThickness, bookHeight, bookDepth),
    [darkEdgeMat, spineMat, darkEdgeMat, darkEdgeMat, darkEdgeMat, darkEdgeMat]
  );
  spineMesh.position.set(-bookWidth / 2 + spineThickness / 2 - 0.012, 0, 0);
  spineMesh.castShadow = true;
  bookGroup.add(spineMesh);

  // Paper Block
  const pageBlockMat = [
    new THREE.MeshStandardMaterial({ map: pageVertTex, roughness: 0.88 }),
    darkEdgeMat,
    new THREE.MeshStandardMaterial({ map: pageHorizTex, roughness: 0.88 }),
    new THREE.MeshStandardMaterial({ map: pageHorizTex, roughness: 0.88 }),
    darkEdgeMat,
    darkEdgeMat
  ];
  const pageBlock = new THREE.Mesh(
    new THREE.BoxGeometry(bookWidth - 0.09, bookHeight - 0.12, bookDepth - boardThickness * 1.9),
    pageBlockMat
  );
  pageBlock.position.set(0.015, 0, 0);
  bookGroup.add(pageBlock);

  // Silk Bookmark Ribbon
  const ribbonMat = new THREE.MeshStandardMaterial({
    color: 0x0f6b53,
    roughness: 0.45,
    metalness: 0.12
  });
  const ribbon = new THREE.Mesh(new THREE.BoxGeometry(0.11, 0.44, 0.012), ribbonMat);
  ribbon.position.set(0.32, -bookHeight / 2 - 0.16, 0.01);
  ribbon.rotation.z = 0.06;
  bookGroup.add(ribbon);

  // Gold ribbon tip
  const ribbonTip = new THREE.Mesh(
    new THREE.BoxGeometry(0.115, 0.05, 0.015),
    new THREE.MeshStandardMaterial({ color: 0xd4af37, roughness: 0.25, metalness: 0.85 })
  );
  ribbonTip.position.set(0.332, -bookHeight / 2 - 0.37, 0.01);
  ribbonTip.rotation.z = 0.06;
  bookGroup.add(ribbonTip);

  // Center of mass normalization -> exact (0,0,0)
  const boundingBox = new THREE.Box3().setFromObject(bookGroup);
  const center = boundingBox.getCenter(new THREE.Vector3());
  bookGroup.position.sub(center);

  // Compute BoundingSphere radius for anti-clipping camera auto-fit
  const boundingSphere = new THREE.Sphere();
  boundingBox.getBoundingSphere(boundingSphere);

  bookPivot.add(bookGroup);
  scene.add(bookPivot);

  // Ambient Floating Gold Dust Particles
  let particlesMesh = null;
  if (opts.particles && !opts.reducedMotion) {
    const particleCount = 65;
    const positions = new Float32Array(particleCount * 3);
    for (let i = 0; i < particleCount; i++) {
      positions[i * 3] = (Math.random() - 0.5) * 5.4;
      positions[i * 3 + 1] = (Math.random() - 0.5) * 5.2;
      positions[i * 3 + 2] = (Math.random() - 0.5) * 3.8;
    }
    const pGeo = new THREE.BufferGeometry();
    pGeo.setAttribute('position', new THREE.BufferAttribute(positions, 3));
    const pMat = new THREE.PointsMaterial({
      color: 0xe6c86e,
      size: 0.038,
      transparent: true,
      opacity: 0.68,
      blending: THREE.AdditiveBlending,
      depthWrite: false
    });
    particlesMesh = new THREE.Points(pGeo, pMat);
    scene.add(particlesMesh);
  }

  // Auto-Fit Camera Distance inside ResizeObserver (Anti-Clipping)
  function fitCamera() {
    const width = Math.max(container.clientWidth, 180);
    const height = Math.max(container.clientHeight, 240);
    camera.aspect = width / height;
    const vFov = THREE.MathUtils.degToRad(camera.fov);
    const hFov = 2 * Math.atan(Math.tan(vFov / 2) * camera.aspect);
    const limitingFov = Math.min(vFov, hFov);
    const safeDistance = (boundingSphere.radius * opts.paddingFactor) / Math.sin(limitingFov / 2);
    camera.position.set(0, 0.08, safeDistance);
    camera.lookAt(0, 0, 0);
    camera.updateProjectionMatrix();
    renderer.setSize(width, height, false);
  }

  const resizeObserver = new ResizeObserver(() => fitCamera());
  resizeObserver.observe(container);
  fitCamera();

  // Interactive Rotation + Parallax + Inertia
  const defaultRotX = 0.14;
  const defaultRotY = -0.34;
  let targetRotX = defaultRotX;
  let targetRotY = defaultRotY;
  let currentRotX = defaultRotX;
  let currentRotY = defaultRotY;
  let velX = 0;
  let velY = 0;
  let parallaxX = 0;
  let parallaxY = 0;
  let isDragging = false;
  let lastPointerX = 0;
  let lastPointerY = 0;

  const onPointerDown = (e) => {
    isDragging = true;
    lastPointerX = e.clientX;
    lastPointerY = e.clientY;
    canvas.style.cursor = 'grabbing';
    canvas.setPointerCapture?.(e.pointerId);
  };

  const onPointerMove = (e) => {
    const rect = canvas.getBoundingClientRect();
    if (rect.width > 0 && rect.height > 0) {
      parallaxX = ((e.clientX - rect.left) / rect.width - 0.5) * 0.18;
      parallaxY = ((e.clientY - rect.top) / rect.height - 0.5) * 0.14;
    }
    if (!isDragging) return;
    const dx = e.clientX - lastPointerX;
    const dy = e.clientY - lastPointerY;
    lastPointerX = e.clientX;
    lastPointerY = e.clientY;
    velY = dx * 0.0085;
    velX = dy * 0.0075;
    targetRotY += velY;
    targetRotX = THREE.MathUtils.clamp(targetRotX + velX, -1.35, 1.35);
  };

  const onPointerUp = (e) => {
    isDragging = false;
    canvas.style.cursor = 'grab';
    canvas.releasePointerCapture?.(e.pointerId);
  };

  canvas.style.cursor = 'grab';
  canvas.style.touchAction = 'none';
  canvas.addEventListener('pointerdown', onPointerDown);
  window.addEventListener('pointermove', onPointerMove);
  window.addEventListener('pointerup', onPointerUp);

  function applyCoverUrl(url, meta = {}) {
    const activeTitle = meta.title || opts.title;
    const activeAuthor = meta.author || opts.author;
    const activeSubtitle = meta.subtitle || opts.subtitle;

    const rebuildBackAndSpine = (palette = null) => {
      const styleOpts = palette
        ? {
            title: activeTitle,
            author: activeAuthor,
            subtitle: activeSubtitle,
            coverColor: palette.coverColor,
            coverMidColor: palette.coverMidColor,
            coverDeepColor: palette.coverDeepColor,
            accentColor: palette.accentColor,
            textColor: palette.textColor,
            isLight: palette.isLight
          }
        : {
            title: activeTitle,
            author: activeAuthor,
            subtitle: activeSubtitle
          };

      const nextBack = buildCoverCanvas({ ...styleOpts, isBack: true });
      backMat.map?.dispose();
      backMat.map = nextBack;
      backMat.roughness = palette?.isLight ? 0.56 : 0.42;
      backMat.metalness = palette?.isLight ? 0.06 : 0.18;
      backMat.needsUpdate = true;

      const nextSpine = buildSpineCanvas(styleOpts);
      spineMat.map?.dispose();
      spineMat.map = nextSpine;
      spineMat.roughness = palette?.isLight ? 0.56 : 0.44;
      spineMat.metalness = palette?.isLight ? 0.08 : 0.22;
      spineMat.needsUpdate = true;

      if (palette) {
        frontMat.roughness = palette.isLight ? 0.56 : 0.42;
        frontMat.metalness = palette.isLight ? 0.06 : 0.18;
        frontMat.needsUpdate = true;
        darkEdgeMat.color.set(palette.edgeColor);
        darkEdgeMat.needsUpdate = true;
      } else {
        frontMat.roughness = 0.42;
        frontMat.metalness = 0.18;
        frontMat.needsUpdate = true;
        darkEdgeMat.color.setHex(0x0f1a17);
        darkEdgeMat.needsUpdate = true;
      }
    };

    if (meta.title || meta.author || meta.subtitle) {
      const nextProc = buildCoverCanvas({
        title: activeTitle,
        subtitle: activeSubtitle,
        author: activeAuthor,
        badge: meta.badge || 'SELECTED VOLUME'
      });
      frontMat.map?.dispose();
      frontMat.map = nextProc;
      frontMat.needsUpdate = true;
      rebuildBackAndSpine(null);
    }

    if (!url) return;
    const loader = new THREE.TextureLoader();
    loader.setCrossOrigin('anonymous');
    loader.load(
      url,
      (loadedTex) => {
        loadedTex.colorSpace = THREE.SRGBColorSpace;
        loadedTex.anisotropy = 4;
        frontMat.map?.dispose();
        frontMat.map = loadedTex;
        frontMat.needsUpdate = true;

        const palette = extractPaletteFromImage(loadedTex.image);
        rebuildBackAndSpine(palette);
      },
      undefined,
      () => {
        // Keep crisp procedural cover when remote cover cannot load due to CORS or offline
      }
    );
  }

  if (opts.coverUrl) applyCoverUrl(opts.coverUrl);

  function resetView() {
    targetRotX = defaultRotX;
    targetRotY = defaultRotY;
    velX = 0;
    velY = 0;
  }

  // Render Loop paused when off-screen
  let isVisible = true;
  let rafId = 0;
  const clock = new THREE.Clock();

  const intersectionObserver = new IntersectionObserver((entries) => {
    isVisible = entries.some((entry) => entry.isIntersecting);
    if (isVisible && !rafId) animate();
  }, { threshold: 0.05 });
  intersectionObserver.observe(container);

  function animate() {
    if (!isVisible) {
      rafId = 0;
      return;
    }
    rafId = requestAnimationFrame(animate);
    const t = clock.getElapsedTime();

    if (!isDragging) {
      targetRotY += velY;
      targetRotX = THREE.MathUtils.clamp(targetRotX + velX, -1.35, 1.35);
      velX *= 0.92;
      velY *= 0.92;
      if (!opts.reducedMotion) {
        targetRotY += Math.sin(t * 0.42) * 0.00075;
      }
    }

    currentRotX += (targetRotX + parallaxY - currentRotX) * 0.09;
    currentRotY += (targetRotY + parallaxX - currentRotY) * 0.09;

    bookPivot.rotation.x = currentRotX;
    bookPivot.rotation.y = currentRotY;
    bookPivot.position.y = opts.reducedMotion ? 0 : Math.sin(t * 1.35) * 0.085;

    if (particlesMesh) {
      particlesMesh.rotation.y = t * 0.035;
      particlesMesh.position.y = Math.sin(t * 0.5) * 0.06;
    }

    renderer.render(scene, camera);
  }
  animate();

  return {
    resetView,
    updateBook(meta) {
      applyCoverUrl(meta.coverUrl, meta);
      resetView();
    },
    destroy() {
      cancelAnimationFrame(rafId);
      resizeObserver.disconnect();
      intersectionObserver.disconnect();
      canvas.removeEventListener('pointerdown', onPointerDown);
      window.removeEventListener('pointermove', onPointerMove);
      window.removeEventListener('pointerup', onPointerUp);
      renderer.dispose();
    }
  };
}
