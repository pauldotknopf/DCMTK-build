properties {
    $base_dir = resolve-path .
    $dcmtk_source_dir = "$base_dir\dcmtk-source"
    $dcmtk_build_dir = "$base_dir\dcmtk-build"
    $dcmtk_support_libraries_type = "MD"
    $dcmtk_support_libraries_dir = "$base_dir\support-libraries\dcmtk-3.6.0-win32-i386-support_$dcmtk_support_libraries_type"
    $dcmtk_libpng_dir = "$dcmtk_support_libraries_dir\libpng-1.4.3"
    $dcmtk_libxml_dir = "$dcmtk_support_libraries_dir\libxml2-2.7.7"
    $dcmtk_openssl_dir = "$dcmtk_support_libraries_dir\openssl-1.0.0c"
    $dcmtk_tiff_dir = "$dcmtk_support_libraries_dir\tiff-3.9.4"
    $dcmtk_zlib_dir = "$dcmtk_support_libraries_dir\zlib-1.2.5"
}

task default -depends cmake

task clean {
    # delete_directory $dcmtk_source_dir
    delete_directory $dcmtk_build_dir
}

task checkout {
    # exec { git clone http://git.dcmtk.org/dcmtk.git $dcmtk_source_dir } 
}

task cmake -depends clean, checkout  {
    exec { cmake -D DCMTK_WITH_ZLIB=ON -D WITH_ZLIBINC="$dcmtk_zlib_dir" -D DCMTK_WITH_PNG=ON -D WITH_LIBPNGINC="$dcmtk_libpng_dir" -D DCMTK_WITH_XML=ON -D WITH_LIBXMLINC="$dcmtk_libxml_dir" -DCMTK_WITH_OPENSSL=ON -D WITH_OPENSSLINC="$dcmtk_openssl_dir" -D DCMTK_WITH_TIFF=ON -D WITH_LIBTIFFINC="$dcmtk_tiff_dir" -D DCMTK_OVERWRITE_WIN32_COMPILER_FLAGS=OFF "$dcmtk_source_dir" -B"$dcmtk_build_dir"  } 
}

function global:delete_directory($directory_name) {
    rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}
