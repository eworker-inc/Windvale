namespace Windvale.Bytecode;

public static class Bytecodeˉlimits
{
    public const int MAX_MODULE_BYTES = 16 * 1024 * 1024;
    public const int SECTION_COUNT = 7;
    public const int MAX_UTF8_VALUE_BYTES = 1024 * 1024;
    public const int MAX_NAME_BYTES = 255;
    public const int MAX_CAPABILITIES = 32;
    public const int MAX_PLATFORM_SCOPES = 32;
    public const int MAX_CAPABILITY_REQUIREMENTS = 32;
    public const int MAX_DATA_DECLARATIONS = 4096;
    public const int MAX_FUNCTIONS = 4096;
    public const int MAX_PARAMETERS_OR_LOCALS = 8192;
    public const int MAX_CODE_BYTES_PER_FUNCTION = 1024 * 1024;
    public const int MAX_INSTRUCTIONS_PER_FUNCTION = 100_000;
    public const int MAX_OPERAND_STACK = 4096;
    public const int MAX_I32_ARRAY_ELEMENTS = 262_144;
    public const int MAX_BYTE_DATA_BYTES = 4 * 1024 * 1024;
    public const int MAX_NOMINAL_TYPES = 1_024;
    public const int MAX_RECORD_FIELDS = 64;
    public const int MAX_ENUM_MEMBERS = 256;
    public const int MAX_VARIANT_CASES = 256;
    public const uint MAX_SEQUENCE_ELEMENTS = 4095;
}
