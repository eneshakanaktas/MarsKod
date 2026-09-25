// Metro (paketleyici) ayarı.
// Three.js'in CommonJS sürümü telefonda olmayan process.emitWarning'i çağırıyor; ayrıca iki kopya
// yüklenirse 3D nesneler birbirini tanımaz. Her yerde tek ve modern sürümü (three.module.js) kullan.

const path = require('path');
const { getDefaultConfig } = require('expo/metro-config');

const config = getDefaultConfig(__dirname);
const threeModule = path.resolve(__dirname, 'node_modules/three/build/three.module.js');
const defaultResolve = config.resolver.resolveRequest;

config.resolver.resolveRequest = (context, moduleName, platform) => {
  if (moduleName === 'three') return { type: 'sourceFile', filePath: threeModule };
  return (defaultResolve ?? context.resolveRequest)(context, moduleName, platform);
};

module.exports = config;
