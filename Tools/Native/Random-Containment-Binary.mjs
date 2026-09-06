import {
    Forˉeachˉbounded,
    Hostˉartifact,
    Oneˉline,
    Require,
    Requireˉinputˉpreserved,
    Runˉprocess,
    Verifyˉartifact,
} from "./Random-Containment-Host.mjs";

export async function Testˉwvb(Repositoryˉroot, Cases) {
    Require(Cases.length === 1000, "The selected WVB case count differs.");
    const Artifact = Hostˉartifact({
        win32: [
            "Artifacts/Native-Front-Door/windows-x64/wvverify.exe",
            3_983_360,
            "acfdf67d0c93ef9a7c503263d573f5466d4825841007149d7c0e7e1fbfc4b9a6",
        ],
        linux: [
            "Artifacts/Native-Front-Door/linux-x64/wvverify.elf",
            3_985_408,
            "bacd557c03dd92ebd9a11d32ae85e4c243822d2819a8b22730043b240a4b145f",
        ],
    });
    const Verifier = await Verifyˉartifact(
        Repositoryˉroot,
        Artifact,
        true,
        "native WVB verifier",
    );
    await Forˉeachˉbounded(Cases, 4, async Case => {
        const Result = await Runˉprocess(Verifier.Fileˉpath, [Case.Inputˉpath]);
        Require(
            Result.Code === 1,
            `${Case.Name}: native WVB verifier exit ${Result.Code} differs; ` +
                `diagnostic=${JSON.stringify(Result.Error.toString("utf8"))}.`,
        );
        Require(Result.Output.byteLength === 0, `${Case.Name}: WVB rejection wrote output.`);
        const Diagnostic = Oneˉline(Result.Error, `${Case.Name} WVB diagnostic`);
        Require(
            /^wvb status=Invalid phase=[A-Za-z0-9ˉ]+(?: step=[a-z-]+)?$/u
                .test(Diagnostic),
            `${Case.Name}: WVB rejection report differs.`);
        await Requireˉinputˉpreserved(Case);
    });
}

export async function Testˉwvo(Repositoryˉroot, Cases) {
    Require(Cases.length === 500, "The selected WVO case count differs.");
    const Artifact = Hostˉartifact({
        win32: [
            "Artifacts/Native-Wvo-Object-Candidate/Wvo-Object.exe",
            1_058_304,
            "182739a91046cf3563924668cf724ba1ad17ac5007d91c023e6687de7f2b83a4",
        ],
        linux: [
            "Artifacts/Native-Wvo-Object-Candidate/Wvo-Object.elf",
            1_056_768,
            "b8f0367a8ced12227c9554101152bd5199ec0fd32e5e78210f5dd8a0761b81c7",
        ],
    });
    const Verifier = await Verifyˉartifact(
        Repositoryˉroot,
        Artifact,
        true,
        "native WVO verifier",
    );
    await Forˉeachˉbounded(Cases, 4, async Case => {
        const Result = await Runˉprocess(
            Verifier.Fileˉpath,
            ["verify", Case.Inputˉpath],
        );
        Require(
            Result.Code === 2,
            `${Case.Name}: native WVO verifier exit ${Result.Code} differs; ` +
                `diagnostic=${JSON.stringify(Result.Error.toString("utf8"))}.`,
        );
        Require(Result.Output.byteLength === 0, `${Case.Name}: WVO rejection wrote output.`);
        const Diagnostic = Oneˉline(Result.Error, `${Case.Name} WVO diagnostic`);
        Require(
            /^object status=[A-Za-z0-9ˉ]+ sections=[0-9]+ symbols=[0-9]+ relocations=[0-9]+ offset=[0-9]+$/u
                .test(Diagnostic),
            `${Case.Name}: WVO rejection report differs.`,
        );
        await Requireˉinputˉpreserved(Case);
    });
}
