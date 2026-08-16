export interface Featureˉmanifest {
  readonly Identifier: string;
  readonly Dependsˉon: readonly string[];
}

export function Resolveˉfeatureˉorder(
  Manifests: readonly Featureˉmanifest[]
): readonly Featureˉmanifest[] {
  const Byˉidentifier = new Map<string, Featureˉmanifest>();
  for (const Manifest of Manifests) {
    if (Manifest.Identifier.length === 0 || Byˉidentifier.has(Manifest.Identifier)) {
      throw new Error(`Invalid or duplicate feature identifier: ${Manifest.Identifier}`);
    }
    Byˉidentifier.set(Manifest.Identifier, Manifest);
  }

  const Visiting = new Set<string>();
  const Visited = new Set<string>();
  const Ordered: Featureˉmanifest[] = [];

  const Visit = (Manifest: Featureˉmanifest): void => {
    if (Visited.has(Manifest.Identifier)) {
      return;
    }
    if (Visiting.has(Manifest.Identifier)) {
      throw new Error(`Feature dependency cycle at ${Manifest.Identifier}.`);
    }

    Visiting.add(Manifest.Identifier);
    for (const Dependencyˉidentifier of Manifest.Dependsˉon) {
      const Dependency = Byˉidentifier.get(Dependencyˉidentifier);
      if (Dependency === undefined) {
        throw new Error(
          `Feature ${Manifest.Identifier} requires missing feature ${Dependencyˉidentifier}.`
        );
      }
      Visit(Dependency);
    }
    Visiting.delete(Manifest.Identifier);
    Visited.add(Manifest.Identifier);
    Ordered.push(Manifest);
  };

  for (const Manifest of Manifests) {
    Visit(Manifest);
  }
  return Object.freeze(Ordered);
}
