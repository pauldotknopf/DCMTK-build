#l "common.cake"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var dcmtkTag = Argument("dcmtkTag", "DCMTK-3.6.1_20170228");

var baseDir = System.IO.Directory.GetCurrentDirectory();
var dcmtkSourceDir = System.IO.Path.Combine(baseDir, "dcmtk-source");
var dcmtkBuildDir = System.IO.Path.Combine(baseDir, "dcmtk-build");
var dcmtkOutputDir = System.IO.Path.Combine(baseDir, "dcmtk-output");
var dcmtkSupportLibrariesType = "MD";
var dcmtkSupportLibrariesDir = System.IO.Path.Combine(baseDir, "support-libraries", "dcmtk-3.6.0-win32-i386-support_" + dcmtkSupportLibrariesType);
var dcmtkLibPngDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "libpng-1.4.3");
var dcmtkLibXmlDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "libxml2-2.7.7");
var dcmtkOpenSslDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "openssl-1.0.0c");
var dcmtkTiffDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "tiff-3.9.4");
var dcmtkzLibDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "zlib-1.2.5");
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
    if(System.IO.Directory.Exists(dcmtkBuildDir))
        DeleteDirectory(dcmtkBuildDir, true);
    if(System.IO.Directory.Exists(dcmtkOutputDir))
        DeleteDirectory(dcmtkOutputDir, true);
});

Task("Checkout")
    .Does(() =>
{
    ExecuteCommand("git clone http://git.dcmtk.org/dcmtk.git " + dcmtkSourceDir);
    ExecuteCommand("git checkout " + dcmtkTag, dcmtkSourceDir);
});

Task("Cmake")
    .Does(() =>
{
    var command = new StringBuilder();
    
    command.Append("cmake");
    command.Append(" -D CMAKE_INSTALL_PREFIX=\"" + dcmtkOutputDir + "\"");
    
    command.Append(" -G \"Visual Studio 12 2013\"");
    
    // add SAFESEH:NO for exe
    command.Append(" " + string.Join(" ", dcmtkBuildConfigurations.Select(x => string.Format("-D CMAKE_EXE_LINKER_FLAGS_{0}=\"/SAFESEH:NO\"", x))));
    // add SAFESEH:NO for shared
    command.Append(" " + string.Join(" ", dcmtkBuildConfigurations.Select(x => string.Format("-D CMAKE_SHARED_LINKER_FLAGS_{0}=\"/SAFESEH:NO\"", x))));
    // add SAFESEH:NO for module
    command.Append(" " + string.Join(" ", dcmtkBuildConfigurations.Select(x => string.Format("-D CMAKE_MODULE_LINKER_FLAGS_{0}=\"/SAFESEH:NO\"", x))));
    
    // enable zlib
    command.Append(" -D DCMTK_WITH_ZLIB=ON");
    command.Append(" -D WITH_ZLIBINC=\"" + dcmtkzLibDir + "\"");

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
    });
});

Task("Install")
    .Does(() =>
{
    if(!System.IO.Directory.Exists(dcmtkOutputDir))
        System.IO.Directory.CreateDirectory(dcmtkOutputDir);
    MSBuild("./dcmtk-build/INSTALL.vcxproj", settings => {
        settings.SetConfiguration(configuration);
    });
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Checkout")
    .IsDependentOn("Cmake")
    .IsDependentOn("Build")
    .IsDependentOn("Install");

RunTarget(target);