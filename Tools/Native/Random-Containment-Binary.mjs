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
            1_255_936,
            "a1dc701cc8d5ace0a680a15e19435c48b3bccde3cf6197bfdd07ee04a4bf9871",
        ],
        linux: [
            "Artifacts/Native-Front-Door/linux-x64/wvverify.elf",
            1_257_472,
            "cb77e47f1d69530a16c661deecd91640764a13994d75c4994780e488e938b1f4",
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
        Require(/^wvb status=Invalid phase=[A-Za-z0-9ˉ]+$/u.test(Diagnostic),
            `${Case.Name}: WVB rejection report differs.`);
        await Requireˉinputˉpreserved(Case);
    });
}

export async function Testˉwvo(Repositoryˉroot, Cases) {
    Require(Cases.length === 500, "The selected WVO case count differs.");
    const Artifact = Hostˉartifact({
        win32: [
            "Artifacts/Native-Wvo-Object-Candidate/Wvo-Object.exe",
            1_037_312,
            "5362372e826958470eee7d90eb01938de5b91dcb3e1b0f952722e00578a82d03",
        ],
        linux: [
            "Artifacts/Native-Wvo-Object-Candidate/Wvo-Object.elf",
            1_036_288,
            "fcfd134222b05482a6ac432fc4acbfb72f3dfce92c3c646fc17595ddb078b840",
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
