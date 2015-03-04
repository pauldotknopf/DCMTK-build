Framework '4.5.1'

properties {
    $base_dir = resolve-path .
    $dcmtk_source_dir = "$base_dir\dcmtk-source"
    $dcmtk_build_dir = "$base_dir\dcmtk-build"
    $dcmtk_output_dir = "$base_dir\dcmtk-output"
    $dcmtk_support_libraries_type = "MD"
    $dcmtk_support_libraries_dir = "$base_dir\support-libraries\dcmtk-3.6.0-win32-i386-support_$dcmtk_support_libraries_type"
    $dcmtk_libpng_dir = "$dcmtk_support_libraries_dir\libpng-1.4.3"
    $dcmtk_libxml_dir = "$dcmtk_support_libraries_dir\libxml2-2.7.7"
    $dcmtk_openssl_dir = "$dcmtk_support_libraries_dir\openssl-1.0.0c"
    $dcmtk_tiff_dir = "$dcmtk_support_libraries_dir\tiff-3.9.4"
    $dcmtk_zlib_dir = "$dcmtk_support_libraries_dir\zlib-1.2.5"
}

task default -depends clean, checkout, cmake, build, install

task clean {
    delete_directory $dcmtk_source_dir
    delete_directory $dcmtk_build_dir
    delete_directory $dcmtk_output_dir
}

task checkout {
    exec { git clone http://git.dcmtk.org/dcmtk.git $dcmtk_source_dir } 
}

task cmake -depends clean, checkout  {
    exec { cmake -D CMAKE_INSTALL_PREFIX="$dcmtk_output_dir" -D CMAKE_EXE_LINKER_FLAGS_DEBUG="/SAFESEH:NO" -D CMAKE_EXE_LINKER_FLAGS_MINSIZEREL="/SAFESEH:NO" -D CMAKE_EXE_LINKER_FLAGS_RELEASE="/SAFESEH:NO" -D CMAKE_EXE_LINKER_FLAGS_RELWITHDEBINFO="/SAFESEH:NO" -D CMAKE_SHARED_LINKER_FLAGS_DEBUG="/SAFESEH:NO" -D CMAKE_SHARED_LINKER_FLAGS_MINSIZEREL="/SAFESEH:NO" -D CMAKE_SHARED_LINKER_FLAGS_RELEASE="/SAFESEH:NO" -D CMAKE_SHARED_LINKER_FLAGS_RELWITHDEBINFO="/SAFESEH:NO" -D CMAKE_MODULE_LINKER_FLAGS_DEBUG="/SAFESEH:NO" -D CMAKE_MODULE_LINKER_FLAGS_MINSIZEREL="/SAFESEH:NO" -D CMAKE_MODULE_LINKER_FLAGS_RELEASE="/SAFESEH:NO" -D CMAKE_MODULE_LINKER_FLAGS_RELWITHDEBINFO="/SAFESEH:NO" -D DCMTK_WITH_ZLIB=ON -D WITH_ZLIBINC="$dcmtk_zlib_dir" -D DCMTK_WITH_PNG=ON -D WITH_LIBPNGINC="$dcmtk_libpng_dir" -D DCMTK_WITH_XML=ON -D WITH_LIBXMLINC="$dcmtk_libxml_dir" -DCMTK_WITH_OPENSSL=ON -D WITH_OPENSSLINC="$dcmtk_openssl_dir" -D DCMTK_WITH_TIFF=ON -D WITH_LIBTIFFINC="$dcmtk_tiff_dir" -D DCMTK_OVERWRITE_WIN32_COMPILER_FLAGS=OFF "$dcmtk_source_dir" -B"$dcmtk_build_dir"  } 
}

task build {
    exec { msbuild "$dcmtk_build_dir\DCMTK.sln" /p:Configuration=Release }
}

task install {
    create_directory $dcmtk_output_dir;
    exec { msbuild "$dcmtk_build_dir\INSTALL.vcxproj" /p:Configuration=Release }
}

function global:delete_directory($directory_name) {
    rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}

function global:create_directory($directory_name) {
    if (!(Test-Path -path $directory_name)) { new-item $directory_name -force -type directory}
}