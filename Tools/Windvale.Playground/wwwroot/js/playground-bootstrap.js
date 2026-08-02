const BUILD_VERSION = "__WINDVALE_BUILD_VERSION__";

function Resolveˉbootˉresource(Resourceˉtype, Resourceˉname, Defaultˉuri) {
    void Resourceˉname;

    if (Resourceˉtype !== "dotnetjs") {
        return Defaultˉuri;
    }

    const Versionedˉuri = new URL(Defaultˉuri, document.baseURI);
    Versionedˉuri.searchParams.set("v", BUILD_VERSION);
    return Versionedˉuri.toString();
}

Blazor.start({ loadBootResource: Resolveˉbootˉresource })
    .catch(Error => console.error(Error));
