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
            1_004_032,
            "5f0a83681f54c7e047d6b68c86f71767d6c3584330bef1e68108f9b3465167a7",
        ],
        linux: [
            "Artifacts/Native-Front-Door/linux-x64/wvverify.elf",
            1_003_520,
            "824e90ae07e82af3d6d0b4cf23bc4d3327fc3367684215171247fa71ab274982",
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
            606_720,
            "8c6f30b0b55898776d8dc394ea763313527650a361ceb6f478ffad48979084f1",
        ],
        linux: [
            "Artifacts/Native-Wvo-Object-Candidate/Wvo-Object.elf",
            606_208,
            "f94d2e16da76c949e15978bd879bff38205685be08d7afa1670f48d3f6592ea1",
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
