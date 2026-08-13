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
            1_257_472,
            "2b870ae276ee8c53e7b6f7277a19067ddb9466e5ac7e0b640745a5d40810efd1",
        ],
        linux: [
            "Artifacts/Native-Front-Door/linux-x64/wvverify.elf",
            1_257_472,
            "fe84ab498fde5112e62398982bc76e3334e4bdec9e2502b87a2e4bb191fbdab3",
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
