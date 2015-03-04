# About

This helper script is used to checkout, cmake, compile and install dcmtk and all of its components on Windows. The generated binaries currently only support the x86 architecture.

# How to use

1. Install git and cmake (both for windows) and ensure that they are in your PATH.
2. Checkout this repository
3. Run the command ```psake``` from the command line
4. Check the created ```dcmtk-output``` directory to see all the generated executables and lib files. 

# Details

```psake``` - By default, will run ```clean```, ```checkout```, ```cmake```, ```build```, ```install```

```psake clean``` - Will delete all directories (source code, build, and output).

```psake checkout``` - Will checkout the latest dcmtk repository from ```http://git.dcmtk.org/dcmtk.git``` into ```dcmtk-source```

```psake cmake``` - Will run cmake to generate Visual Studio projects/sln files into ```dcmtk-build```.

```psake build``` - Will simply compile the Visual Studio files (created from ```cmake```). Nothing else.

```psake install``` - Will copy over all build outoputs, header files, etc, into the ```dcmtk-output``` directory. Be sure to run ```build``` before ```install```).


