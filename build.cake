#l "common.cake"

var target = Argument("target", "Install");
var configuration = Argument("configuration", "Release");
var dcmtkTag = Argument("dcmtkTag", "DCMTK-3.6.7");

var baseDir = System.IO.Directory.GetCurrentDirectory();
var dcmtkSourceDir = System.IO.Path.Combine(baseDir, "dcmtk-source");
var dcmtkBuildDir = System.IO.Path.Combine(baseDir, "dcmtk-build");
var dcmtkOutputDir = System.IO.Path.Combine(baseDir, "dcmtk-output");
var dcmtkSupportLibrariesType = "MD";
var dcmtkSupportLibrariesDir = System.IO.Path.Combine(baseDir, "support-libraries", "dcmtk-3.6.7-win32-support-" + dcmtkSupportLibrariesType + "-iconv-msvc-15.9");
var dcmtkLibPngDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "libpng-1.6.37");
var dcmtkLibXmlDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "libxml2-iconv-2.9.13");
var dcmtkOpenSslDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "openssl-1.1.1n");
var dcmtkTiffDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "libtiff-4.3.0");
var dcmtkzLibDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "zlib-1.2.12");
var dcmtkLibICONVDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "libiconv-1.16");

var dcmtkBuildConfigurations = new List<string>{"DEBUG", "MINSIZEREL", "RELEASE", "RELWITHDEBINFO"};

System.Console.WriteLine("Configuration=" + configuration);

Task("EnsureDependencies")
    .Does(() =>
{
    EnsureTool("git", "--version");
    EnsureTool("cmake", "--version");
});

Task("Clean")
    .Does(() => 
{
    CleanDirectory(dcmtkBuildDir);
    CleanDirectory(dcmtkOutputDir);
});

Task("Checkout")
    .Does(() =>
{
    ExecuteCommand("git clone https://github.com/DCMTK/dcmtk.git " + dcmtkSourceDir);
    ExecuteCommand("git checkout " + dcmtkTag, dcmtkSourceDir);
});

Task("Cmake")
    .Does(() =>
{
    var command = new StringBuilder();
    
    command.Append("cmake");
    command.Append(" -D CMAKE_INSTALL_PREFIX=\"" + dcmtkOutputDir + "\"");
    
    command.Append(" -G \"Visual Studio 15 2017\"");
    
    // add SAFESEH:NO for exe
    command.Append(" " + string.Join(" ", dcmtkBuildConfigurations.Select(x => string.Format("-D CMAKE_EXE_LINKER_FLAGS_{0}=\"/SAFESEH:NO\"", x))));
    // add SAFESEH:NO for shared
    command.Append(" " + string.Join(" ", dcmtkBuildConfigurations.Select(x => string.Format("-D CMAKE_SHARED_LINKER_FLAGS_{0}=\"/SAFESEH:NO\"", x))));
    // add SAFESEH:NO for module
    command.Append(" " + string.Join(" ", dcmtkBuildConfigurations.Select(x => string.Format("-D CMAKE_MODULE_LINKER_FLAGS_{0}=\"/SAFESEH:NO\"", x))));
    
    // enable zlib
    command.Append(" -D DCMTK_WITH_ZLIB=ON");
    command.Append(" -D WITH_ZLIBINC=\"" + dcmtkzLibDir + "\"");

    command.Append(" -D DCMTK_WITH_ICONV=ON");
    command.Append(" -D WITH_LIBICONVINC=\"" + dcmtkLibICONVDir + "\"");

    // enable png
    command.Append(" -D DCMTK_WITH_PNG=ON");
    command.Append(" -D WITH_LIBPNGINC=\"" + dcmtkLibPngDir + "\"");
    
    // enable xml
    command.Append(" -D DCMTK_WITH_XML=ON");
    command.Append(" -D WITH_LIBXMLINC=\"" + dcmtkLibXmlDir + "\"");

    // enable openssl
    command.Append(" -D DCMTK_WITH_OPENSSL=ON");
    command.Append(" -D WITH_OPENSSLINC=\"" + dcmtkOpenSslDir + "\"");
    
    // enable tiff
    command.Append(" -D DCMTK_WITH_TIFF=ON");
    command.Append(" -D WITH_LIBTIFFINC=\"" + dcmtkTiffDir + "\"");
    
    // don't overrite compiler flags
    command.Append(" -D DCMTK_OVERWRITE_WIN32_COMPILER_FLAGS=OFF");
    
    // Build as a single shared library
    command.Append(" -D BUILD_SHARED_LIBS=ON");
    command.Append(" -D BUILD_SINGLE_SHARED_LIBRARY=ON");
    
    // our source directory we are running cmake in
    command.Append(" \"" + dcmtkSourceDir + "\"");
    
    // the output directory that will contain our sln/proj files.
    command.Append(" -B\"" + dcmtkBuildDir + "\"");
    
    // run the command!
    ExecuteCommand(command.ToString());
});

Task("Build")
    .Does(() =>
{
    MSBuild("./dcmtk-build/DCMTK.sln", settings => {
        settings.SetConfiguration(configuration);
        settings.UseToolVersion(MSBuildToolVersion.VS2017);
    });
});

Task("Install")
    .Does(() =>
{
    if(!System.IO.Directory.Exists(dcmtkOutputDir))
        System.IO.Directory.CreateDirectory(dcmtkOutputDir);
    MSBuild("./dcmtk-build/INSTALL.vcxproj", settings => {
        settings.SetConfiguration(configuration);
        settings.UseToolVersion(MSBuildToolVersion.VS2017);
    });
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Checkout")
    .IsDependentOn("Cmake")
    .IsDependentOn("Build")
    .IsDependentOn("Install");

RunTarget(target);