﻿namespace CSPspEmu.Core.Cpu
{
    public class DynarecConfig
    {
        //public const bool UpdatePCEveryInstruction = true;
        //public const bool UpdatePCEveryInstruction = false;
        public static bool UpdatePCEveryInstruction = false;

        public static bool FunctionCallWithStaticReferences = true;
        //public const bool FunctionCallWithStaticReferences = true;
        //public const bool FunctionCallWithStaticReferences = false;

        //public const bool EnableFastPspMemoryUtilsGetFastMemoryReader = false;
        public static bool EnableFastPspMemoryUtilsGetFastMemoryReader = false;
        //public const bool EnableFastPspMemoryUtilsGetFastMemoryReader = true;

        //public const bool AllowFastMemory = true;
        public static bool AllowFastMemory = true;


        //public const bool EmitCallTick = true;
        //public const bool EmitCallTick = false;
        public static bool EmitCallTick = false;

        //public const bool EnableTailCalling = true;
        public static bool EnableTailCalling = true;
        //public const bool EnableTailCall = false;

        //public const bool BranchFlagAsLocal = true;
        public static bool BranchFlagAsLocal = true;

        //public const bool DebugFunctionCreation = true;
        //public const bool DebugFunctionCreation = false;
        public static bool DebugFunctionCreation = false;

        //public const bool EnableGpuSignalsCallback = true;
        //public const bool EnableGpuFinishCallback = true;

        public const bool EnableGpuSignalsCallback = false;
        public const bool EnableGpuFinishCallback = false;

        //public const bool EnableRenderTarget = false;
        //public const bool EnableRenderTarget = true;
        public static bool EnableRenderTarget = true;

        //public const bool ImmediateLinking = false;
        //public const bool ImmediateLinking = true;
        public static bool ImmediateLinking = true;

        //public const bool AllowCreatingUsedFunctionsInBackground = false;
        //public const bool AllowCreatingUsedFunctionsInBackground = true; // Cause sometimes "System.InvalidProgramException: Common Language Runtime detectó un programa no válido."
        public static bool AllowCreatingUsedFunctionsInBackground = false;

        //public const bool DisableDotNetJitOptimizations = true;
        public const bool DisableDotNetJitOptimizations = false;

        //public const bool ForceJitOptimizationsOnEvenLargeFunctions = true;
        public static bool ForceJitOptimizationsOnEvenLargeFunctions = true;

        //public const int InstructionCountToDisableOptimizations = 500;
        //public const int InstructionCountToDisableOptimizations = 200;
        //public const int InstructionCountToDisableOptimizations = 100;
        public static int InstructionCountToDisableOptimizations = 100;

        //public const bool AllowCreatingUsedFunctionsInBackground = true;
    }
}