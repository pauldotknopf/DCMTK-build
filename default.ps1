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
    delete_directory $dcmtk_source_dir
    delete_directory $dcmtk_build_dir
}

task checkout {
    exec { git clone http://git.dcmtk.org/dcmtk.git $dcmtk_source_dir } 
}

task cmake -depends clean, checkout  {
    exec { cd $dcmtk_build_dir cmake } 
}

function global:delete_directory($directory_name) {
    rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}
