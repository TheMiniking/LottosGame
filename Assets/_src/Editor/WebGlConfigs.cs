using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class WebGlConfigs : Editor
{
    [MenuItem("WebGl/Optimize")]
    public static void OpenMyMenuWindow()
    {
        NamedBuildTarget namedBuildTarget = NamedBuildTarget.WebGL;
        BuildOptions buildOptions = BuildOptions.CompressWithLz4HC;
        // Set IL2CPP code generation to Optimize Size 
        PlayerSettings.SetIl2CppCodeGeneration(namedBuildTarget,
                                               Il2CppCodeGeneration.OptimizeSize);
        // Set the Managed Stripping Level to High
        PlayerSettings.SetManagedStrippingLevel(namedBuildTarget,
                                                ManagedStrippingLevel.High);
        PlayerSettings.SplashScreen.show = false;
        // Strip unused mesh components           
        PlayerSettings.stripUnusedMeshComponents = true;
        // Enable data caching
        PlayerSettings.WebGL.dataCaching = false;
        // Set the compression format to Brotli
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        // Deactivate exceptions
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        // Deactivate debug symbols
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        // Set Platform Settings to optimize for disk size (LTO)
        EditorUserBuildSettings.SetPlatformSettings(namedBuildTarget.TargetName,
                                                    "CodeOptimization",
                                                    "disksizelto");
        Debug.Log("Otimizado!");
    }
}
