#l "common.cake"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var dcmtkTag = Argument("dcmtkTag", "DCMTK-3.6.5");

var baseDir = System.IO.Directory.GetCurrentDirectory();
var dcmtkSourceDir = System.IO.Path.Combine(baseDir, "dcmtk-source");
var dcmtkBuildDir = System.IO.Path.Combine(baseDir, "dcmtk-build");
var dcmtkOutputDir = System.IO.Path.Combine(baseDir, "dcmtk-output");
var dcmtkSupportLibrariesType = "MD";
var dcmtkSupportLibrariesDir = System.IO.Path.Combine(baseDir, "support-libraries", "dcmtk-3.6.5-win32-support-" + dcmtkSupportLibrariesType + "-iconv-msvc-15.8");
var dcmtkLibPngDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "libpng-1.6.37");
var dcmtkLibXmlDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "libxml2-iconv-2.9.9");
var dcmtkOpenSslDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "openssl-1.1.1d");
var dcmtkTiffDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "libtiff-4.0.10");
var dcmtkzLibDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "zlib-1.2.11");
var dcmtkiConvLibDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "libiconv-1.15");
var dcmtkJpegLibDir = System.IO.Path.Combine(dcmtkSupportLibrariesDir, "openjpeg-2.3.1");
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
    if(System.IO.Directory.Exists(dcmtkSourceDir))		
         DeleteDirectory(dcmtkSourceDir, true);
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
    
    command.Append(" -D DCMTK_WITH_ZLIB=ON");
    command.Append(" -D WITH_ZLIBINC=\"" + dcmtkzLibDir + "\"");

    command.Append(" -D DCMTK_WITH_PNG=ON");
    command.Append(" -D WITH_LIBPNGINC=\"" + dcmtkLibPngDir + "\"");

    command.Append(" -D DCMTK_WITH_XML=ON");
    command.Append(" -D WITH_LIBXMLINC=\"" + dcmtkLibXmlDir + "\"");

    command.Append(" -D DCMTK_WITH_OPENSSL=ON");
    command.Append(" -D WITH_OPENSSLINC=\"" + dcmtkOpenSslDir + "\"");

    command.Append(" -D DCMTK_WITH_OPENJPEG=ON");
    command.Append(" -D WITH_OPENJPEGINC=\"" + dcmtkJpegLibDir + "\"");

    command.Append(" -D DCMTK_WITH_TIFF=ON");
    command.Append(" -D WITH_LIBTIFFINC=\"" + dcmtkTiffDir + "\"");

    command.Append(" -D DCMTK_WITH_ICONV=ON");
    command.Append(" -D WITH_LIBICONVINC=\"" + dcmtkiConvLibDir + "\"");

    // Build a single shared library.
    command.Append(" -D DCMTK_SHARED_LIBRARIES=ON -D DCMTK_SINGLE_SHARED_LIBRARY=ON");
    command.Append(" -D BUILD_SHARED_LIBS=ON -D BUILD_SINGLE_SHARED_LIBRARY=ON");

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