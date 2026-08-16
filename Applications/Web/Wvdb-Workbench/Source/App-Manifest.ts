import type { Featureˉmanifest } from '../../../../Libraries/Web/Framework/Composition/Feature-Manifest';

export const Wvdbˉworkbenchˉfeatures: readonly Featureˉmanifest[] = Object.freeze([
  { Identifier: 'wv.framework.lifecycle', Dependsˉon: [] },
  { Identifier: 'wv.framework.state', Dependsˉon: [] },
  { Identifier: 'wv.framework.localization', Dependsˉon: [] },
  { Identifier: 'wv.framework.theme', Dependsˉon: [] },
  {
    Identifier: 'wv.components.workbench-shell',
    Dependsˉon: ['wv.framework.lifecycle', 'wv.framework.state']
  },
  {
    Identifier: 'wvdb.workbench',
    Dependsˉon: [
      'wv.components.workbench-shell',
      'wv.framework.localization',
      'wv.framework.theme'
    ]
  }
]);
